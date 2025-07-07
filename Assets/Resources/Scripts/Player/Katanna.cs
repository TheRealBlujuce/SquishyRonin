using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class Katanna : MonoBehaviour
{
    [Header("Throw Settings")]
    public float thrownSpeed = 12f;
    public float rotationSpeed = 720f; // degrees per second
    public float throwDuration = 6f;
    public bool canCut = true;

    [Header("References")]
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
        StartThrow();
    }

    public void SetDirection(Vector2 newDir) => direction = newDir;

    public BoxCollider2D GetKatannaTrigger() => katannaTrigger;

    public void DisableKatannaTrigger() => katannaTrigger.enabled = false;

    public Rigidbody2D GetKatannaRigidBody() => katannaBody;

    private void StartThrow()
    {
        katannaBody.velocity = direction * thrownSpeed;
        katannaTrigger.enabled = false;
		katannaCollider.enabled = false;

        StartCoroutine(ThrowRoutine());
    }

    private IEnumerator ThrowRoutine()
    {
        yield return new WaitForSeconds(0.15f);
        katannaTrigger.enabled = true;

        yield return new WaitForSeconds(0.2f);
        katannaCollider.enabled = true;
		canCut = true;

        //yield return new WaitForSeconds(throwDuration);
		yield return null;
        katannaBody.velocity = Vector2.zero;
		katannaCollider.enabled = false;
		katannaBody.isKinematic = true;
        canRotate = false;
		canPickup = true;
		canCut = false;
    }

    private void Update()
    {
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 1f);

        if (canRotate)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime, Space.Self);
        }
        else
        {
            transform.localRotation = Quaternion.identity;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (canPickup && other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                player.swordEqipped = true;
            }

            Destroy(gameObject);
        }
    }

	// Optional if you want wraparound back

	private void FixedUpdate()
	{
		if (GameController.gameControllerInstance.currentWorldState == GameController.WorldState.ARENA || SceneManager.GetActiveScene().name != "World_Map")
		{
			WrapAroundScreen();
		}
	}

    private void WrapAroundScreen()
    {
        Rect screenBounds = GetScreenBounds();

        Vector3 pos = transform.position;
        if (pos.x > screenBounds.xMax) pos.x = screenBounds.xMin;
        else if (pos.x < screenBounds.xMin) pos.x = screenBounds.xMax;

        if (pos.y > screenBounds.yMax) pos.y = screenBounds.yMin;
        else if (pos.y < screenBounds.yMin) pos.y = screenBounds.yMax;

        transform.position = pos;
    }

	private Rect GetScreenBounds()
	{
		Camera cam = Camera.main;
		float cameraHeight = cam.orthographicSize;
		float cameraWidth = cameraHeight * cam.aspect;

		Vector2 center = cam.transform.position;

		return new Rect(
			center.x - cameraWidth,
			center.y - cameraHeight,
			2 * cameraWidth,
			2 * cameraHeight
		);
	}


}
