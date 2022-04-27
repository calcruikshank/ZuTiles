using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    [SerializeField] GameObject whiteSquare;
    [SerializeField] GameObject boostTilePrefab;
    [SerializeField] GameObject doubleCashTilePrefab;
    [SerializeField] GameObject autoHarvestTilePrefab;

    [SerializeField] GameObject objectToMakeTileSize;

    float cellSizeX = 1f;
    float cellSizeY = 1f;

    public static Testing singleton;
    int x, y;

    [SerializeField] bool gridEnabled = false;

    private void Awake()
    {
        if (!gridEnabled ) return;
        if (singleton != null)
        {
            Destroy(this);
        }
        singleton = this;

        if (objectToMakeTileSize != null)
        {
            SetTileSizeToObject();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (!gridEnabled) return;
        x = 18;
        y = 12;
        GridClass grid = new GridClass(x, y, cellSizeX, cellSizeY, new Vector3(x / -2 * cellSizeX, y / -2 * cellSizeY, 0));

        //GridClass newGrid = new GridClass(12, 5, 1, new Vector3(-6, 1, 0));
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SpawnTile(Vector3 spawnPosition)
    {
        GameObject newTile = Instantiate(whiteSquare, spawnPosition, Quaternion.identity, this.transform);
        newTile.transform.localScale = new Vector3(cellSizeX, newTile.transform.localScale.y, cellSizeY);
    }
    public GameObject SpawnBoostTile(Vector3 spawnPosition)
    {
        GameObject boostTile = Instantiate(boostTilePrefab, spawnPosition, Quaternion.identity, this.transform);
        return boostTile;
    }
    public GameObject SpawnDoubleCashTile(Vector3 spawnPosition)
    {
        GameObject doubleCashTile = Instantiate(doubleCashTilePrefab, spawnPosition, Quaternion.identity, this.transform);
        return doubleCashTile;
    }
    public GameObject SpawnAutoHarvestTile(Vector3 spawnPosition)
    {
        GameObject doubleCashTile = Instantiate(autoHarvestTilePrefab, spawnPosition, Quaternion.identity, this.transform);
        return doubleCashTile;
    }

    public void SetTileSizeToObject()
    {
        cellSizeX = objectToMakeTileSize.GetComponentInChildren<MeshRenderer>().bounds.size.x;
        cellSizeY = objectToMakeTileSize.GetComponentInChildren<MeshRenderer>().bounds.size.z;
    }
}
