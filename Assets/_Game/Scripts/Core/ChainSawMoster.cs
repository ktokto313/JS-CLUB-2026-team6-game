using UnityEngine;
using System.Collections;

public class ChargerEnemy : EnemyBase {
    [Header("Charger Fixed Settings")]
    private float right = 5f; 
    private float left = -5f;  
    private float currentDestX;  

    public float dashSpeed = 5f;
    public float prepTime = 2f;
    private bool isDashing = false;

    protected override void Start() {
        base.Start();
        originalScale = transform.localScale;

        float distToA = Mathf.Abs(transform.position.x - left); 
        float distToB = Mathf.Abs(transform.position.x - right); 

        currentDestX = (distToA < distToB) ? left : right;
    }

    protected override void Update() {
        if (isAirborne || isStunned || isPerformingAction) return;

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

        //(PREP)
        if (anim) {
            anim.SetBool("walk", false);
            anim.SetTrigger("prepDash"); 
        }
        rb.velocity = Vector2.zero;

        // Xác định điểm đối diện để lao tới
        float nextDestX = (currentDestX == right) ? left : right;
        float dashDir = nextDestX > transform.position.x ? 1 : -1;
        
        // Quay mặt về hướng sẽ Dash
        transform.localScale = new Vector3(dashDir * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

        yield return new WaitForSeconds(prepTime);

        // (DASH)
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

        //DỪNG VÀ ĐỔI MỤC TIÊU
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
            Debug.Log("Hit Playyer :########");
        }
    }

    public override void GetHit(int damage, int hitType) {
        StopAllCoroutines(); 
        isPerformingAction = false;
        isDashing = false;
        rb.velocity = Vector2.zero;
        // Chạy logic  của cha
        base.GetHit(damage, hitType);
    }
}