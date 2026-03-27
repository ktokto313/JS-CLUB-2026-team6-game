using System;
using System.Collections;
using UnityEngine;
using _Game.Scripts.Core;
using _Game.Prefabs.Characters.Script;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerHealth health;

    [Header("Check Ground")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private Transform groundCheck;

    // --- EVENTS ---
    public event Action OnPerformLowAttack;
    public event Action OnPerformSmash;
    public event Action OnPerformJumpAttack;
    public event Action OnPerformRisingAttack;
    public event Action OnPerformAirSpin;
    public event Action OnPerformAttack; 
    public event Action OnPerformUppercut;
    public event Action OnPerformAirAttack;

    private PlayerState _state = PlayerState.STANDING;
    public PlayerState state 
    { 
        get => _state; 
        private set 
        {
            if (_state == PlayerState.DEATH && value != PlayerState.DEATH) return;
            _state = value;
        }
    }

    [Header("Air Action Flags")] 
    private bool hasUsedAirSpin = false;
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

    private void OnEnable()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnInputRight += HandleAttackRight;
            GameInput.Instance.OnInputLeft += HandleAttackLeft;
            GameInput.Instance.OnInputUp += HandleJump;
            GameInput.Instance.OnInputDown += HandleDuck;
        }
    }

    private void OnDisable()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnInputRight -= HandleAttackRight;
            GameInput.Instance.OnInputLeft -= HandleAttackLeft;
            GameInput.Instance.OnInputUp -= HandleJump;
            GameInput.Instance.OnInputDown -= HandleDuck;
        }
    }

    private void FixedUpdate() => UpdatePhysicsGrounded();

    private void UpdatePhysicsGrounded()
    {
        if (state == PlayerState.DEATH) return;

        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;

        if (isGrounded)
        {
            if (state == PlayerState.AIRBORNE)
            {
                state = PlayerState.STANDING;
                ResetAirActions();
            } else if (state == PlayerState.SMASHING)
            {
                state = PlayerState.DUCKING;
                ResetAirActions();
            }
        }
        else if (state == PlayerState.STANDING || state == PlayerState.DUCKING)
        {
            state = PlayerState.AIRBORNE;
            ResetAirActions();
        }
    }

    private void ResetAirActions()
    {
        hasUsedAirSpin = false;
        hasDefeatedAirAttack = false;
    }

    public void TakeDamage(int damage = 1)
    {
        if (state == PlayerState.DEATH) return;
    
        if (TryGetComponent(out PlayerMovement movementScript) && !movementScript.CanHit())
        {
            return;
        }
        
        if (health != null && health.TakeHit(damage))
        {
            
            if (health.IsDead)
            {
                state = PlayerState.DEATH;
                StartCoroutine(HandleDeathRoutine(1.3f));
            }
            else
            {
                EventManager.current?.onPlayerHit(transform.position);
            }
        }
    }
    
    private IEnumerator HandleDeathRoutine(float delayTime)
    {
        if (TryGetComponent(out PlayerAttack attackScript))
        {
            attackScript.DropWeapon();
        }

        if (PlayerController.Instance != null) 
        {
            PlayerController.Instance.enabled = false; 
        }

        if (TryGetComponent(out Rigidbody2D rb))
        {
            rb.gravityScale = 10f; 

            rb.velocity = Vector2.zero; 

            rb.simulated = false; 
        }
        
        yield return new WaitForSeconds(delayTime);

        EventManager.current?.onPlayerDead();
    }

    public void OnGetHuggedByDog(float stunDuration)
    {
        if (state == PlayerState.DEATH) return; 

        state = PlayerState.STUNNED;
        if (TryGetComponent(out Rigidbody2D rb)) rb.velocity = Vector2.zero;

        if (TryGetComponent(out PlayerAttack attackScript))
        {
            attackScript.DropWeapon();
        }

        StartCoroutine(StunRoutine(stunDuration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (state != PlayerState.DEATH) state = PlayerState.STANDING;
    }

    private void HandleAttackLeft()
    {
        if (state == PlayerState.DEATH || state == PlayerState.STUNNED) return;
        movement.SetFacing(Facing.LEFT);
        HandleAttack();
    }

    private void HandleAttackRight()
    {
        if (state == PlayerState.DEATH || state == PlayerState.STUNNED) return;
        movement.SetFacing(Facing.RIGHT);
        HandleAttack();
    }

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
        if (state == PlayerState.DEATH || state == PlayerState.SMASHING || state == PlayerState.STUNNED) return;

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
        if (state == PlayerState.DEATH || state == PlayerState.SMASHING || state == PlayerState.STUNNED) return;

        switch (state)
        {
            case PlayerState.STANDING:
            case PlayerState.DUCKING: 
                state = PlayerState.DUCKING;
                OnPerformLowAttack?.Invoke();
                break;
            case PlayerState.AIRBORNE:
                state = PlayerState.SMASHING;
                OnPerformSmash?.Invoke();
                break;
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
}