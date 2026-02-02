using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityTest : MonoBehaviour
{
    private int Health { set; get;  }
    private WeaponTest Weapon { get; set; }

    public Action OnDeathAction;
    public Action OnHitAction;
    
    // Start is called before the first frame update
    protected virtual void Start()
    {
        Health = 0;
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

internal class WeaponTest
{
}
