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
    public event Action<int> OnPerformAttack; // Truyền theo Combo Step
    public event Action OnPerformUppercut;
    public event Action OnPerformAirAttack;


    public PlayerState state { get; private set; } = PlayerState.STANDING;

    [Header("Combo Settings")]
    [SerializeField] private float comboTimeout = 0.8f; 
    private int comboStep = 0;
    private float lastAttackTime = 0f;

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
        if (groundCheck == null) return;
        
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
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

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    // Xu ly Death
    public void TakeDamage(int damage = 1)
    {
        if (state == PlayerState.DEATH) return;

        if (health != null)
        {
            // Trả về true nếu thực sự nhận sát thương (không bị chặn bởi I-frames)
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
                    // Gọi sự kiện để Movement và Animation tự xử lý (Observer)
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
                OnPerformAirAttack.Invoke();
                break;
        }
    }

    private void HandleJump()
    {
        switch (state)
        {
            case PlayerState.STANDING:
                state = PlayerState.AIRBORNE; // Đảm bảo chuyển state ngay lập tức
                OnPerformJumpAttack?.Invoke();
                break;

            case PlayerState.DUCKING:
                state = PlayerState.AIRBORNE; // Tương tự bảo vệ State
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
    public void EquipWeapon(WeaponTBScript newWeapon)
    {
        currentWeapon = newWeapon;
        Debug.Log("Player đã trang bị: " + newWeapon.weaponName);
    
        // Ở đây bạn có thể thêm logic thay đổi Sprite trên tay Player nếu muốn
    }
}