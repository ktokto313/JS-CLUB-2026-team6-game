using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Entity
{
    public event Action OnJumpAction;
    public event Action OnAttackAction;

    public bool IsDucking { get; private set; }
    public bool IsOnAir { get; private set; }

    public PlayerController Instance { get; private set; }

    private Facing Facing = Facing.RIGHT;

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
        IsOnAir = false;

        // Bỏ if null check để nếu quên tạo GameInput thì Unity báo lỗi ngay
        // Hoặc giữ lại nhưng thêm Debug.LogError để biết
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnJump += HandleJump;
            GameInput.Instance.OnAttackLeft += HandleAttackLeft;
            GameInput.Instance.OnAttackRight += HandleAttackRight;
        }
        else
        {
            Debug.LogError("FATAL ERROR: Không tìm thấy GameInput trong Scene!");
        }
    }

    private void OnDestroy()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnJump -= HandleJump;
            GameInput.Instance.OnAttackLeft -= HandleAttackLeft;
            GameInput.Instance.OnAttackRight -= HandleAttackRight;
        }
    }

    private void Update()
    {
        if (GameInput.Instance.IsDucking && !IsOnAir)
        {
            IsDucking = true;
        }
        else
        {
            IsDucking = false;
        }
    }

    private void HandleAttackRight()
    {
        SetFacing(Facing.RIGHT);
        PerformAttack();
    }

    private void HandleAttackLeft()
    {
        SetFacing(Facing.LEFT);
        PerformAttack();
    }

    private void PerformAttack()
    {
        OnAttackAction?.Invoke();
    }

    private void HandleJump()
    {
        OnJumpAction?.Invoke();
    }

    private void SetFacing(Facing newFacing)
    {
        if (Facing != newFacing)
        {
            Facing =  newFacing;
            Vector3 scale = transform.localScale;
            float size = Mathf.Abs(scale.x);
            // BƯỚC 2: Gán dấu dựa trên hướng
            // Nếu quay Phải -> 0.5. Nếu quay Trái -> -0.5
            scale.x = (Facing == Facing.RIGHT) ? size : -size;
            transform.localScale = scale;
        }
        
    }
}