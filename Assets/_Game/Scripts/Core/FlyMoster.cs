using UnityEngine;
using _Game.Scripts.Core;
using System.Collections;

public class RocketRider : EnemyBase 
{
    [Header("Rocket Movement")]
    [SerializeField] private float dashSpeed = 12f; 
    [SerializeField] private float mapLimitLeft = -15f;
    [SerializeField] private float mapLimitRight = 15f;
    [SerializeField] private float[] phaseHeights = { 5f, 3f, 1f };

    [Header("Rocket Explosion")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float explosionRadius = 3f; // Tăng nhẹ để dễ trúng
    [SerializeField] private float headOffset = 0.8f; 

    private int currentPhaseIndex = 0;
    private int dirX = 1;
    private bool isExploded = false;

    protected override void OnEnable() {
        base.OnEnable(); 
        isExploded = false;
        currentPhaseIndex = 0;
        isStunned = false;
        isAirborne = false;
        
        if (rb != null) {
            rb.gravityScale = 0;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        transform.rotation = Quaternion.identity;

        if (phaseHeights != null && phaseHeights.Length > 0) {
            transform.position = new Vector3(transform.position.x, phaseHeights[0], transform.position.z);
        }

        // --- SỬA LỖI HƯỚNG BAN ĐẦU ---
        if (GameManager.Instance != null) player = GameManager.Instance.PlayerTransform;
    
        // Đảm bảo lấy vị trí mới nhất để tính hướng
        if (player != null) {
            // Nếu quái ở bên PHẢI player (x quái > x player) -> bay sang TRÁI (dirX = -1)
            dirX = (transform.position.x > player.position.x) ? -1 : 1;
        } else {
            dirX = (transform.position.x > 0) ? -1 : 1; 
        }
    
        // Gọi trực tiếp để lật hình ảnh ngay lập tức trước khi Update chạy
        UpdateFacing();
    }

    private void Update() {
        if (isExploded) return;

        if (isStunned || isAirborne) {
            if (rb.gravityScale == 0) rb.gravityScale = 2f;
            RotateTowardsVelocity();
            CheckCollisionWithPlayer(); 
            return;
        }

        HandleFlight();
        CheckBoundaries();
        CheckCollisionWithPlayer(); 
    }

    private void CheckCollisionWithPlayer() {
        if (isExploded || player == null) return;

        // Dùng dirX trực tiếp để tính vị trí mũi quét cho chính xác
        Vector2 headPos = (Vector2)transform.position + new Vector2(dirX * headOffset, 0);

        // Dùng OverlapCircle để quét va chạm (nhạy hơn so với distance)
        Collider2D hit = Physics2D.OverlapCircle(headPos, 0.7f);
        if (hit != null && hit.CompareTag("Player")) {
            Explode();
        }
    }

    private void HandleFlight() {
        if (rb != null) {
            rb.velocity = new Vector2(dirX * dashSpeed, 0);
        }
    }

    private void CheckBoundaries() {
        bool hitRight = (dirX == 1 && transform.position.x >= mapLimitRight);
        bool hitLeft = (dirX == -1 && transform.position.x <= mapLimitLeft);

        if (hitRight || hitLeft) {
            dirX *= -1; 
            UpdateFacing(); // Chỉ quay đầu khi chạm biên
            MoveToNextPhase();
        }
    }

    private void UpdateFacing() {
        float sX = Mathf.Abs(originalScale.x);
    
        // Nếu Model gốc hướng sang PHẢI: dùng dirX * sX
        // Nếu Model gốc hướng sang TRÁI: dùng -dirX * sX
        transform.localScale = new Vector3(dirX * sX, originalScale.y, originalScale.z);

        // Khóa góc quay khi đang bay thẳng
        if (!isStunned && !isAirborne) {
            transform.rotation = Quaternion.identity;
        }
    }

    public override void GetHit(int damage, int hitType) {
        if (isExploded) return;
        Health = 0;
        isStunned = true; 
        rb.gravityScale = 2f;
        
        float pushDir = transform.position.x < player.position.x ? -1f : 1f;
        rb.velocity = Vector2.zero;
        rb.AddForce(new Vector2(pushDir * 5f, 5f), ForceMode2D.Impulse);
        if (anim) anim.SetTrigger("gethit");
    }

    protected override void OnCollisionEnter2D(Collision2D collision) {
        if (isExploded) return;
        // Chạm đất hoặc bất kỳ vật cản nào cũng nổ
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Player")) {
            Explode();
        }
    }

    private void Explode() {
        if (isExploded) return;
        isExploded = true;

        // Lấy vị trí nổ ngay tại thời điểm va chạm
        Vector3 impactPos = transform.position;

        if (rb != null) {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
        }

        if (explosionEffect != null) {
            Instantiate(explosionEffect, impactPos, Quaternion.identity);
        }

        // --- SỬA LỖI KHÔNG GÂY DAMAGE ---
        // Quét diện rộng tại điểm nổ để ép Player phải nhận sát thương
        Collider2D[] hits = Physics2D.OverlapCircleAll(impactPos, explosionRadius);
        bool playerDamaged = false;
        foreach (var hit in hits) {
            if (hit.CompareTag("Player")) {
                PlayerController.Instance.TakeDamage();
                EventManager.current.onPlayerHit(impactPos);
                playerDamaged = true;
                break;
            }
        }

        // Dự phòng nếu quét Circle hụt do Player di chuyển quá nhanh
        if (!playerDamaged && player != null) {
            if (Vector2.Distance(impactPos, player.position) <= explosionRadius) {
                PlayerController.Instance.TakeDamage();
                EventManager.current.onPlayerHit(impactPos);
            }
        }

        OnDeathAction?.Invoke();
    }

    private void MoveToNextPhase() {
        if (currentPhaseIndex < phaseHeights.Length - 1) {
            currentPhaseIndex++;
            transform.position = new Vector3(transform.position.x, phaseHeights[currentPhaseIndex], transform.position.z);
        } else {
            Explode();
        }
    }

    private void RotateTowardsVelocity() {
        if (rb != null && rb.velocity.sqrMagnitude > 0.1f) {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnDrawGizmos() {
        // Gizmos dùng dirX để hiển thị chính xác vùng quét nổ
        Vector2 headPos = (Vector2)transform.position + new Vector2(dirX * headOffset, 0);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(headPos, 0.7f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}