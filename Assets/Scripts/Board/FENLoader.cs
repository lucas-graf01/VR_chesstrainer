using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FENLoader : MonoBehaviour
{
    private string lastLoadedFEN;

    [Header("References")]
    public ChessboardGenerator board;
    public PieceSpawner spawner;
    




    private readonly Dictionary<char, string> pieceMap = new Dictionary<char, string>
    {
        {'r', "BlackRook"},
        {'n', "BlackKnight"},
        {'b', "BlackBishop"},
        {'q', "BlackQueen"},
        {'k', "BlackKing"},
        {'p', "BlackPawn"},
        {'R', "WhiteRook"},
        {'N', "WhiteKnight"},
        {'B', "WhiteBishop"},
        {'Q', "WhiteQueen"},
        {'K', "WhiteKing"},
        {'P', "WhitePawn"}
    };

    void Start()
    {
        if (board == null) board = FindAnyObjectByType<ChessboardGenerator>();
        if (spawner == null) spawner = FindAnyObjectByType<PieceSpawner>();
    }

    public void LoadFEN(string fen)
    {
        lastLoadedFEN = fen;

        if (board == null)
        {
            board = FindAnyObjectByType<ChessboardGenerator>();      
        }

        if (board == null)
        {
            Debug.LogError(" Kein ChessboardGenerator gefunden!");
            return;
        }
        if (board.transform.childCount == 0)
        {
            Debug.LogWarning(" Brett noch nicht generiert, warte 0.2 Sekunden...");
            StartCoroutine(WaitAndLoadFEN(fen));
            return;
        }

        ClearBoard();

        string[] parts = fen.Split(' ');
        string layout = parts[0];
        string[] ranks = layout.Split('/');

        for (int rank = 0; rank < 8; rank++)
        {
            string row = ranks[rank];
            int file = 0;

            foreach (char c in row)
            {
                if (char.IsDigit(c))
                {
                    file += c - '0'; // lee
                }
                else if (pieceMap.ContainsKey(c))
                {
                    string coord = $"{(char)('a' + file)}{8 - rank}";
                    SpawnPieceFromSymbol(c, coord);
                    file++;
                }
                else
                {
                    Debug.LogWarning($" Unbekanntes Symbol in FEN: {c}");
                }
            }
        }

        Debug.Log(" FEN geladen: " + fen);
    }

    private IEnumerator WaitAndLoadFEN(string fen)
    {
       
        yield return new WaitForSeconds(0.2f);
        LoadFEN(fen);
    }

    private void ClearBoard()
    {
        foreach (Transform child in spawner.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void SpawnPieceFromSymbol(char symbol, string coord)
    {
        string prefabName = pieceMap[symbol];
        GameObject prefab = spawner.GetPrefabByName(prefabName);

        if (prefab == null)
        {
            Debug.LogError($" Prefab '{prefabName}' nicht gefunden!");
            return;
        }

        Vector3 spawnPos = board.GetTilePosition(coord);
        spawnPos.y += 0.05f;

        GameObject piece = Instantiate(prefab, spawnPos, Quaternion.identity, spawner.transform);
        piece.name = $"{prefabName}_{coord}";
    }
public void ReloadCurrentFEN()
{
    if (!string.IsNullOrEmpty(lastLoadedFEN))
        LoadFEN(lastLoadedFEN);
}

}
