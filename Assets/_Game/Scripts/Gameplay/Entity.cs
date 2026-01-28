using System;
using System.Collections;
using System.Collections.Generic;
using _Game.Scripts.Gameplay;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    private int _health;
    private Weapon _weapon;

    public Action OnDeathAction;
    public Action OnHitAction;

    // Start is called before the first frame update
    void Start()
    {
        _health = 0;
    }
    
    public void SetHealth(int health)
    {
        if (health < this._health) OnHit();
        CheckHealth();
        this._health = health;
    }

    public void LowerHealth(int amount)
    {
        OnHit();
        CheckHealth();
        this._health -= amount;
    }

    private void CheckHealth()
    {
        if (this._health <= 0) OnDeathAction();
    }

    public int GetHealth()
    {
        return _health;
    }

    public void SetWeapon(Weapon weapon)
    {
        _weapon = weapon;
    }

    public Weapon GetWeapon()
    {
        return _weapon;
    }

    protected void OnHit() 
    {
        OnHitAction?.Invoke();
    }
    
    protected void OnDeath() 
    {
        OnDeathAction?.Invoke();
    }
}
