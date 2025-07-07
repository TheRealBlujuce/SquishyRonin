using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LustSpell : MonoBehaviour
{
    public float spellSpeed = 20f;
    public float maxSpellDistance = 14f;
    public float knockbackForce = 26f;
    [SerializeField] private ParticleSystem spellEffect;
    [SerializeField] private Rigidbody2D spellBody;
    [SerializeField] private BoxCollider2D spellCollider;
    [SerializeField] private BoxCollider2D spellTrigger;
    private Vector2 direction;
    private bool isReturnToSender = false;

    private void Awake()
    {
        spellEffect.Play();
    }
    private void Start()
    {
        spellCollider.enabled = false;
        spellTrigger.enabled = true;
        StartCoroutine(CastSpell());
    }

    public void SetDirection(Vector3 newDir)
    {
        direction = (newDir - transform.position).normalized;
    }

    public BoxCollider2D GetSpellTrigger()
    {
        return spellTrigger;
    }

    public void DisableSpellTrigger()
    {
        spellTrigger.enabled = false;
    }

    public bool GetIsReturnToSender()
    {
        return isReturnToSender;
    }

    private IEnumerator CastSpell()
    {

        spellBody.AddForce(direction * spellSpeed, ForceMode2D.Impulse);
        spellTrigger.enabled = false;
        spellBody.velocity = Vector2.Lerp(spellBody.velocity, Vector2.zero, 0.8f);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        yield return new WaitForSeconds(0.1f);
        
        spellTrigger.enabled = true;
        spellCollider.enabled = true;

        yield return new WaitForSeconds(maxSpellDistance);

        Destroy(this.gameObject);

    }

    private void Update()
    {

    }

    private Rect GetScreenBounds()
    {
        float cameraHeight = Camera.main.orthographicSize;
        float cameraWidth = cameraHeight * Camera.main.aspect;

        return new Rect(-cameraWidth, -cameraHeight, 2 * cameraWidth, 2 * cameraHeight);
    }

    private void DestroyOutOfBounds()
    {
        Rect screenBounds = GetScreenBounds();

        // Check if the new position goes beyond the left or right edge
        if (transform.position.x > screenBounds.xMax)
        {
            Destroy(this.gameObject);
        }
        else if (transform.position.x < screenBounds.xMin)
        {
            Destroy(this.gameObject);
        }

        // Check if the new position goes beyond the top or bottom edge
        if (transform.position.y > screenBounds.yMax)
        {
            Destroy(this.gameObject);
        }
        else if (transform.position.y < screenBounds.yMin)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        // destroy the spell
        Destroy(this.gameObject);
    }

    private void FixedUpdate()
    {
        DestroyOutOfBounds();
    }
}
