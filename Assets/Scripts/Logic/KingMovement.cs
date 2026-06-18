using System.Collections.Generic;
using UnityEngine;

public class KingMovement : MonoBehaviour
{
    private ChessboardGenerator board;
    void Start() => board = FindAnyObjectByType<ChessboardGenerator>();

    public List<string> GetLegalMoves(string currentCoord)
    {
        List<string> moves = new();
        if (string.IsNullOrEmpty(currentCoord)) return moves;

        int x = currentCoord[0] - 'a';
        int z = int.Parse(currentCoord[1].ToString()) - 1;

        for (int dx = -1; dx <= 1; dx++)
        for (int dz = -1; dz <= 1; dz++)
        {
            if (dx == 0 && dz == 0) continue;
            int nx = x + dx;
            int nz = z + dz;
            if (IsOnBoard(nx, nz))
                moves.Add(ToCoord(nx, nz));
        }

        return moves;
    }

    private bool IsOnBoard(int x, int z) => x >= 0 && x < 8 && z >= 0 && z < 8;
    private string ToCoord(int x, int z) => $"{(char)('a' + x)}{z + 1}";
}
