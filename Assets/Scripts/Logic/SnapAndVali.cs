using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Reflection;

[RequireComponent(typeof(XRGrabInteractable))]
public class SnapAndVali : MonoBehaviour
{
    private Vector3 originalPosition;
    private XRGrabInteractable grab;
    private ChessboardGenerator board;
    private string startCoord;
    private TacticLogger logger;

    [Header("Smooth Snap Settings")]
    public float snapDuration = 0.25f;
    public AnimationCurve snapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private List<GameObject> highlights = new List<GameObject>();
    private Coroutine currentMove;

    AutoUpright upright;
    public bool isGrabbed;

    void Start()
    {
        grab = GetComponent<XRGrabInteractable>();
        upright = GetComponent<AutoUpright>();
        board = FindAnyObjectByType<ChessboardGenerator>();
        logger = FindAnyObjectByType<TacticLogger>();

        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);

        string start = GetClosestCoord(transform.position);
        PieceRegistry.Instance.RegisterPiece(start, this);
    }

    // --- beim Greifen ---
    void OnGrab(SelectEnterEventArgs args)
    {
        if (upright != null)
            upright.DisableUpriight();

        startCoord = GetClosestCoord(transform.position);
        ShowAllowedHighlights(true);
        originalPosition = transform.position;
    }

    // --- b
    void OnRelease(SelectExitEventArgs args)
    {
        ShowAllowedHighlights(false);

        board = FindAnyObjectByType<ChessboardGenerator>();
        var hintSystem = FindAnyObjectByType<HintSystem>();
        var loader = FindAnyObjectByType<TacticLoader>();

        string targetCoord = GetClosestCoord(transform.position);
        Debug.Log($"MOVE DEBUG : start: {startCoord}, target: {targetCoord}");

        string move = $"{startCoord}-{targetCoord}";

        // W
        if (startCoord == targetCoord)
        {
            SnapBack(startCoord);

            if (upright != null)
            {
                isGrabbed = false;
                upright.EnableUpright();
            }

            return;
        }

        // Zug
        if (loader != null)
        {
            loader.CheckPlayerMove(move);
            StartCoroutine(HandleAfterLoaderResponse(targetCoord));
        }
    }

    // --- 
    IEnumerator SmoothSnap(Vector3 target, bool checkUprightAfter = false)
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

        if (checkUprightAfter)
            yield return new WaitForSeconds(0.5f);

        StartCoroutine(SoftUprightPiece());
    }

    // --- Hil
    string GetClosestCoord(Vector3 pos)
    {
        if (board == null)
        {
            board = FindAnyObjectByType<ChessboardGenerator>();

            if (board == null)
            {
                Debug.LogError("GetClosestCoord: Board == null! Kein Schachbrett verfügbar");
                return "";
            }
        }

        float min = Mathf.Infinity;
        string best = "";

        foreach (Transform tile in board.transform)
        {
            float d = Vector3.Distance(pos, tile.position);

            if (d < min)
            {
                min = d;
                best = tile.name; // a1..h8
            }
        }

        return best;
    }

    // --- Runde
    void ShowAllowedHighlights(bool on)
    {
        foreach (var h in highlights)
            if (h != null) Destroy(h);

        highlights.Clear();

        if (!on) return;

        foreach (var logic in GetComponents<MonoBehaviour>())
        {
            MethodInfo method = logic.GetType().GetMethod("GetLegalMoves");
            if (method == null) continue;

            var list = method.Invoke(logic, new object[] { startCoord }) as List<string>;
            if (list == null) continue;

            foreach (string coord in list)
            {
                Vector3 pos = board.GetTilePosition(coord) + Vector3.up * 0.01f;

                GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                dot.transform.position = pos;
                dot.transform.localScale = Vector3.one * 0.03f;
                dot.GetComponent<Collider>().enabled = false;

                var r = dot.GetComponent<Renderer>();
                r.material = new Material(Shader.Find("Standard"));
                r.material.color = new Color(0f, 1f, 0.3f, 0.35f);
                r.material.EnableKeyword("_EMISSION");
                r.material.SetColor("_EmissionColor", new Color(0f, 0.8f, 0f));

                highlights.Add(dot);
            }
        }
    }

    IEnumerator FlashRed()
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend == null) yield break;

        Color original = rend.material.color;

        for (int i = 0; i < 2; i++)
        {
            rend.material.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            rend.material.color = original;
            yield return new WaitForSeconds(0.15f);
        }
    }

    IEnumerator HandleAfterLoaderResponse(string targetCoord)
    {
        yield return null;

        var loader = FindAnyObjectByType<TacticLoader>();
        var hintSystem = FindAnyObjectByType<HintSystem>();

        // War 
        if (!loader.lastMoveCorrect)
        {
            StartCoroutine(ReturnAfterDelay(startCoord, hintSystem));
            yield break;
        }

        var targetPiece = PieceRegistry.Instance.GetPieceAt(targetCoord);

        if (targetPiece != null)
        {
            SoundManager.Instance.PlayCapturedSound();
            PieceRegistry.Instance.UnregisterPiece(targetCoord);
            targetPiece.gameObject.SetActive(false);
            loader.capturedCoords.Add(targetCoord);
        }

        PieceRegistry.Instance.UnregisterPiece(startCoord);
        PieceRegistry.Instance.RegisterPiece(targetCoord, this);

        Vector3 targetPos = board.GetTilePosition(targetCoord);
        targetPos.y += 0.05f;

        StartCoroutine(SmoothSnap(targetPos, true));

        isGrabbed = false;

        if (upright != null)
            upright.EnableUpright();
    }

    public string GetCurrentCoord(Vector3 pos)
    {
        return GetClosestCoord(transform.position);
    }

    public void MoveToCoord(string coord)
    {
        var board = FindAnyObjectByType<ChessboardGenerator>();
        Vector3 target = board.GetTilePosition(coord);
        target.y += 0.05f;

        StartCoroutine(SmoothSnap(target));
    }

    void SnapBack(string coord)
    {
        Vector3 backPos = board.GetTilePosition(startCoord);
        backPos.y += 0.05f;

        if (currentMove != null)
            StopCoroutine(currentMove);

        currentMove = StartCoroutine(SmoothSnap(backPos, true));
    }

    IEnumerator ReturnAfterDelay(string startCoord, HintSystem hintSystem)
    {
        Debug.Log("Taktikfehler – Figur wird zurückgesetzt");
        yield return new WaitForSeconds(1.0f);

        SnapBack(startCoord);
        StartCoroutine(FlashRed());
        hintSystem?.RegisterMistake();
    }

    IEnumerator SoftUprightPiece(float duration = 0.2f)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) yield break;

        float tilt = Vector3.Angle(transform.up, Vector3.up);

        if (tilt < 60f)
            yield break;

        rb.isKinematic = true;

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(
            transform.position.x,
            Mathf.Max(transform.position.y, 0.05f),
            transform.position.z
        );

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0, 1, t / duration);

            transform.rotation = Quaternion.Lerp(startRot, targetRot, k);
            transform.position = Vector3.Lerp(startPos, targetPos, k);

            yield return null;
        }

        rb.isKinematic = false;
    }
}
