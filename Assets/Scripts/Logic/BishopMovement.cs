using System.Collections.Generic;
using UnityEngine;

public class BishopMovement : MonoBehaviour
{
    private ChessboardGenerator board;
    void Start() => board = FindAnyObjectByType<ChessboardGenerator>();

    public List<string> GetLegalMoves(string currentCoord)
    {
        List<string> moves = new();
        if (string.IsNullOrEmpty(currentCoord)) return moves;

        int x = currentCoord[0] - 'a';
        int z = int.Parse(currentCoord[1].ToString()) - 1;

        int[][] dirs = {
            new int[]{1, 1},   // nordost
            new int[]{-1, 1},  // nordwest
            new int[]{1, -1},  // südost
            new int[]{-1, -1}  // südwest
        };

        foreach (var d in dirs)
        {
            for (int step = 1; step < 8; step++)
            {
                int nx = x + d[0] * step;
                int nz = z + d[1] * step;
                if (!IsOnBoard(nx, nz)) break;
                moves.Add(ToCoord(nx, nz));
            }
        }

        return moves;
    }

    private bool IsOnBoard(int x, int z) => x >= 0 && x < 8 && z >= 0 && z < 8;
    private string ToCoord(int x, int z) => $"{(char)('a' + x)}{z + 1}";
}
