using UnityEngine;
using System.Collections;

public class ChargerEnemy : EnemyBase {
    [Header("Charger Fixed Settings")]
    private float targetX = 5f; 
    private float startX = -5f;  
    private float currentDestX;  

    public float dashSpeed = 18f;
    public float prepTime = 1.0f;
    private bool isDashing = false;

    protected override void Start() {
        base.Start();
        originalScale = transform.localScale;
        // Chọn điểm đích ban đầu là điểm xa quái nhất để bắt đầu vòng lặp
        currentDestX = (Mathf.Abs(transform.position.x - targetX) > Mathf.Abs(transform.position.x - startX)) ? targetX : startX;
    }

    protected override void Update() {
        if (player == null || isAirborne || isStunned || isPerformingAction) return;

        float distToDest = Mathf.Abs(transform.position.x - currentDestX);

        if (distToDest > 0.3f && !isDashing) {
            if (anim) anim.SetBool("walk", true);
            MoveToX(currentDestX);
        } 
        else if (!isPerformingAction) {
            StartCoroutine(ChargeSequence());
        }
    }

    void MoveToX(float x) {
        float dir = x > transform.position.x ? 1 : -1;
        transform.Translate(Vector3.right * (dir * moveSpeed * Time.deltaTime));
        transform.localScale = new Vector3(dir * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
    }

    IEnumerator ChargeSequence() {
        isPerformingAction = true;

        // BƯỚC 1: GỒNG (PREP)
        if (anim) {
            anim.SetBool("walk", false);
            anim.SetTrigger("prepDash"); 
        }
        rb.velocity = Vector2.zero;

        // Xác định điểm đối diện để lao tới
        float nextDestX = (currentDestX == targetX) ? startX : targetX;
        float dashDir = nextDestX > transform.position.x ? 1 : -1;
        
        // Quay mặt về hướng sẽ Dash
        transform.localScale = new Vector3(dashDir * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

        yield return new WaitForSeconds(prepTime);

        // BƯỚC 2: LAO (DASH)
        if (anim) anim.SetTrigger("dash");
        isDashing = true;

        while (isDashing) {
            rb.velocity = new Vector2(dashDir * dashSpeed, rb.velocity.y);
            CheckDamage();

            // Kiểm tra nếu đã chạm hoặc vượt qua tọa độ đích
            if ((dashDir > 0 && transform.position.x >= nextDestX) || 
                (dashDir < 0 && transform.position.x <= nextDestX)) {
                break;
            }
            yield return null;
        }

        // BƯỚC 3: DỪNG VÀ ĐỔI MỤC TIÊU
        rb.velocity = Vector2.zero;
        isDashing = false;
        currentDestX = nextDestX; // Lưu lại điểm vừa đến để làm mốc xuất phát mới

        yield return new WaitForSeconds(1.0f); // Nghỉ 1s như bạn yêu cầu
        isPerformingAction = false;
    }

    void CheckDamage() {
        // Kiểm tra trước mặt 1.5f có Player không
        float dir = transform.localScale.x > 0 ? 1 : -1;
        Vector2 checkPos = new Vector2(transform.position.x + (dir * 1.5f), transform.position.y);
        
        if (Mathf.Abs(checkPos.x - player.position.x) < 0.8f && Mathf.Abs(checkPos.y - player.position.y) < 1.5f) {
            PlayerController.Instance.TakeDamage();
        }
    }

    public override void GetHit(int damage, int hitType) {
        StopAllCoroutines(); // Ngắt ngay lập tức cú dash hoặc gồng
        isPerformingAction = false;
        isDashing = false;
        rb.velocity = Vector2.zero;

        base.GetHit(damage, hitType); // Chạy logic bị đẩy lùi và StunRoutine(1s) của cha
    }
}