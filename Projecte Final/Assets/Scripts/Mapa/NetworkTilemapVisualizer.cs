using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
using System.Collections.Generic;
using System; // Añade esta línea para acceder a la clase Convert

[RequireComponent(typeof(NetworkIdentity))]
public class NetworkTilemapVisualizer : NetworkBehaviour
{
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTop, wallSideRight, wallSideLeft, wallBottom, wallFull;
    [SerializeField] private TileBase wallInnerCornerDownLeft, wallInnerCornerDownRight;
    [SerializeField] private TileBase wallDiagonalCornerDownRight, wallDiagonalCornerDownLeft;
    [SerializeField] private TileBase wallDiagonalCornerUpRight, wallDiagonalCornerUpLeft;

    [ClientRpc]
    public void RpcClearAllTiles()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }

    [ClientRpc]
    public void RpcPaintFloorTiles(List<Vector2Int> floorPositions)
    {
        foreach (var position in floorPositions)
        {
            var tilePosition = floorTilemap.WorldToCell((Vector3Int)position);
            floorTilemap.SetTile(tilePosition, floorTile);
        }
    }

    [ClientRpc]
    public void RpcPaintBasicWalls(List<WallTileData> basicWalls)
    {
        foreach (var wall in basicWalls)
        {
            PaintSingleBasicWall(wall.position, wall.binaryType);
        }
    }

    [ClientRpc]
    public void RpcPaintCornerWalls(List<WallTileData> cornerWalls)
    {
        foreach (var wall in cornerWalls)
        {
            PaintSingleCornerWall(wall.position, wall.binaryType);
        }
    }

    private void PaintSingleBasicWall(Vector2Int position, string binaryType)
    {
        int typeAsInt = Convert.ToInt32(binaryType, 2); // Ahora Convert será reconocido
        TileBase tile = GetBasicWallTile(typeAsInt);
        if (tile != null)
        {
            var tilePosition = wallTilemap.WorldToCell((Vector3Int)position);
            wallTilemap.SetTile(tilePosition, tile);
        }
    }

    private void PaintSingleCornerWall(Vector2Int position, string binaryType)
    {
        int typeAsInt = Convert.ToInt32(binaryType, 2); // Ahora Convert será reconocido
        TileBase tile = GetCornerWallTile(typeAsInt);
        if (tile != null)
        {
            var tilePosition = wallTilemap.WorldToCell((Vector3Int)position);
            wallTilemap.SetTile(tilePosition, tile);
        }
    }

    private TileBase GetBasicWallTile(int typeAsInt)
    {
        if (WallTypesHelper.wallTop.Contains(typeAsInt)) return wallTop;
        if (WallTypesHelper.wallSideRight.Contains(typeAsInt)) return wallSideRight;
        if (WallTypesHelper.wallSideLeft.Contains(typeAsInt)) return wallSideLeft;
        if (WallTypesHelper.wallBottom.Contains(typeAsInt)) return wallBottom;
        if (WallTypesHelper.wallFull.Contains(typeAsInt)) return wallFull;
        return null;
    }

    private TileBase GetCornerWallTile(int typeAsInt)
    {
        if (WallTypesHelper.wallInnerCornerDownLeft.Contains(typeAsInt)) return wallInnerCornerDownLeft;
        if (WallTypesHelper.wallInnerCornerDownRight.Contains(typeAsInt)) return wallInnerCornerDownRight;
        if (WallTypesHelper.wallDiagonalCornerDownLeft.Contains(typeAsInt)) return wallDiagonalCornerDownLeft;
        if (WallTypesHelper.wallDiagonalCornerDownRight.Contains(typeAsInt)) return wallDiagonalCornerDownRight;
        if (WallTypesHelper.wallDiagonalCornerUpLeft.Contains(typeAsInt)) return wallDiagonalCornerUpLeft;
        if (WallTypesHelper.wallDiagonalCornerUpRight.Contains(typeAsInt)) return wallDiagonalCornerUpRight;
        if (WallTypesHelper.wallFullEightDirections.Contains(typeAsInt)) return wallFull;
        if (WallTypesHelper.wallBottomEightDirections.Contains(typeAsInt)) return wallBottom;
        return null;
    }
}