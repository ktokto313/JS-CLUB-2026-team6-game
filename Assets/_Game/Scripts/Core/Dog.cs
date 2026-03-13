using UnityEngine;
using System.Collections;

public class JumpingHugger : EnemyBase 
{
    [Header("Movement Settings")]
    public float jumpForceX = 2f;
    public float jumpForceY = 1f;
    public float landRestTime = 1.0f;
    public float leftBoundary = -5f;
    public float rightBoundary = 5f;

    [Header("Detection Settings")]
    public float hugRadius = 1f; // Bán kính để "dính" vào Player

    private bool isGrounded = false;
    private bool isHugging = false;
    

    protected override void Start() {
        // Lấy thông tin player từ GameManager (đã có trong EnemyBase)
        base.Start(); 
        originalScale = transform.localScale;
        
        // Bắt đầu: Rơi tự do
        isGrounded = false;
        isHugging = false;
        rb.isKinematic = false;
    }

    // GHI ĐÈ HOÀN TOÀN Update của EnemyBase
    protected override void Update() {
        if ( isStunned) return;

        // Nếu đang ôm, khóa chặt vị trí vào Player
        if (isHugging) {
            return; 
        }

        // Logic 1: Kiểm tra xem có chạm Player khi đang bay không
        if (!isGrounded) {
            CheckForHug();
        }
    }

    void CheckForHug() {
        // Quét vùng xung quanh xem có Player không
        Collider2D hit = Physics2D.OverlapCircle(transform.position, hugRadius);
        if (hit != null && hit.CompareTag("Player")) {
            StopAllCoroutines();
            StartCoroutine(HugRoutine(hit.gameObject));
        }
    }

    void Jump() {
        if ( isHugging || isStunned) return;

        // Xác định hướng nhảy dựa trên vị trí Player
        float direction = (player.position.x > transform.position.x) ? 1 : -1;
        
        // Kiểm tra nếu chạm biên thì ép quay đầu
        if (transform.position.x >= rightBoundary) direction = -1;
        else if (transform.position.x <= leftBoundary) direction = 1;
        LookAtPlayer();

        // Nhảy
        isGrounded = false;
        rb.velocity = new Vector2(direction * jumpForceX, jumpForceY);
        
        Debug.Log("Quái: Nhảy về hướng " + direction);
    }

    // Xử lý va chạm đất
    protected new void OnCollisionEnter2D(Collision2D col) {
        if ( isHugging) return;

        if (col.gameObject.CompareTag("Ground")) {
            // Kiểm tra chạm mặt trên của Ground (Normal vector hướng lên)
            if (col.contacts[0].normal.y > 0.5f) {
                isGrounded = true;
                rb.velocity = Vector2.zero;
                StartCoroutine(WaitAndJump());
                Debug.Log("Quái: Đã chạm đất");
            }
        }
    }

    IEnumerator WaitAndJump() {
        yield return new WaitForSeconds(landRestTime);
        if (isGrounded && !isHugging) {
            Jump();
        }
    }

    IEnumerator HugRoutine(GameObject target) {
        isHugging = true;
        isGrounded = false;

        // Vô hiệu hóa vật lý
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;

        // Làm "con" của Player
        transform.SetParent(target.transform);
        transform.localPosition = new Vector3(0, 0.2f, 0); // Vị trí ngang mặt
        
        Debug.Log("Quái: ĐANG ÔM PLAYER");

        yield return new WaitForSeconds(2.0f); // Ôm trong 2s
        
        Detach();
    }

    void Detach() {
        transform.SetParent(null);
        GetComponent<Collider2D>().enabled = true;
        rb.isKinematic = false;
        isHugging = false;
        isGrounded = false;

        // Văng ra phía sau
        float bounceDir = (player.position.x > transform.position.x) ? -1 : 1;
        rb.velocity = new Vector2(bounceDir * 3f, 5f);
        
        Debug.Log("Quái: Đã nhả Player");
    }



    // Gọi từ Player khi chém trúng
    public override void GetHit(int damage, int hitType) {
        if (isHugging) Detach();
        base.GetHit(damage, hitType);
    }
}