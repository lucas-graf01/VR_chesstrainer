using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceAutoReset : MonoBehaviour
{
    private string correctCoord;
    private Vector3 correctWorldPosition;

    public float CheckTimer =0f;
    public float CheckInterval = 2f;

    
    
    void start(){
            correctWorldPosition = transform.position;        
            }
     void Update(){
           
           CheckTimer += Time.deltaTime;
           if(CheckTimer >= CheckInterval)
        {
            CheckTimer = 0f;

            CheckIfFallen();
            CheckIfMoved();

         
        }
           
    }
    void CheckIfFallen()
    {
        if (Vector3.Dot(transform.up, Vector3.up) < 0.8f)
        {
            Reset();
        }
    }

        void CheckIfMoved(){
            if (Vector3.Distance(transform.position, correctWorldPosition) < 0.25f){ 
            Reset();
 }

        }

    public void UpdateCorrectPosition(Vector3 newPos, string newCoord){
            
        correctWorldPosition = newPos;
        correctCoord = newCoord;
    }
    public void Reset()
    {
        if (this == null || gameObject == null) return;
        if (!gameObject.activeInHierarchy) return;
        transform.position = correctWorldPosition;
        transform.rotation = Quaternion.identity;

    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.collider.CompareTag("floor"))
        StartCoroutine(FixThroughFloor());
    }
    IEnumerator FixThroughFloor()
    {
        yield return new WaitForSeconds(0.1f);
        transform.position= correctWorldPosition;
        transform.rotation = Quaternion.identity;


    }

}
