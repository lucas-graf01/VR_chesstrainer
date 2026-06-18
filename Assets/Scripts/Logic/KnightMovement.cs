using System.Collections.Generic;
using UnityEngine;

public class KnightMovement : MonoBehaviour
{
    public List<string> GetLegalMoves(string from)
    {
        List<string> legal = new List<string>();

        int x = from[0] - 'a';
        int y = int.Parse(from[1].ToString()) - 1;

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(2, 1),
            new Vector2Int(1, 2),
            new Vector2Int(-1, 2),
            new Vector2Int(-2, 1),
            new Vector2Int(-2, -1),
            new Vector2Int(-1, -2),
            new Vector2Int(1, -2),
            new Vector2Int(2, -1)
        };

        foreach (var dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;
            if (nx >= 0 && nx < 8 && ny >= 0 && ny < 8)
            {
                char file = (char)('a' + nx);
                string rank = (ny + 1).ToString();
                legal.Add(file + rank);
            }
        }

        return legal;
    }
}
