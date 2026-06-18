using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PieceRegistry : MonoBehaviour
{
    public static PieceRegistry Instance;

    public Dictionary<string, SnapAndVali> pieces = new Dictionary<string, SnapAndVali>();

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RegisterPiece( string coord, SnapAndVali piece)
    {
        pieces[coord] = piece;
    }
    public void UnregisterPiece(string coord)
    {
        if (pieces.ContainsKey(coord))
        pieces.Remove(coord);
    }
    public SnapAndVali GetPieceAt(string coord)
    {
        if (pieces.ContainsKey(coord))
        return pieces[coord];
        return null;
    }
    public void Cleanup()
    {
        List<string> dead = new List<string>();

        foreach(var kvp in pieces)
        {
            
            if (kvp.Value == null)
            dead.Add(kvp.Key);
        }
        foreach(string key in dead)
        pieces.Remove(key);

    }

    public void EnableAllPiece()
    {
        foreach (var piece in pieces.Values)
        {
            if(piece != null)
            {
                
                piece.gameObject.SetActive(true);

                var rb = piece.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }


        }



    }




}
