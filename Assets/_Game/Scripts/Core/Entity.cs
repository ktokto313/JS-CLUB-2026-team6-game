using System;
using System.Collections;
using System.Collections.Generic;
using _Game.Scripts.Core;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public int Health { set; get;  }
    public float moveSpeed { set; get;  }
    private WeaponTBScript _weaponTbScript;

    public Action OnDeathAction;
    public Action OnHitAction;
    
    // Start is called before the first frame update
    void Start()
    {
        Health = 0;
        moveSpeed = 0;
    }

    protected void onHit() 
    {
        OnHitAction?.Invoke();
        EventManager.current?.onHit(transform.position);
    }
    
    protected void onDeath() 
    {
        OnDeathAction?.Invoke();
        EventManager.current?.onDead();
    }
}
