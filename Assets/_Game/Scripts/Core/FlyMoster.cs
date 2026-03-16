using UnityEngine;
using _Game.Scripts.Core;

public class RocketRider : EnemyBase 
{
    [Header("Rocket Movement")]
    [SerializeField] private float dashSpeed = 10f; // Tên lửa nên bay nhanh
    [SerializeField] private float mapLimitLeft = -15f;
    [SerializeField] private float mapLimitRight = 15f;
    [SerializeField] private float[] phaseHeights = { 5f, 3f, 1f };

    [Header("Rocket Explosion")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float explosionRadius = 2.5f; 

    private int currentPhaseIndex = 0;
    private int dirX = 1;
    private bool isExploded = false;

    protected override void Start() {
        base.Start();
        Health = 1; 
        rb.gravityScale = 0;
        moveSpeed = dashSpeed;

        if (phaseHeights.Length > 0) {
            transform.position = new Vector3(transform.position.x, phaseHeights[0], transform.position.z);
        }
        UpdateFacing();
    }

    protected override void Update() {
        // Nếu đã nổ thì biến mất luôn, không chạy gì thêm
        if (isExploded) return;

        // Nếu bị đánh trúng (Stun/Airborne từ EnemyBase)
        if (isStunned || isAirborne) {
            if (rb.gravityScale == 0) rb.gravityScale = 2f;
            RotateTowardsVelocity();
            CheckDamageLikeCharger(); // Quan trọng: Đang rơi trúng player vẫn phải nổ
            return;
        }

        HandleFlight();
        CheckBoundaries();
        CheckDamageLikeCharger(); // Gây dame liên tục khi đang bay
    }

    private void HandleFlight() {
        rb.velocity = new Vector2(dirX * moveSpeed, 0);
    }

    // COPY CHUẨN LOGIC GÂY DAME CỦA CHARGER
    void CheckDamageLikeCharger() {
        if (isExploded || player == null) return;

        // Charger dùng: checkPos = transform.position + (dir * 1.5f)
        // Với Rocket, ta để 1.0f để nó sát mũi hơn
        float facingDir = transform.localScale.x > 0 ? 1 : -1;
        Vector2 checkPos = new Vector2(transform.position.x + (facingDir * 1.0f), transform.position.y);

        // Tăng độ rộng kiểm tra (0.8f -> 1.2f) để dễ trúng Player hơn
        float diffX = Mathf.Abs(checkPos.x - player.position.x);
        float diffY = Mathf.Abs(checkPos.y - player.position.y);

        if (diffX < 1.2f && diffY < 1.5f) {
            Debug.Log("Rocket đâm trúng Player bằng tọa độ!");
            Explode();
        }
    }

    public override void GetHit(int damage, int hitType) {
        if (isExploded) return;
        
        // Không gọi base.GetHit để bỏ qua logic máu 5, combo...
        Health = 0;
        isStunned = true; 
        rb.gravityScale = 2f;
        
        // Văng ra khi bị đánh (giữ nguyên hướng văng vật lý)
        float pushDir = transform.position.x < player.position.x ? -1f : 1f;
        rb.velocity = Vector2.zero;
        rb.AddForce(new Vector2(pushDir * 5f, 5f), ForceMode2D.Impulse);

        if (anim) anim.SetTrigger("gethit");
    }

    protected override void OnCollisionEnter2D(Collision2D collision) {
        // Luôn check va chạm với đất
        if (collision.gameObject.CompareTag("Ground")) {
            Explode();
        }
    }

    private void Explode() {
        if (isExploded) return;
        isExploded = true;

        if (explosionEffect != null) {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Gây sát thương nổ diện rộng (Radius nổ)
        if (Vector2.Distance(transform.position, player.position) <= explosionRadius) {
            PlayerController.Instance.TakeDamage();
            EventManager.current.onPlayerHit(transform.position);
        }

        OnDeathAction?.Invoke(); 
    }

    // --- CÁC LOGIC HỖ TRỢ KHÁC ---
    private void UpdateFacing() {
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
        if (rb.velocity.sqrMagnitude > 0.1f) {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}