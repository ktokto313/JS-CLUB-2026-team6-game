using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 8; 
    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public bool TakeHit(int damage = 1)
    {
        currentHealth -= damage;
        return currentHealth <= 0;
    }
}

