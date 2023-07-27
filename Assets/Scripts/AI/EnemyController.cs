using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float movementSpeed = 3f;
    public float attackRange = 8f;
    public float blockChance = 0.5f;
    public float bounceForce = 5f;
    public float knockbackForce = 5f;
    public GameObject corpsePrefab;
    public GameObject attackIndicator;

    private Transform player;
    private Player playerObject;
    private Rigidbody2D rb;
    private SpriteRenderer actorRenderer;
    private ActorSpriteRenderer actorSpriteRenderer;
    [SerializeField] private GameObject collisionBox;
    Vector2 moveDirection;
    public bool isMoving;
    public bool isAttacking;
    public bool isBlocking;
    public bool isKnockedBack;
    public bool attackSquash;
    public bool isDead;
    private int animCombo = 0;
    private float attackAngle;
    private IEnumerator performAttackCoroutine;
    private IEnumerator performBlockCoroutine;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        actorRenderer = GetComponent<SpriteRenderer>();
        actorSpriteRenderer = GetComponent<ActorSpriteRenderer>();
        playerObject = FindObjectOfType<Player>();
    }

    private void FixedUpdate()
    {
        if ( player != null ) {
            if (playerObject.isDead != true) {
                // Calculate the distance between the enemy and the player
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);

                if (distanceToPlayer >= attackRange)
                {
                    // Move towards the player
                    moveDirection = (player.position - transform.position).normalized;

                    if (!isBlocking)
                    { 
                        rb.velocity = moveDirection * movementSpeed; 
                        isMoving = true;
                        actorSpriteRenderer.attack.StopAnimating();
                        actorSpriteRenderer.run.currentSpriteSet = actorSpriteRenderer.run.spriteSetRun; 
                        if (actorSpriteRenderer.run.isAnimating != true)
                        {
                            actorSpriteRenderer.run.AnimateLoop();
                            actorSpriteRenderer.run.enabled = isMoving;
                        }
                    } 
                    else { isMoving = false; actorSpriteRenderer.run.StopAnimating();}
                    
                    // Rotate the collision box based on movement direction
                    if (!isAttacking && moveDirection != Vector2.zero)
                    {
                        attackAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                        collisionBox.transform.rotation = Quaternion.AngleAxis(attackAngle, Vector3.forward);
                    }
                    
                }
                else
                if (distanceToPlayer <= attackRange || distanceToPlayer < 0.75f){
                   
                    isMoving = false;
                    actorSpriteRenderer.run.StopAnimating();
                    actorSpriteRenderer.run.enabled = isMoving;
                    // Stop moving and perform an attack
                    if (!isAttacking && !isBlocking)
                    {
                        // Perform Attack
                        isAttacking = true;
                        isBlocking = false;
                        StopBlockCoroutine(); // Stop the attack coroutine if it's running
                        performAttackCoroutine = PerformAttack();
                        StartCoroutine(performAttackCoroutine);
                    }
                }
                // Flip the renderer if the player is moving
                if (player.position.x > this.transform.position.x ) { actorRenderer.flipX = false; } else if (player.position.x < this.transform.position.x ) { actorRenderer.flipX = true; }
            
                WrapAroundScreen();
            }
        }
        else
        {
            isMoving = false;
            actorSpriteRenderer.run.StopAnimating();
            actorSpriteRenderer.run.enabled = isMoving;
            rb.velocity = Vector2.zero;
            actorRenderer.sprite = actorSpriteRenderer.idle;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the player's attack hits the enemy
        PlayerController playerController = collision.gameObject.GetComponentInParent<PlayerController>();
        

        if (collision.CompareTag("PlayerAttack") && playerController != null && playerController.isAttacking)
        {
            // Perform block chance check
            if (!isAttacking && Random.value < blockChance)
            {
                if (!isBlocking)
                {
                    isBlocking = true;
                    // Debug.LogWarning("Blocked Player Attack!");
                    StopAttackCoroutine(); // Stop the attack coroutine if it's running
                    performBlockCoroutine = PerformBlock();
                    StartCoroutine(performBlockCoroutine);
                    collision.GetComponentInParent<Rigidbody2D>().velocity = Vector2.zero;
                }
            }
            else
            {
                // Debug.LogWarning("Enemy is Dead!");
                // Destroy the enemy and instantiate a corpse object
                isDead = true;
                collisionBox.SetActive(false);
                GameController.gameControllerInstance.screenshake.TriggerShake(1f);
                StopAttackCoroutine(); // Stop the attack coroutine if it's running
                StopBlockCoroutine(); // position: Stop the Block coroutine if it's running
                var i = Instantiate(corpsePrefab, transform.position, Quaternion.identity);
                i.GetComponent<SpriteRenderer>().flipX = actorRenderer.flipX;

                var renderers = i.GetComponentsInChildren<SpriteRenderer>();
                foreach(SpriteRenderer renderer in renderers)
                {
                    renderer.flipX = actorRenderer.flipX;
                }
                GameController.gameControllerInstance.AddKill();
                
                Destroy(gameObject);
            }
        }
    }




    private IEnumerator PerformAttack()
    {
        if (isAttacking) {
             rb.velocity = Vector2.zero;
            // Calculate the attackMoveDirection based on the stored attackAngle
            Vector2 attackMoveDirection = Quaternion.AngleAxis(attackAngle, Vector3.forward) * Vector2.right;

            switch(animCombo)
            {
                case 0:
                    actorSpriteRenderer.run.StopAnimating();
                    actorSpriteRenderer.attack.currentSpriteSet = actorSpriteRenderer.attack.spriteSetAttackOne; 
                    actorSpriteRenderer.attack.AnimateOnce();
                    actorSpriteRenderer.attack.enabled = isAttacking;
                    animCombo++;
                    if (animCombo > 1){ animCombo = 0; }
                break;
                case 1:
                    actorSpriteRenderer.run.StopAnimating();
                    actorSpriteRenderer.attack.currentSpriteSet = actorSpriteRenderer.attack.spriteSetAttackTwo; 
                    actorSpriteRenderer.attack.AnimateOnce();
                    actorSpriteRenderer.attack.enabled = isAttacking;
                    animCombo++;
                    if (animCombo > 1){ animCombo = 0; }
                break;
            }


            yield return new WaitForSeconds(0.4f);
            
            // create an attack indicator prior to activating the collision
            if (isAttacking && actorSpriteRenderer.attack.frame >= actorSpriteRenderer.attack.currentSpriteSet.Length-6)
            {
                Vector2 indicatorSpawnPos = new Vector2(transform.position.x, transform.position.y + 0.25f);
                var i = Instantiate(attackIndicator, indicatorSpawnPos, Quaternion.identity);
                i.transform.parent = this.transform;
            }
            else
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);

            if (isAttacking)
            {
                rb.velocity = Vector2.zero;
                rb.AddForce(attackMoveDirection * knockbackForce, ForceMode2D.Impulse);
                attackSquash = true;
            }

            // activate the collision box
            if (actorSpriteRenderer.attack.frame >= actorSpriteRenderer.attack.currentSpriteSet.Length-4)
            {
                // Debug.Log("Trigger On!");
                collisionBox.SetActive(true);
            }

            yield return new WaitForSeconds(0.15f);

            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.8f);
            attackSquash = false;
            collisionBox.SetActive(false);
            
            yield return new WaitForSeconds(0.5f);
            
            rb.velocity = Vector2.zero;
            actorSpriteRenderer.attack.StopAnimating();

            if (actorSpriteRenderer.attack.isAnimating == false)
            {
                //Debug.Log("No longer Attacking!");
                actorSpriteRenderer.attack.frame = 0;

                isAttacking = false;
                actorSpriteRenderer.attack.enabled = isAttacking;
            }
            else
            {
                // Debug.Log(actorSpriteRenderer.attack.isAnimating);
            }
        }
        else
        {
            attackSquash = false;
            yield break;
        }

    }
    private IEnumerator PerformBlock()
    {
        //Debug.Log("Is Blocking!");
        actorSpriteRenderer.run.StopAnimating();
        actorSpriteRenderer.attack.StopAnimating();
        actorSpriteRenderer.parry.currentSpriteSet = actorSpriteRenderer.parry.spriteSetParry; 
        actorSpriteRenderer.parry.AnimateOnce();
        actorSpriteRenderer.parry.enabled = isBlocking;

        Vector2 knockbackMoveDirection = (transform.position - player.position).normalized;
        ApplyKnockback(knockbackMoveDirection, knockbackForce);

        yield return new WaitForSeconds(0.3f);

        if(isKnockedBack)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.8f);
        }

        yield return new WaitForSeconds(0.2f);

        if(isKnockedBack){ isKnockedBack = false; }
        actorSpriteRenderer.parry.StopAnimating();

        if (actorSpriteRenderer.parry.isAnimating == false)
        {
            //Debug.Log("No longer Blocking!");
            actorSpriteRenderer.parry.frame = 0;
            // actorSpriteRenderer.attack.StopAnimating();

            isBlocking = false;
            actorSpriteRenderer.parry.enabled = isBlocking;
        }
        else
        {
            // Debug.Log(actorSpriteRenderer.parry.isAnimating);
        }

    }   

    private void StopAttackCoroutine()
    {
        if (performAttackCoroutine != null)
        {
            StopCoroutine(performAttackCoroutine);
            performAttackCoroutine = null;
        }
    }

    private void StopBlockCoroutine()
    {
        if (performBlockCoroutine != null)
        {
            StopCoroutine(performBlockCoroutine);
            performBlockCoroutine = null;
        }
    }

     public void ApplyKnockback(Vector2 knockbackDirection, float knockbackForce)
    {
        // Debug.LogWarning("Applying Knockback on Enemy!");
        isKnockedBack = true;
        rb.velocity = Vector2.zero;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
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


}
