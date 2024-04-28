using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chessboard : MonoBehaviour
{
    ///
    [SerializeField] private Material tileMaterial;

    private const int TileCountX = 8;
    private const int TileCountY = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private void Awake()
    {
        GenerateAllTiles(1, TileCountX, TileCountY);
    }
    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.current;
            return;
        }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile")))
        {
            //Vector2Int hitPosition = LookUpTileIndex(info.transform.gameObject);
        }
    }


    /// Board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                tiles[x,y] = GenerateSingleTile(tileSize, x, y);
    }
        
    private GameObject GenerateSingleTile (float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, 0, y * tileSize);
        vertices[1] = new Vector3(x * tileSize, 0, (y+1) * tileSize);
        vertices[2] = new Vector3((x+1) * tileSize, 0, y * tileSize);
        vertices[3] = new Vector3((x+1) * tileSize, 0, (y + 1) * tileSize);

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();


        return tileObject;
    }

    //Operations
    //private Vector2Int LookUpTileIndex(GameObject hitInfo)
    //{
    //    for (int x = 0; x < Tile_Count_X; x++)
    //        for (int y = 0; y = < Tile_Count_Y; y++)
    //            if (tiles[ x, y]  ) == hitInfo)
    //                    return (new Vector2Int(x, y));
    //}   

    //
}
