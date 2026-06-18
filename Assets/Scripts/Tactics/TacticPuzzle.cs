using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TacticPuzzle
{
    public int id;
    public string fen;
    public List<string> solution;
    public string difficulty;
    public string mode; 
    public List<string> hints;

}
