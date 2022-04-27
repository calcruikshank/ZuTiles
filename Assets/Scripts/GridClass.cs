using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridClass
{
    private int width;
    private int height;
    private float cellSizeX, cellSizeY;
    private Vector3 originPosition;
    private int[,] gridArray;

    public GridClass(int width, int height, float cellSizeX, float cellSizeY, Vector3 originPosition)
    {
        this.width = width;
        this.height = height;
        this.cellSizeX = cellSizeX;
        this.cellSizeY = cellSizeY;
        this.originPosition = originPosition;

        gridArray = new int[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                Testing.singleton.SpawnTile(GetWorldPosition(x, y) + new Vector3(cellSizeX, 0, cellSizeY) * .5f);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
            }
        }

        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3((x * cellSizeX + originPosition.x), -6, (y * cellSizeY + originPosition.y));
    }
    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSizeX);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSizeY);
    }

    public void SetValue(int x, int y, int value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
        }
    }
}
