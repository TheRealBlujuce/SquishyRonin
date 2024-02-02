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

    [Header("Use Squash/Stretch, Is Corpse or Effect?,\n Scale Speed, and Attack Combo")]
    public bool usesSquash = false;
    public bool isCorpse = false;
    public bool isEffect = false;
    public float scaleSpeed = 0.5f;
    public int attackCombo = 0;
    public Color corpseColor;

    [Header("Squash and Strech")]
    public float squashXAttack = 0.5f;
    public float squashYAttack = 0.5f;
    // public float squashXLand = 0.5f;
    // public float squashYLand = 0.5f;

    [Header("Sprite Set ")]
    public Sprite idle;
    // public Sprite jump;
    // public Sprite slide;
    // public Sprite slideDash;

    // [Header("Small Sprite Set 2")]
    // public Sprite idleTwo;
    // public Sprite jumpTwo;
    // public Sprite slideTwo;
    // public Sprite slideDashTwo;

    // [Header("Big Sprite Set 1")]
    // public Sprite idleBig;
    // public Sprite jumpBig;
    // public Sprite slideBig;
    // public Sprite slideDashBig;

    // [Header("Big Sprite Set 2")]
    // public Sprite idleBigTwo;
    // public Sprite jumpBigTwo;
    // public Sprite slideBigTwo;
    // public Sprite slideDashBigTwo;

    [Header("Animated Sprites")]

    [Header("Run Sprite")]
    public AnimatedSprite run;

    [Header("Attack Sprite")]
    public AnimatedSprite attack;
    public int animCombo = 0;

    [Header("Parry Sprite")]
    public AnimatedSprite parry;

    [Header("Roll Sprite")]
    public AnimatedSprite roll;




    private void Awake()
    {
        actorRenderer = GetComponent<SpriteRenderer>();

        if (GetComponentInParent<PlayerController>() != null) { actorMovement = GetComponentInParent<PlayerController>(); }
        if (GetComponentInParent<EnemyController>() != null) { enemyActorMovement = GetComponentInParent<EnemyController>(); }
        
        actorTransform = GetComponent<Transform>();
    }

    private void LateUpdate()
    {

        
        UpdateSpriteSet();
        // UpdateEnemySpriteSet();

    }


    private void UpdateSpriteSet()
    {
        
        switch(isEffect)
        {
            case true:
                if (actorMovement != null && actorRenderer.enabled == true && run.isAnimating != true)
                {
                    run.currentSpriteSet = run.spriteSetRun; 
                    run.AnimateLoop();
                    run.enabled = true;
                }
            break;
            case false:
                if (actorMovement != null)
                {
                    if (actorMovement.isMoving && !actorMovement.isAttacking && !actorMovement.isBlocking && !actorMovement.isRolling)
                    { 
                        attack.StopAnimating();
                        run.currentSpriteSet = run.spriteSetRun; 
                        if (run.isAnimating != true)
                        {
                            run.AnimateLoop();
                            run.enabled = actorMovement.isMoving;
                        }
                    }
                    else
                    if (!actorMovement.isMoving && !actorMovement.isAttacking && !actorMovement.isBlocking && !actorMovement.isRolling)
                    { actorRenderer.sprite = idle; run.StopAnimating(); run.enabled = actorMovement.isMoving; }

                }
            break;
        }

    }

    private void Update()
    {

        // used to update the squash and stretch
        if (usesSquash)
        {
            Vector3 baseScale = new Vector3(currentXScale, currentYScale, 1f);
            Vector3 targetScale = new Vector3(targetXScale, targetYScale, 1f);
            if (actorMovement != null)
            {
                if ((actorMovement.isAttacking || actorMovement.isRolling) && actorMovement.attackSquash)
                {
                    actorTransform.localScale = Vector3.Lerp(actorTransform.localScale, targetScale, scaleSpeed * Time.deltaTime); 
                }
                else
                {
                    actorTransform.localScale = Vector3.Lerp(actorTransform.localScale, baseScale, scaleSpeed * Time.deltaTime); 
                }
            }
            if (enemyActorMovement != null)
            {
                if (enemyActorMovement.isAttacking && enemyActorMovement.attackSquash)
                {
                    actorTransform.localScale = Vector3.Lerp(actorTransform.localScale, targetScale, scaleSpeed * Time.deltaTime); 
                }
                else
                {
                    actorTransform.localScale = Vector3.Lerp(actorTransform.localScale, baseScale, scaleSpeed * Time.deltaTime); 
                }
            }
            
        }

        if (isCorpse)
        {   
            
            if (!run.corpseAnimEnd)
            {
                actorRenderer.color = Color.Lerp(actorRenderer.color, corpseColor, 0.01f);
                run.currentSpriteSet = run.spriteSetRun; run.AnimateCorpse();
            }
            else
            {
                actorRenderer.sprite = idle;
                actorRenderer.color = corpseColor;
            }
        }
    }

    IEnumerator Squash()
    {
        
        // used for squash and stretch.
        yield break;
    }
}
