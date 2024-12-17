using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Katanna : MonoBehaviour
{
    public float thrownSpeed = 12f;
    public float rotationSpeed = 12f;
    public float throwDistance = 6;
    [SerializeField] private Rigidbody2D katannaBody;
    [SerializeField] private BoxCollider2D katannaCollider;
    [SerializeField] private BoxCollider2D katannaTrigger;
    private Vector2 direction;
    private bool canRotate = true;
    private bool canPickup = false;

    private void Start()
    {
        katannaCollider.enabled = false;
        katannaTrigger.enabled = true;
        StartCoroutine(ThrowDistance());
    }

    public void SetDirection(Vector2 newDir)
    {
        direction = newDir;
    }

    public BoxCollider2D GetKatannaTrigger()
    {
        return katannaTrigger;
    }

    public void DisableKatannaTrigger()
    {
        katannaTrigger.enabled = false;
    }

    private IEnumerator ThrowDistance()
    {

        katannaBody.AddForce(direction * thrownSpeed, ForceMode2D.Impulse);
        katannaTrigger.enabled = false;
        katannaBody.velocity = Vector2.Lerp(katannaBody.velocity, Vector2.zero, 0.8f);

        yield return new WaitForSeconds(0.07f);

        katannaTrigger.enabled = true;

        yield return new WaitForSeconds(0.2f);

        katannaCollider.enabled = true;

        yield return new WaitForSeconds(throwDistance);

        katannaBody.velocity = Vector2.zero;

        yield return null;

        canRotate = false;

        yield return new WaitForSeconds(0.2f);

        canPickup = true;
    }

    private void Update()
    {
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 1f);
        if (canRotate)
        {
            // Assuming rotationSpeed is in degrees per second
            float rotationAmount = rotationSpeed * Time.deltaTime;

            // Create a rotation quaternion that represents the change in rotation around the z-axis
            Quaternion deltaRotation = Quaternion.AngleAxis(rotationAmount, Vector3.forward);

            // Apply the delta rotation to the current rotation
            transform.localRotation = transform.localRotation * deltaRotation;
        }
        else
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Player") && canPickup == true)
        {
            other.gameObject.GetComponent<PlayerController>().hasSword = true;
            Destroy(this.gameObject);
        }
    }

    private Rect GetScreenBounds()
    {
        float cameraHeight = Camera.main.orthographicSize;
        float cameraWidth = cameraHeight * Camera.main.aspect;

        return new Rect(-cameraWidth, -cameraHeight, 2 * cameraWidth, 2 * cameraHeight);
    }

    private void WrapAroundScreen()
    {
        Rect screenBounds = GetScreenBounds();

        // Check if the new position goes beyond the left or right edge
        if (transform.position.x > screenBounds.xMax)
        {
            transform.position = new Vector3(screenBounds.xMin, transform.position.y, transform.position.z);
        }
        else if (transform.position.x < screenBounds.xMin)
        {
            transform.position = new Vector3(screenBounds.xMax, transform.position.y, transform.position.z);
        }

        // Check if the new position goes beyond the top or bottom edge
        if (transform.position.y > screenBounds.yMax)
        {
            transform.position = new Vector3(transform.position.x, screenBounds.yMin, transform.position.z);
        }
        else if (transform.position.y < screenBounds.yMin)
        {
            transform.position = new Vector3(transform.position.x, screenBounds.yMax, transform.position.z);
        }
    }

    private void FixedUpdate()
    {
        WrapAroundScreen();
    }

}
