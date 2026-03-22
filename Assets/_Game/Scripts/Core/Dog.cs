using UnityEngine;
using System.Collections;

public class JumpingHugger : EnemyBase {
    [Header("Movement Settings")]
    public float jumpForceX = 5f;
    public float jumpForceY = 12f;
    public float landRestTime = 1.2f;
    [SerializeField] private float leftBoundary = -10f;
    [SerializeField] private float rightBoundary = 10f;

    [Header("Detection Settings")]
    public float hugRadius = 0.1f; 
    public float hugCooldownTime = 1.5f;

    private bool isGrounded = true, isHugging, canHug = true;
    private int currentDir = 1;
    private float nextJumpTime;

    protected override void OnEnable() {
        base.OnEnable();
        StopAllCoroutines();
        isHugging = false; isGrounded = true; canHug = true;
        rb.isKinematic = false; 
        rb.gravityScale = 0.5f; 
        GetComponent<Collider2D>().enabled = true;
        
        nextJumpTime = Time.time + landRestTime;
        DetermineNextJumpDirection();
    }

    protected override void Update() {
        if (isStunned || isHugging) return;

        if (!isGrounded && canHug && player != null) {
            float diffX = transform.position.x - player.position.x;
            if (Mathf.Abs(diffX) < hugRadius) {
                TryCatchPlayer();
            }
        }

        if (isGrounded && Time.time >= nextJumpTime) {
            Jump();
        }
    }

    void LateUpdate() {
        if (isHugging && player != null) {
            transform.rotation = Quaternion.identity;
            float topY = player.GetComponent<Collider2D>().bounds.max.y + 0.1f;
            transform.position = new Vector3(player.position.x, topY, transform.position.z);
        }
    }

    void Jump() {
        isGrounded = false;
        DetermineNextJumpDirection();

        Vector2 force = new Vector2(currentDir * jumpForceX, jumpForceY);
        rb.AddForce(force, ForceMode2D.Impulse);

        if (anim) anim.SetTrigger("jump"); // Nếu bạn có animation
    }

    void TryCatchPlayer() {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, hugRadius, LayerMask.GetMask("Player"));
        if (hit) {
            StartCoroutine(HugRoutine());
        }
    }

    IEnumerator HugRoutine() {
        isHugging = true; canHug = false;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true; 
        GetComponent<Collider2D>().enabled = false;
        
        if (anim) anim.SetTrigger("hug");

        yield return new WaitForSeconds(1.5f);
        
        Detach();
    }

    void Detach() {
        isHugging = false;
        rb.isKinematic = false;
        GetComponent<Collider2D>().enabled = true;
        
        // Văng ra sau khi ôm xong
        rb.velocity = new Vector2(-currentDir * 4f, 6f);
        
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown() {
        yield return new WaitForSeconds(hugCooldownTime);
        canHug = true;
        nextJumpTime = Time.time + landRestTime;
    }

    public override void GetHit(int damage, int hitType) {
        if (isHugging) Detach(); 
        
        base.GetHit(damage, hitType);
        
        nextJumpTime = Time.time + landRestTime; 
    }

    protected override void OnCollisionEnter2D(Collision2D col) {
        if (col.gameObject.CompareTag("Ground")) {
            isGrounded = true;
            rb.velocity = new Vector2(0, rb.velocity.y);
            nextJumpTime = Time.time + landRestTime;
            DetermineNextJumpDirection();
        }
    }

    void DetermineNextJumpDirection() {
        if (player) currentDir = (player.position.x > transform.position.x) ? 1 : -1;
        
        if (transform.position.x <= leftBoundary) currentDir = 1;
        else if (transform.position.x >= rightBoundary) currentDir = -1;
        
        transform.localScale = new Vector3(currentDir * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
    }
}