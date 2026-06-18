using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#region Data Classes

[Serializable]
public class LogEntry
{
    public int puzzleId;
    public string fen;
    public string playerMove;
    public bool correct;
    public float timeTaken;
    public int CurrentHintLevel;
    public bool hintUsedThisMove;
    public int totalHintsUsed;
    public string playerName;

    public float timeSinceStart;
    public List<HintSystem.HintEvent> hintEvents;
}

[Serializable]
public class PuzzleSummary
{
    public int puzzleId;
    public string fen;
    public int totalMoves;
    public int correctMoves;
    public int wrongMoves;
    public float totalTime;
    public float averageMoveTime;
    public float successRate;
    public string playerName;
}

[Serializable]
public class SessionSummary
{
    public string playerName;
    public int puzzleSolved;
    public int totalMovesSession;
    public int correctMovesSession;
    public int wrongMovesSession;
    public float totalTimeSession;
    public float avgMoveTimeSession;
    public float successRateSession;
}

[Serializable]
public class HintEvent
{
    public bool isManual;
    public bool isAutomatic;
    public int HintLvel;
}

#endregion

public class TacticLogger : MonoBehaviour
{
    #region Fields

    [Header("Session Info")]
    public string participantName = "TestPerson";

    private string sessionId;

    private List<LogEntry> entries = new List<LogEntry>();
    private List<PuzzleSummary> summaries = new List<PuzzleSummary>();

    public List<HintEvent> hintEvents;
    private SessionSummary sessionSummary;

    private int currentPuzzleId;
    private string currentFen;
    private List<string> solution;

    private float startTime;
    private float lastMoveTime;

    private int correctMoves = 0;
    private int wrongMoves = 0;

    private string logJsonPath;
    private string logCsvPath;

    public float puzzleStartTime;
    public bool timingActive;

    private bool isPaused = false;
    private float pauseStartTime;
    private float totalPausedTime = 0f;

    private bool IsTutorialPuzzle => currentPuzzleId == 1;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        string folder = Path.Combine(Application.persistentDataPath, "Logs");
        Directory.CreateDirectory(folder);

        logJsonPath = Path.Combine(folder, $"tactic_log_{sessionId}.json");
        logCsvPath = Path.Combine(folder, $"tactic_log_{sessionId}.csv");

