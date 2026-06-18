using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

public class HintSystem : MonoBehaviour
{
    [Header("Hint Configuration (seconds)")]
    [Header("Default Hint Times (used if no difficulty assigned)")]
    public float defaultTimeToHint1 = 30f;
    public float defaultTimeToHint2 = 45f;
    public float defaultTimeToHint3 = 60f;

    [Header("Active Hint Timers (runtime)")]
    public float timeToHint1 = 30f;
    public float timeToHint2 = 45f;
    public float timeToHint3 = 60f;

    [Header("Hint Tracking")]
    public int totalHintsUsed = 0;
    public bool hintJustUsed = false;

    [Header("Visual Settings")]
    public float flashDuration = 10f; //10
    public float arrowDuration = 10f; //10
    public float tileHighlightDuration = 10f;
    public Color tileHighlightColor = new Color(0.2f, 0.4f, 1f, 0.5f);

    [Header("Runtime State")]
    public int currentHintLevel = 0;
    public int baseHintLevel = 0;
    public int errorCount = 0;

    [Header("Highlight Overlay")]
    public GameObject tileHighlightPrefab;

    private List<GameObject> activeHighlights = new List<GameObject>();

    private float timer = 0f;
    private bool puzzleActive = false;
    private TacticLogger logger;
    private bool keepFlashing = false;

    private Color baseColor = Color.white;

    public List<ManualHintInfo> manualHintsSinceLastMove = new List<ManualHintInfo>();
    public List<HintEvent> hintEventsThisMove = new List<HintEvent>();

    public bool manualHintTriggered = false;
    public bool automaticHintTriggered = false;

    private Tile startTile;
    private Tile endTile;

    public enum HintMode
    {
        Adaptive,
        Static
    }

    public HintMode CurrentMode { get; private set; }

    [Serializable]
    public class ManualHintInfo
    {
        public int level;
        public float time;
        public bool isAutomatic;
    }

    [Serializable]
    public class HintEvent
    {
        public bool isManual;
        public bool isAutomatic;
        public int HintLevel;
    }

    [SerializeField] private FloatingHint floatingHint;
    [SerializeField] private Transform hintAnchor;
    [SerializeField] private int maxHintLevel = 3;

 


    void Awake()
    {
        Debug.Log("HintSystem instanz: " + gameObject.name);
        Debug.Log("Logger sieht Hintsystem: " + GetInstanceID());
    }

    void Start()
    {
        logger = FindAnyObjectByType<TacticLogger>();
    }

    void Update()
    {
        if (!puzzleActive) return;

        if (CurrentMode == HintMode.Static)
        return;

        timer += Time.deltaTime;

        if (currentHintLevel < 1 && timer >= timeToHint1)
            TriggerHint(1, false);
        else if (currentHintLevel < 2 && timer >= timeToHint2)
            TriggerHint(2, false);
        else if (currentHintLevel < 3 && timer >= timeToHint3)
            TriggerHint(3, false);
    }

    public void StartPuzzle()
    {
        puzzleActive = true;
        hintJustUsed = false;
        timer = 0f;
        errorCount = 0;
        currentHintLevel = baseHintLevel;

        Debug.Log($" Hint-System aktiviert (Startlevel {currentHintLevel})");
    }

    public void EndPuzzle()
    {
        puzzleActive = false;
        AdjustHintLevelAfterPuzzle();
    }

    public void RegisterMistake()
    {
        if (CurrentMode == HintMode.Static)
        return;

        errorCount++;

        if (currentHintLevel < 3)
        {
            currentHintLevel++;
            TriggerHint(currentHintLevel, false);
        }

        timer = 0f;
    }


    private void TriggerHint(int level, bool isManual)
    {


        if (isManual)
            RegisterHintEvent(true);
        else
            RegisterHintEvent(false);

        timer = 0f;
        totalHintsUsed++;
        hintJustUsed = true;

        Debug.Log($" Hinweis ausgelöst – Stufe {level}");
        ShowHint(level);
    }

    public void ShowHint(int level)
    {
        var loader = FindAnyObjectByType<TacticLoader>();
        if (loader == null) return;

        var puzzle = loader.puzzles[loader.GetCurrentPuzzleIndex()];
        if (puzzle == null || puzzle.solution.Count == 0) return;

        int step = loader.GetCurrentStep();
        if (step >= puzzle.solution.Count) return;

        string move = puzzle.solution[step];
        string cleanedMove = move.Replace("K", "")
                                 .Replace("Q", "")
                                 .Replace("B", "")
                                 .Replace("N", "")
                                 .Replace("R", "");

        string[] parts = cleanedMove.Split('-');
        if (parts.Length != 2) return;

        string from = parts[0];
        string to = parts[1];

        var board = FindAnyObjectByType<ChessboardGenerator>();
        if (board == null) return;

        switch (level)
        {
            case 1:
                string text = GetTextHint(loader.GetCurrentStep());
                FloatingHint.Instance.ShowHint(text, hintAnchor);
                Debug.Log("[Hint] Text-Hint ausgelöst: " + text);
                break;

            case 2:
                HighlightPiece(from, Color.green);
                break;

            case 3:
            
                Vector3 fromPos = board.GetTilePosition(from);
                Vector3 toPos = board.GetTilePosition(to);

                DrawArrow(fromPos, toPos, Color.yellow);

                ClearTileHighlights();
                HighlightTileOverlay(from);
                HighlightTileOverlay(to);
                break;
        }
    }

