using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class EnemySpawnInfo
{
    public GameObject enemyPrefab;
    public int countPerRoom;
    
}
public class CorridorFirstMapGeneration : SimpleRandomWalkMapGenerator
{
    [SerializeField]
    private int corridorLength = 14, corridorCount = 5;
    [SerializeField]
    [Range(0.1f, 1)]
    public float roomPercent = 0.8f;
    [SerializeField] 
    private GameObject playerPrefab;
    [SerializeField] 
    private GameObject exitPrefab;
    [SerializeField] 
    private GameObject keyPrefab;
    [SerializeField] 
    private GameObject enemyPrefab;
    [SerializeField] 
    private int numberOfKeys = 3;
    [SerializeField] 
    private int numberOfEnemies = 5;
    [SerializeField] 
    private List<EnemySpawnInfo> enemyTypes;
    [SerializeField]
    private GameObject waypointPrefab;
    [SerializeField]
    private int waypointsPerRoom = 1;
    [SerializeField]
    public GetWaypoints getWaypoints;


    //PCG Data
    private Dictionary<Vector2Int, HashSet<Vector2Int>> roomsDictionary = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
    private HashSet<Vector2Int> floorPositions, corridorPositions;
    private List<Color> roomColors = new List<Color>();
    protected override void RunProceduralGeneration()
    {
        CorridorFirstGeneration();
    }

    private void CorridorFirstGeneration()
    {
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> potentialRoomPositions = new HashSet<Vector2Int>();

        List<List<Vector2Int>> corridors = CreateCorridors(floorPositions, potentialRoomPositions);

        HashSet<Vector2Int> roomPositions = CreateRooms(potentialRoomPositions);

        floorPositions.UnionWith(roomPositions);

        for (int i = 0; i < corridors.Count; i++)
        {
            corridors[i] = IncreaseCorridorBrush3by3(corridors[i]);
            floorPositions.UnionWith(corridors[i]);
        }

        tilemapVisualizer.PaintFloorTiles(floorPositions);
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);
        PlaceGameplayObjects();
    }

    private void PlaceGameplayObjects()
    {
        if (roomsDictionary.Count == 0) return;

        var roomCenters = roomsDictionary.Keys.ToList();
        var startRoom = roomCenters[0];
        var farthestRoom = roomCenters
            .OrderByDescending(r => Vector2Int.Distance(startRoom, r))
            .First();

        // Instanciar jugador
        Instantiate(playerPrefab, new Vector3(startRoom.x, startRoom.y, 0), Quaternion.identity);

        // Instanciar salida
        Instantiate(exitPrefab, new Vector3(farthestRoom.x, farthestRoom.y, 0), Quaternion.identity);

        // Habitaciones restantes
        var middleRooms = roomCenters.Except(new[] { startRoom, farthestRoom }).OrderBy(x => Guid.NewGuid()).ToList();

        // Llaves
        int keysToPlace = Mathf.Min(numberOfKeys, middleRooms.Count);
        for (int i = 0; i < keysToPlace; i++)
        {
            Vector2Int keyRoom = middleRooms[i];
            Instantiate(keyPrefab, new Vector3(keyRoom.x, keyRoom.y, 0), Quaternion.identity);
        }

        // Enemigos y Waypoints
        for (int i = keysToPlace; i < middleRooms.Count; i++)
        {
            Vector2Int roomCenter = middleRooms[i];
            if (!roomsDictionary.TryGetValue(roomCenter, out var roomFloor)) continue;

            // Instanciar enemigos
            foreach (var enemyInfo in enemyTypes)
            {
                for (int j = 0; j < enemyInfo.countPerRoom; j++)
                {
                    Vector2Int randomPos = roomFloor.ElementAt(UnityEngine.Random.Range(0, roomFloor.Count));
                    Instantiate(enemyInfo.enemyPrefab, new Vector3(randomPos.x, randomPos.y, 0), Quaternion.identity);
                }
            }

            // Instanciar waypoints
            for (int w = 0; w < waypointsPerRoom; w++)
            {
                Vector2Int randomWaypointPos = roomFloor.ElementAt(UnityEngine.Random.Range(0, roomFloor.Count));
                Transform newWaypoint = Instantiate(waypointPrefab, new Vector3(randomWaypointPos.x, randomWaypointPos.y, 0), Quaternion.identity).transform;

                // Añadir el waypoint a la lista de GetWaypoints
                if (getWaypoints != null)
                {
                    getWaypoints.RegisterWaypoint(newWaypoint);
                }
            }
        }
    }


    public List<Vector2Int> IncreaseCorridorBrush3by3(List<Vector2Int> corridor)
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

    //2025_05_08_modif_______
    private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions)
    {
        HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int>();
        int roomToCreateCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent);

        List<Vector2Int> roomsToCreate = potentialRoomPositions.OrderBy( x => Guid.NewGuid()).Take(roomToCreateCount).ToList();
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
        roomColors.Add(UnityEngine.Random.ColorHSV());

    }

    private void ClearRoomData()
    {
        roomsDictionary.Clear();
        roomColors.Clear();
    }

    //2025_05_08_modif_______
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


}
