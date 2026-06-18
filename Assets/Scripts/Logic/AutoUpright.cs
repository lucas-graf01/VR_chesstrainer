using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoUpright : MonoBehaviour
{
    
    public float uprightSpeed = 5f;
    public float maxTiltAngle = 25f;
    public float delayAfterMove = 1f;

    public Rigidbody rb;
    public bool isUprighting = false;

    public Quaternion uprightRotation;
    public SnapAndVali snap;

    bool isRunning;
    bool isEnabled = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        snap = GetComponent<SnapAndVali>();
        uprightRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
    }

    void Update()
    {
        if (snap != null && snap.isGrabbed)
        return;
        if (isUprighting)
        return;

        float tilt = Vector3.Angle(transform.up, Vector3.up);
        if (tilt > maxTiltAngle)
        StartCoroutine(RestoreUpright());
    }

    public IEnumerator RestoreUpright()
    {
        isRunning = true;
        yield return new WaitForSeconds(delayAfterMove);
        bool originalKinematic = rb.isKinematic;
        rb.isKinematic = true;
        float t = 0f;
        Quaternion startRot = transform.rotation;

        while ( t < 1f)
        {  
            t += Time.deltaTime * uprightSpeed;
            transform.rotation = Quaternion.Slerp(startRot, uprightRotation, t);
            yield return null;
        }

        rb.isKinematic = false;
        isUprighting = false;
    }

    public void OnEnable()
    {
        isUprighting = false;
    }

    public void DisableUpriight()
    {
        isUprighting = false;
        enabled = false;
    }
        
        public void EnableUpright()
    {
        enabled = true;
    }

} 
