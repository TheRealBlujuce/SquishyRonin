using System.Collections;
using UnityEngine;

public class ActorSpriteRenderer : MonoBehaviour
{
    private SpriteRenderer actorRenderer;
    private PlayerController actorMovement;
    private EnemyController enemyActorMovement;
    private Transform actorTransform;

    [SerializeField] private float currentXScale;
    [SerializeField] private float currentYScale;
    [SerializeField] private float targetXScale;
    [SerializeField] private float targetYScale;

    [Header("Use Squash/Stretch, Is Corpse or Effect?, Scale Speed, and Attack Combo")]
    public bool usesSquash = false;
    public bool isCorpse = false;
    public bool isEffect = false;
    public float scaleSpeed = 0.5f;
    public int attackCombo = 0;
    public Color corpseColor;

    [Header("Squash and Stretch")]
    public float squashXAttack = 0.5f;
    public float squashYAttack = 0.5f;

    [Header("Sprite Set")]
    public Sprite idle;
    public Sprite altIdle;

    [Header("Animated Sprites")]
    public AnimatedSprite run;
    public AnimatedSprite attack;
    public AnimatedSprite parry;
    public AnimatedSprite roll;

    private void Awake()
    {
        actorRenderer = GetComponent<SpriteRenderer>();
        actorTransform = transform;

        actorMovement = GetComponentInParent<PlayerController>();
        enemyActorMovement = GetComponentInParent<EnemyController>();
    }

    private void LateUpdate()
    {
        UpdateSpriteSet();
    }

    private void Update()
    {
        UpdateSquashStretch();
        UpdateCorpseState();
    }

    private void UpdateSpriteSet()
    {
        if (isEffect)
        {
            if (actorMovement != null && actorRenderer.enabled && !run.isAnimating)
            {
                run.currentSpriteSet = run.spriteSetRunTwo;
                run.AnimateLoop();
                run.enabled = true;
            }
            return;
        }

        if (actorMovement == null) return;

        if (actorMovement.isMoving && !actorMovement.isAttacking && !actorMovement.isBlocking && !actorMovement.isRolling && !actorMovement.hasThrown)
        {
            attack.StopAnimating();

            //run.currentSpriteSet = actorMovement.swordEqipped ? run.spriteSetRunTwo : run.spriteSetRunOne;
            run.currentSpriteSet = run.spriteSetRunTwo;

            if (!run.isAnimating)
            {
                run.AnimateLoop();
                run.enabled = true;
            }
        }
        else if (!actorMovement.isMoving && !actorMovement.isAttacking && !actorMovement.isBlocking && !actorMovement.isRolling)
        {
            //actorRenderer.sprite = actorMovement.swordEqipped ? altIdle : idle;
            actorRenderer.sprite = altIdle;
            run.StopAnimating();
            run.enabled = false;
        }
    }

    private void UpdateSquashStretch()
    {
        if (!usesSquash) return;

        Vector3 baseScale = new(currentXScale, currentYScale, 1f);
        Vector3 targetScale = new(targetXScale, targetYScale, 1f);

        bool shouldSquash =
            (actorMovement != null && (actorMovement.isAttacking || actorMovement.isRolling || actorMovement.hasThrown) && actorMovement.attackSquash) ||
            (enemyActorMovement != null && enemyActorMovement.isAttacking && enemyActorMovement.attackSquash);

        actorTransform.localScale = Vector3.Lerp(
            actorTransform.localScale,
            shouldSquash ? targetScale : baseScale,
            scaleSpeed * Time.deltaTime
        );
    }

    private void UpdateCorpseState()
    {
        if (!isCorpse) return;

        if (!run.corpseAnimEnd)
        {
            actorRenderer.color = Color.Lerp(actorRenderer.color, corpseColor, 0.01f);
            run.currentSpriteSet = run.spriteSetRunOne;
            run.AnimateCorpse();
        }
        else
        {
            actorRenderer.sprite = idle;
            actorRenderer.color = corpseColor;
        }
    }
}
