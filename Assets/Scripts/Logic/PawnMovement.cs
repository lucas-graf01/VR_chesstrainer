using System.Collections.Generic;
using UnityEngine;

public class PawnMovement : MonoBehaviour
{
    private ChessboardGenerator board;
    private bool isWhite;  // au

    void Start()
    {
        board = FindAnyObjectByType<ChessboardGenerator>();

        // Farbe anhand
        string lower = gameObject.name.ToLower();

        if (lower.Contains("white"))
            isWhite = true;
        else if (lower.Contains("black"))
            isWhite = false;
        else
        {
            Debug.LogWarning($" Konnte Farbe für {gameObject.name} nicht erkennen! Default=White");
            isWhite = true;
        }
    }

    // -------------------------------------------------------------
    //  L
    // -------------------------------------------------------------
    public List<string> GetLegalMoves(string from)
    {
        List<string> legal = new List<string>();

        int x = from[0] - 'a';
        int z = int.Parse(from[1].ToString()) - 1;

        int dir = isWhite ? 1 : -1;

        // 
        bool atStartRow = (isWhite && z == 1) || (!isWhite && z == 6);

        // ---------------------------------
        // 
        // ---------------------------------
        if (IsEmpty(x, z + dir))
        {
            legal.Add(ToCoord(x, z + dir));

            // ---------------------------------
            // 
            // ---------------------------------
            if (atStartRow && IsEmpty(x, z + dir * 2))
                legal.Add(ToCoord(x, z + dir * 2));
        }

        // ---------------------------------
        //
        // ---------------------------------
        foreach (int dx in new int[] { -1, 1 })
        {
            int nx = x + dx;
            int nz = z + dir;

            if (nx < 0 || nx > 7 || nz < 0 || nz > 7)
                continue;

            if (HasOpposingPiece(nx, nz))
                legal.Add(ToCoord(nx, nz));
        }

        return legal;
    }

    // -------------------------------------------------------------
    // FE
    // -------------------------------------------------------------

    private bool IsEmpty(int x, int z)
    {
        if (!IsOnBoard(x, z)) return false;

        Vector3 pos = board.GetTilePosition(ToCoord(x, z));
        Collider[] hits = Physics.OverlapSphere(pos, 0.02f);

        foreach (var h in hits)
            if (h.CompareTag("Piece"))
                return false;

        return true;
    }

    private bool HasOpposingPiece(int x, int z)
    {
        if (!IsOnBoard(x, z)) return false;

        Vector3 pos = board.GetTilePosition(ToCoord(x, z));
        Collider[] hits = Physics.OverlapSphere(pos, 0.02f);

        foreach (var h in hits)
        {
            if (!h.CompareTag("Piece")) continue;

            string lower = h.name.ToLower();

            // Gegner erkennen
            if (isWhite && lower.Contains("black")) return true;
            if (!isWhite && lower.Contains("white")) return true;
        }

        return false;
    }

    private bool IsOnBoard(int x, int z)
    {
        return x >= 0 && x < 8 && z >= 0 && z < 8;
    }

    private string ToCoord(int x, int z)
    {
        return $"{(char)('a' + x)}{z + 1}";
    }
}
