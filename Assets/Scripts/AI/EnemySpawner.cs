using System.Collections;
using UnityEngine;
using TMPro;
public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;      // The enemy prefab to spawn
    public Transform spawnPoint;        // The spawn point of the enemies
    public float waveDuration = 60f;    // Duration of each wave (in seconds)
    public float timeBetweenWaves = 5f; // Time between waves (in seconds)
    public int totalWaves = 10;         // Total number of waves
    private int currentWave = 1;       // Current wave number
    public int waveCountdown = 5;
    private bool isWaveActive = false; // Tracks if a wave is active
    public bool canStartNextWave = true; // Tracks if the player can start the next wave
    public bool playerIsInTrigger = false;
    public bool isCountingDown = false;
    [SerializeField] private TextMeshProUGUI waveCountdownText;
    [SerializeField] private SpriteRenderer triggerArea;
    [SerializeField] private Player player;
    private GameLocalization localization;

    private void Start()
    {
        waveCountdownText.gameObject.SetActive(false);
        triggerArea.gameObject.SetActive(false);
        player = FindObjectOfType<Player>();
        localization = FindObjectOfType<GameLocalization>();
    }

    private void Update()
    {
        if (playerIsInTrigger && canStartNextWave)
        {
            if (!isCountingDown){ StartCoroutine(StartWaveCountdown()); }
        }
        
        if (!playerIsInTrigger)
        {
            waveCountdown = 5;
            waveCountdownText.text = localization.GetLocalizedTextByValue("wavesCountdownText") + waveCountdown.ToString();
        }

        if (player.isDead)
        {
            StopCoroutine(SpawnWave());
            this.enabled = false;
        }

    }

    public void SetCanStartNextWave(bool value)
    {
        canStartNextWave = value;
    }


    public void StartNextWave()
    {
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {

            // Ensure only one wave can be active at a time
            if (isWaveActive)
                yield break;

            // Prevent the player from starting another wave while this one is active
            canStartNextWave = false;

            yield return new WaitForSeconds(2f); // Wait for 2 seconds before starting the wave

            isWaveActive = true;

            float waveEndTime = Time.time + waveDuration;

    
            while (!player.isDead && Time.time < waveEndTime)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(2f); // Adjust the time between enemy spawns
            }

            // After we spawn an enemy, check to see if the player was killed
            if (player.isDead)
                yield break;

            // Wait for the remaining time in the wave
            yield return new WaitForSeconds(waveEndTime - Time.time);

            if (waveCountdown != 5){ waveCountdown = 5; }
            waveCountdownText.text = localization.GetLocalizedTextByValue("wavesCountdownText") + waveCountdown.ToString();
            isWaveActive = false;
            currentWave++;
            GameController.gameControllerInstance.AddWave();

            // Check if all waves have been completed
            if (currentWave <= totalWaves)
            {
                canStartNextWave = true; // Allow the player to start the next wave
            }
            else
            {
                canStartNextWave = false;
                // Display "All Waves Completed" or appropriate UI feedback
                // Set a boolean or trigger animation to display this text or UI element.
            }
        

    }

    private IEnumerator StartWaveCountdown()
    {

        isCountingDown = true;

        if (waveCountdown > 0)
        {
            if (playerIsInTrigger) {
                yield return new WaitForSeconds(0.85f);
                waveCountdown--;
                waveCountdownText.text = localization.GetLocalizedTextByValue("wavesCountdownText") + waveCountdown.ToString();
                yield return null;
            }
            else
            {
                waveCountdown = 5;
                isCountingDown = false;
                yield break;
            }
        }

        if (canStartNextWave && waveCountdown <= 0) {
            waveCountdown = 5;
            waveCountdownText.text = localization.GetLocalizedTextByValue("waveStartText");
            StartNextWave();
            ResetActiveObjects();
            isCountingDown = false;
            yield break;
        }
        else
        {
            isCountingDown = false;
            yield break;
        }
    }

    private void ResetActiveObjects()
    {
        waveCountdownText.gameObject.SetActive(false);
        triggerArea.gameObject.SetActive(false);
    }

    private void SpawnEnemy()
    {   
        Vector2 spawnPos = new Vector2(spawnPoint.position.x, spawnPoint.position.y - 1f);
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player enters the trigger area
        if (other.CompareTag("Player") && canStartNextWave)
        {
            // Show the "Press F to Start" text (you can implement this with UI or a Text component)
            // Set a boolean or trigger animation to display the text to guide the player.
            waveCountdownText.gameObject.SetActive(true);
            // Debug.Log("Player is in Trigger!");
            playerIsInTrigger = true;
            
            triggerArea.gameObject.SetActive(true);

        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInTrigger = true;   
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the player leaves the trigger area
        if (other.CompareTag("Player"))
        {
            // Hide the "Press F to Start" text (you can implement this with UI or a Text component)
            // Set a boolean or trigger animation to hide the text.
           waveCountdownText.gameObject.SetActive(false);
           playerIsInTrigger = false;
           triggerArea.gameObject.SetActive(false);
           waveCountdown = 5;
           waveCountdownText.text = localization.GetLocalizedTextByValue("wavesCountdownText") + waveCountdown.ToString();
           isCountingDown = false;
        }
    }

}

