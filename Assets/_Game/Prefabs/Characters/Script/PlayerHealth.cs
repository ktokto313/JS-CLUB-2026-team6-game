using System.Collections;
using System.Collections.Generic;
using _Game.Scripts.Core;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 8; 
    private int currentHealth;

    public bool IsDead => currentHealth <= 0;

    private void Start()
    {
        currentHealth = maxHealth;
        EventManager.current.onPlayerHealthUpdate(currentHealth);
    }

    // Trả về TRUE nếu nhận sát thương thành công, FALSE nếu bị chặn (đã chết)
    public bool TakeHit(int damage = 1)
    {
        if (IsDead)
        {
            EventManager.current?.onPlayerDead();
            return false;
        }

        currentHealth -= damage;
        EventManager.current.onPlayerHealthUpdate(currentHealth);

        return true;
    }
}

