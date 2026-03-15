using UnityEngine;
using System.Collections;

public class JumpingHugger : EnemyBase 
{
    [Header("Movement Settings")]
    public float jumpForceX = 4f;
    public float jumpForceY = 8f;
    public float landRestTime = 1.0f;
    public float leftBoundary = -5f;
    public float rightBoundary = 5f;

    [Header("Detection Settings")]
    public float hugRadius = 0.8f; 

    private bool isGrounded = false;
    private bool isHugging = false;
    private int currentDir = 1; 
    private int lastJumpDir = 1;
    private Coroutine jumpRoutine;

    protected override void Start() {
        base.Start(); 
        originalScale = transform.localScale;
        InitialFacingSetup();
    }
    
    protected override void Update() {
        if (isStunned || isHugging) return;
        if (!isGrounded) CheckForHug();
    }

    // --- HƯỚNG NHÌN ---
    void InitialFacingSetup() {
        if (transform.position.x >= rightBoundary) currentDir = -1;
        else if (transform.position.x <= leftBoundary) currentDir = 1;
        else currentDir = (Mathf.Abs(transform.position.x - leftBoundary) > Mathf.Abs(transform.position.x - rightBoundary)) ? -1 : 1;
        
        SetFacingDirection(currentDir);
        lastJumpDir = currentDir; 
    }

    void DetermineNextJumpDirection() {
        currentDir = (Mathf.Abs(transform.position.x - leftBoundary) > Mathf.Abs(transform.position.x - rightBoundary)) ? -1 : 1;
        SetFacingDirection(currentDir);
    }

    void SetFacingDirection(float dir) {
        if (dir != 0) {
            float facing = dir > 0 ? 1 : -1;
            transform.localScale = new Vector3(facing * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
    }

    // --- DI CHUYỂN ---
    void Jump() {
        if (isHugging || isStunned) return;

        if (anim != null) anim.SetTrigger("jump");

        DetermineNextJumpDirection(); 
        lastJumpDir = currentDir;   
        isGrounded = false;

        // Tắt vật lý tạm thời để Code Transform làm chủ hoàn toàn
     //   rb.isKinematic = true; 
       // rb.velocity = Vector2.zero;

    }

    

    protected override void OnCollisionEnter2D(Collision2D col) {
        base.OnCollisionEnter2D(col); // Reset isAirborne của cha
        if (isHugging) return;

        if (col.gameObject.CompareTag("Ground")) {
            if (anim != null) anim.SetTrigger("land");
            if (col.contacts[0].normal.y > 0.5f) {
                isGrounded = true;
                rb.velocity = Vector2.zero;
                StartJumpCycle();
            }
        }
    }

    void StartJumpCycle() {
        if (jumpRoutine != null) StopCoroutine(jumpRoutine);
        jumpRoutine = StartCoroutine(WaitAndJump());
    }

    IEnumerator WaitAndJump() {
        yield return new WaitForSeconds(landRestTime);
        if (isGrounded && !isHugging && !isStunned) Jump();
        else if (isGrounded && !isHugging) StartJumpCycle(); // Đợi tiếp nếu vẫn đang bị stun
    }

    // --- VA CHẠM PLAYER ---
    void CheckForHug() {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, hugRadius);
        if (hit != null && hit.CompareTag("Player")) {
            if (jumpRoutine != null) StopCoroutine(jumpRoutine); 
            StartCoroutine(HugRoutine(hit.gameObject));
        }
    }

    IEnumerator HugRoutine(GameObject target) {
        if (anim != null) anim.SetBool("isHugging", true);
        isHugging = true;
        isGrounded = false;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;
        transform.SetParent(target.transform);
        transform.localPosition = new Vector3(0, 0.4f, 0); 
        yield return new WaitForSeconds(2.0f);
        Detach();
    }

    void Detach() {
        if (anim != null) anim.SetBool("isHugging", false);
        transform.SetParent(null);
        GetComponent<Collider2D>().enabled = true;
        rb.isKinematic = false;
        isHugging = false;
        isGrounded = false;
        SetFacingDirection(-lastJumpDir); 
        rb.velocity = new Vector2(-lastJumpDir * 3f, 5f);
    }

    // --- SỬA LỖI ĐƠ ---
    public override void GetHit(int damage, int hitType) {
        if (isHugging) Detach();
    
        if (jumpRoutine != null) StopCoroutine(jumpRoutine);

        // 1. Gọi base GetHit trước để xử lý trừ máu và Death logic
        base.GetHit(damage, hitType);

        // 2. KIỂM TRA: Nếu sau khi trúng đòn mà quái đã chết hoặc bị ẩn đi (do Death logic ở Base)
        // thì thoát ngay, không chạy logic hồi phục nữa để tránh lỗi "Inactive"
        if (!gameObject.activeInHierarchy || Health <= 0) return;

        // 3. Nếu vẫn còn sống thì mới chạy logic hồi phục
        StopCoroutine("RecoveryLogic");
        StartCoroutine("RecoveryLogic");
    }

    IEnumerator RecoveryLogic() {
        // Chờ đến khi lớp cha reset isStunned về false
        while (isStunned) yield return null;

        // Nếu lúc này đang ở trên mặt đất (bị đẩy lùi ngang) thì kích hoạt nhảy
        if (!isHugging && !isAirborne) {
            isGrounded = true; // Ép trạng thái ground vì nó ko rời đất
            StartJumpCycle();
        }
    }
}