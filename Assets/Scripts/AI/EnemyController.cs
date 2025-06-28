// Optimized EnemyController.cs
// Full functionality preserved, cleaned, and structured for performance and readability.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public EnemyAiType.EnemyType enemytype = EnemyAiType.EnemyType.WRATH;
    [SerializeField] private EnemyAiType enemyAiType;
    [Header("Stats")]
    public float movementSpeed = 3f;
    public float attackRange = 8f;
    public float blockChance = 0.5f;
    public float bounceForce = 5f;
    public float knockbackForce = 5f;
    public float prideTeleportCooldown = 5f;
    public GameObject corpsePrefab;
    public GameObject attackIndicator;

    [Header("Teleportation")]
    [SerializeField] private Vector2 areaCenter;
    [SerializeField] private float teleportRadius;
    [SerializeField] private float checkRadius;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float teleportCooldown = 5f;

    [Header("References")]
    [SerializeField] private SpriteRenderer actorRenderer;
    [SerializeField] private GameObject collisionBox;
    [SerializeField] private GameObject spellObject;

	private List<Node> currentPath;
	private int pathIndex = 0;
	private float pathUpdateRate = 0.5f;
	private float pathTimer = 0f;


    private Transform player;
    private Player playerObject;
    private Rigidbody2D rb;
    private ActorSpriteRenderer actorSpriteRenderer;

    private Vector2 moveDirection;
    private float attackAngle;
    private float prideCooldownTimer = 0f;

    private IEnumerator performAttackCoroutine;
    private IEnumerator performBlockCoroutine;

    private int animCombo = 0;
    private bool isTeleporting = false;

    public bool isMoving, isAttacking, isShooting, isBlocking, isKnockedBack;
    public bool prideCanTeleport = true, prideHasTeleported = false, attackSquash = false, isDead = false, applyKnockback = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        actorSpriteRenderer = GetComponentInChildren<ActorSpriteRenderer>();
        playerObject = FindFirstObjectByType<Player>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (!player) return;

        if (enemytype is EnemyAiType.EnemyType.LUST or EnemyAiType.EnemyType.PRIDE)
            areaCenter = player.localPosition;

        if (enemytype == EnemyAiType.EnemyType.PRIDE)
            UpdatePrideTeleportCooldown();

        actorRenderer.flipX = player.position.x < transform.position.x;
    }

    private void FixedUpdate()
    {
        if (!player || playerObject.isDead)
        {
            ResetMovement();
            return;
        }

        switch (enemytype)
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
    }

    private void ResetMovement()
    {
        isAttacking = false;
        isBlocking = false;
        rb.velocity = Vector2.zero;
        actorSpriteRenderer.run.StopAnimating();
        actorSpriteRenderer.run.enabled = false;
        actorRenderer.sprite = actorSpriteRenderer.idle;
    }

    private void UpdatePrideTeleportCooldown()
    {
        if (!prideCanTeleport)
        {
            prideCooldownTimer += Time.deltaTime;
            if (prideCooldownTimer >= prideTeleportCooldown)
            {
                prideCooldownTimer = 0f;
                prideCanTeleport = true;
            }
        }
    }

    private Vector3 GetRandomPosition()
    {
        Vector2 randomOffset = new(Random.Range(-teleportRadius, teleportRadius), Random.Range(-teleportRadius, teleportRadius));
        return areaCenter + randomOffset;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        var playerController = collision.GetComponentInParent<PlayerController>();
        var katanna = collision.GetComponentInParent<Katanna>();
        var lustSpell = collision.GetComponentInParent<LustSpell>();

        if (collision.CompareTag("PlayerAttack") && playerController?.isAttacking == true && !lustSpell)
        {
            if (!isAttacking && Random.value < blockChance && enemytype == EnemyAiType.EnemyType.WRATH)
            {
                TryBlock(collision);
                return;
            }
            Die();
        }
        else if (katanna)
        {
            DieFromKatanna(katanna);
        }
    }

    private void TryBlock(Collider2D collision)
    {
        if (isBlocking) return;

        applyKnockback = true;
        isBlocking = true;
        StopAttackCoroutine();

        performBlockCoroutine = PerformBlock();
        StartCoroutine(performBlockCoroutine);

        var attackerRb = collision.GetComponentInParent<Rigidbody2D>();
        attackerRb.velocity = Vector2.zero;
    }

    private void Die()
    {
        isDead = true;
        collisionBox?.SetActive(false);
        GameController.gameControllerInstance.screenshake.TriggerShake(1f);
        GameController.gameControllerInstance.PlayCutSound();
        StopAllCoroutines();

        var corpse = Instantiate(corpsePrefab, transform.position, Quaternion.identity);
        corpse.GetComponent<SpriteRenderer>().flipX = actorRenderer.flipX;

        foreach (var sr in corpse.GetComponentsInChildren<SpriteRenderer>())
            sr.flipX = actorRenderer.flipX;

        GameController.gameControllerInstance.AddKill();
        Destroy(gameObject);
    }

    private void DieFromKatanna(Katanna katanna)
    {
        isDead = true;
        katanna.GetKatannaRigidBody().velocity = Vector2.zero;
        katanna.canCut = false;
        collisionBox?.SetActive(false);
        GameController.gameControllerInstance.screenshake.TriggerShake(1f);
        GameController.gameControllerInstance.PlayCutSound();
        StopAllCoroutines();

        var corpse = Instantiate(corpsePrefab, transform.position, Quaternion.identity);
        corpse.GetComponent<SpriteRenderer>().flipX = actorRenderer.flipX;

        foreach (var sr in corpse.GetComponentsInChildren<SpriteRenderer>())
            sr.flipX = actorRenderer.flipX;

        GameController.gameControllerInstance.AddKill();
        Destroy(gameObject);
    }

    private IEnumerator PerformAttack()
{
    if (!isAttacking) yield break;

    rb.velocity = Vector2.zero;

    Vector2 attackDirection = (player.position - transform.position).normalized;
    attackAngle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;

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
                PlayPrideRangedAttackAnimation();
            else
                PlayDefaultAttackAnimation();
            break;
    }

    float halfAttackDuration = (actorSpriteRenderer.attack.currentSpriteSet.Length / 2f) / 10f;
    yield return new WaitForSeconds(halfAttackDuration);

    if (actorSpriteRenderer.attack.frame >= actorSpriteRenderer.attack.currentSpriteSet.Length / 2)
    {
        Vector2 indicatorPos = (Vector2)transform.position + Vector2.up * 0.25f;
        var indicator = Instantiate(attackIndicator, indicatorPos, Quaternion.identity, transform);
    }
    else
    {
        yield return null;
    }

    yield return new WaitForSeconds(0.1f);

    if ((enemytype == EnemyAiType.EnemyType.LUST || (enemytype == EnemyAiType.EnemyType.PRIDE && isShooting))
        && player != null && !playerObject.isDead)
    {
        GameObject spell = Instantiate(spellObject, transform.position, Quaternion.identity);
        spell.GetComponent<LustSpell>().SetDirection(player.position);
    }

    if (!isShooting && enemyAiType.GetCanDash())
    {
        rb.velocity = Vector2.zero;
        rb.AddForce(attackDirection * knockbackForce, ForceMode2D.Impulse);
        attackSquash = true;
    }

    if (!isShooting && collisionBox != null && actorSpriteRenderer.attack.frame >= actorSpriteRenderer.attack.currentSpriteSet.Length - 4)
    {
        var boxCollider = collisionBox.GetComponentInChildren<BoxCollider2D>();
        var spriteRenderer = collisionBox.GetComponentInChildren<SpriteRenderer>();
        boxCollider.enabled = true;
        spriteRenderer.enabled = true;
    }

    yield return new WaitForSeconds(0.15f);

    rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.8f);
    attackSquash = false;

    if (collisionBox != null)
    {
        var runAnimator = collisionBox.GetComponentInChildren<ActorSpriteRenderer>().run;
        runAnimator.StopAnimating();

        if (!runAnimator.isAnimating)
        {
            runAnimator.enabled = false;
            runAnimator.frame = 0;

            var spriteRenderer = collisionBox.GetComponentInChildren<SpriteRenderer>();
            var boxCollider = collisionBox.GetComponentInChildren<BoxCollider2D>();
            spriteRenderer.enabled = false;
            boxCollider.enabled = false;
        }
    }

    yield return null;

    rb.velocity = Vector2.zero;

    yield return new WaitForSeconds(0.5f);

    actorSpriteRenderer.attack.StopAnimating();

    if (!actorSpriteRenderer.attack.isAnimating)
    {
        actorSpriteRenderer.attack.frame = 0;
        isAttacking = false;
        isShooting = false;
        actorSpriteRenderer.attack.enabled = false;
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

    if (!isBlocking) yield break;

    actorSpriteRenderer.run.StopAnimating();
    actorSpriteRenderer.attack.StopAnimating();
    actorSpriteRenderer.parry.currentSpriteSet = actorSpriteRenderer.parry.spriteSetParry;
    actorSpriteRenderer.parry.AnimateOnce();
    actorSpriteRenderer.parry.enabled = true;

    if (applyKnockback)
    {
        Vector2 knockbackDir = (transform.position - player.position).normalized;
        ApplyKnockback(knockbackDir, knockbackForce);
    }

    yield return new WaitForSeconds(0.3f);

    if (isKnockedBack)
        rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.8f);

    yield return new WaitForSeconds(0.2f);

    if (isKnockedBack && applyKnockback)
    {
        isKnockedBack = false;
        applyKnockback = false;
    }

    actorSpriteRenderer.parry.StopAnimating();

    if (!actorSpriteRenderer.parry.isAnimating)
    {
        actorSpriteRenderer.parry.frame = 0;
        isBlocking = false;
        actorSpriteRenderer.parry.enabled = false;
    }
}

