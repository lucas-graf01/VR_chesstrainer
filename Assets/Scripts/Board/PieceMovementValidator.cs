using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class SnapAndValidate : MonoBehaviour
{
    private XRGrabInteractable grab;
    private ChessboardGenerator board;
    private string startCoord;

    [Header("Smooth Snap")]
    public float snapDuration = 0.25f;
    public AnimationCurve snapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    void Start()
    {
        grab = GetComponent<XRGrabInteractable>();
        board = FindAnyObjectByType<ChessboardGenerator>();

        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        startCoord = GetClosestCoord(transform.position);
        ShowAllowedHighlights(true);
    }

    void OnRelease(SelectExitEventArgs args)
    {
        ShowAllowedHighlights(false);

        string targetCoord = GetClosestCoord(transform.position);

        if (IsMoveAllowed(startCoord, targetCoord))
        {
            Vector3 targetPos = board.GetTilePosition(targetCoord);
            targetPos.y += 0.05f;
            StopAllCoroutines();
            StartCoroutine(SmoothSnap(targetPos));
        }
        else
        {
            Vector3 backPos = board.GetTilePosition(startCoord);
            backPos.y += 0.05f;
            StopAllCoroutines();
            StartCoroutine(SmoothSnap(backPos));
        }
    }

    IEnumerator SmoothSnap(Vector3 target)
    {
        Vector3 start = transform.position;
        float t = 0f;
        while (t < snapDuration)
        {
            t += Time.deltaTime;
            float k = snapCurve.Evaluate(t / snapDuration);
            transform.position = Vector3.Lerp(start, target, k);
            yield return null;
        }
        transform.position = target;
    }

    string GetClosestCoord(Vector3 pos)
    {
        float min = Mathf.Infinity;
        string best = "";
        foreach (Transform tile in board.transform)
        {
            float d = Vector3.Distance(pos, tile.position);
            if (d < min)
            {
                min = d;
                best = tile.name; 
            }
        }
        return best;
    }

    bool IsMoveAllowed(string from, string to)
    {
        if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) return false;

        var pawn = GetComponent<PawnMovement>();
        if (pawn != null)
        {
            List<string> legal = pawn.GetLegalMoves(from);
            return legal.Contains(to);
        }

        return false;
    }

    List<GameObject> highlights = new List<GameObject>();
    void ShowAllowedHighlights(bool on)
    {     
        foreach (var g in highlights) if (g) Destroy(g);
        highlights.Clear();
        if (!on) return;

        var pawn = GetComponent<PawnMovement>();
        if (pawn == null) return;

        foreach (var coord in pawn.GetLegalMoves(startCoord))
        {
            var pos = board.GetTilePosition(coord) + Vector3.up * 0.001f;
            var p = GameObject.CreatePrimitive(PrimitiveType.Plane);
            p.transform.position = pos;
            p.transform.localScale = Vector3.one * 0.1f;
            var r = p.GetComponent<Renderer>();
            r.material.color = new Color(0f, 1f, 0f, 0.35f);
            highlights.Add(p);
        }
    }
}
