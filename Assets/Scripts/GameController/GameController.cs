using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public PlayerInput gameInput;
    public static GameController gameControllerInstance { get; private set; }

    public int kills {get; private set;}
    public int currentWave {get; private set;}
    public int currentWaveCountdown {get; private set;}
    public bool gameOver {get; private set;}
    public bool gamePaused {get; private set;}
    public GameState currentGameState = GameState.PLAYING;
    public TextMeshProUGUI currentWaveText;
    public TextMeshProUGUI killsText;
    public AnimateMenu gameOverScreen;
    public Canvas gameUI;
    private GameLocalization localization;
    public Screenshake screenshake;
    private string currentKillsText;
    private string currentWavesText;

    public enum GameState 
    {
        MENU,
        PLAYING,
        PAUSED,
        GAMEOVER
    }


    private void Awake()
    {
        gameInput = new PlayerInput();
        if (gameControllerInstance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            gameControllerInstance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        gameInput.Enable();
    }

    private void OnDisable()
    {
        gameInput.Disable();
    }


    private void OnDestroy()
    {
        if (gameControllerInstance == this)
        {
            gameControllerInstance = null;
        }
    }

    private void Start()
    {
        // Maybe do something with the main menu?
        screenshake = FindObjectOfType<Screenshake>();
        localization = GetComponent<GameLocalization>();
    }

    private void NewGame()
    {
        currentWave = 1;
        kills = 0;
        currentGameState = GameState.PLAYING;
        gameInput.Enable();
    }

    private IEnumerator MainMenu()
    {
        currentGameState = GameState.MENU;

        yield return null;

        gameInput.Enable();
    }

    private void FindDependenciesIfMissing()
    {
        // Check if references are null and find them in the scene if they are
        if (currentWaveText == null)
        {
            currentWaveText = GameObject.Find("wavesText").GetComponent<TextMeshProUGUI>();
            currentWavesText = currentWaveText.text;
        }

        if (killsText == null)
        {
            killsText = GameObject.Find("killsText").GetComponent<TextMeshProUGUI>();
            currentKillsText = killsText.text;
        }

        if (gameOverScreen == null)
        {
            gameOverScreen = GameObject.Find("GameOverScreen").GetComponent<AnimateMenu>();
        }

        if (gameUI == null)
        {
            gameUI = GameObject.Find("GameUI").GetComponent<Canvas>();
        }

        if (screenshake == null)
        {
            screenshake = FindObjectOfType<Screenshake>();
        }

    }

    public void AddKill()
    {
        kills++;
    }
    public void AddWave()
    {
        currentWave++;
    }

    public void EndGame()
    {
        currentGameState = GameState.GAMEOVER;
        if (gameUI != null) { gameUI.gameObject.SetActive(false); }
        
        gameOverScreen.StartMenuAnimation();
    }

    public void RestartLevel()
    {
        // Restart the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        NewGame();
    }

    public void PlayGame()
    {
        if (currentGameState == GameState.MENU)
        {
            gameInput.Disable();
            SceneManager.LoadScene("World_Map");
            // StartCoroutine(DelayedLocalizationUpdate());
            localization.SetLanguage(localization.currentLanguage);
            NewGame();
        }
    }

    public void ReturnToMenu()
    {
        if (currentGameState == GameState.GAMEOVER)
        {
            gameInput.Disable();
            SceneManager.LoadScene("MainMenu");
            StartCoroutine(MainMenu());
        }
    }


    private void Update()
    {
        if (currentGameState == GameState.PLAYING)
        {
            FindDependenciesIfMissing();
            currentWaveText.text = localization.GetLocalizedTextByValue("wavesText") + currentWave.ToString();
            killsText.text = localization.GetLocalizedTextByValue("killsText") + kills.ToString();
            currentWaveCountdown = FindObjectOfType<EnemySpawner>().waveCountdown;
        }

    }

    private void LateUpdate()
    {
        if (currentGameState != GameState.PLAYING)
        {
            localization.SetLanguage(localization.currentLanguage);
        }
    }



}