private IEnumerator PerformTeleportMove()
{
    if (enemytype == EnemyAiType.EnemyType.LUST)
        PlayTeleportAnimation();
    else if (enemytype == EnemyAiType.EnemyType.PRIDE)
        PlayPrideTeleportAnimation();

    yield return new WaitForSeconds(0.25f);

    actorSpriteRenderer.run.frame = actorSpriteRenderer.run.currentSpriteSet.Length - 1;

    yield return new WaitForSeconds(0.05f);

    if (isTeleporting) yield break;
    isTeleporting = true;

    Vector3 randomPos = GetRandomPosition();

    // Retry until no obstacle found
    int tries = 0;
    while (Physics2D.OverlapCircle(randomPos, checkRadius, obstacleLayer) && tries < 10)
    {
        randomPos = GetRandomPosition();
        tries++;
        yield return null;
    }

    transform.position = randomPos;

    yield return new WaitForSeconds(teleportCooldown);

    isTeleporting = false;
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
    isKnockedBack = true;
    rb.velocity = Vector2.zero;
    rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
}

private void MeleeAttackCalculation()
{
    float dist = Vector2.Distance(transform.position, player.position);

    if (dist >= attackRange)
    {
        moveDirection = (player.position - transform.position).normalized;

        if (!isAttacking && !isBlocking)
        {
            rb.velocity = moveDirection * movementSpeed;
			isMoving = true;
            actorSpriteRenderer.attack.StopAnimating();
            actorSpriteRenderer.run.currentSpriteSet = actorSpriteRenderer.run.spriteSetRunOne;
            if (!actorSpriteRenderer.run.isAnimating)
            {
                actorSpriteRenderer.run.AnimateLoop();
                actorSpriteRenderer.run.enabled = true;
            }
        }
        else
        {
            isMoving = false;
            actorSpriteRenderer.run.StopAnimating();
        }

        if (!isAttacking && moveDirection != Vector2.zero)
        {
            attackAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            collisionBox.transform.rotation = Quaternion.AngleAxis(attackAngle, Vector3.forward);
        }
    }
    else
    {
        isMoving = false;
        actorSpriteRenderer.run.StopAnimating();
        actorSpriteRenderer.run.enabled = false;

        if (!isAttacking && !isBlocking)
        {
            if (Random.value < blockChance / 3f && enemytype == EnemyAiType.EnemyType.WRATH)
            {
                isAttacking = false;
                isBlocking = true;
                StopAttackCoroutine();
                performBlockCoroutine = PerformBlock();
                StartCoroutine(performBlockCoroutine);
            }
            else
            {
                isAttacking = true;
                isBlocking = false;
                StopBlockCoroutine();
                performAttackCoroutine = PerformAttack();
                StartCoroutine(performAttackCoroutine);
            }
        }
    }
}

