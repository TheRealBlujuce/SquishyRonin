using UnityEngine;

public class SilhouetteEnabler : MonoBehaviour
{
    public SilhouetteSpriteRenderer silhouetteRenderer;

	private void Awake()
	{
		silhouetteRenderer = FindFirstObjectByType<SilhouetteSpriteRenderer>();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (silhouetteRenderer != null && other.transform == silhouetteRenderer.playerTransform.parent)
		{
			Debug.Log("Player entered silhouette trigger");
			silhouetteRenderer.SetSilhouetteVisible(true);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (silhouetteRenderer != null && other.transform == silhouetteRenderer.playerTransform.parent)
		{
			Debug.Log("Player exited silhouette trigger");
			silhouetteRenderer.SetSilhouetteVisible(false);
		}
	}

}
