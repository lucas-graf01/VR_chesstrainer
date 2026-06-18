using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    int currentHintLevel = 0;

    [Header("Menu Panel")]
    public GameObject menuPanel;

    [Header("References")]
    public TacticLoader tacticLoader;
    public HintSystem hintSystem;
    public TacticLogger logger;

    [Header("Menu Button Icon")]
    public UnityEngine.UI.Image menuButtonImage;
    public Sprite openSprite;
    public Sprite closeSprite;

    void Start()
    {
        
        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    public void ToggleMenu()
    {
            if (menuPanel == null || logger == null)
        return;

    bool willOpen = !menuPanel.activeSelf;

    menuPanel.SetActive(willOpen);

    if (menuButtonImage != null){

        menuButtonImage.sprite = willOpen ? closeSprite : openSprite;
    }

    if (willOpen)
    {
        logger.PauseTiming();
    }
    else
    {
        logger.ResumeTiming();
       
    }
}

    public void RequestHint()
    {
        if (menuPanel != null)
            menuPanel.SetActive(!menuPanel.activeSelf);

        Debug.Log("Button clicked - Level now: " + currentHintLevel);
        Debug.Log("Manual hint getriggert");

        if (hintSystem != null)
        {
            hintSystem?.RequestManualHint();
            Debug.Log("CurrentHintLevel vor : " + currentHintLevel);
        }
    }

    public void OnStartPuzzle()
    {
        if (menuPanel != null)
            menuPanel.SetActive(!menuPanel.activeSelf);

        tacticLoader.StartGame();
        logger.StartTimerFromZero();

        Debug.Log("Spieler hat Start Knopf gestartet");
    }

    public void RestartApplication()
    {
        if (menuPanel != null)
            menuPanel.SetActive(!menuPanel.activeSelf);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnQuitGame()
    {
        if (menuPanel != null)
            menuPanel.SetActive(!menuPanel.activeSelf);

        Application.Quit();
        Debug.Log("Spiel abgebrochen");
    }

    public void ToggleQuestionnaire()
    {
    

}

}