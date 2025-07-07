using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Player Variables")]
    public float movementSpeed = 5f;
    public float runningSpeedMultiplier = 1.5f;
    public float attackMovementSpeedMultiplier = 1.75f;
    public float knockbackForce = 5f;
    public float movementFriction = 0.5f;
    public GameObject thrownWep;
	public float hitCooldown = 0.5f;

    [Header("Player Input")]
    [SerializeField] public Player player;
    [SerializeField] private PlayerInput currentGameInput;
    private Vector2 movementInput;
    private Vector2 movementVelocity;

    [SerializeField] private HeartUI heartUI;
	[SerializeField] private SpriteRenderer katanna;

    public bool isMoving;
    public bool isRunning;
    public bool isAttacking;
    public bool isInteracting;
    public bool isBlocking;
    public bool isRolling;
    public bool hasThrown;
    public bool canBeHit = true; // I-frames
    public bool hasSword = true;
	public bool swordEqipped = false;
    public bool attackSquash;
    public int animCombo = 0;
    public bool isBlockParryTiming;
    public bool isKnockedBack;

    private float attackAngle;
	private int lastFacingDirection = 1; // 1 = right, -1 = left

    private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer actorRenderer;
    [SerializeField] private GameObject collisionHitBox;
    [SerializeField] private ActorSpriteRenderer actorSpriteRenderer;
    public GameObject corpsePrefab;

    // Cache components from collisionHitBox to avoid repeated GetComponentInChildren calls
    private BoxCollider2D collisionBoxCollider;
    private SpriteRenderer collisionSpriteRenderer;
    private ActorSpriteRenderer collisionActorSpriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentGameInput = GameController.gameControllerInstance.gameInput;
        actorRenderer = GetComponentInChildren<SpriteRenderer>();
        player = GetComponent<Player>();
        heartUI = FindFirstObjectByType<HeartUI>();

        collisionBoxCollider = collisionHitBox.GetComponentInChildren<BoxCollider2D>();
        collisionSpriteRenderer = collisionHitBox.GetComponentInChildren<SpriteRenderer>();
        collisionActorSpriteRenderer = collisionHitBox.GetComponentInChildren<ActorSpriteRenderer>();
    }

    private void Update()
    {
        if (player.isDead)
            return;

        movementInput = currentGameInput.PlayerMovement.Movement.ReadValue<Vector2>().normalized;

        isMoving = movementInput.sqrMagnitude > 0;

		if (!isAttacking)
		{
			if (movementInput.x != 0)
			{
				lastFacingDirection = movementInput.x > 0 ? 1 : -1;
			}
			actorRenderer.flipX = (lastFacingDirection == -1);

			float katannaFaceRotation = actorRenderer.flipX ? 200f : -200f;
			katanna.flipX = actorRenderer.flipX;
			katanna.transform.localRotation = Quaternion.Euler(0, 0, katannaFaceRotation);
		}

        if (!isAttacking && !hasThrown && movementVelocity != Vector2.zero)
        {
            attackAngle = Mathf.Atan2(movementVelocity.y, movementVelocity.x) * Mathf.Rad2Deg;
            collisionHitBox.transform.rotation = Quaternion.Lerp(
                collisionHitBox.transform.rotation,
                Quaternion.AngleAxis(attackAngle, Vector3.forward),
                36f * Time.deltaTime);
        }

		if (hasSword && swordEqipped && !isAttacking && !isBlocking)
		{
			katanna.enabled = true;
		}

        // Handle attack input
        if (currentGameInput.PlayerMovement.Attack.triggered && swordEqipped && !isAttacking && !isBlocking && !isRolling)
        {
            isAttacking = true;
			katanna.enabled = false;
            StartCoroutine(PerformAttack());
        }

        // Handle throw input
        if (currentGameInput.PlayerMovement.Throw.triggered && swordEqipped && !isAttacking && !isBlocking && !isRolling)
        {
            hasThrown = true;
			katanna.enabled = false;
            StartCoroutine(PerformThrow());
        }

        // Handle roll input
        if (currentGameInput.PlayerMovement.Run.triggered && !isRolling && !isRunning)
        {
            isRolling = true;
            StartCoroutine(PerformRoll());
        }

        // Stop running if run input is released
        if (!currentGameInput.PlayerMovement.Run.IsPressed() && isRunning && !isRolling)
        {
            isRunning = false;
        }

        // Handle block input
        if (currentGameInput.PlayerMovement.Block.triggered && swordEqipped && !isBlocking)
        {
            isBlocking = true;
            isAttacking = false;
			katanna.enabled = false;
            StartCoroutine(PerformBlock());
        }

        // Check block parry timing based on animation frame
        isBlockParryTiming = isBlocking && actorRenderer.sprite == actorSpriteRenderer.parry.spriteSetParry[3];

		// Do the cooldowns
		CanBeHitCooldown();

    }

    private void FixedUpdate()
    {
        movementVelocity = movementInput * movementSpeed;

        if (isRunning)
            movementVelocity *= runningSpeedMultiplier;

        // Movement applied only if not attacking/blocking/rolling
        if (!isAttacking && !isBlocking && !isRolling)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, movementVelocity, Time.fixedDeltaTime * movementFriction);
        }

        // Arena wrap (commented out)
        if (GameController.gameControllerInstance.currentWorldState == GameController.WorldState.ARENA || SceneManager.GetActiveScene().name != "World_Map")
        {
				WrapAroundScreen();
        }
    }

    public PlayerInput GetPlayerInput() => currentGameInput;

    private IEnumerator PerformAttack()
    {
        Vector2 attackMoveDirection = Quaternion.AngleAxis(attackAngle, Vector3.forward) * Vector2.right;

        rb.velocity = Vector2.zero;
        rb.AddForce(attackMoveDirection * knockbackForce, ForceMode2D.Impulse);
        attackSquash = true;

        actorSpriteRenderer.run.StopAnimating();
        actorSpriteRenderer.roll.StopAnimating();

        // Enable hitbox components once at start of attack
        collisionBoxCollider.enabled = true;
        collisionSpriteRenderer.enabled = true;
        collisionBoxCollider.isTrigger = true;

        GameController.gameControllerInstance.PlaySwordSwingSound();

        // Handle animation combo
        switch (animCombo)
        {
            case 0:
                actorSpriteRenderer.attack.currentSpriteSet = actorSpriteRenderer.attack.spriteSetAttackOne;
                animCombo++;
                break;
            case 1:
                actorSpriteRenderer.attack.currentSpriteSet = actorSpriteRenderer.attack.spriteSetAttackTwo;
                animCombo++;
                break;
        }
        if (animCombo > 1) animCombo = 0;

        actorSpriteRenderer.attack.AnimateOnce();
        actorSpriteRenderer.attack.enabled = true;

        yield return new WaitForSeconds(0.1f);

        if (!isRolling)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.8f);
        }

        yield return new WaitForSeconds(0.025f);
        yield return new WaitForEndOfFrame();

        collisionActorSpriteRenderer.run.StopAnimating();
        if (!collisionActorSpriteRenderer.run.isAnimating)
        {
            collisionActorSpriteRenderer.run.enabled = false;
            collisionActorSpriteRenderer.run.frame = 0;
            collisionSpriteRenderer.enabled = false;
        }

        attackSquash = false;

        yield return new WaitForSeconds(0.3f);

        if (!isRolling)
            rb.velocity = Vector2.zero;

        actorSpriteRenderer.attack.StopAnimating();

        if (!actorSpriteRenderer.attack.isAnimating)
        {
            actorSpriteRenderer.attack.frame = 0;
            collisionBoxCollider.enabled = false;
            collisionBoxCollider.isTrigger = false;
            isAttacking = false;
            actorSpriteRenderer.attack.enabled = false;
        }
    }

    private IEnumerator PerformThrow()
    {
        swordEqipped = false;
        isAttacking = true;

        Vector2 throwDirection = Quaternion.AngleAxis(attackAngle, Vector3.forward) * Vector2.right;

        actorSpriteRenderer.run.StopAnimating();
        actorSpriteRenderer.roll.StopAnimating();

        actorSpriteRenderer.attack.currentSpriteSet = actorSpriteRenderer.attack.spriteSetAttackOne;
        actorSpriteRenderer.attack.AnimateOnce();
        actorSpriteRenderer.attack.enabled = true;

        collisionSpriteRenderer.enabled = true;

        var thrownWeapon = Instantiate(thrownWep, new Vector2(transform.localPosition.x, transform.localPosition.y + 0.5f), quaternion.identity);
        thrownWeapon.GetComponent<Katanna>().SetDirection(throwDirection);

        yield return new WaitForSeconds(0.1f);
        yield return new WaitForSeconds(0.025f);
        yield return new WaitForEndOfFrame();

        collisionActorSpriteRenderer.run.StopAnimating();
        if (!collisionActorSpriteRenderer.run.isAnimating)
        {
            collisionActorSpriteRenderer.run.enabled = false;
            collisionActorSpriteRenderer.run.frame = 0;
            collisionSpriteRenderer.enabled = false;
        }

        yield return new WaitForSeconds(0.05f);

        actorSpriteRenderer.attack.StopAnimating();

        if (!actorSpriteRenderer.attack.isAnimating)
        {
            actorSpriteRenderer.attack.frame = 0;
            hasThrown = false;
            isAttacking = false;
            actorSpriteRenderer.attack.enabled = false;
        }
    }

    private IEnumerator PerformRoll()
    {
        canBeHit = false;

        Vector2 rollMoveDirection = Quaternion.AngleAxis(attackAngle, Vector3.forward) * Vector2.right;

        actorSpriteRenderer.run.StopAnimating();
        actorSpriteRenderer.attack.StopAnimating();
        actorSpriteRenderer.parry.StopAnimating();

        actorSpriteRenderer.roll.currentSpriteSet = actorSpriteRenderer.roll.spriteSetRunOne;
        actorSpriteRenderer.roll.frame = 0;
        actorSpriteRenderer.roll.AnimateOnce();
        actorSpriteRenderer.roll.enabled = true;

        rb.velocity = Vector2.zero;
        rb.AddForce(rollMoveDirection * (knockbackForce + movementSpeed), ForceMode2D.Impulse);
        attackSquash = true;

        yield return new WaitForSeconds(0.2f);

        rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.6f);
        yield return new WaitForSeconds(0.125f);

        attackSquash = false;
        yield return new WaitForSeconds(0.2f);

        actorSpriteRenderer.roll.StopAnimating();

        if (!actorSpriteRenderer.roll.isAnimating)
        {
            rb.velocity = Vector2.zero;
            isRolling = false;
            actorSpriteRenderer.roll.enabled = false;
            isRunning = true;
            canBeHit = true;
        }
    }

    private IEnumerator PerformBlock()
    {
        rb.velocity = Vector2.zero;

        actorSpriteRenderer.run.StopAnimating();
        actorSpriteRenderer.attack.StopAnimating();

        actorSpriteRenderer.parry.currentSpriteSet = actorSpriteRenderer.parry.spriteSetParry;
        actorSpriteRenderer.parry.AnimateOnce();
        actorSpriteRenderer.parry.enabled = true;

        yield return new WaitForSeconds(0.3f);

        if (isKnockedBack)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.8f);
            GameController.gameControllerInstance.PlayParrySound();
        }

        yield return new WaitForSeconds(0.2f);

        if (isKnockedBack) isKnockedBack = false;

        actorSpriteRenderer.parry.StopAnimating();

        if (!actorSpriteRenderer.parry.isAnimating)
        {
            actorSpriteRenderer.parry.frame = 0;
            isBlocking = false;
            actorSpriteRenderer.parry.enabled = false;
        }
    }

    public void ApplyKnockback(Vector2 knockbackDirection, float force)
    {
        isKnockedBack = true;
        rb.velocity = Vector2.zero;
        rb.AddForce(knockbackDirection * force, ForceMode2D.Impulse);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyAttack"))
        {
            var enemyController = collision.GetComponentInParent<EnemyController>();
            if (enemyController == null) return;

            Vector2 knockbackMoveDirection = (transform.position - enemyController.transform.position).normalized;

            if (!enemyController.isAttacking) return;

            if ((isBlocking && isBlockParryTiming) || isBlocking)
            {
                GameController.gameControllerInstance.PlayParrySound();
                ApplyKnockback(knockbackMoveDirection, knockbackForce);
            }
            else if (!isBlocking && !isBlockParryTiming && canBeHit)
            {
                TakeDamage();
            }
        }
        else if (collision.CompareTag("EnemySpell"))
        {
            var spell = collision.GetComponentInParent<LustSpell>();
            if (spell == null) return;

            Vector2 knockbackMoveDirection = (transform.position - spell.transform.position).normalized;

            if ((isBlocking && isBlockParryTiming) || isBlocking)
            {
                Destroy(collision.gameObject);
                ApplyKnockback(knockbackMoveDirection, spell.knockbackForce);
            }
            else if (!isBlocking && !isBlockParryTiming && canBeHit)
            {
                TakeDamage();
            }
        }
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

    private void TakeDamage()
    {
        heartUI.TakeDamage(1);
		canBeHit = false;

        if (heartUI.currentHealth <= 0)
        {
            KillPlayer();
        }
    }

    private void KillPlayer()
    {
        player.isDead = true;
        Instantiate(corpsePrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
        GameController.gameControllerInstance.EndGame();
    }


	#region Cooldowns

		private void CanBeHitCooldown()
		{
			if (player.isDead) return;

			if (!canBeHit)
			{
				var cooldownTimer = 0f;

				cooldownTimer += 0.1f;

				if (cooldownTimer > hitCooldown)
				{
					canBeHit = true;
				}

			}
		}

	#endregion

}
