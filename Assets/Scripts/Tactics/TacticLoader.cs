using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TacticLoader : MonoBehaviour
{
    [Header("References")]
    public FENLoader fenLoader;
    public TacticLogger logger;

    [Header("Puzzle Data")]
    public List<TacticPuzzle> puzzles = new List<TacticPuzzle>();

    private int currentIndex = 0;
    private int currentStep = 0;

    public int GetCurrentPuzzleIndex() => currentIndex;
    public int GetCurrentStep() => currentStep;

    public string activeMode = "adaptive";
    public TacticPuzzle currentPuzzle;

    private HintSystem hintSystem;
    private ChessboardGenerator board;

    public bool lastMoveCorrect;

    public List<string> appliedMoves = new List<string>();
    public List<string> capturedCoords = new List<string>();

    public enum TacticOrder
    {
        AdaptiveFirst,
        StaticFirst
    }

    [SerializeField]
    public TacticOrder order;

    private bool pendingAutoReplay = false;

    // --------------------------------------------------
    // 
    // --------------------------------------------------

    public void Start()
    {
        LoadAllPuzzles();

        hintSystem = FindAnyObjectByType<HintSystem>();
        board = FindAnyObjectByType<ChessboardGenerator>();

        LoadCurrentPuzzle();
    }

    public void Update()
    {
    }

    private void OnEnable()
    {
        ChessboardGenerator.OnBoardRebuilt += HandleBoardRebuilt;
    }

    private void OnDisable()
    {
        ChessboardGenerator.OnBoardRebuilt -= HandleBoardRebuilt;
    }

    // --------------------------------------------------
    
    // --------------------------------------------------
    public void StartGame() 
    {
    logger.ResetTimer();
    appliedMoves.Clear();
    currentIndex = 0;         
    currentPuzzle.id = 2;
    }
    // --------------------------------------------------
    //
    // --------------------------------------------------

    public void LoadAllPuzzles()
    {
        string path = order == TacticOrder.AdaptiveFirst
            ? "Tactics/tactics_adaptive_first"
            : "Tactics/tactics_static_first";

        Debug.Log(" Loading puzzles from path: " + path);

        TextAsset jsonFile = Resources.Load<TextAsset>(path);

        if (jsonFile == null)
        {
            Debug.LogError("Keine tactics.json wurde nicht in Resources gefunden");
            return;
        }

        string json = jsonFile.text;
        puzzles = JsonUtilityWrapper.FromJsonList<TacticPuzzle>(json);

        if (puzzles == null || puzzles.Count == 0)
        {
            Debug.LogError(" Keine gültigen Taktikaufgaben in der Datei gefunden.");
            return;
        }

        puzzles = Enumerable
            .OrderBy(puzzles, p => p.mode == "adaptive" ? 0 : 1) 
            .ThenBy(p => DifficultyRank(p.difficulty))
            .ToList();

        Debug.Log($" {puzzles.Count} Taktikaufgaben geladen (sortiert).");

        currentIndex = 0;

        for (int i = 0; i < puzzles.Count; i++)
        {
            Debug.Log($"[{i}] ID={puzzles[i].id} | {puzzles[i].mode} | {puzzles[i].difficulty}");
        }
    }

    public void LoadCurrentPuzzle()
    {
       
        if (hintSystem != null)
        {
        if (currentPuzzle.mode == "adaptive")
        {
        hintSystem.SetMode(HintSystem.HintMode.Adaptive);
        }
        else
        {
        hintSystem.SetMode(HintSystem.HintMode.Static);
        }
        }

       
       
        if (currentIndex >= puzzles.Count)
        {
            Debug.Log(" Alle Puzzles abgeschlossen!");
            return;
        }

        currentPuzzle = puzzles[currentIndex];
        currentStep = 0;

        fenLoader.LoadFEN(currentPuzzle.fen);

        hintSystem?.ResetHints();
        hintSystem.totalHintsUsed = 0;
        hintSystem?.StartPuzzle();
        hintSystem.ClearHintFlag();

        if (hintSystem != null && !string.IsNullOrEmpty(currentPuzzle.difficulty))
        {
            hintSystem.SetDifficulty(currentPuzzle.difficulty);
            Debug.Log($"Schwierigkeitsgrad: {currentPuzzle.difficulty}");
        }

        logger.StartNewPuzzle(
            currentPuzzle.id,
            currentPuzzle.fen,
            currentPuzzle.solution
        );

        Debug.Log($" Puzzle {currentPuzzle.id} gestartet – Schritt 1/{currentPuzzle.solution.Count}");
    }

    // --------------------------------------------------
    // Move Handling
    // --------------------------------------------------

    public void CheckPlayerMove(string move)
    {
        Debug.Log($"[TacticLoader] CheckPlayerMove: {move}");

        if (currentPuzzle == null)
        {
            Debug.LogWarning("[TacticLoader] Kein Puzzle aktiv – Move ignoriert.");
            return;
        }

        if (currentStep >= currentPuzzle.solution.Count)
        {
            Debug.LogWarning("[TacticLoader] Puzzle bereits fertig – Move ignoriert.");
            return;
        }

        string expected = NormalizeMove(currentPuzzle.solution[currentStep]);
        string played = NormalizeMove(move);

        if (played == expected)
        {
            logger.LogMove(move, true);
            hintSystem.ClearTileHighlights();
            lastMoveCorrect = true;

            hintSystem?.ResetHintLevelForNextMove();
            appliedMoves.Add(move);

            currentStep++;

            if (currentStep >= currentPuzzle.solution.Count)
            {
                logger.FinishPuzzle();
                hintSystem?.EndPuzzle();
                LoadNextPuzzle();
                return;
            }

            string autoMove = currentPuzzle.solution[currentStep];
            PlayAutoMove(autoMove);

            currentStep++;

            if (currentStep >= currentPuzzle.solution.Count)
            {
                logger.FinishPuzzle();
                hintSystem?.EndPuzzle();
                LoadNextPuzzle();
            }

            return;
        }

        logger.LogMove(move, false);
        lastMoveCorrect = false;
    }

    public void PlayAutoMove(string move)
    {
        if (string.IsNullOrEmpty(move)) return;

        if (board == null)
            board = FindAnyObjectByType<ChessboardGenerator>();

        string norm = NormalizeMove(move);

        if (norm.Length < 4)
        {
            Debug.LogError($"[AutoMove] Ungültiges Format: {move}");
            return;
        }

        appliedMoves.Add(move);

        string from = norm.Substring(0, 2);
        string to = norm.Substring(2, 2);

        SnapAndVali selected = null;
        foreach (var p in FindObjectsOfType<SnapAndVali>())
        {
            if (p.GetCurrentCoord(p.transform.position) == from)
            {
                selected = p;
                break;
            }
        }

        if (selected == null)
        {
            Debug.LogError($"[AutoMove] Keine Figur auf Feld {from}");
            return;
        }

        var targetPiece = PieceRegistry.Instance.GetPieceAt(to);
        if (targetPiece != null)
        {
            targetPiece.gameObject.SetActive(false);
            PieceRegistry.Instance.UnregisterPiece(to);
        }

        PieceRegistry.Instance.UnregisterPiece(from);
        PieceRegistry.Instance.RegisterPiece(to, selected);

        selected.MoveToCoord(to);
    }

    // --------------------------------------------------
    // 
    // --------------------------------------------------

    public void LoadNextPuzzle()
    {
        StartCoroutine(LoadNextPuzzleDelayed());
    }

    private IEnumerator LoadNextPuzzleDelayed()
    {
        yield return new WaitForSeconds(0.6f);

        currentIndex++;

        if (currentIndex < puzzles.Count)
        {
            LoadCurrentPuzzle();
        }
        else
        {
            Debug.Log(" Alle Taktiken abgeschlossen!");
        }
    }

    public void LoadPreviousPuzzle()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            LoadCurrentPuzzle();
        }
        else
        {
            Debug.Log("Du bist schon bei der ersten Aufgabe");
        }
    }

    public void ResetCurrentPuzzle()
    {
        LoadCurrentPuzzle();
        hintSystem?.ResetHints();
        logger?.StartNewPuzzle(currentPuzzle.id, currentPuzzle.fen, currentPuzzle.solution);
        currentStep = 0;
    }

    public void ReloadPuzzleCompletely(){

        Debug.Log("ReloadPuzzleCompletely gestartet");
        
        LoadCurrentPuzzle();
        logger?.StartNewPuzzle(currentPuzzle.id, currentPuzzle.fen, currentPuzzle.solution);
        currentStep = 0;
        StartCoroutine(ReapplyMovesDelayed());
    }


    // --------------------------------------------------
    // 
    // --------------------------------------------------

    private string NormalizeMove(string move)
    {
        if (string.IsNullOrEmpty(move)) return "";

        move = move.ToLower().Trim();
        move = System.Text.RegularExpressions.Regex.Replace(move, "[A-Z]", "");
        move = move.Replace("x", "")
                   .Replace("+", "")
                   .Replace("#", "")
                   .Replace("=", "")
                   .Replace(" ", "")
                   .Replace("-", "");

        return move;
    }

    public string GetExpectedMove()
    {
        return NormalizeMove(currentPuzzle.solution[currentStep]);
    }

    private void HandleBoardRebuilt()
    {
        if (pendingAutoReplay)
        {
            pendingAutoReplay = false;
            StartCoroutine(ReapplyMovesDelayed());
        }
    }

    private int DifficultyRank(string difficulty)
    {
        return difficulty switch
        {
            "easy" => 0,
            "medium" => 1,
            "hard" => 2,
            _ => 99
        };
    }

    private IEnumerator ReapplyMoves()
    {
        foreach (string move in new List<string>(appliedMoves))
        {
            PlayAutoMove(move);
            yield return new WaitForSeconds(4f);
        }
    }

    public IEnumerator ReapplyMovesDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(ReapplyMoves());
    }
}

// --------------------------------------------------
// 
// --------------------------------------------------

public static class JsonUtilityWrapper
{
    public static List<T> FromJsonList<T>(string json)
    {
        try
        {
            json = json.Trim();

            if (string.IsNullOrEmpty(json))
                throw new ArgumentException("JSON ist leer.");

            if (json.StartsWith("["))
                json = "{\"Items\":" + json + "}";

            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);

            if (wrapper == null || wrapper.Items == null)
                throw new ArgumentException("Wrapper oder Items sind null.");

            return wrapper.Items;
        }
        catch (Exception e)
        {
            Debug.LogError($"Fehler beim JSON-Parsing: {e.Message}");
            return new List<T>();
        }
    }

    [Serializable]
    private class Wrapper<T>
    {
        public List<T> Items;
    }
}
