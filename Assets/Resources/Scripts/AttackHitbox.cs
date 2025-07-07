using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private float xScale, yScale;

    // Start is called before the first frame update
    void Start()
    {
        xScale = this.transform.localScale.x;
        yScale = this.transform.localScale.y;
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.localScale = new Vector3(xScale, yScale, 1f);
    }
}
