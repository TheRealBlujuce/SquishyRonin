using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public EnemyAiType.EnemyType enemytype = EnemyAiType.EnemyType.WRATH;
    [SerializeField] EnemyAiType enemyAiType;
    public float movementSpeed = 3f;
    public float attackRange = 8f;
    public float blockChance = 0.5f;
    public float bounceForce = 5f;
    public float knockbackForce = 5f;
    public float prideTeleportCooldown = 5f;
    public float prideCooldownTimer = 0f;
    public GameObject corpsePrefab;
    public GameObject attackIndicator;

    private Transform player;
    private Player playerObject;
    private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer actorRenderer;
    private ActorSpriteRenderer actorSpriteRenderer;
    [SerializeField] private GameObject collisionBox;
    [SerializeField] private GameObject spellObject;
    Vector2 moveDirection;
    public bool isMoving;
    public bool isAttacking;
    public bool isShooting;
    public bool isBlocking;
    public bool isKnockedBack;
    public bool prideCanTeleport;
    public bool prideHasTeleported;
    public bool attackSquash;
    public bool isDead;
    public bool applyKnockback;
    private int animCombo = 0;
    private float attackAngle;
    private IEnumerator performAttackCoroutine;
    private IEnumerator performBlockCoroutine;
    private IEnumerator performShootCoroutine;

    [Header("Teleportation Variables")]
    [SerializeField] private Vector2 areaCenter;     // Center of the area where the AI can teleport
    [SerializeField] private float teleportRadius;   // Radius of the teleport area
    [SerializeField] private float checkRadius;      // Radius for checking obstacles
    [SerializeField] private LayerMask obstacleLayer; // Layer that defines what is considered an obstacle
    [SerializeField] private float teleportCooldown = 5f;  // Time between teleports


    private bool isTeleporting = false;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        actorSpriteRenderer = GetComponentInChildren<ActorSpriteRenderer>();
        playerObject = FindFirstObjectByType<Player>();
    }

    private void Start()
    {
        moveDirection = (player.position - transform.position).normalized;
    }

    private void FixedUpdate()
    {
        if ( player != null ) {
            if (playerObject.isDead != true) {
                
                switch(enemytype)
                {
                    case EnemyAiType.EnemyType.WRATH:
                    case EnemyAiType.EnemyType.GREED:
                        MeleeAttackCalculation();
                    break;
                    case EnemyAiType.EnemyType.LUST:
                        RangedAttackCalculation();
                    break;
                    case EnemyAiType.EnemyType.PRIDE:
                        MeleeRangeAttackCalculation();
                    break;
                }

				if (GameController.gameControllerInstance.currentWorldState == GameController.WorldState.ARENA) 
				{
					WrapAroundScreen();
				}
                
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

    private void Update()
    {
        if (player != null)
        {
            if (enemytype == EnemyAiType.EnemyType.LUST || enemytype == EnemyAiType.EnemyType.PRIDE) { areaCenter = player.localPosition; }

            if (enemytype == EnemyAiType.EnemyType.PRIDE) { PrideTeleportCooldown(); }

            // Flip the renderer if the player is moving
            if (player.position.x > this.transform.position.x ) { actorRenderer.flipX = false; } 
            else 
            if (player.position.x < this.transform.position.x ) { actorRenderer.flipX = true; }
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the player's attack hits the enemy
        PlayerController playerController = collision.gameObject.GetComponentInParent<PlayerController>();
        Katanna playerKatanna = collision.gameObject.GetComponentInParent<Katanna>();
        LustSpell lustSpell = collision.gameObject.GetComponentInParent<LustSpell>();

        if (collision.CompareTag("PlayerAttack") && playerController != null && playerController.isAttacking && !lustSpell)
        {
            // Perform block chance check
            if (!isAttacking && Random.value < blockChance && enemytype == EnemyAiType.EnemyType.WRATH)
            {
                if (!isBlocking)
                {
                    applyKnockback = true;
                    isBlocking = true;
                    // Debug.LogWarning("Blocked Player Attack!");
                    StopAttackCoroutine(); // Stop the attack coroutine if it's running
                    performBlockCoroutine = PerformBlock();
                    StartCoroutine(performBlockCoroutine);
                    collision.GetComponentInParent<Rigidbody2D>().velocity = Vector2.zero;
                }
            }
            
            if(!isBlocking)
            {
                // Debug.LogWarning("Enemy is Dead!");
                // Destroy the enemy and instantiate a corpse object
                isDead = true;
                if(collisionBox != null) { collisionBox.SetActive(false); }
                GameController.gameControllerInstance.screenshake.TriggerShake(1f);
                GameController.gameControllerInstance.PlayCutSound();
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
        else
        if (playerKatanna != null)
        {
            // Debug.LogWarning("Enemy is Dead. Died to Katanna!");
            // Destroy the enemy and instantiate a corpse object
            isDead = true;
            playerKatanna.DisableKatannaTrigger();
            if(collisionBox != null) { collisionBox.SetActive(false); }
            GameController.gameControllerInstance.screenshake.TriggerShake(1f);
            GameController.gameControllerInstance.PlayCutSound();
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

    private IEnumerator PerformAttack()
    {
        if (isAttacking) {
             rb.velocity = Vector2.zero;
            // Calculate the attackMoveDirection based on the stored attackAngle
            Vector2 attackDirection = (player.position - transform.position).normalized;
			attackAngle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
			Vector2 attackMoveDirection = attackDirection;


            switch (enemytype)
            {
                case EnemyAiType.EnemyType.WRATH:
                    PlayAngerAttackAnimation();
                break;
                case EnemyAiType.EnemyType.GREED:
                    PlayDefaultAttackAnimation();
                break;
                case EnemyAiType.EnemyType.LUST:
                    PlayRangedAttackAnimation();
                break;
                case EnemyAiType.EnemyType.PRIDE:
                    if (isShooting)
                    {
                        PlayPrideRangedAttackAnimation();
                    }
                    else
                    {
                        PlayDefaultAttackAnimation();
                    }
                break;
            }

            float waitLength = (actorSpriteRenderer.attack.currentSpriteSet.Length / 2f ) / 10;

            yield return new WaitForSeconds(waitLength);
            
            // create an attack indicator prior to activating the collision
            if (actorSpriteRenderer.attack.frame >= actorSpriteRenderer.attack.currentSpriteSet.Length/2)
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

            // If the enemy is a spellcaster, instantiate the spell object
            if (enemytype == EnemyAiType.EnemyType.LUST || enemytype == EnemyAiType.EnemyType.PRIDE && isShooting == true)
            {
                if (!playerObject.isDead || player != null)
                {
                    GameObject s = Instantiate(spellObject, transform.position, Quaternion.identity);
                    s.GetComponent<LustSpell>().SetDirection(player.position);
                }
            }
            // apply squash
            if (isAttacking && !isShooting && enemyAiType.GetCanDash() )
            {
                rb.velocity = Vector2.zero;
                rb.AddForce(attackMoveDirection * knockbackForce, ForceMode2D.Impulse);
                attackSquash = true;
            }

            // activate the collision box
            if (!isShooting && actorSpriteRenderer.attack.frame >= actorSpriteRenderer.attack.currentSpriteSet.Length-4 && collisionBox != null)
            {
                // Debug.Log("Trigger On!");
                collisionBox.GetComponentInChildren<BoxCollider2D>().enabled = true;
                collisionBox.GetComponentInChildren<SpriteRenderer>().enabled = true;
            }

            yield return new WaitForSeconds(0.15f);

            // end squash
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.8f);

            attackSquash = false;
            
            // turn off collider
            if(collisionBox != null)
            {
                collisionBox.GetComponentInChildren<ActorSpriteRenderer>().run.StopAnimating();  

                if (collisionBox.GetComponentInChildren<ActorSpriteRenderer>().run.isAnimating != true)
                {
                    collisionBox.GetComponentInChildren<ActorSpriteRenderer>().run.enabled = false;
                    collisionBox.GetComponentInChildren<ActorSpriteRenderer>().run.frame = 0;
                    collisionBox.GetComponentInChildren<SpriteRenderer>().enabled = false;
                    collisionBox.GetComponentInChildren<BoxCollider2D>().enabled = false;
                    collisionBox.GetComponentInChildren<SpriteRenderer>().enabled = false;
                }
            }
  

            yield return null;
            
            rb.velocity = Vector2.zero;

            yield return new WaitForSeconds(0.5f);
            
            actorSpriteRenderer.attack.StopAnimating();

            if (actorSpriteRenderer.attack.isAnimating == false)
            {
                //Debug.Log("No longer Attacking!");
                actorSpriteRenderer.attack.frame = 0;
                isAttacking = false;
                isShooting = false;
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
        if (enemytype != EnemyAiType.EnemyType.WRATH)
        {
            isBlocking = false;
            applyKnockback = false;
            yield break;
        }

        if (isBlocking)
        {
            //Debug.Log("Is Blocking!");
            actorSpriteRenderer.run.StopAnimating();
            actorSpriteRenderer.attack.StopAnimating();
            actorSpriteRenderer.parry.currentSpriteSet = actorSpriteRenderer.parry.spriteSetParry; 
            actorSpriteRenderer.parry.AnimateOnce();
            actorSpriteRenderer.parry.enabled = isBlocking;

            if(applyKnockback == true)
            {
            Vector2 knockbackMoveDirection = (transform.position - player.position).normalized;
            ApplyKnockback(knockbackMoveDirection, knockbackForce);
            }

            yield return new WaitForSeconds(0.3f);

            if(isKnockedBack)
            {
                rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.8f);
            }

            yield return new WaitForSeconds(0.2f);

            if(isKnockedBack && applyKnockback){ isKnockedBack = false; applyKnockback = false; }
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
        else
        {
            yield break;
        }

    }   

    private IEnumerator PerformTeleportMove()
    {
        if ( enemytype == EnemyAiType.EnemyType.LUST ) { PlayTeleportAnimation(); }
        if ( enemytype == EnemyAiType.EnemyType.PRIDE ) { PlayPrideTeleportAnimation(); }

        yield return new WaitForSeconds(0.25f);

        actorSpriteRenderer.run.frame = actorSpriteRenderer.run.currentSpriteSet.Length-1;

        yield return new WaitForSeconds(0.05f);

        if (!isTeleporting)
        {
            isTeleporting = true;
            Vector3 randomPosition = GetRandomPosition();

            // Check for obstacles and retry if necessary
            while (Physics2D.OverlapCircle(randomPosition, checkRadius, obstacleLayer))
            {
                randomPosition = GetRandomPosition();
                yield return null; // Wait for the next frame to avoid freezing
            }

            // Teleport the AI to the valid position
            transform.position = randomPosition;

            // Wait for cooldown before the next teleport
            yield return new WaitForSeconds(teleportCooldown);
            isTeleporting = false;
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

    private void MeleeAttackCalculation()
    {
        // Calculate the distance between the enemy and the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer >= attackRange)
        {
            // Move towards the player
            moveDirection = (player.position - transform.position).normalized;

            if (!isAttacking && !isBlocking)
            { 
                rb.velocity = moveDirection * movementSpeed; 
                isMoving = true;
                actorSpriteRenderer.attack.StopAnimating();
                actorSpriteRenderer.run.currentSpriteSet = actorSpriteRenderer.run.spriteSetRunOne; 
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
        if (distanceToPlayer <= attackRange){
            
            isMoving = false;
            actorSpriteRenderer.run.StopAnimating();
            actorSpriteRenderer.run.enabled = isMoving;
            // Stop moving and perform an attack
            if (!isAttacking && !isBlocking)
            {
                // have a small chance to block when in range, or attack
                if (Random.value < blockChance/3 && enemytype == EnemyAiType.EnemyType.WRATH)
                {
                    // Perform Block
                    isAttacking = false;
                    isBlocking = true;
                    StopAttackCoroutine(); // Stop the attack coroutine if it's running
                    performBlockCoroutine = PerformBlock();
                    StartCoroutine(performBlockCoroutine);
                }
                else
                {
                    // Perform Attack
                    isAttacking = true;
                    isBlocking = false;
                    StopBlockCoroutine(); // Stop the attack coroutine if it's running
                    performAttackCoroutine = PerformAttack();
                    StartCoroutine(performAttackCoroutine);
                }
                
            }
        }
    }

    private void RangedAttackCalculation()
    {
        // Calculate the distance between the enemy and the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer >= attackRange || distanceToPlayer < attackRange-6f)
        {
            // Move towards the player unless the AI can teleport. If they can, teleport instead.

            if (enemyAiType.GetCanTeleport() == true)
            {
                StartCoroutine(PerformTeleportMove());
            }
            else
            {
                moveDirection = (player.position - transform.position).normalized;
            }

            if (!isAttacking && !isBlocking)
            { 
                rb.velocity = moveDirection * movementSpeed; 
                isMoving = true;
                actorSpriteRenderer.attack.StopAnimating();
                actorSpriteRenderer.run.currentSpriteSet = actorSpriteRenderer.run.spriteSetRunOne; 
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
            }

        }
        else
        if (distanceToPlayer <= attackRange){
            
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
    }

    // Function to generate a random position within the defined area for enemies that teleport.
    private Vector3 GetRandomPosition()
    {
        float randomX = Random.Range(-teleportRadius, teleportRadius);
        float randomY = Random.Range(-teleportRadius, teleportRadius);
        Vector2 randomPosition = new Vector2(randomX, randomY) + areaCenter;

        // You can modify the Y value to match your terrain or set it dynamically

        return randomPosition;
    }

    private void MeleeRangeAttackCalculation()
    {
        // Calculate the distance between the enemy and the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer >= attackRange)
        {
            // Move towards the player
            if (enemyAiType.GetCanTeleport() == true && prideCanTeleport == true)
            {
                prideCanTeleport = false;
                prideHasTeleported = true;
                StartCoroutine(PerformTeleportMove());
            }
            else
            {
                moveDirection = (player.position - transform.position).normalized;
            }

            if (!isAttacking && !isShooting)
            { 
                rb.velocity = moveDirection * movementSpeed; 
                isMoving = true;
                PlayDefaultWalkAnimation();
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
        if (distanceToPlayer <= attackRange){
            
            isMoving = false;
            actorSpriteRenderer.run.StopAnimating();
            actorSpriteRenderer.run.enabled = isMoving;
            // Stop moving and perform an attack
            if (!isAttacking && !isBlocking)
            {
                // have a chance to shoot when in range, or lunge for a melee attack
                if (prideHasTeleported){
                    // Perform Attack
                    isAttacking = true;
                    isShooting = true;
                    prideHasTeleported = false;
                    StopBlockCoroutine(); // Stop the attack coroutine if it's running
                    performAttackCoroutine = PerformAttack();
                    StartCoroutine(performAttackCoroutine);
                }
                else
                {
                    // Perform Attack
                    isAttacking = true;
                    isShooting = false;
                    StopBlockCoroutine(); // Stop the attack coroutine if it's running
                    performAttackCoroutine = PerformAttack();
                    StartCoroutine(performAttackCoroutine);
                }
                
            }
        }
    }

    private void PrideTeleportCooldown()
    {
        if (prideCanTeleport == false)
        {
            if (prideCooldownTimer < prideTeleportCooldown){
                prideCooldownTimer += Time.deltaTime;
            }
            else
            if (prideCooldownTimer > prideTeleportCooldown){
                prideCooldownTimer = 0;
                prideCanTeleport = true;
            }
        }
    }

#region Enemy Animations

    // Only Use these in Coroutiune.

    private void PlayAngerAttackAnimation()
    {
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
    }

    private void PlayDefaultAttackAnimation()
    {
        if ( actorSpriteRenderer.attack.frame != 0) { actorSpriteRenderer.attack.frame = 0; }
        actorSpriteRenderer.run.StopAnimating();
        actorSpriteRenderer.attack.currentSpriteSet = actorSpriteRenderer.attack.spriteSetAttackOne; 
        actorSpriteRenderer.attack.AnimateOnce();
        actorSpriteRenderer.attack.enabled = isAttacking;
    }

    private void PlayRangedAttackAnimation()
    {
        if ( actorSpriteRenderer.attack.frame != 0) { actorSpriteRenderer.attack.frame = 0; }
        actorSpriteRenderer.run.StopAnimating();
        actorSpriteRenderer.attack.currentSpriteSet = actorSpriteRenderer.attack.spriteSetAttackOne; 
        actorSpriteRenderer.attack.AnimateOnce();
        actorSpriteRenderer.attack.enabled = isAttacking;
    }

    private void PlayPrideRangedAttackAnimation()
    {
        if ( actorSpriteRenderer.attack.frame != 0) { actorSpriteRenderer.attack.frame = 0; }
        actorSpriteRenderer.run.StopAnimating();
        actorSpriteRenderer.attack.currentSpriteSet = actorSpriteRenderer.attack.spriteSetAttackTwo; 
        actorSpriteRenderer.attack.AnimateOnce();
        actorSpriteRenderer.attack.enabled = isAttacking;
    }

    private void PlayTeleportAnimation()
    {
        if ( actorSpriteRenderer.attack.frame != 0) { actorSpriteRenderer.attack.frame = 0; }
        actorSpriteRenderer.attack.StopAnimating();
        actorSpriteRenderer.run.currentSpriteSet = actorSpriteRenderer.run.spriteSetAttackOne; 
        actorSpriteRenderer.run.AnimateOnce();
        actorSpriteRenderer.run.enabled = isTeleporting;
    }

    private void PlayPrideTeleportAnimation()
    {
        if ( actorSpriteRenderer.attack.frame != 0) { actorSpriteRenderer.attack.frame = 0; }
        actorSpriteRenderer.attack.StopAnimating();
        actorSpriteRenderer.run.currentSpriteSet = actorSpriteRenderer.run.spriteSetRunTwo; 
        actorSpriteRenderer.run.AnimateOnce();
        actorSpriteRenderer.run.enabled = isTeleporting;
    }

    private void PlayDefaultWalkAnimation()
    {
        if ( actorSpriteRenderer.attack.frame != 0) { actorSpriteRenderer.attack.frame = 0; }
        actorSpriteRenderer.attack.StopAnimating();
        actorSpriteRenderer.run.currentSpriteSet = actorSpriteRenderer.run.spriteSetRunOne; 
        actorSpriteRenderer.run.AnimateLoop();
        actorSpriteRenderer.run.enabled = isMoving;
    }


#endregion



}
