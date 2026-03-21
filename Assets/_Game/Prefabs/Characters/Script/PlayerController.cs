using System;
using System.Collections;
using System.Collections.Generic;
using _Game.Prefabs.Characters.Script;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] private PlayerMovement movement;

    [SerializeField] private PlayerHealth health;

    // Check Ground
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private Transform groundCheck;

    // Check Death
    public event Action OnHitAction;
    public event Action OnDeathAction;

    // Nhom S
    public event Action OnPerformLowAttack;
    public event Action OnPerformSmash;

    // Nhom W
    public event Action OnPerformJumpAttack;
    public event Action OnPerformRisingAttack;
    public event Action OnPerformAirSpin;

    // Nhom A D
    public event Action<int> OnPerformAttack; 
    public event Action OnPerformUppercut;
    public event Action OnPerformAirAttack;

    // Weapon Action
    public event Action<bool> OnWeaponEquipped;


    public PlayerState state { get; private set; } = PlayerState.STANDING;

    [Header("Combo Settings")] [SerializeField]
    private float comboTimeout = 0.8f;

    private int comboStep = 0;
    private float lastAttackTime = 0f;

    [Header("Air Action Flags")] private bool hasUsedAirSpin = false;
    private bool hasDefeatedAirAttack = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    protected void Start()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnInputRight += HandleAttackRight;
            GameInput.Instance.OnInputLeft += HandleAttackLeft;
            GameInput.Instance.OnInputUp += HandleJump;
            GameInput.Instance.OnInputDown += HandleDuck;
        }
    }

    private void HandleAttackLeft()
    {
        if (state == PlayerState.DEATH) return;
        movement.SetFacing(Facing.LEFT);
        HandleAttack();
    }

    private void HandleAttackRight()
    {
        if (state == PlayerState.DEATH) return;
        movement.SetFacing(Facing.RIGHT);
        HandleAttack();
    }

    private void OnDestroy()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnInputRight -= HandleAttackRight;
            GameInput.Instance.OnInputLeft -= HandleAttackLeft;
            GameInput.Instance.OnInputUp -= HandleJump;
            GameInput.Instance.OnInputDown -= HandleDuck;
        }
    }

    private void FixedUpdate()
    {
        UpdatePhysicsGrounded();
    }

    private void UpdatePhysicsGrounded()
    {
        if (state == PlayerState.DEATH) return;

        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            if (state == PlayerState.AIRBORNE || state == PlayerState.SMASHING)
            {
                state = PlayerState.STANDING;

                ResetAirActions();
            }
        }
        else
        {
            if (state == PlayerState.STANDING || state == PlayerState.DUCKING)
            {
                state = PlayerState.AIRBORNE;

                ResetAirActions();
            }
        }
    }

    private void ResetAirActions()
    {
        hasUsedAirSpin = false;
        hasDefeatedAirAttack = false;
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (state == PlayerState.DEATH) return;

        if (health != null)
        {
            bool tookDamage = health.TakeHit(damage);

            if (tookDamage)
            {
                if (health.IsDead)
                {
                    state = PlayerState.DEATH;
                    OnDeathAction?.Invoke();
                }
                else
                {
                    OnHitAction?.Invoke();
                }
            }
        }
    }

    // Xu ly Action:
    private void HandleAttack()
    {
        switch (state)
        {
            case PlayerState.STANDING:
                if (Time.time - lastAttackTime > comboTimeout)
                {
                    comboStep = 0;
                }

                comboStep++;
                if (comboStep > 3) comboStep = 1;

                lastAttackTime = Time.time;

                OnPerformAttack?.Invoke(comboStep);
                break;

            case PlayerState.DUCKING:
                state = PlayerState.STANDING;
                OnPerformUppercut?.Invoke();
                break;

            case PlayerState.SMASHING:
                break;

            case PlayerState.AIRBORNE:
                if (!hasDefeatedAirAttack)
                {
                    hasDefeatedAirAttack = true;
                    OnPerformAirAttack?.Invoke();
                }
                break;
        }
    }

    private void HandleJump()
    {
        if (state == PlayerState.DEATH) return;

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
                if (!hasUsedAirSpin)
                {
                    hasUsedAirSpin = true; 
                    OnPerformAirSpin?.Invoke();
                }

                break;
        }
    }

    private void HandleDuck()
    {
        if (state == PlayerState.DEATH) return;

        switch (state)
        {
            case PlayerState.STANDING:
                state = PlayerState.DUCKING;
                OnPerformLowAttack?.Invoke();
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

    [Header("Weapon System")] 
    public WeaponTBScript currentWeapon;
    public bool weaponHasBeenUsedMelee = false;
    [SerializeField] private SpriteRenderer weaponSpriteRenderer;

    public void EquipWeapon(WeaponTBScript newWeapon)
    {
        if (newWeapon == null) 
        {
            currentWeapon = null;
            weaponHasBeenUsedMelee = false;
            
            if (weaponSpriteRenderer != null) 
            {
                weaponSpriteRenderer.sprite = null; 
                weaponSpriteRenderer.gameObject.SetActive(false);
            }
            OnWeaponEquipped?.Invoke(false);
            return;
        }

        currentWeapon = newWeapon;
        weaponHasBeenUsedMelee = false;
        
        if (weaponSpriteRenderer != null) 
        {
            weaponSpriteRenderer.sprite = newWeapon.worldSprite;
            weaponSpriteRenderer.gameObject.SetActive(true); 
        }
        else
        {
            Debug.LogWarning("Chưa kéo Weapon Sprite Renderer vào PlayerController!");
        }
        
        OnWeaponEquipped?.Invoke(true);
            
        Debug.Log("Player đã trang bị: " + newWeapon.weaponName);
    }
}