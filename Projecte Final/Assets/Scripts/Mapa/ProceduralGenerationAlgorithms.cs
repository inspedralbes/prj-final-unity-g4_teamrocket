using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ProceduralGenerationAlgorithms
{
    public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startPosition, int walkLength)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();

        path.Add(startPosition);
        var previousposition = startPosition;
        for (int i = 0; i < walkLength; i++)
        {
            var newPosition = previousposition + Direction2D.GetRandomCardinalDirection();
            path.Add(newPosition);
            previousposition = newPosition;
        }
        return path;
    }

    public static List<Vector2Int> RandomWalkCorridor(Vector2Int startPosition, int corridorLength)
    {
        List<Vector2Int> corridor = new List<Vector2Int>();
        var direction = Direction2D.GetRandomCardinalDirection();
        var currentPosition = startPosition;
        corridor.Add(currentPosition);

        for (int i = 0; i < corridorLength; i++)
        {
            currentPosition += direction;
            corridor.Add(currentPosition);
        }
        return corridor;
    }
}

public static class Direction2D
{
    public static List<Vector2Int> cardinalDirectionsList = new List<Vector2Int>
    {
        new Vector2Int(0,1), //arriba
        new Vector2Int(1,0), //derecha
        new Vector2Int(0,-1), //abajo
        new Vector2Int(-1,0) //izquierda
    };

    public static List<Vector2Int> diagonalDirectionsList = new List<Vector2Int>
    {
        new Vector2Int(1,1), //arriba-derecha
        new Vector2Int(1,-1), //derecha-abajo
        new Vector2Int(-1,-1), //abajo-derecha
        new Vector2Int(-1,1) //izquierda-arriba
    };

    public static List<Vector2Int> eigthDirectionsList = new List<Vector2Int>
    {
        new Vector2Int(0,1), //arriba
        new Vector2Int(1,1), //arriba-derecha
        new Vector2Int(1,0), //derecha
        new Vector2Int(1,-1), //derecha-abajo
        new Vector2Int(0,-1), //abajo
        new Vector2Int(-1,-1), //abajo-derecha
        new Vector2Int(-1,0), //izquierda
        new Vector2Int(-1,1) //izquierda-arriba
    };

    public static Vector2Int GetRandomCardinalDirection()
    {
        return cardinalDirectionsList[Random.Range(0, cardinalDirectionsList.Count)];
    }
}
