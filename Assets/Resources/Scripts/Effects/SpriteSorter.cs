using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSorter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float lastYPosition = float.MinValue;

    [SerializeField] private int sortingMultiplier = 100;
    [SerializeField] private float zOffset = 0f;
    public bool staticDepth = false;

	void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();

		// Ensure we use a unique instance of the material (avoid shared reference)
		spriteRenderer.material = new Material(spriteRenderer.material);
	}


    void LateUpdate()
    {
        if (!staticDepth)
        {
            float y = transform.position.y;

            // 1. Update sorting order based on Y position
            int order = -(int)(y * sortingMultiplier);
            if (Mathf.Abs(y - lastYPosition) > 0.001f)
            {
                spriteRenderer.sortingOrder = order;
                lastYPosition = y;
            }

            // 2. Optional: update material.renderQueue to reflect sorting order
            // RenderQueue = 3000 (Transparent) + order to maintain global render consistency
            if (spriteRenderer.material != null)
            {
                spriteRenderer.material.renderQueue = 3000 + spriteRenderer.sortingOrder;
            }

            // 3. Set Z to 0 for 2D, use yOffset only if needed
            Vector3 pos = transform.position;
            pos.z = zOffset;
            transform.position = pos;
        }
        else
        {
            // Static objects maintain fixed z and queue
            Vector3 pos = transform.position;
            pos.z = zOffset;
            transform.position = pos;

            if (spriteRenderer.material != null)
            {
                spriteRenderer.material.renderQueue = 3000 + spriteRenderer.sortingOrder;
            }
        }
    }
}
