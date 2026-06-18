using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using System.Collections.Generic;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChessboardGenerator : MonoBehaviour
{
    [Header("Board Reference")]
    public Transform chessboard;          

    [Header("Board Settings")]
    public float boardSize = 0.8f;        
    public float tileHeight = 0.11f;      
    public float tileThickness = 0.001f;  

    public Material whiteMaterial;
    public Material blackMaterial;

    private GameObject[,] tiles = new GameObject[8, 8];

    public static event Action OnBoardRebuilt;

   
    



    void Start()
    {
        GenerateBoard();
    }

    public void GenerateBoard()
    {
        
        if (tiles != null)
        {
            foreach (var t in tiles)
            {
                if (t != null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        DestroyImmediate(t);
                    else
                        Destroy(t);
#else
                    Destroy(t);
#endif
                }
            }
        }

        tiles = new GameObject[8, 8];

        
        if (chessboard != null)
        {
            transform.position = chessboard.position + Vector3.up * tileHeight;
            boardSize = chessboard.localScale.x;
        }

        float tileSize = boardSize / 8f;
        char[] columns = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };

        for (int x = 0; x < 8; x++)
        {
            for (int z = 0; z < 8; z++)
            {
                Vector3 localPos = new Vector3(
                    -boardSize / 2 + tileSize / 2 + x * tileSize,
                    0f, 
                    -boardSize / 2 + tileSize / 2 + z * tileSize
                );

                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = localPos;
                tile.transform.localScale = new Vector3(tileSize, tileThickness, tileSize);
                tile.name = $"{columns[x]}{z + 1}";

                tile.tag = "Tile";
                tile.AddComponent<Tile>();
                Tile tileComponent = tile.GetComponent<Tile>();
                tileComponent.coord = $"{columns[x]}{z + 1}";

                bool isWhite = (x + z) % 2 == 1;
               
               Renderer r = tile.GetComponent<Renderer>();
               r.sharedMaterial = isWhite ? whiteMaterial : blackMaterial;
                       
                     tiles[x, z] = tile;
            }
        }

        Debug.Log("Brett erfolgreich wiederhergestellt.");
        OnBoardRebuilt?.Invoke();

    }
 public GameObject GetTileObject (string coord){
        var board = FindAnyObjectByType<ChessboardGenerator>();
        if (board == null) return null;

        Vector3 pos = board.GetTilePosition(coord);
        Collider[] hits = Physics.OverlapSphere(pos, 0.05f);

        foreach (var h in hits){

            if(h.CompareTag("Tile"))
            return h.gameObject;
        }
        return null;
    }

    public void ResetAllHighlight(){

foreach (var tile in tiles){

    for (int x = 0; x < 8; x++){
        for (int y = 0; y < 8 ; y++)
        {
            if(tiles[x,y] != null)
                tiles[x,y].GetComponent<Tile>().ResetColor();
        }
    }
}
    }
    public Vector3 GetTilePosition(string coord)
    {
        int x = coord[0] - 'a';
        int z = int.Parse(coord[1].ToString()) - 1;

        return tiles[x, z].transform.position;
    }

    public Vector3 SnapToClosestTile(Vector3 worldPosition)
    {
        Vector3 closestPos = Vector3.zero;
        float closestDist = Mathf.Infinity;

        foreach (var tile in tiles)
        {
            if (tile == null) continue;

            float dist = Vector3.Distance(worldPosition, tile.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestPos = tile.transform.position;
            }
        }
        closestPos.y += 0.05f;
        return closestPos;
    } 
}

#if UNITY_EDITOR
[CustomEditor(typeof(ChessboardGenerator))]
public class ChessboardGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ChessboardGenerator gen = (ChessboardGenerator)target;

        if (GUILayout.Button(" Brett wiederhergestellt"))
        {
            gen.GenerateBoard();
        }
    }
}
#endif

