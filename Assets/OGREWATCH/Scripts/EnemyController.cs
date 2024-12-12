using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyController : MonoBehaviour
{
    public Combat combat;
    public GameObject target;
    public float aggroRange;

    [Header("Debug")]
    public float currentRange;

    void Update()
    {
        currentRange = Vector2.Distance(this.transform.position, target.transform.position);
        if (currentRange <= aggroRange)
        {
            combat.OnAttack(true);
        }
        else
        {
            combat.OnAttack(false);
        }
    }
}
