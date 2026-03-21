using UnityEngine;
using _Game.Scripts.Core;
using System.Collections;

public class RocketRider : EnemyBase 
{
    [Header("Rocket Movement")]
    [SerializeField] private float dashSpeed = 10f; 
    [SerializeField] private float mapLimitLeft = -15f;
    [SerializeField] private float mapLimitRight = 15f;
    [SerializeField] private float[] phaseHeights = { 5f, 3f, 1f };

    [Header("Rocket Explosion")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float explosionRadius = 2.5f; 

    private int currentPhaseIndex = 0;
    private int dirX = 1;
    private bool isExploded = false;

    // HÀM QUAN TRỌNG NHẤT CHO POOLING
    protected override void OnEnable() {
        base.OnEnable(); // Reset Health, anim, velocity trong EnemyBase

        // Reset trạng thái riêng của Rocket
        isExploded = false;
        currentPhaseIndex = 0;
        isStunned = false;
        isAirborne = false;
        
        // Reset Vật lý
        if (rb != null) {
            rb.gravityScale = 0;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Reset Rotation (về nằm ngang)
        transform.rotation = Quaternion.identity;

        // Đặt lại vị trí độ cao ban đầu nếu có
        if (phaseHeights != null && phaseHeights.Length > 0) {
            transform.position = new Vector3(transform.position.x, phaseHeights[0], transform.position.z);
        }

        // Quyết định hướng bay ban đầu (ví dụ: luôn bay về phía player hoặc theo vị trí map)
        dirX = transform.position.x > 0 ? -1 : 1; 
        UpdateFacing();
    }

    protected override void Start() {
        base.Start();
        // Các thiết lập cố định không đổi trong suốt vòng đời object
        moveSpeed = dashSpeed;
    }

    protected override void Update() {
        if (isExploded) return;

        // Nếu bị đánh trúng hoặc đang rơi
        if (isStunned || isAirborne) {
            if (rb.gravityScale == 0) rb.gravityScale = 2f;
            RotateTowardsVelocity();
            CheckDamageLikeCharger(); 
            return;
        }

        HandleFlight();
        CheckBoundaries();
        CheckDamageLikeCharger(); 
    }

    private void HandleFlight() {
        if (rb != null) {
            rb.velocity = new Vector2(dirX * moveSpeed, 0);
        }
    }

    void CheckDamageLikeCharger() {
        if (isExploded || player == null) return;

        float facingDir = dirX; // Dùng dirX trực tiếp cho chính xác hướng bay
        Vector2 checkPos = new Vector2(transform.position.x + (facingDir * 1.0f), transform.position.y);

        float diffX = Mathf.Abs(checkPos.x - player.position.x);
        float diffY = Mathf.Abs(checkPos.y - player.position.y);

        if (diffX < 1.2f && diffY < 1.5f) {
            Explode();
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
        // Không gọi CheckAndDropWeapon vì Rocket thường nổ chứ không rơi vũ khí (tùy bạn)
    }

    protected override void OnCollisionEnter2D(Collision2D collision) {
        // Nếu chạm đất hoặc tường khi đang bay/rơi -> Nổ
        if (collision.gameObject.CompareTag("Ground")) {
            Explode();
        }
    }

    private void Explode() {
        if (isExploded) return;
        isExploded = true;

        // Ngừng mọi chuyển động vật lý
        if (rb != null) rb.velocity = Vector2.zero;

        if (explosionEffect != null) {
            // Dùng Pool cho Effect nếu có, nếu không thì Instantiate
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        if (Vector2.Distance(transform.position, player.position) <= explosionRadius) {
            PlayerController.Instance.TakeDamage();
            EventManager.current.onPlayerHit(transform.position);
        }

        // Trả về Pool
        onDeath();
    }

    private void UpdateFacing() {
        // originalScale.x có thể âm hoặc dương tùy prefab, nên dùng Mathf.Abs để tránh bị ngược
        transform.localScale = new Vector3(dirX * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
    }

    private void CheckBoundaries() {
        bool hitRight = dirX == 1 && transform.position.x >= mapLimitRight;
        bool hitLeft = dirX == -1 && transform.position.x <= mapLimitLeft;

        if (hitRight || hitLeft) {
            dirX *= -1;
            UpdateFacing();
            MoveToNextPhase();
        }
    }

    private void MoveToNextPhase() {
        if (currentPhaseIndex < phaseHeights.Length - 1) {
            currentPhaseIndex++;
            transform.position = new Vector3(transform.position.x, phaseHeights[currentPhaseIndex], transform.position.z);
        }
    }

    private void RotateTowardsVelocity() {
        if (rb != null && rb.velocity.sqrMagnitude > 0.1f) {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}