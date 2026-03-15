using System.Collections;
using System.Collections.Generic;
using _Game.Scripts.Core;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 8; 
    private int currentHealth;

    [Header("I-Frames Settings")]
    [SerializeField] private float invincibilityDuration = 1f;
    public bool IsInvincible { get; private set; }
    public bool IsDead => currentHealth <= 0;

    private void Start()
    {
        currentHealth = maxHealth;
        EventManager.current.onPlayerHealthUpdate(currentHealth);
    }

    // Trả về TRUE nếu nhận sát thương thành công, FALSE nếu bị chặn (bất tử/đã chết)
    public bool TakeHit(int damage = 1)
    {
        if (IsInvincible || IsDead) return false;

        currentHealth -= damage;
        EventManager.current.onPlayerHealthUpdate(currentHealth);
        
        if (currentHealth > 0)
        {
            StartCoroutine(InvincibilityRoutine());
        }

        return true;
    }

    private IEnumerator InvincibilityRoutine()
    {
        IsInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        IsInvincible = false;
    }
}

