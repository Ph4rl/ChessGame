using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Chessboard : MonoBehaviour
{
    /// Art and Models
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;


    /// Prefab & Material
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    // Logic and values
    private ChessPiece[,] chessPieces;
    private ChessPiece currentlyDragging;
    private const int TileCountX = 8;
    private const int TileCountY = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;

    private void Awake()
    {
        GenerateAllTiles(tileSize, TileCountX, TileCountY);
        SpawnAllPieces();
        PositionAllPieces();

    }
    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        // read mouse position
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = currentCamera.ScreenPointToRay(mousePosition);

        RaycastHit info;
        
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile")))
        {
            // get indexes of the tile hit
            Vector2Int hitPosition = LookUpTileIndex(info.transform.gameObject);

            // if hovering tile after not hovering tile
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // if already hovering tile, change previous
            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }    

            // mouse click
            if (Input.GetMouseButtonDown(0))
            {
                if (chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    // our turn?
                    if (true)
                    {
                        currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];

                    }
                }
            }

            // mouse click release
            if(currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);

                bool validMove = MoveTo (currentlyDragging, hitPosition.x, hitPosition.y);
                if (!validMove)
                {
                    currentlyDragging.transform.position = GetTileCenter (previousPosition.x, previousPosition.y);
                    currentlyDragging = null;
                }
            }
        }
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }
        }
    }



    /// Board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;

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
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y+1) * tileSize) - bounds;
        vertices[2] = new Vector3((x+1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x+1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();


        return tileObject;
    }

    /// Spawn
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TileCountX, TileCountY];

        int whiteTeam = 0;
        int blackTeam = 1;

        // White spawn
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);

        for (int i = 0; i < TileCountX; i++)
            chessPieces[i,1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);

        // Black spawn
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);

        for (int i = 0; i < TileCountX; i++)
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
    }
    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();

        cp.type = type;
        cp.team = team;
        cp.GetComponent<MeshRenderer>().material = teamMaterials[team];

        return cp;
    }   

    // Positioning
    private void PositionAllPieces()
    {
        for (int x = 0; x < TileCountX; x++)
            for (int y = 0; y < TileCountY; y++)
                if (chessPieces[x, y] != null)
                    PositionSinglePiece(x, y, true);
    }
    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].transform.position = GetTileCenter (x, y);

    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3 (tileSize / 2, 0, tileSize / 2);
    }
    // Operations
    private Vector2Int LookUpTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TileCountX; x++)
            for (int y = 0; y < TileCountY; y++)
                if (tiles[ x, y]  == hitInfo)
                        return (new Vector2Int(x, y));

        return -Vector2Int.one; //invalid
    }   

    private bool MoveTo(ChessPiece cp, int x, int y)
    {
        Vector2Int previousPosition = new Vector2Int(cp.currentX, cp.currentY);

        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        return true;
    }

    
}
