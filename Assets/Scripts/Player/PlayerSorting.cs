using UnityEngine;

public class PlayerSorting : MonoBehaviour
{
    public string foregroundSortingLayerName = "Foreground";
    private SpriteRenderer playerSpriteRenderer;

    private void Awake()
    {
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Get the player's y position
        float playerY = transform.position.y;

        // Find the sorting layer index of the foreground sorting layer
        int foregroundSortingLayerIndex = SortingLayer.NameToID(foregroundSortingLayerName);

        // Get the sorting layer index of the player's sprite
        int playerSortingLayerIndex = playerSpriteRenderer.sortingLayerID;

        // Determine if the player is in front of the foreground sorting layer
        bool isPlayerInFront = playerY > 0f; // Replace 0f with the y position of your foreground sprites

        // Set the sorting order based on the player's position relative to the foreground sorting layer
        if (isPlayerInFront)
        {
            playerSpriteRenderer.sortingOrder = foregroundSortingLayerIndex + 1;
        }
        else
        {
            playerSpriteRenderer.sortingOrder = foregroundSortingLayerIndex - 1;
        }
    }
}
