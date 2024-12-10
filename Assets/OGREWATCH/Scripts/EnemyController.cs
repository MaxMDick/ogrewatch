using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyController : MonoBehaviour
{
    public PlayerCombat myCombat;
    public GameObject target;
    public float aggroRange;

    [Header("Debug")]
    public float currentRange;

    void Update()
    {
        currentRange = Vector2.Distance(this.transform.position, target.transform.position);
        if (currentRange <= aggroRange)
        {
            myCombat.OnAttack(true);
        }
        else
        {
            myCombat.OnAttack(false);
        }
    }
}
