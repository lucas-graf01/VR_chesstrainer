using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    [Header("References")]
    public ChessboardGenerator board;

    [Header("Prefabs")]
    public GameObject WhitePawn;
    public GameObject WhiteRook;
    public GameObject WhiteKnight;
    public GameObject WhiteBishop;
    public GameObject WhiteQueen;
    public GameObject WhiteKing;

    public GameObject BlackPawn;
    public GameObject BlackRook;
    public GameObject BlackKnight;
    public GameObject BlackBishop;
    public GameObject BlackQueen;
    public GameObject BlackKing;

    // inter
    private Dictionary<string, GameObject> prefabMap;

    void Awake()
    {

        Debug.Log(" Awake " + gameObject.name);

        prefabMap = new Dictionary<string, GameObject>
        {
            { "WhitePawn", WhitePawn },
            { "WhiteRook", WhiteRook },
            { "WhiteKnight", WhiteKnight },
            { "WhiteBishop", WhiteBishop },
            { "WhiteQueen", WhiteQueen },
            { "WhiteKing", WhiteKing },
            { "BlackPawn", BlackPawn },
            { "BlackRook", BlackRook },
            { "BlackKnight", BlackKnight },
            { "BlackBishop", BlackBishop },
            { "BlackQueen", BlackQueen },
            { "BlackKing", BlackKing }
        };
    }

    public GameObject GetPrefabByName(string name)
    {
        if (prefabMap.TryGetValue(name, out GameObject prefab))
        {
            return prefab;
        }
        else
        {
            Debug.LogError($"Kein Prefab gefunden für Name: {name}");
            return null;
        }
    }

    public void SpawnPieceAt(string coord, string prefabName)
    {
        if (board == null)
            board = FindAnyObjectByType<ChessboardGenerator>();

        GameObject prefab = GetPrefabByName(prefabName);
        if (prefab == null) return;

        Vector3 pos = board.GetTilePosition(coord);
        pos.y += 0.05f;

        Instantiate(prefab, pos, Quaternion.identity, transform);
        Debug.Log("SpawnPieces aufgerufen");
    }
}
