using UnityEngine;
using System.Collections;
using _Game.Scripts.Core;

public class ChargerEnemy : EnemyBase {
    [Header("Charger Fixed Settings")]
    [SerializeField] private float rightBoundary = 5f; 
    [SerializeField] private float leftBoundary = -5f;  
    public float spearRadius = 0.5f;
    public float offsetForward = 0.5f;
    public float dashSpeed = 12f;
    public float prepTime = 0.5f;
    private bool isDashing = false;
    private bool hasHitPlayerInThisDash;

    protected override void OnEnable()
    {
        base.OnEnable();
        hasHitPlayerInThisDash = false;
        isDashing = false;
    }
    protected override void Update() {
        if (isAirborne || isStunned || isPerformingAction) return;

        if (transform.position.x > rightBoundary + 0.2f) {
            MoveToX(rightBoundary);
            return;
        } 
        else if (transform.position.x < leftBoundary - 0.2f) {
            MoveToX(leftBoundary);
            return;
        }

        if (!isPerformingAction) {
            StartCoroutine(ChargeSequence());
        }
    }

    void MoveToX(float targetX) {
        if (anim) anim.SetBool("walk", true);
        float dir = targetX > transform.position.x ? 1 : -1;
        
        // Sử dụng Velocity thay vì Translate để mượt hơn với hệ thống GetHit của bạn
        rb.velocity = new Vector2(dir * moveSpeed, rb.velocity.y);
        
        transform.localScale = new Vector3(dir * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

        // Nếu đã đến rất gần điểm cần về biên, dừng lại để chuẩn bị Dash
        if (Mathf.Abs(transform.position.x - targetX) < 0.2f) {
            rb.velocity = new Vector2(0, rb.velocity.y);
            if (anim) anim.SetBool("walk", false);
        }
    }

    IEnumerator ChargeSequence() {
        isPerformingAction = true;
        hasHitPlayerInThisDash = false;

        // (PREP)
        if (anim) {
            anim.SetBool("walk", false);
            anim.SetTrigger("prepDash");
        }
        rb.velocity = Vector2.zero;

        // TỰ ĐỘNG XÁC ĐỊNH ĐÍCH: 
        // Nếu đang đứng gần biên trái -> Dash sang phải Boundary. Và ngược lại.
        float targetX = (transform.position.x < (leftBoundary + rightBoundary) / 2f) ? rightBoundary : leftBoundary;
        float dashDir = targetX > transform.position.x ? 1 : -1;
        
        transform.localScale = new Vector3(dashDir * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

        yield return new WaitForSeconds(prepTime);

        // (DASH)
        if (anim) anim.SetTrigger("dash");
        isDashing = true;

        while (isDashing) {
            rb.velocity = new Vector2(dashDir * dashSpeed, rb.velocity.y);
            
            if (!hasHitPlayerInThisDash) CheckDamage();

            if ((dashDir > 0 && transform.position.x >= rightBoundary) || 
                (dashDir < 0 && transform.position.x <= leftBoundary)) {
                break;
            }
            yield return null;
        }

        // KẾT THÚC DASH
        rb.velocity = new Vector2(0, rb.velocity.y);
        isDashing = false;
        anim.SetTrigger("rotate");
        yield return new WaitForSeconds(1.5f); 
        isPerformingAction = false;
    }

    void CheckDamage() {
        if (visualAxe == null) return;
        float facingDir = transform.localScale.x > 0 ? 1 : -1;

        Vector2 checkPos = (Vector2)visualAxe.transform.position + new Vector2(facingDir * offsetForward, 0);

        // 3. Quét Player
        Collider2D hit = Physics2D.OverlapCircle(checkPos, spearRadius, LayerMask.GetMask("Player"));

        if (hit != null) {
            hasHitPlayerInThisDash = true;
            PlayerController.Instance.TakeDamage();
        }
    }
    public override void GetHit(int damage, int hitType) {
        isDashing = false;
        isPerformingAction = false;
        hasHitPlayerInThisDash = false;
        rb.velocity = new Vector2(0, rb.velocity.y);
    
        base.GetHit(damage, hitType);
    }
}