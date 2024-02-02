using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Variables")]
    public float movementSpeed = 5f;
    public float runningSpeedMultiplier = 1.5f;
    public float attackMovementSpeedMultiplier = 1.75f;
    public float knockbackForce = 5f;

    [Header("Player Input")]
    [SerializeField] public Player player;
    [SerializeField] private PlayerInput currentGameInput;
    [SerializeField] private Vector2 movementInput;
    [SerializeField] private Vector2 previousMovementInput;
    [SerializeField] private Vector2 movementVelocity;

    public bool isMoving;
    public bool isRunning;
    public bool isAttacking;
    public bool isInteracting;
    public bool isBlocking;
    public bool isRolling;
    public bool canBeHit = true; // this is to apply I-Frames to the player  when rolling
    public bool attackSquash;
    public int animCombo = 0;
    public bool isBlockParryTiming;
    public bool isKnockedBack;

    private float attackAngle;
    private Vector2 movementDirection;
    private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer actorRenderer;
    [SerializeField] private GameObject collisionHitBox;
    [SerializeField] private BoxCollider2D collisionTriggerBox;
    [SerializeField] private ActorSpriteRenderer actorSpriteRenderer;
    public GameObject corpsePrefab;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentGameInput = GameController.gameControllerInstance.gameInput;
        actorRenderer = GetComponentInChildren<SpriteRenderer>();
        player = GetComponent<Player>();
        
    }

    private void Update()
    {
        // Read movement input from the Input System
        movementInput = currentGameInput.PlayerMovement.Movement.ReadValue<Vector2>().normalized;
        
        // Read moving and running input from the Input System
        if (movementInput.x != 0 || movementInput.y != 0) { isMoving = true; } else { isMoving = false; }

        // Flip the renderer if the player is moving
        if (!isAttacking && movementInput.x > 0 ) { actorRenderer.flipX = false; } else if (!isAttacking && movementInput.x < 0 ) { actorRenderer.flipX = true; }
        
        // Rotate the collision box based on movement direction
        if ( !isAttacking && movementVelocity != Vector2.zero)
        {
            attackAngle = Mathf.Atan2(movementVelocity.y, movementVelocity.x) * Mathf.Rad2Deg;
            collisionHitBox.transform.rotation = Quaternion.Lerp(collisionHitBox.transform.rotation, Quaternion.AngleAxis(attackAngle, Vector3.forward), 32f * Time.deltaTime);
        }

        // Read attack input from the Input System
        if (currentGameInput.PlayerMovement.Attack.triggered)
        {
            if (!isAttacking && !isBlocking && !isRolling)
            {
                isAttacking = true;
                StartCoroutine(PerformAttack());
            }
   
        }

        // Read attack input from the Input System
        if (currentGameInput.PlayerMovement.Run.triggered)
        {
            if (!isRolling && !isRunning)
            {
                isRolling = true;
                StartCoroutine(PerformRoll());
            }
   
        }

        if (!currentGameInput.PlayerMovement.Run.IsPressed() && isRunning && !isRolling)
        {
            isRunning = false;
        }

        // Read block input from the Input System
        if (currentGameInput.PlayerMovement.Block.triggered)
        {
            if (!isBlocking)
            {
                isBlocking = true;
                isAttacking = false;
                StartCoroutine(PerformBlock());
            }
   
        }


        
        // Check if the player is in the block parry timing window
        if (isBlocking && actorRenderer.sprite == actorSpriteRenderer.parry.spriteSetParry[3] )
        {
            isBlockParryTiming = true;
        }
        else
        {
            isBlockParryTiming = false;
        }
    }

    private void FixedUpdate()
    {
        // Calculate movement velocity
        movementVelocity = movementInput.normalized * movementSpeed;
        movementDirection = rb.velocity.normalized;
        // Apply running speed multiplier if running
        if (isRunning)
        {
            movementVelocity *= runningSpeedMultiplier;
        }

        // Apply attack movement velocity if attacking
        // if (isAttacking)
        // {
        //     movementVelocity *= attackMovementSpeedMultiplier;
        // }

        if (!isAttacking && !isBlocking && !isRolling)
        {
            // Apply movement velocity to the rigidbody
            rb.velocity = movementVelocity;
        }

        WrapAroundScreen();
        
    }

    public bool CheckIsMoving()
    {
        return isMoving;
    }
    public bool CheckIsRunning()
    {
        return isRunning;
    }
    public bool CheckIsBlocking()
    {
        return isBlocking;
    }
    public bool CheckIsAttacking()
    {
        return isAttacking;
    }
    public bool CheckIsRolling()
    {
        return isRolling;
    }
    public PlayerInput GetPlayerInput()
    {
        return currentGameInput;
    }

    private IEnumerator PerformAttack()
    {
        // Calculate the attackMoveDirection based on the stored attackAngle
        Vector2 attackMoveDirection = Quaternion.AngleAxis(attackAngle, Vector3.forward) * Vector2.right;

        rb.velocity = Vector2.zero;
        rb.AddForce(attackMoveDirection * knockbackForce, ForceMode2D.Impulse);
        attackSquash = true;

        actorSpriteRenderer.run.StopAnimating();
        actorSpriteRenderer.roll.StopAnimating();

        switch(animCombo)
        {
            case 0:
                actorSpriteRenderer.attack.currentSpriteSet = actorSpriteRenderer.attack.spriteSetAttackOne; 
                actorSpriteRenderer.attack.AnimateOnce();
                actorSpriteRenderer.attack.enabled = isAttacking;
                animCombo++;
                collisionHitBox.GetComponentInChildren<BoxCollider2D>().enabled = true;
                collisionHitBox.GetComponentInChildren<SpriteRenderer>().enabled = true;
                collisionHitBox.GetComponentInChildren<BoxCollider2D>().isTrigger = true;
                if (animCombo > 1){ animCombo = 0; }
            break;
            case 1:
                actorSpriteRenderer.attack.currentSpriteSet = actorSpriteRenderer.attack.spriteSetAttackTwo; 
                actorSpriteRenderer.attack.AnimateOnce();
                actorSpriteRenderer.attack.enabled = isAttacking;
                animCombo++;
                collisionHitBox.GetComponentInChildren<BoxCollider2D>().enabled = true;
                collisionHitBox.GetComponentInChildren<SpriteRenderer>().enabled = true;
                collisionHitBox.GetComponentInChildren<BoxCollider2D>().isTrigger = true;
                if (animCombo > 1){ animCombo = 0; }
            break;
        }

        yield return new WaitForSeconds(0.1f);
        
        if (!isRolling)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.8f);
        }
        // Hitbox Animation
        yield return new WaitForSeconds(0.025f);
        // Wait for a second frame
        yield return new WaitForEndOfFrame();

        collisionHitBox.GetComponentInChildren<ActorSpriteRenderer>().run.StopAnimating();  
        
        if (collisionHitBox.GetComponentInChildren<ActorSpriteRenderer>().run.isAnimating != true)
        {
            collisionHitBox.GetComponentInChildren<ActorSpriteRenderer>().run.enabled = false;
            collisionHitBox.GetComponentInChildren<ActorSpriteRenderer>().run.frame = 0;
            collisionHitBox.GetComponentInChildren<SpriteRenderer>().enabled = false;
        }

        // End Squash
        attackSquash = false;

        yield return new WaitForSeconds(0.3f);

        if (!isRolling)
        {
            rb.velocity = Vector2.zero; 
        }

        actorSpriteRenderer.attack.StopAnimating();

        if (actorSpriteRenderer.attack.isAnimating == false)
        {
            //Debug.Log("No longer Attacking!");
            actorSpriteRenderer.attack.frame = 0;
            collisionHitBox.GetComponentInChildren<BoxCollider2D>().enabled = false;
            collisionHitBox.GetComponentInChildren<BoxCollider2D>().isTrigger = false;
            
            isAttacking = false;
            actorSpriteRenderer.attack.enabled = isAttacking;
        }
        else
        {
            // Debug.Log(actorSpriteRenderer.attack.isAnimating);
        }

    }

    private IEnumerator PerformRoll()
    {
        canBeHit = false;
        // Calculate the attackMoveDirection based on the stored attackAngle
        Vector2 rollMoveDirection = Quaternion.AngleAxis(attackAngle, Vector3.forward) * Vector2.right;
        
        // stop any other animations
        actorSpriteRenderer.run.StopAnimating();
        actorSpriteRenderer.attack.StopAnimating();
        actorSpriteRenderer.parry.StopAnimating();

        // temporarily disable the collision box so that the player can go through enemies.
        collisionTriggerBox.enabled = false;

        // play the roll animation
        actorSpriteRenderer.roll.currentSpriteSet = actorSpriteRenderer.roll.spriteSetRun; 
        actorSpriteRenderer.roll.frame = 0;
        actorSpriteRenderer.roll.AnimateOnce();
        actorSpriteRenderer.roll.enabled = isRolling;

        // apply force to the rb
        rb.velocity = Vector2.zero;
        rb.AddForce(rollMoveDirection * (knockbackForce + movementSpeed), ForceMode2D.Impulse);
        attackSquash = true;

        yield return new WaitForSeconds(0.2f);
        
        rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.6f);
        collisionTriggerBox.enabled = true;
        
        yield return new WaitForSeconds(0.125f);

        attackSquash = false;

        yield return new WaitForSeconds(0.2f);

        actorSpriteRenderer.roll.StopAnimating();

        if (actorSpriteRenderer.roll.isAnimating == false)
        {
            //Debug.Log("No longer Attacking!");
            
            rb.velocity = Vector2.zero;
            isRolling = false;
            actorSpriteRenderer.roll.enabled = isRolling;
            // Re-enable the collision box and set the player to running and reset the canBeHit flag.
            isRunning = true;
            canBeHit = true;
        }
        else
        {
            // Debug.Log(actorSpriteRenderer.attack.isAnimating);
        }

    }

    private IEnumerator PerformBlock()
    {
        
        //Debug.Log("Is Blocking!");
        rb.velocity = Vector2.zero;
        actorSpriteRenderer.run.StopAnimating();
        actorSpriteRenderer.attack.StopAnimating();
        actorSpriteRenderer.parry.currentSpriteSet = actorSpriteRenderer.parry.spriteSetParry; 
        actorSpriteRenderer.parry.AnimateOnce();
        actorSpriteRenderer.parry.enabled = isBlocking;

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

    public void ApplyKnockback(Vector2 knockbackDirection, float knockbackForce)
    {
        isKnockedBack = true;
        // Debug.LogWarning("Applying Knockback on Player!");
        rb.velocity = Vector2.zero;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the player's attack hits the enemy
        if (collision.CompareTag("EnemyAttack"))
        {
            EnemyController enemyController = collision.gameObject.GetComponentInParent<EnemyController>();
            Vector2 knockbackMoveDirection = (transform.position - enemyController.transform.position).normalized;

            if (enemyController != null && enemyController.isAttacking)
            {
                if (isBlocking && isBlockParryTiming || isBlocking)
                {
                    // Debug.Log("Blocked Enemy Attack!");
                    // collision.enabled = false;
                    ApplyKnockback(knockbackMoveDirection, knockbackForce);
                }
                else if (!isBlocking && !isBlockParryTiming && canBeHit)
                {
                    // Debug.LogWarning("Player is Dead!");
                    // Destroy the player and instantiate a corpse object
                    player.isDead = true;
                    GameController.gameControllerInstance.EndGame();
                    Instantiate(corpsePrefab, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.LogError("Error. Enemy not attacking or controller does not exist");
            }
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

}
