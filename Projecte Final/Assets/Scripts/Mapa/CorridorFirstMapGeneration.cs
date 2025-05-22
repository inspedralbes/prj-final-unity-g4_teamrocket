using System;
using System.Collections.Generic;
using System.Linq;
using NavMeshPlus.Components;
using UnityEngine;
using Mirror;
using Random = UnityEngine.Random;

[System.Serializable]
public class EnemySpawnInfo
{
    public GameObject enemyPrefab;
    public int countPerRoom;
}

[System.Serializable]
public struct WallTileData
{
    public Vector2Int position;
    public string binaryType;
}

public class CorridorFirstMapGeneration : SimpleRandomWalkMapGenerator
{
    [Header("Network Configuration")]
    [SyncVar(hook = nameof(OnSeedChanged))] 
    private int networkSeed;
    
    [Header("Map Generation Settings")]
    [SerializeField] private int corridorLength = 14;
    [SerializeField] private int corridorCount = 5;
    [SerializeField] [Range(0.1f, 1)] public float roomPercent = 0.8f;
    
    [Header("Gameplay Objects")]
    [SerializeField] private GameObject exitPrefab;
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private int numberOfKeys = 3;

    [Header("Enemies Configuration")]
    [SerializeField] private GameObject stalkerEnemyPrefab;
    [SerializeField] private GameObject freezeEnemyPrefab;
    [SerializeField] private GameObject ghostEnemyPrefab;
    [SerializeField] private GameObject blindEnemyPrefab;
    [SerializeField] private List<EnemySpawnInfo> enemyTypes;

    [Header("References")]
    [SerializeField] private GameObject waypointPrefab;
    [SerializeField] public GetWaypoints getWaypoints;
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private NetworkTilemapVisualizer networkTilemapVisualizer;

    private Dictionary<Vector2Int, HashSet<Vector2Int>> roomsDictionary = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
    private HashSet<Vector2Int> floorPositions, corridorPositions;
    private List<Color> roomColors = new List<Color>();

    protected override void RunProceduralGeneration()
    {
        if (isServer)
        {
            networkSeed = Random.Range(int.MinValue, int.MaxValue);
            GenerateMap();
        }
    }

    private void GenerateMap()
    {
        Random.InitState(networkSeed);
        CorridorFirstGeneration();
    }

    private void OnSeedChanged(int oldSeed, int newSeed)
    {
        if (!isServer)
        {
            GenerateMap();
        }
    }

    private void CorridorFirstGeneration()
    {
        floorPositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> potentialRoomPositions = new HashSet<Vector2Int>();

        List<List<Vector2Int>> corridors = CreateCorridors(floorPositions, potentialRoomPositions);
        HashSet<Vector2Int> roomPositions = CreateRooms(potentialRoomPositions);
        
        floorPositions.UnionWith(roomPositions);

        for (int i = 0; i < corridors.Count; i++)
        {
            corridors[i] = IncreaseCorridorBrush3by3(corridors[i]);
            floorPositions.UnionWith(corridors[i]);
        }

        SyncMapWithClients(floorPositions);
        
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }
        else
        {
            Debug.LogWarning("NavMeshSurface no asignado en el Inspector.");
        }

