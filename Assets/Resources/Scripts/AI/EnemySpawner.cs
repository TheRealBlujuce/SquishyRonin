using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class EnemySpawner : MonoBehaviour
{
    public List<GameObject> enemyPrefabs = new List<GameObject>();
	public GameObject spawnPrefab;
    public Transform spawnPoint;
    public float spawnRadius = 5f; // New: Radius around the spawn point to randomize
    public float waveDuration = 60f;
    public float timeBetweenWaves = 5f;
    public int totalWaves = 10;
    public float spawnSpeed = 1.25f;
    private int currentWave = 1;
    public int waveCountdown = 5;
    private bool isWaveActive = false;
    public bool canStartNextWave = true;
    public bool playerIsInTrigger = false;
    public bool isCountingDown = false;

    [SerializeField] private TextMeshProUGUI waveCountdownText;
    [SerializeField] private SpriteRenderer triggerArea;
    [SerializeField] private Player player;
    private GameLocalization localization;

    private void Start()
    {
        waveCountdownText.gameObject.SetActive(false);
        triggerArea.GetComponent<SpriteRenderer>().enabled = false;
        player = FindFirstObjectByType<Player>();
        localization = FindFirstObjectByType<GameLocalization>();
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
			GameController.gameControllerInstance.currentWorldState = GameController.WorldState.ARENA;

            yield return new WaitForSeconds(2f); // Wait for 2 seconds before starting the wave

            isWaveActive = true;

            float waveEndTime = Time.time + waveDuration;

    
            while (!player.isDead && Time.time < waveEndTime)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(spawnSpeed); // Adjust the time between enemy spawns
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
			
			GameController.gameControllerInstance.currentWorldState = GameController.WorldState.WORLD;

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
        triggerArea.gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }

	private void SpawnEnemy()
    {
        int chooseEnemy = Random.Range(0, enemyPrefabs.Count);

        // Generate a random point within a circle
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector2 spawnPos = (Vector2)spawnPoint.position + randomOffset;

        GameObject spawn = Instantiate(spawnPrefab, spawnPos, Quaternion.identity);
		spawn.GetComponent<SpawnAfterBuildup>().enemyPrefab = enemyPrefabs[chooseEnemy];
    }

	public TextMeshProUGUI GetWaveCountdownText()
	{
		return waveCountdownText;
	}

	public GameLocalization GetLocalization()
	{
		return localization;
	}

	private void OnDrawGizmosSelected()
	{
		if (spawnPoint != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(spawnPoint.position, spawnRadius);
		}
	}


}

