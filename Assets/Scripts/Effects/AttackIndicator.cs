using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackIndicator : MonoBehaviour
{
    public Vector2 maxSize = new Vector2(1.5f, 1.5f);
    public float scaleSpeed = 0.5f;
    private Transform objectRenderer;
    private SpriteRenderer spriteRenderer;
    private void Start()
    {
        objectRenderer = GetComponent<Transform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {   
        objectRenderer.localScale = Vector2.Lerp(objectRenderer.localScale, maxSize, scaleSpeed * Time.deltaTime);
        spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.white, scaleSpeed * Time.deltaTime);
        
        if (objectRenderer.localScale.x >= maxSize.x - 0.1f)
        {
            Destroy(this.gameObject);
        }
    }
}