        Debug.Log($" Logs werden gespeichert unter:\n{folder}");
        Debug.Log("Logger sieht Hintsystem: " + GetInstanceID());
    }

    #endregion

    #region Puzzle Lifecycle

    public void StartNewPuzzle(int id, string fen, List<string> sol)
    {
        currentPuzzleId = id;
        currentFen = fen;
        solution = sol;

        puzzleStartTime = Time.time;
        lastMoveTime = puzzleStartTime;

        correctMoves = 0;
        wrongMoves = 0;

        Debug.Log($" Neues Puzzle gestartet: {id}");
    }

    public bool LogMove(string move, bool isCorrect)
    {
        if (IsTutorialPuzzle)
            return false;

        var hintSystem = FindAnyObjectByType<HintSystem>();
        float now = Time.time - totalPausedTime;


        // Ze
        float timeSinceStart = now - puzzleStartTime;
        float timeTaken = now - lastMoveTime;
        lastMoveTime = now;

        int hintLevel = hintSystem != null ? hintSystem.currentHintLevel : 0;
        bool hintUsedNow = hintSystem != null && hintSystem.hintJustUsed;
        int totalHints = hintSystem != null ? hintSystem.totalHintsUsed : 0;

        int solvedSoFar = entries.Count(e => e.puzzleId == currentPuzzleId && e.correct);
        string expectedMove = (solution != null && solvedSoFar < solution.Count)
            ? solution[solvedSoFar]
            : null;

        bool correct = isCorrect || (expectedMove != null && move == expectedMove);
        float roundedTimeTaken = Mathf.Round(timeTaken * 10000f) / 10000f;
        float roundedTimeSinceStart = Mathf.Round(timeSinceStart * 10000f) / 10000f;


        var entry = new LogEntry
        {
            puzzleId = currentPuzzleId,
            fen = currentFen,
            playerMove = move,
            correct = correct,
            timeTaken = roundedTimeTaken,
            timeSinceStart = roundedTimeSinceStart,
            CurrentHintLevel = hintLevel,
            hintUsedThisMove = hintUsedNow,
            totalHintsUsed = totalHints,
            hintEvents = hintSystem != null
                ? hintSystem.hintEventsThisMove.ToList()
                : new List<HintSystem.HintEvent>()
        };

        entries.Add(entry);

        if (hintSystem != null)
        {
            hintSystem.hintEventsThisMove.Clear();
            hintSystem.ClearHintFlag();
        }

        if (correct) correctMoves++;
        else wrongMoves++;

        SaveLogs();

        Debug.Log(
            $" Move loggt | Puzzle {currentPuzzleId} | " +
            $"Δt={timeTaken:F2}s | t={timeSinceStart:F2}s | correct={correct}"
        );

        return correct;
    }

    public void FinishPuzzle()
    {
        if (IsTutorialPuzzle)
        {
            Debug.Log("Tutorial-Puzzle beendet – keine Statistik");
            return;
        }

        float totalTime = Time.time - puzzleStartTime;
        int totalMoves = correctMoves + wrongMoves;

        float avgMoveTime = totalMoves > 0 ? totalTime / totalMoves : 0f;
        float successRate = totalMoves > 0 ? (float)correctMoves / totalMoves : 0f;

        var summary = new PuzzleSummary
        {
            puzzleId = currentPuzzleId,
            fen = currentFen,
            totalMoves = totalMoves,
            correctMoves = correctMoves,
            wrongMoves = wrongMoves,
            totalTime = totalTime,
            averageMoveTime = avgMoveTime,
            successRate = successRate,
        };

        summaries.Add(summary);
        SaveLogs();

        Debug.Log(
            $"=== PUZZLE {currentPuzzleId} ===\n" +
            $"Korrekte Züge: {correctMoves}/{totalMoves}\n" +
            $"Gesamtzeit: {totalTime:F1}s\n" +
            $"Durchschnitt: {avgMoveTime:F2}s/Zug\n" +
            $"Trefferquote: {successRate:P0}\n" +
            $"=========================="
        );
    }

    #endregion

    #region Timer Control

    public void StartTimerFromZero()
    {
        puzzleStartTime = Time.time;
        lastMoveTime = puzzleStartTime;
        timingActive = true;

        Debug.Log("[Logger] Timer startet bei: " + lastMoveTime);
    }

    public void ResetTimer()
    {
        puzzleStartTime = Time.time;
        lastMoveTime = puzzleStartTime;

        Debug.Log("[Logger] Timer reset");
    }
    public void PauseTiming()
    {
    if (!timingActive || isPaused)
        return;

    isPaused = true;
    pauseStartTime = Time.time;

    Debug.Log("[Logger] Timing pausiert");
    }

    public void ResumeTiming()
    {
    if (!timingActive || !isPaused)
        return;

    float pauseDuration = Time.time - pauseStartTime;
    totalPausedTime += pauseDuration;

    isPaused = false;

    Debug.Log($"[Logger] Timing verbraucht nach {pauseDuration:F2}s pause");
    }


    #endregion

    #region Persistence

    private void SaveLogs()
    {
        try
        {
            string json = JsonUtility.ToJson(
                new LogWrapper
                {
                    entries = entries,
                    summaries = summaries,
                    sessionSummary = sessionSummary
                },
                true
            );

            File.WriteAllText(logJsonPath, json);
            SaveCsv();
        }
        catch (Exception e)
        {
            Debug.LogError($" Fehler beim Speichern des Logs: {e.Message}");
        }
    }
    


    private void SaveCsv()
    {
        using (StreamWriter sw = new StreamWriter(logCsvPath))
        {
            sw.WriteLine("Participant,SessionID,PuzzleID,Move,Correct,TimeTaken,FEN");

            foreach (var e in entries)
            {
                sw.WriteLine(
                    $"{participantName},{sessionId},{e.puzzleId}," +
                    $"{e.playerMove},{e.correct},{e.timeTaken:F2},\"{e.fen}\""
                );
            }

            sw.WriteLine();
            sw.WriteLine("PuzzleID,TotalMoves,Correct,Wrong,TotalTime,AvgMoveTime,SuccessRate,FEN");

            foreach (var s in summaries)
            {
                sw.WriteLine(
                    $"{s.puzzleId},{s.totalMoves},{s.correctMoves}," +
                    $"{s.wrongMoves},{s.totalTime:F2}," +
                    $"{s.averageMoveTime:F2},{s.successRate:F2},\"{s.fen}\""
                );
            }

            int totalCorrect = 0;
            int totalWrong = 0;
            float totalTime = 0f;

            foreach (var s in summaries)
            {
                if (s.puzzleId == 1) continue;

                totalCorrect += s.correctMoves;
                totalWrong += s.wrongMoves;
                totalTime += s.totalTime;
            }

            int totalMoves = totalCorrect + totalWrong;
            float avgTime = totalMoves > 0 ? totalTime / totalMoves : 0f;
            float successRate = totalMoves > 0 ? (float)totalCorrect / totalMoves : 0f;

            sw.WriteLine();
            sw.WriteLine("SESSION_TOTAL,TotalMoves,Correct,Wrong,TotalTime,AvgMoveTime,SuccessRate");
            sw.WriteLine(
                $"ALL,{totalMoves},{totalCorrect},{totalWrong}," +
                $"{totalTime:F2},{avgTime:F2},{successRate:F2}"
            );
        }
    }

    #endregion

    #region Wrapper

    [Serializable]
    private class LogWrapper
    {
        public List<LogEntry> entries;
        public List<PuzzleSummary> summaries;
        public SessionSummary sessionSummary;
    }

    #endregion
}

