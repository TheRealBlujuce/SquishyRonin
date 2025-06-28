using System.Collections;
using UnityEngine;

public class AnimatedSprite : MonoBehaviour
{
    public Sprite[] spriteSetRunOne;
    public Sprite[] spriteSetRunTwo;
    public Sprite[] spriteSetAttackOne;
    public Sprite[] spriteSetAttackTwo;
    public Sprite[] spriteSetParry;
    public Sprite[] currentSpriteSet;

    public float framerate = 1f / 6f;
    public SpriteRenderer actorRenderer;

    public int frame;
    public bool animateOnce = false;

    [SerializeField] public bool isAnimating = false;
    [SerializeField] public bool corpseAnimEnd = false;

    private Coroutine activeCoroutine;
    private Player actorPlayer;

    private void Awake()
    {
        actorRenderer = GetComponent<SpriteRenderer>();
        actorPlayer = GetComponentInParent<Player>();
    }

    public void AnimateLoop()
    {
        if (isAnimating) return;
        isAnimating = true;
        activeCoroutine = StartCoroutine(AnimateFramesLoop());
    }

    public void AnimateOnce()
    {
        if (isAnimating) return;
        isAnimating = true;
        activeCoroutine = StartCoroutine(AnimateFramesOnce());
    }

    public void AnimateCorpse()
    {
        if (isAnimating) return;
        isAnimating = true;
        activeCoroutine = StartCoroutine(AnimateCorpseFrames());
    }

    public void StopAnimating()
    {
        isAnimating = false;
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }
    }

    private void Animate()
    {
        if (currentSpriteSet == null || currentSpriteSet.Length == 0) return;

        frame = (frame + 1) % currentSpriteSet.Length;
        actorRenderer.sprite = currentSpriteSet[frame];
    }

    private IEnumerator AnimateFramesLoop()
    {
        while (isAnimating)
        {
            Animate();
            yield return new WaitForSeconds(framerate);

            if (actorPlayer != null && actorPlayer.isDead)
                yield break;
        }
    }

    private IEnumerator AnimateFramesOnce()
    {
        while (isAnimating && frame < currentSpriteSet.Length - 1)
        {
            Animate();
            yield return new WaitForSeconds(framerate);

            if (actorPlayer != null && actorPlayer.isDead)
                yield break;
        }

        isAnimating = false;
    }

    private IEnumerator AnimateCorpseFrames()
    {
        while (isAnimating && frame < currentSpriteSet.Length - 1)
        {
            Animate();
            yield return new WaitForSeconds(framerate);
        }

        isAnimating = false;
        corpseAnimEnd = true;

        if (gameObject.name == "Doubt-Oni-Corpse")
        {
            Destroy(gameObject);
        }
    }
}
