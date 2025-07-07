using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SilhouetteSpriteRenderer : MonoBehaviour
{
    public static List<SilhouetteSpriteRenderer> AllSilhouettes = new();

    [Tooltip("Assign the actor's Transform whose SpriteRenderer we will follow.")]
    public Transform actorTransform;

    private SpriteRenderer silhouetteRenderer;
    private SpriteRenderer playerSpriteRenderer;
    private bool isVisible = false;

    private void Awake()
    {
        silhouetteRenderer = GetComponent<SpriteRenderer>();

        if (actorTransform != null)
            playerSpriteRenderer = actorTransform.GetComponent<SpriteRenderer>();

        silhouetteRenderer.enabled = false; // Start disabled

        // Add self to static list
        if (!AllSilhouettes.Contains(this))
            AllSilhouettes.Add(this);
    }

    private void OnDestroy()
    {
        // Remove self from static list
        if (AllSilhouettes.Contains(this))
            AllSilhouettes.Remove(this);
    }

    private void LateUpdate()
    {
        if (!isVisible || actorTransform == null || playerSpriteRenderer == null)
            return;

        // Sync visuals with actor
        transform.localScale = actorTransform.localScale;
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