        PlaceGameplayObjects();
    }

    private void SyncMapWithClients(HashSet<Vector2Int> floorPositions)
    {
        if (networkTilemapVisualizer == null)
        {
            Debug.LogError("NetworkTilemapVisualizer no asignado!");
            return;
        }

        networkTilemapVisualizer.RpcClearAllTiles();
        networkTilemapVisualizer.RpcPaintFloorTiles(new List<Vector2Int>(floorPositions));
        
        var basicWalls = new List<WallTileData>();
        var cornerWalls = new List<WallTileData>();
        
        var basicWallPositions = WallGenerator.FindWallsInDirections(floorPositions, Direction2D.cardinalDirectionsList);
        foreach (var position in basicWallPositions)
        {
            string binaryType = GetNeighborsBinaryType(position, floorPositions, Direction2D.cardinalDirectionsList);
            basicWalls.Add(new WallTileData { position = position, binaryType = binaryType });
        }
        
        var cornerWallPositions = WallGenerator.FindWallsInDirections(floorPositions, Direction2D.diagonalDirectionsList);
        foreach (var position in cornerWallPositions)
        {
            string binaryType = GetNeighborsBinaryType(position, floorPositions, Direction2D.eigthDirectionsList);
            cornerWalls.Add(new WallTileData { position = position, binaryType = binaryType });
        }
        
        networkTilemapVisualizer.RpcPaintBasicWalls(basicWalls);
        networkTilemapVisualizer.RpcPaintCornerWalls(cornerWalls);
    }

    private string GetNeighborsBinaryType(Vector2Int position, HashSet<Vector2Int> floorPositions, List<Vector2Int> directions)
    {
        string neighboursBinaryType = "";
        foreach (var direction in directions)
        {
            var neighbourPosition = position + direction;
            neighboursBinaryType += floorPositions.Contains(neighbourPosition) ? "1" : "0";
        }
        return neighboursBinaryType;
    }

    private void PlaceGameplayObjects()
    {
        if (roomsDictionary.Count == 0) return;

        var roomCenters = roomsDictionary.Keys.ToList();
        var startRoom = roomCenters[0];
        var farthestRoom = roomCenters.OrderByDescending(r => Vector2Int.Distance(startRoom, r)).First();

        SpawnNetworkObject(exitPrefab, new Vector3(farthestRoom.x, farthestRoom.y, 0));

        var middleRooms = roomCenters.Except(new[] { startRoom, farthestRoom }).OrderBy(x => Guid.NewGuid()).ToList();

        int keysToPlace = Mathf.Min(numberOfKeys, middleRooms.Count);
        for (int i = 0; i < keysToPlace; i++)
        {
            Vector2Int keyRoom = middleRooms[i];
            SpawnNetworkObject(keyPrefab, new Vector3(keyRoom.x, keyRoom.y, 0));
        }

        foreach (var kvp in roomsDictionary)
        {
            Vector2Int roomCenter = kvp.Key;
            var roomFloor = kvp.Value;

            if (roomCenter == startRoom) continue;

            Vector2 center = GetAveragePosition(roomFloor);
            Transform newWaypoint = Instantiate(waypointPrefab, new Vector3(center.x, center.y, 0), Quaternion.identity).transform;

            if (getWaypoints != null)
            {
                getWaypoints.RegisterWaypoint(newWaypoint);
            }
            
            if (middleRooms.Contains(roomCenter) && enemyTypes.Count > 0)
            {
                var randomEnemyType = enemyTypes[Random.Range(0, enemyTypes.Count)];
                SpawnNetworkObject(randomEnemyType.enemyPrefab, newWaypoint.position);
            }
        }

        SpawnSpecialEnemies(middleRooms, keysToPlace);
    }

    private void SpawnSpecialEnemies(List<Vector2Int> middleRooms, int keysToPlace)
    {
        if (stalkerEnemyPrefab != null && getWaypoints != null && getWaypoints.waypoints.Count > 0)
        {
            Transform chosenRespawn = getWaypoints.waypoints[Random.Range(0, getWaypoints.waypoints.Count)];
            SpawnNetworkObject(stalkerEnemyPrefab, chosenRespawn.position);
        }

        int freezeEnemiesToPlace = Mathf.FloorToInt(roomsDictionary.Count / 4f);
        var freezeEnemyRooms = middleRooms.Take(freezeEnemiesToPlace).ToList();
        foreach (var roomCenter in freezeEnemyRooms)
        {
            Transform chosenRespawn = getWaypoints.waypoints[Random.Range(0, getWaypoints.waypoints.Count)];
            SpawnNetworkObject(freezeEnemyPrefab, chosenRespawn.position);
        }

        int ghostEnemiesToSpawn = 3;
        if (getWaypoints != null && getWaypoints.waypoints.Count >= ghostEnemiesToSpawn)
        {
            var selectedWaypoints = getWaypoints.waypoints
                .OrderBy(x => Random.value)
                .Take(ghostEnemiesToSpawn)
                .ToList();

            foreach (var waypoint in selectedWaypoints)
            {
                SpawnNetworkObject(ghostEnemyPrefab, waypoint.position);
            }
        }

        if (blindEnemyPrefab != null && keysToPlace > 0)
        {
            Vector2Int guardianRoom = middleRooms[Random.Range(0, keysToPlace)];
            if (roomsDictionary.TryGetValue(guardianRoom, out var roomFloor))
            {
                Vector2 center = GetAveragePosition(roomFloor);
                SpawnNetworkObject(blindEnemyPrefab, new Vector3(center.x, center.y, 0));
            }
        }
    }

    private void SpawnNetworkObject(GameObject prefab, Vector3 position)
    {
        if (!NetworkServer.active) return;

        GameObject obj = Instantiate(prefab, position, Quaternion.identity);
        NetworkServer.Spawn(obj);
    }

    private Vector2 GetAveragePosition(HashSet<Vector2Int> roomFloor)
    {
        Vector2 sum = Vector2.zero;
        foreach (var pos in roomFloor)
        {
            sum += new Vector2(pos.x, pos.y);
        }
        return sum / roomFloor.Count;
    }

    private List<Vector2Int> IncreaseCorridorBrush3by3(List<Vector2Int> corridor)
    {
        List<Vector2Int> newCorridor = new List<Vector2Int>();
        for (int i = 1; i < corridor.Count; i++)
        {
            for (int x = -1; x < 2; x++)
            {
                for(int y = -1; y < 2; y++)
                {
                    newCorridor.Add(corridor[i - 1] + new Vector2Int(x, y));
                }
            }
        }
        return newCorridor;
    }

    private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions)
    {
        HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int>();
        int roomToCreateCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent);

        List<Vector2Int> roomsToCreate = potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomToCreateCount).ToList();
        ClearRoomData();
        foreach (var roomPosition in roomsToCreate)
        {
            var roomFloor = RunRandomWalk(randomWalkParameters, roomPosition);
            SaveRoomData(roomPosition, roomFloor);
            roomPositions.UnionWith(roomFloor);
        }
        return roomPositions;
    }

    private void SaveRoomData(Vector2Int roomPosition, HashSet<Vector2Int> roomFloor)
    {
        roomsDictionary[roomPosition] = roomFloor;
        roomColors.Add(Random.ColorHSV());
    }

    private void ClearRoomData()
    {
        roomsDictionary.Clear();
        roomColors.Clear();
    }

    private List<List<Vector2Int>> CreateCorridors(HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> potentialRoomPositions)
    {
        var currentPosition = startPosition;
        potentialRoomPositions.Add(currentPosition);
        List<List<Vector2Int>> corridors = new List<List<Vector2Int>>();

        for (int i = 0; i < corridorCount; i++)
        {
            var corridor = ProceduralGenerationAlgorithms.RandomWalkCorridor(currentPosition, corridorLength);
            corridors.Add(corridor);
            currentPosition = corridor[corridor.Count - 1];
            potentialRoomPositions.Add(currentPosition);
            floorPositions.UnionWith(corridor);
        }
        corridorPositions = new HashSet<Vector2Int>(floorPositions);
        return corridors;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(startPosition.x, startPosition.y, 0), 0.5f);
    }
}