using System.Collections.Generic;
using UnityEngine;

public class QueenMovement : MonoBehaviour
{
    private RookMovement rook;
    private BishopMovement bishop;

    void Start()
    {
        rook = gameObject.AddComponent<RookMovement>();
        bishop = gameObject.AddComponent<BishopMovement>();
    }

    public List<string> GetLegalMoves(string currentCoord)
    {
        List<string> moves = new();
        moves.AddRange(rook.GetLegalMoves(currentCoord));
        moves.AddRange(bishop.GetLegalMoves(currentCoord));
        return moves;
    }
}
