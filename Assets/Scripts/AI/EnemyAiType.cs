using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAiType : MonoBehaviour
{
    public enum EnemyType{
        WRATH,
        GREED,
        LUST,
        PRIDE
    }

    [SerializeField] private bool canTeleport;
    [SerializeField] private bool canDash;
    

    // Start is called before the first frame update
    void Start()
    {
        EnemyType enemyType = GetComponent<EnemyController>().enemytype;
        switch(enemyType)
        {
            case EnemyType.WRATH:
            case EnemyType.GREED:
                canTeleport = false;
                canDash = true;
            break;
            case EnemyType.LUST:
                canTeleport = true;
                canDash = false;
            break;
            case EnemyType.PRIDE:
                canTeleport = true;
                canDash = true;
            break;

        }
    }

    public bool GetCanTeleport() { return canTeleport; }
    public bool GetCanDash() { return canDash; }



}
