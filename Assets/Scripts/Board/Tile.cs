using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tile : MonoBehaviour
{

    public Renderer baserenderer;
    public Renderer overlayRenderer; 
    public string coord;
    
    Color baseColor;

    


    private void Awake()
    {
        Debug.Log("Tile erstellt: " + coord + " auf position " + transform.position);
        
    }
    public void Highlight(Color color)
    {   
      
    }
    public void ResetHighlight()
    {
        

    }
    public void ResetColor(){
     
    }



}
