using System.Collections;
using UnityEngine;

public class AnimatedSprite : MonoBehaviour
{
    public Sprite[] spriteSetRun;
    public Sprite[] spriteSetAttackOne;
    public Sprite[] spriteSetAttackTwo;
    public Sprite[] spriteSetParry;
    public Sprite[] currentSpriteSet;

    private Player actorPlayer;

    public float framerate = 1f / 6f;
    public SpriteRenderer actorRenderer;
    public int frame;
    public bool useFastFramerate = false;
    public bool animateOnce = false;
    [SerializeField] public bool isAnimating = false;
    [SerializeField] public bool corpseAnimEnd = false;
    private Coroutine animateFramesCoroutine;
    private Coroutine animateFramesOnceCoroutine;
    private Coroutine animateCorpseFramesCoroutine;
    private void Awake()
    {
        actorRenderer = GetComponent<SpriteRenderer>();
        actorPlayer = GetComponentInParent<Player>();
        
    }

    private void Animate()
    {
        
        frame++;
        if(frame >= currentSpriteSet.Length)
        {
            frame = 0;
        }
        if (frame >= 0 && frame < currentSpriteSet.Length)
        {
            actorRenderer.sprite = currentSpriteSet[frame];
        }
        
    }

    public void AnimateLoop()
    {
        if(!isAnimating){ isAnimating = true; StartCoroutine(AnimateFrames()); }
    }

    public void AnimateOnce()
    {
        if (!isAnimating)
        {
            isAnimating = true;
            StartCoroutine(AnimateFramesOnce());
        }

    }
    public void AnimateCorpse()
    {
        if (!isAnimating)
        {
            isAnimating = true;
            StartCoroutine(AnimateCorpseFrames());
        }

    }

    public void StopAnimating()
    {
        
        isAnimating = false;
        if (animateFramesCoroutine != null)
            StopCoroutine(animateFramesCoroutine);
        if (animateFramesOnceCoroutine != null)
            StopCoroutine(animateFramesOnceCoroutine);
        if (animateCorpseFramesCoroutine != null)
            StopCoroutine(animateCorpseFramesCoroutine);
    
    }
    public bool CheckAnimating()
    {
        return isAnimating;
    }

    private IEnumerator AnimateFrames()
    {
        if (isAnimating)
        {
            Animate();
            
            yield return new WaitForSeconds(framerate);

            if (actorPlayer != null)
            {
                if (actorPlayer.isDead == true){ yield break;}
            }

            animateFramesCoroutine = StartCoroutine(AnimateFrames()); 
        }

    }

    private IEnumerator AnimateFramesOnce()
    {
   
        // if (useFastFramerate) { currentFramerate = fastframerate; } else { currentFramerate = framerate; }
        if (isAnimating)
        {
            Animate();
            
            yield return new WaitForSeconds(framerate);

            if (actorPlayer != null)
            {
                if (actorPlayer.isDead == true){ yield break;}
            }
            
            if (frame < currentSpriteSet.Length-1){ animateFramesOnceCoroutine = StartCoroutine(AnimateFramesOnce()); }
            else
            if (frame >= currentSpriteSet.Length-1){ isAnimating = false; }
        }
    }
    private IEnumerator AnimateCorpseFrames()
    {
   
        // if (useFastFramerate) { currentFramerate = fastframerate; } else { currentFramerate = framerate; }
        if (isAnimating)
        {
            Animate();
            
            yield return new WaitForSeconds(framerate);

            if (actorPlayer != null)
            {
                if (actorPlayer.isDead == true){ yield break;}
            }
            
            if (frame < currentSpriteSet.Length-1){ animateCorpseFramesCoroutine = StartCoroutine(AnimateCorpseFrames()); }
            else
            if (frame >= currentSpriteSet.Length-1){ isAnimating = false; corpseAnimEnd = true; yield break; }
        }
    }
}
