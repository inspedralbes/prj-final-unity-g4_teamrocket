using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        // Instancia el jugador en la primera habitación
        Vector3 playerPos = new Vector3(startRoom.x, startRoom.y, 0);
        Instantiate(playerPrefab, playerPos, Quaternion.identity);

        // Instancia la salida en la habitación más alejada
        Vector3 exitPos = new Vector3(farthestRoom.x, farthestRoom.y, 0);
        Instantiate(exitPrefab, exitPos, Quaternion.identity);

        // Excluir la habitación inicial y la de salida
        var middleRooms = roomCenters.Except(new[] { startRoom, farthestRoom }).OrderBy(x => Guid.NewGuid()).ToList();

        // Llaves
        for (int i = 0; i < Mathf.Min(numberOfKeys, middleRooms.Count); i++)
        {
            var keyPos = new Vector3(middleRooms[i].x, middleRooms[i].y, 0);
            Instantiate(keyPrefab, keyPos, Quaternion.identity);
        }

        // Enemigos
        for (int i = 0; i < Mathf.Min(numberOfEnemies, middleRooms.Count - numberOfKeys); i++)
        {
            var enemyRoom = middleRooms[numberOfKeys + i];
            var enemyPos = new Vector3(enemyRoom.x, enemyRoom.y, 0);
            Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
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