private void RangedAttackCalculation()
{
    float dist = Vector2.Distance(transform.position, player.position);

    if (dist >= attackRange || dist < attackRange - 6f)
    {
        if (enemyAiType.GetCanTeleport())
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
            if (!actorSpriteRenderer.run.isAnimating)
            {
                actorSpriteRenderer.run.AnimateLoop();
                actorSpriteRenderer.run.enabled = true;
            }
        }
        else
        {
            isMoving = false;
            actorSpriteRenderer.run.StopAnimating();
        }

        if (!isAttacking && moveDirection != Vector2.zero)
        {
            attackAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        }
    }
    else
    {
        isMoving = false;
        actorSpriteRenderer.run.StopAnimating();
        actorSpriteRenderer.run.enabled = false;

        if (!isAttacking && !isBlocking)
        {
            isAttacking = true;
            isBlocking = false;
            StopBlockCoroutine();
            performAttackCoroutine = PerformAttack();
            StartCoroutine(performAttackCoroutine);
        }
    }
}

private void MeleeRangeAttackCalculation()
{
    float dist = Vector2.Distance(transform.position, player.position);

    if (dist >= attackRange)
    {
        if (enemyAiType.GetCanTeleport() && prideCanTeleport)
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
        else
        {
            isMoving = false;
            actorSpriteRenderer.run.StopAnimating();
        }

        if (!isAttacking && moveDirection != Vector2.zero)
        {
            attackAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            collisionBox.transform.rotation = Quaternion.AngleAxis(attackAngle, Vector3.forward);
        }
    }
    else
    {
        isMoving = false;
        actorSpriteRenderer.run.StopAnimating();
        actorSpriteRenderer.run.enabled = false;

        if (!isAttacking && !isBlocking)
        {
            if (prideHasTeleported)
            {
                isAttacking = true;
                isShooting = true;
                prideHasTeleported = false;
                StopBlockCoroutine();
                performAttackCoroutine = PerformAttack();
                StartCoroutine(performAttackCoroutine);
            }
            else
            {
                isAttacking = true;
                isShooting = false;
                StopBlockCoroutine();
                performAttackCoroutine = PerformAttack();
                StartCoroutine(performAttackCoroutine);
            }
        }
    }
}

