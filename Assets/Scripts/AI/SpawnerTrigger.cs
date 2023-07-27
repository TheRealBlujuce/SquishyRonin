using UnityEngine;
using TMPro;
public class SpawnerTrigger : MonoBehaviour
{
    public GameObject enemySpawner; // Reference to the enemy spawner GameObject
    [SerializeField] private GameObject startTextObject;
    [SerializeField] private TextMeshProUGUI startText;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player enters the trigger area
        if (other.CompareTag("Player"))
        {
            // Show the "Press F to Start" text (you can implement this with UI or a Text component)
            // Set a boolean or trigger animation to display the text to guide the player.
            startTextObject.SetActive(true);
            startText.gameObject.SetActive(true);

            if (other.GetComponent<PlayerController>().isInteracting == true)
            {
                // Debug.Log("Starting the Wave!");
                StartWave();
            }
        }
    }

    public void StartWave()
    {
        enemySpawner.GetComponent<EnemySpawner>().StartNextWave();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the player leaves the trigger area
        if (other.CompareTag("Player"))
        {
            // Hide the "Press F to Start" text (you can implement this with UI or a Text component)
            // Set a boolean or trigger animation to hide the text.
           startTextObject.SetActive(false);
           startText.gameObject.SetActive(false);
        }
    }
}
