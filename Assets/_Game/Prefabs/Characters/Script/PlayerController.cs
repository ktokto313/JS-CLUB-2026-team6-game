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
    [SerializeField] private LayerMask groundLayer; // Lớp đất
    [SerializeField] private float groundCheckRadius = 0.2f;

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

    private Facing facing = Facing.RIGHT;

    [SerializeField] private Transform groundCheck;

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

    private void Update()
    {
        UpdatePhysicsGrounded();
    }

    private void UpdatePhysicsGrounded()
    {
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        Debug.Log($"Ground Check: {isGrounded} | Radius: {groundCheckRadius}");
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
    public void TakeDamage()
    {
        if (state == PlayerState.DEATH) return;

        if (health != null)
        {
            bool isDead = health.TakeHit();

            if (isDead)
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