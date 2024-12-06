using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log(currentHealth);
    }
}
