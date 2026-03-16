using UnityEngine;
using System.Collections;
using DG.Tweening;

public class JumpingHugger : EnemyBase 
{
    [Header("Movement Settings")]
    public float jumpForceX = 4f;
    public float jumpForceY = 10f;
    public float landRestTime = 1.0f;
    [SerializeField] private float leftBoundary = -5f;
    [SerializeField] private float rightBoundary = 5f;

    [Header("Detection Settings")]
    public float hugRadius = 1.2f; 
    public float hugCooldownTime = 1.0f; // Thời gian chờ giữa 2 lần ôm

    private bool isGrounded, isHugging;
    private bool canHug = true; 
    private int currentDir = 1; 
    private Coroutine jumpCycleRoutine;
    private Transform targetPlayer;
    private Tween rotateTween; // Quản lý riêng Tween xoay

    protected override void Start() {
        base.Start();
        originalScale = transform.localScale;
        DetermineNextJumpDirection();
    }
    
    protected override void Update() {
        if (isStunned) return;

        // Nếu đang ôm, thoát Update sớm (logic Follow nằm ở LateUpdate)
        if (isHugging && targetPlayer != null) return;

        // Chỉ quét Player khi đang bay và không trong thời gian hồi chiêu
        if (!isGrounded && canHug) TryCatchPlayer();
    }

    // Dùng LateUpdate để đảm bảo vị trí và góc xoay được cập nhật SAU CÙNG
    void LateUpdate() {
        if (isHugging && targetPlayer != null) {
            FollowPlayerHead();
            // ÉP CỨNG GÓC XOAY: Không cho phép bất kỳ Tween nào làm nghiêng con chó
            transform.rotation = Quaternion.identity;
        }
    }

    void FollowPlayerHead() {
        Collider2D playerCol = targetPlayer.GetComponent<Collider2D>();
        if (playerCol != null) {
            // Ghim vào điểm cao nhất của Collider Player + một chút offset
            float topY = playerCol.bounds.max.y + 0.1f;
            transform.position = new Vector3(targetPlayer.position.x, topY, transform.position.z);
        }
    }

    void Jump() {
        if (isHugging || isStunned) return;
        
        DetermineNextJumpDirection();
        isGrounded = false;

        Vector3 targetPos = transform.position + new Vector3(currentDir * jumpForceX, 0, 0);
        float duration = 0.8f;

        StopMovement();

        // 1. Tween Nhảy
        transform.DOJump(targetPos, jumpForceY, 1, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                if (!gameObject.activeInHierarchy || isHugging) return;
                OnLanded();
            });

        // 2. Tween Xoay (Lưu vào biến để dễ Kill)
        rotateTween?.Kill();
        Sequence s = DOTween.Sequence();
        s.Append(transform.DORotate(new Vector3(0, 0, currentDir * 30f), duration * 0.4f).SetEase(Ease.OutQuad));
        s.Append(transform.DORotate(new Vector3(0, 0, currentDir * -40f), duration * 0.6f).SetEase(Ease.InQuad));
        rotateTween = s;
    }

    void OnLanded() {
        isGrounded = true;
        ResetPosture();
        rb.velocity = Vector2.zero;
        StartJumpCycle();
    }

    void StopMovement() {
        transform.DOKill();
        rotateTween?.Kill();
        if (jumpCycleRoutine != null) StopCoroutine(jumpCycleRoutine);
    }

    void ResetPosture() {
        rotateTween?.Kill();
        transform.rotation = Quaternion.identity;
        transform.localScale = originalScale;
    }

    void TryCatchPlayer() {
        if (isHugging || isStunned || !canHug) return;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, hugRadius);
        if (hit != null && hit.CompareTag("Player")) {
            StopMovement();
            StartCoroutine(HugRoutine(hit.transform));
        }
    }

    IEnumerator HugRoutine(Transform player) {
        isHugging = true;
        canHug = false; 
        targetPlayer = player;
    
        // Dừng tuyệt đối mọi Tween cũ
        transform.DOKill(true);
        rotateTween?.Kill();
        
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;

        // Reset về tư thế đứng thẳng ngay frame đầu tiên
        ResetPosture();

        yield return new WaitForSeconds(2.0f);
    
        if (gameObject.activeInHierarchy) Detach();
    }

    void Detach() {
        targetPlayer = null;
        ResetPosture();
        
        GetComponent<Collider2D>().enabled = true;
        rb.isKinematic = false;
        isHugging = isGrounded = false;
        
        // Văng ngược hướng vừa nhảy
        SetFacingDirection(-currentDir); 
        rb.velocity = new Vector2(-currentDir * 3f, 5f);

        StartCoroutine(HugCooldownRoutine());
    }

    IEnumerator HugCooldownRoutine() {
        yield return new WaitForSeconds(hugCooldownTime);
        canHug = true;
    }

    void StartJumpCycle() {
        if (!gameObject.activeInHierarchy) return;
        if (jumpCycleRoutine != null) StopCoroutine(jumpCycleRoutine);
        jumpCycleRoutine = StartCoroutine(WaitAndJump());
    }

    IEnumerator WaitAndJump() {
        yield return new WaitForSeconds(landRestTime);
        if (isGrounded && !isHugging && !isStunned) Jump();
    }

    protected override void OnCollisionEnter2D(Collision2D col) {
        base.OnCollisionEnter2D(col);
        if (col.gameObject.CompareTag("Ground") && !isHugging) {
            StopMovement();
            OnLanded();
        }
    }

    void DetermineNextJumpDirection() {
        currentDir = (Mathf.Abs(transform.position.x - leftBoundary) > Mathf.Abs(transform.position.x - rightBoundary)) ? -1 : 1;
        SetFacingDirection(currentDir);
    }

    void SetFacingDirection(float dir) {
        if (dir == 0) return;
        float facing = dir > 0 ? 1 : -1;
        transform.localScale = new Vector3(facing * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
    }
}