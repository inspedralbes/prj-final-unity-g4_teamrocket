#pragma warning disable 0649
using UnityEngine;
using Mirror;
using System.Collections.Generic;

public abstract class SimpleRandomWalkMapGenerator : NetworkBehaviour
{
    [SerializeField] protected Vector2Int startPosition = Vector2Int.zero;
    [SerializeField] protected SimpleRandomWalkSO randomWalkParameters;
    [SerializeField] protected NetworkTilemapVisualizer networkTilemapVisualizer;

    public void GenerateDungeon()
    {
        RunProceduralGeneration();
    }

    protected abstract void RunProceduralGeneration();

    protected HashSet<Vector2Int> RunRandomWalk(SimpleRandomWalkSO parameters, Vector2Int position)
    {
        var currentPosition = position;
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        
        for (int i = 0; i < parameters.iterations; i++)
        {
            var path = ProceduralGenerationAlgorithms.SimpleRandomWalk(currentPosition, parameters.walkLength);
            floorPositions.UnionWith(path);
            
            if (parameters.startRandomlyEachIteration)
            {
                var positionsList = new List<Vector2Int>(floorPositions);
                currentPosition = positionsList[Random.Range(0, positionsList.Count)];
            }
        }
        return floorPositions;
    }
}
#pragma warning restore 0649