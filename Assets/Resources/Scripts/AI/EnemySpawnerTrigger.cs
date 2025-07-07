using UnityEngine;

public class EnemySpawnerTrigger : MonoBehaviour
{
	private EnemySpawner spawnerParent;

	private void Start()
	{
		spawnerParent = GetComponentInParent<EnemySpawner>();
	}

	private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player enters the trigger area
        if (other.CompareTag("Player") && spawnerParent.canStartNextWave)
        {
            // Show the "Press F to Start" text (you can implement this with UI or a Text component)
            // Set a boolean or trigger animation to display the text to guide the player.
            spawnerParent.GetWaveCountdownText().gameObject.SetActive(true);
            // Debug.Log("Player is in Trigger!");
            spawnerParent.playerIsInTrigger = true;
            
            this.gameObject.GetComponent<SpriteRenderer>().enabled = true; 

        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") == true)
        {
            spawnerParent.playerIsInTrigger = true;   
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the player leaves the trigger area
        if (other.CompareTag("Player") == true)
        {
			// Hide the "Press F to Start" text (you can implement this with UI or a Text component)
			// Set a boolean or trigger animation to hide the text.
			spawnerParent.GetWaveCountdownText().gameObject.SetActive(false);
			spawnerParent.playerIsInTrigger = false;
			this.gameObject.GetComponent<SpriteRenderer>().enabled = false; 
			spawnerParent.waveCountdown = 5;
			spawnerParent.GetWaveCountdownText().text = spawnerParent.GetLocalization().GetLocalizedTextByValue("wavesCountdownText") + spawnerParent.waveCountdown.ToString();
			spawnerParent.isCountingDown = false;
        }
    }
}
