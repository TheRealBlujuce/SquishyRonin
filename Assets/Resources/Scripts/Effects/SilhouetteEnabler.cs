using UnityEngine;

public class SilhouetteEnabler : MonoBehaviour
{
    private void SetSilhouetteVisibleForCollider(Transform otherTransform, bool visible)
    {
        foreach (var silhouette in SilhouetteSpriteRenderer.AllSilhouettes)
        {
            if (otherTransform == silhouette.actorTransform.parent)
            {
                //Debug.Log($"{otherTransform.name} {(visible ? "entered" : "exited")} silhouette trigger");
                silhouette.SetSilhouetteVisible(visible);
                break;  // Only one silhouette per transform expected
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        SetSilhouetteVisibleForCollider(other.transform, true);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        SetSilhouetteVisibleForCollider(other.transform, true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        SetSilhouetteVisibleForCollider(other.transform, false);
    }
}
