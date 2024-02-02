using UnityEngine;

public class Screenshake : MonoBehaviour
{
    private Vector3 originalCameraPosition;
    private float shakeDuration = 0f;
    public float shakeMagnitude = 0.7f;
    public float dampingSpeed = 1.0f;

    private void Start()
    {
        originalCameraPosition = transform.localPosition;
    }

    void Update()
    {
        if (shakeDuration > 0)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition,  originalCameraPosition + Random.insideUnitSphere * shakeMagnitude, 0.25f);

            shakeDuration -= Time.deltaTime * dampingSpeed;
        }
        else
        {
            shakeDuration = 0f;
            transform.localPosition = originalCameraPosition;
        }
    }

    public void TriggerShake(float duration)
    {
        originalCameraPosition = transform.localPosition;
        shakeDuration = duration+0.5f;
    }
}
