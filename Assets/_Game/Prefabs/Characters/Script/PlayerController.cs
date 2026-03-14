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
    [SerializeField] private GroundSensor groundSensor;

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
    public event Action OnPerformAttack;
    public event Action OnPerformUppercut;
    public event Action OnPerformAirAttack;


    public PlayerState state { get; private set; } = PlayerState.STANDING;

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
        movement.SetFacing(Facing.LEFT);
        HandleAttack();
    }

    private void HandleAttackRight()
    {
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

    private bool wasGrounded = true;

    private void UpdatePhysicsGrounded()
    {
        if (groundSensor == null) return;
        
        bool isGrounded = groundSensor.IsGrounded;
        
        // Only trigger log and state change when grounded state changes
        if (isGrounded != wasGrounded)
        {
            Debug.Log($"Ground Check Changed: {isGrounded} | state: {state}");
            wasGrounded = isGrounded;
        }

        if (isGrounded)
        {
            // === CHẠM ĐẤT ===
            if (state == PlayerState.AIRBORNE || state == PlayerState.SMASHING)
            {
                // Nếu đang lao xuống (Smash) -> Nổ (Sau này thêm logic)
                if (state == PlayerState.SMASHING) Debug.Log("Smash Landed!");

                state = PlayerState.STANDING;
            }
        }
        else
        {
            // === TRÊN KHÔNG ===
            // Nếu đang Đứng/Ngồi mà hẫng chân -> Airborne
            // (Giữ nguyên nếu đang Smash)
            if (state == PlayerState.STANDING || state == PlayerState.DUCKING)
            {
                state = PlayerState.AIRBORNE;
            }
        }
    }

    // Xu ly Death
    public void TakeDamage(int damage = 1)
    {
        if (state == PlayerState.DEATH) return;

        if (health != null)
        {
            bool isDead = health.TakeHit(damage);

            if (isDead)
            {
                state = PlayerState.DEATH;
                OnDeathAction?.Invoke();
            }
            else
            {
                // Bị đẩy nảy lên khi nhận sát thương
                if (movement != null) movement.ApplyKnockback(6f);
                OnHitAction?.Invoke();
            }
        }
    }

    // Xu ly Action:
    private void HandleAttack()
    {
        switch (state)
        {
            case PlayerState.STANDING:
                OnPerformAttack?.Invoke();
                break;

            case PlayerState.DUCKING:
                state = PlayerState.STANDING;
                OnPerformUppercut?.Invoke();
                break;

            case PlayerState.SMASHING:
                break;

            case PlayerState.AIRBORNE:
                if (movement != null) movement.PerformAirAttackHang(); // <-- Khựng trên không khi đánh
                OnPerformAirAttack.Invoke();
                break;
        }
    }

    private void HandleJump()
    {
        switch (state)
        {
            case PlayerState.STANDING:
                movement.PerformJump();
                OnPerformJumpAttack?.Invoke();
                break;

            case PlayerState.DUCKING:
                movement.PerformJump();
                OnPerformRisingAttack?.Invoke();
                break;

            case PlayerState.SMASHING:
                break;

            case PlayerState.AIRBORNE:
                if (movement != null) movement.PerformAirSpinHang(); // <-- Khựng trên không khi đá xoáy
                OnPerformAirSpin?.Invoke();
                break;
        }
    }

    private void HandleDuck()
    {
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
                movement.PerformSmash(); // <-- Kích hoạt rơi cực nhanh
                OnPerformSmash?.Invoke();
                break;
        }
    }

    [Header("Weapon System")]
    public WeaponTBScript currentWeapon;
    public void EquipWeapon(WeaponTBScript newWeapon)
    {
        currentWeapon = newWeapon;
        Debug.Log("Player đã trang bị: " + newWeapon.weaponName);
    
        // Ở đây bạn có thể thêm logic thay đổi Sprite trên tay Player nếu muốn
    }
}