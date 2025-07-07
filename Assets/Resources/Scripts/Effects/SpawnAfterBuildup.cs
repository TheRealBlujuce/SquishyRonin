using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class SpawnAfterBuildup : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem spawnEffect;
    public GameObject enemyPrefab;
    public Transform spawnLocation;
    public SpriteRenderer buildupSprite;
    public Light2D spawnLight;

    [Header("Buildup Settings")]
    public float fadeDuration = 1.5f;
    public float targetLightIntensity = 2f;

    [Header("Optional")]
    public float delayAfterEffect = 0f;
    public bool destroyAfterSpawn = true;

	private Player player;

    private void Start()
    {
		player = FindFirstObjectByType<Player>();

		if (player == null ) {Destroy(gameObject);}

        if (spawnEffect == null || enemyPrefab == null || buildupSprite == null || spawnLight == null)
        {
            Debug.LogError("SpawnAfterBuildUp is missing one or more required references.");
            return;
        }

        // Initialize values
        Color spriteColor = buildupSprite.color;
        spriteColor.a = 0f;
        buildupSprite.color = spriteColor;
        spawnLight.intensity = 0f;

        StartCoroutine(HandleSpawnSequence());
    }

    private IEnumerator HandleSpawnSequence()
    {
        float timer = 0f;

        // Step 1: Fade in sprite and light together
        while (timer < fadeDuration)
        {
            float t = timer / fadeDuration;

            // Update sprite opacity
            Color color = buildupSprite.color;
            color.a = Mathf.Lerp(0f, 1f, t);
            buildupSprite.color = color;

            // Update light intensity
            spawnLight.intensity = Mathf.Lerp(0f, targetLightIntensity, t);

            timer += Time.deltaTime;
            yield return null;
        }

        // Ensure final values
        Color finalColor = buildupSprite.color;
        finalColor.a = 1f;
        buildupSprite.color = finalColor;
        spawnLight.intensity = targetLightIntensity;

        // Step 2: Play particle effect
        spawnEffect.Play();

        // Step 3: Spawn the enemy
        Instantiate(enemyPrefab, spawnLocation.position, Quaternion.identity);

        // Step 4: Wait a bit before fading out
        yield return new WaitForSeconds(delayAfterEffect + 0.4f);

        // Step 5: Fade out sprite and light
        timer = 0f;
        while (timer < fadeDuration)
        {
            float t = timer / fadeDuration;

            // Fade out sprite opacity
            Color color = buildupSprite.color;
            color.a = Mathf.Lerp(1f, 0f, t);
            buildupSprite.color = color;

            // Fade out light intensity
            spawnLight.intensity = Mathf.Lerp(targetLightIntensity, 0f, t);

            timer += Time.deltaTime;
            yield return null;
        }

        // Final cleanup
        buildupSprite.color = new Color(buildupSprite.color.r, buildupSprite.color.g, buildupSprite.color.b, 0f);
        spawnLight.intensity = 0f;

        if (destroyAfterSpawn)
            Destroy(gameObject);
    }

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.transform.CompareTag("Collision"))
		Destroy(gameObject);

	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.transform.CompareTag("Collision"))
		Destroy(gameObject);
	}

}
