using UnityEngine;

public enum GameMode
{
    Adaptive,
    Static
}

public class ModeManager : MonoBehaviour
{
    public static GameMode CurrentMode = GameMode.Adaptive;

    public void SetModeAdaptive()
    {
        CurrentMode = GameMode.Adaptive;
        Debug.Log("Modus: Adaptiv");
    }

    public void SetModeStatic()
    {
        CurrentMode = GameMode.Static;
        Debug.Log("Modus: Statisch");
    }
}
