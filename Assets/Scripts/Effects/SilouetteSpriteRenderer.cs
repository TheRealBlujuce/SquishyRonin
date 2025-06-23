using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SilhouetteSpriteRenderer : MonoBehaviour
{
    [Tooltip("Assign the player's Transform whose SpriteRenderer we will follow.")]
    public Transform playerTransform;

    private SpriteRenderer silhouetteRenderer;
    private SpriteRenderer playerSpriteRenderer;
    private bool isVisible = false;

    private void Awake()
    {
        silhouetteRenderer = GetComponent<SpriteRenderer>();

        if (playerTransform != null)
            playerSpriteRenderer = playerTransform.GetComponent<SpriteRenderer>();

        silhouetteRenderer.enabled = false; // Start disabled
    }

    private void LateUpdate()
    {
        if (!isVisible || playerTransform == null || playerSpriteRenderer == null)
            return;

        // Sync visuals with player
        transform.localScale = playerTransform.localScale;
        silhouetteRenderer.sprite = playerSpriteRenderer.sprite;
        silhouetteRenderer.flipX = playerSpriteRenderer.flipX;
    }

    public void SetSilhouetteVisible(bool visible)
    {
        isVisible = visible;
        if (silhouetteRenderer != null)
            silhouetteRenderer.enabled = visible;
    }
}
