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
    public WorldState currentWorldState = WorldState.WORLD;

    public TextMeshProUGUI currentWaveText;
    public TextMeshProUGUI killsText;
    public AnimateMenu gameOverScreen;
    public Canvas gameUI;
    private GameLocalization localization;
    public int currentLocalization = 0;
    public Screenshake screenshake;
    private string currentKillsText;
    private string currentWavesText;
    private bool hasLoaded = false;
    private TextMeshProUGUI tempTEXT;

    [Header("Sound Effects")]
    public AudioSource audioSource;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip cutSound;
    [SerializeField] private AudioClip parrySound;

    // Levels for Vertical Slice
    private string mapleGrove = "Maple_Grove";
    private string birchGrove = "Birch_Grove";
    private string sakuraGrove = "Sakura_Grove";
	private string overWorld = "World_Map";

    public enum GameState 
    {
        MENU,
        PLAYING,
        PAUSED,
        GAMEOVER
    }

	public enum WorldState
	{
		WORLD,
		ARENA,
		DUNGEON
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
        screenshake = FindFirstObjectByType<Screenshake>();
        localization = GetComponent<GameLocalization>();
        tempTEXT = GetComponentInChildren<TextMeshProUGUI>();
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

        yield return new WaitForSeconds(0.25f);

        gameInput.Enable();
    }

    private void FindDependenciesIfMissing()
    {
        // Check if references are null and find them in the scene if they are
        if (currentWaveText == tempTEXT || currentWaveText == null)
        {
            currentWaveText = GameObject.Find("wavesText").GetComponent<TextMeshProUGUI>();
            currentWavesText = currentWaveText.text;
        }

        if (killsText == tempTEXT || killsText == null)
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
            screenshake = Camera.main.GetComponent<Screenshake>();
        }

        if (currentWaveCountdown == 0)
        {
            currentWaveCountdown = FindFirstObjectByType<EnemySpawner>().waveCountdown;
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
        hasLoaded = false;
        NewGame();  
    }

    public void PlayGame()
    {
        if (currentGameState == GameState.MENU)
        {
            gameInput.Disable();
			//var randomMap = Random.Range(0,3);

			//switch(randomMap)
			//{
			//	case 0:
			//	SceneManager.LoadScene(mapleGrove);
			//	break;
			//	case 1:
			//	SceneManager.LoadScene(birchGrove);
			//	break;
			//	case 2:
			//	SceneManager.LoadScene(sakuraGrove);
			//	break;
			//}

			SceneManager.LoadScene(overWorld);
			localization.SetLanguage(localization.currentLanguage);
            NewGame();
        }
    }

    public void ReturnToMenu()
    {
        if (currentGameState == GameState.GAMEOVER)
        {
            gameInput.Disable();
            hasLoaded = false;
            currentWaveText = tempTEXT;
            killsText = tempTEXT;
            SceneManager.LoadScene("MainMenu");
            StartCoroutine(MainMenu());
        }
    }


    // Sound Effects
    public void PlaySwordSwingSound()
    {
        audioSource.clip = attackSound;
        audioSource.Play();
    }

    public void PlayParrySound()
    {
        audioSource.clip = parrySound;
        audioSource.Play();
    }

    public void PlayCutSound()
    {
        audioSource.clip = cutSound;
        audioSource.Play();
    }

    private void Update()
    {
        if (currentGameState == GameState.PLAYING)
        {
            if (hasLoaded == false) { StartCoroutine(GetDependencies());}
            currentWaveText.text = localization.GetLocalizedTextByValue("wavesText") + currentWave.ToString();
            killsText.text = localization.GetLocalizedTextByValue("killsText") + kills.ToString();
            
        }

    }

    private void LateUpdate()
    {
        if (currentGameState != GameState.PLAYING)
        {
            localization.SetLanguage(localization.currentLanguage);
        }
    }

    private IEnumerator GetDependencies()
    {
        yield return null;

        FindDependenciesIfMissing();

        yield return null;

        hasLoaded = true;
    }


} 