using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PieceGrabDebug : MonoBehaviour
{
    void OnEnable()
    {
        var grab = GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.selectEntered.AddListener(OnGrabEnter);
            grab.selectExited.AddListener(OnGrabExit);
        }
    }

    void OnDisable()
    {
        var grab = GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(OnGrabEnter);
            grab.selectExited.RemoveListener(OnGrabExit);
        }
    }

    void OnGrabEnter(SelectEnterEventArgs args)
    {
        Debug.Log(" GRAB ENTER: " + gameObject.name);
    }

    void OnGrabExit(SelectExitEventArgs args)
    {
        Debug.Log(" GRAB EXIT: " + gameObject.name);
    }
}