#region Enemy Animations

	private void PlayAngerAttackAnimation()
	{
		actorSpriteRenderer.run.StopAnimating();

		switch (animCombo)
		{
			case 0:
				actorSpriteRenderer.attack.currentSpriteSet = actorSpriteRenderer.attack.spriteSetAttackOne;
				break;
			case 1:
				actorSpriteRenderer.attack.currentSpriteSet = actorSpriteRenderer.attack.spriteSetAttackTwo;
				break;
		}

		actorSpriteRenderer.attack.AnimateOnce();
		actorSpriteRenderer.attack.enabled = true;

		animCombo = (animCombo + 1) % 2;
	}

	private void PlayDefaultAttackAnimation()
	{
		if (actorSpriteRenderer.attack.frame != 0)
			actorSpriteRenderer.attack.frame = 0;

		actorSpriteRenderer.run.StopAnimating();
		actorSpriteRenderer.attack.currentSpriteSet = actorSpriteRenderer.attack.spriteSetAttackOne;
		actorSpriteRenderer.attack.AnimateOnce();
		actorSpriteRenderer.attack.enabled = true;
	}

	private void PlayRangedAttackAnimation()
	{
		PlayDefaultAttackAnimation();
	}

	private void PlayPrideRangedAttackAnimation()
	{
		if (actorSpriteRenderer.attack.frame != 0)
			actorSpriteRenderer.attack.frame = 0;

		actorSpriteRenderer.run.StopAnimating();
		actorSpriteRenderer.attack.currentSpriteSet = actorSpriteRenderer.attack.spriteSetAttackTwo;
		actorSpriteRenderer.attack.AnimateOnce();
		actorSpriteRenderer.attack.enabled = true;
	}

	private void PlayTeleportAnimation()
	{
		if (actorSpriteRenderer.attack.frame != 0)
			actorSpriteRenderer.attack.frame = 0;

		actorSpriteRenderer.attack.StopAnimating();
		actorSpriteRenderer.run.currentSpriteSet = actorSpriteRenderer.run.spriteSetAttackOne;
		actorSpriteRenderer.run.AnimateOnce();
		actorSpriteRenderer.run.enabled = true;
	}

	private void PlayPrideTeleportAnimation()
	{
		if (actorSpriteRenderer.attack.frame != 0)
			actorSpriteRenderer.attack.frame = 0;

		actorSpriteRenderer.attack.StopAnimating();
		actorSpriteRenderer.run.currentSpriteSet = actorSpriteRenderer.run.spriteSetRunTwo;
		actorSpriteRenderer.run.AnimateOnce();
		actorSpriteRenderer.run.enabled = true;
	}

	private void PlayDefaultWalkAnimation()
	{
		if (actorSpriteRenderer.attack.frame != 0)
			actorSpriteRenderer.attack.frame = 0;

		actorSpriteRenderer.attack.StopAnimating();
		actorSpriteRenderer.run.currentSpriteSet = actorSpriteRenderer.run.spriteSetRunOne;
		actorSpriteRenderer.run.AnimateLoop();
		actorSpriteRenderer.run.enabled = true;
	}

#endregion




}
