using System;
using System.Collections;
using System.Collections.Generic;
using _Game.Prefabs.Characters.Script;
using UnityEngine;

public class PlayerController : Entity
{
    public static PlayerController Instance { get; private set; }
    
    // Nhom S
    public event Action OnPerformLowAttack;
    public event Action OnPerformSmash;
    
    // Nhom W
    public event Action OnPerformJumpAttack;
    public event Action OnPerformRisingAttack;
    public event Action OnPerformAirSpin;
    
    // Nhom A D
    public event Action<Facing> OnPerformAttack;
    public event Action<Facing> OnPerformUppercut;
    public event Action<Facing> OnPerformAirAttack;


    public PlayerState state { get; private set; } = PlayerState.STANDING;

    private Facing facing = Facing.RIGHT;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    protected override void Start()
    {
        base.Start();

        
        
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnInputAttack += HandleAttack;
            GameInput.Instance.OnInputJump += HandleJump;
            GameInput.Instance.OnInputDuck += HandleDuck;
        }
        
    }

    private void Update()
    {
        // UpdatePhysicsGrounded();
    }

    // Xu ly Action:
    private void HandleAttack(Facing newFacing) 
    {
        SetFacing(newFacing);
        switch (state)
        {
            case PlayerState.STANDING:
                OnPerformAttack?.Invoke(facing);
                break;
            
            case PlayerState.DUCKING:
                state = PlayerState.STANDING;
                OnPerformUppercut?.Invoke(facing);
                break;
            
            case PlayerState.SMASHING:
                break;
            
            case PlayerState.AIRBORNE:
                OnPerformAirAttack.Invoke(facing);
                break;
        }
        
    }
    
    private void HandleJump()
    {
        switch (state)
        {
            case PlayerState.STANDING:
                state = PlayerState.AIRBORNE;
                OnPerformJumpAttack?.Invoke();
                break;
            
            case PlayerState.DUCKING:
                state = PlayerState.AIRBORNE;
                OnPerformRisingAttack?.Invoke();
                break;
            
            case PlayerState.SMASHING:
                break;
            
            case PlayerState.AIRBORNE:
                OnPerformAirSpin?.Invoke();
                break;
        }
    }

    private void HandleDuck()
    {
        switch (state)
        {
            case PlayerState.STANDING:
                OnPerformLowAttack?.Invoke();
                state = PlayerState.DUCKING;
                break;
            
            case PlayerState.DUCKING:
                OnPerformLowAttack?.Invoke();
                break;
            
            case PlayerState.SMASHING:
                break;
            
            case PlayerState.AIRBORNE:
                state = PlayerState.SMASHING;
                OnPerformSmash?.Invoke();
                break;
        }
        
    }
    
    
    
    
    // Private Helper

    private void SetFacing(Facing newFacing)
    {
        if (facing != newFacing)
        {
            facing = newFacing;
            
            Vector3 scale = transform.localScale;
            
            float size = Mathf.Abs(scale.x);

            scale.x = (facing == Facing.RIGHT) ? size : -size;
            transform.localScale = scale;
        }
    }
}