using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Gameplay;

public abstract class Entity : MonoBehaviour
{
    public int Health { set; get;  }
    [SerializeField]
    private Weapon _weapon;
    
    public Action OnDeathAction;
    public Action OnHitAction;
    
    // Start is called before the first frame update
    void Start()
    {
        Health = 0;
    }

    protected void onHit() 
    {
        OnHitAction?.Invoke();
    }
    
    protected void onDeath() 
    {
        OnDeathAction?.Invoke();
    }
}