    private void HighlightPiece(string coord, Color color)
    {
        var board = FindAnyObjectByType<ChessboardGenerator>();
        if (board == null) return;

        Vector3 pos = board.GetTilePosition(coord);
        Collider[] hits = Physics.OverlapSphere(pos, 0.05f);

        foreach (var h in hits)
        {
            if (h.CompareTag("Piece"))
            {
                Renderer r = h.GetComponentInChildren<Renderer>();
                if (r != null)
                    StartCoroutine(FlashPiece(r, color));
            }
        }
    }

    private IEnumerator FlashPiece(Renderer rend, Color color)
    {
        Color original = rend.material.color;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            rend.material.color = color;
            yield return new WaitForSeconds(0.3f);
            rend.material.color = original;
            yield return new WaitForSeconds(0.3f);
            elapsed += 0.6f;
        }

        rend.material.color = original;
    }

    private void DrawArrow(Vector3 from, Vector3 to, Color color)
    {
        GameObject arrow = new GameObject("HintArrow");
        var line = arrow.AddComponent<LineRenderer>();

        line.positionCount = 2;
        line.SetPosition(0, from + Vector3.up * 0.05f);
        line.SetPosition(1, to + Vector3.up * 0.05f);
        line.startWidth = 0.02f;
        line.endWidth = 0.005f;

        Shader shader = Shader.Find("Unlit/Color") ??
                        Shader.Find("Legacy Shaders/Particles/Alpha Blended") ??
                        Shader.Find("Standard");

        line.material = new Material(shader);
        line.material.color = color;

        Destroy(arrow, arrowDuration);
    }

    private void AdjustHintLevelAfterPuzzle()
    {
        int nextBase = Mathf.Max(currentHintLevel - 2, 0);
        baseHintLevel = nextBase;

        Debug.Log($" Puzzle beendet. Aktuelle Stufe {currentHintLevel} → nächste Startstufe {nextBase}");
    }

    public void ResetHints()
    {
        hintJustUsed = false;
        timer = 0f;
        errorCount = 0;
        currentHintLevel = baseHintLevel;
        puzzleActive = true;
    }

    public void ClearHintFlag()
    {
        hintJustUsed = false;
    }

    public string GetTextHint(int step)
    {
        var loader = FindAnyObjectByType<TacticLoader>();
        var puzzle = loader.currentPuzzle;

        if (puzzle.hints != null &&
            step < puzzle.hints.Count &&
            !string.IsNullOrEmpty(puzzle.hints[step]))
        {
            return puzzle.hints[step];
        }

        return "Kein Hinweis verfügbar";
    }

    public void RegisterHintEvent(bool manual)
    {

        hintEventsThisMove.Add(new HintEvent
        {
            isManual = manual,
            isAutomatic = !manual,
            HintLevel = currentHintLevel,
        });
    }

    public void HighlightTileOverlay(string coord)
    {
        var board = FindAnyObjectByType<ChessboardGenerator>();
        if (board == null) return;

        Vector3 tilePos = board.GetTilePosition(coord);
        tilePos.y += 0.01f;

        GameObject overlay = Instantiate(
            tileHighlightPrefab,
            tilePos,
            Quaternion.Euler(90, 0, 0)
        );

        overlay.transform.localScale = Vector3.one * 0.12f;
        activeHighlights.Add(overlay);
    }

    public void RequestManualHint()
    {
        if (currentHintLevel < 3)
            currentHintLevel++;
        else
            currentHintLevel = 3;

        TriggerHint(currentHintLevel, true);
    }

    public void ClearTileHighlights()
    {
        foreach (var obj in activeHighlights)
        {
            if (obj != null)
                Destroy(obj);
        }

        activeHighlights.Clear();
    }
    public void ResetHintLevelForNextMove()
{
    Debug.Log("Hint reset: next move at level at level 0");
    currentHintLevel = 0;
    hintJustUsed = false;
}
public void SetMode(HintMode mode)
{
    CurrentMode = mode;
    Debug.Log($"HintMode gesetzt auf: {mode}");
}

 public void SetDifficulty(string difficulty)
    {
        if (string.IsNullOrEmpty(difficulty)) difficulty = "medium";

        timeToHint1 = defaultTimeToHint1;
        timeToHint2 = defaultTimeToHint2;
        timeToHint3 = defaultTimeToHint3;


        switch (difficulty.ToLowerInvariant())
        {
            case "easy":
            case "leicht":
                timeToHint1 = 30f;   
                timeToHint2 = 40f;   
                timeToHint3 = 50f;  
                break;

            case "medium":
            case "mittel":
            default:
                timeToHint1 = 40f;
                timeToHint2 = 50f;
                timeToHint3 = 60f;
                break;

            case "hard":
            case "schwer":
                timeToHint1 = 50f;
                timeToHint2 = 60f;
                timeToHint3 = 70f;
                break;

        }

        Debug.Log($" HintSystem konfiguriert: {difficulty} " +
                  $"(t1={timeToHint1}s, t2={timeToHint2}s, t3={timeToHint3}s)");
    }

}
