using System.Collections.Generic;
using UnityEngine;

public static class WallGenerator
{
    public static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionList)
    {
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
        foreach (var position in floorPositions)
        {
            foreach (var direction in directionList)
            {
                var neighbourPosition = position + direction;
                if (!floorPositions.Contains(neighbourPosition))
                {
                    wallPositions.Add(neighbourPosition);
                }
            }
        }
        return wallPositions;
    }
}