using System;
using _Game.Scripts.Gameplay;
using UnityEngine;

public class Entity : MonoBehaviour
{
    private int _health;
    [SerializeField]
    private Projectile projectile;

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
        this._health -= amount;
        CheckHealth();
    }

    private void CheckHealth()
    {
        if (this._health <= 0) OnDeathAction();
    }

    public int GetHealth()
    {
        return _health;
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
