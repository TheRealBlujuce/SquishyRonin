using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform playerTarget;
    private Transform currentTarget;

    [Header("Follow Settings")]
    public float smoothSpeed = 5f;
    public Vector3 offset;

    [Header("POI Settings")]
    public float activationDistance = 5f;
    public float deactivationDistance = 10f;

    private GameController controllerInstance;

    private void Awake()
    {
        controllerInstance = GameController.gameControllerInstance;
        currentTarget = playerTarget;
    }

    private void LateUpdate()
    {
        if (playerTarget == null ||
            controllerInstance.currentWorldState == GameController.WorldState.ARENA ||
            controllerInstance.currentWorldState == GameController.WorldState.DUNGEON)
            return;

        // If already following POI, check if player is too far to return control
        if (currentTarget != playerTarget)
        {
            float distanceToPOI = Vector2.Distance(playerTarget.position, currentTarget.position);
            if (distanceToPOI > deactivationDistance)
            {
                currentTarget = playerTarget;
            }
        }
        else
        {
            // Look for a nearby POI to switch to
            GameObject[] pois = GameObject.FindGameObjectsWithTag("Point Of Interest");
            Transform closestPOI = null;
            float closestDistance = Mathf.Infinity;

            foreach (GameObject poi in pois)
            {
                float distance = Vector2.Distance(playerTarget.position, poi.transform.position);
                if (distance < activationDistance && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPOI = poi.transform;
                }
            }

            if (closestPOI != null)
            {
                currentTarget = closestPOI;
            }
        }

        // Smooth follow the current target
        Vector3 desiredPosition = currentTarget.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
    }
}
