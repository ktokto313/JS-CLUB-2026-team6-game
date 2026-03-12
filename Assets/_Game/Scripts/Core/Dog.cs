using UnityEngine;
using System.Collections;

public class JumpingHugger : MonoBehaviour
{
    [Header("Health")]
    public int health = 2;

    [Header("Jump Settings")]
    public float jumpForceX = 7f; 
    public float jumpForceY = 12f; 
    public float latchDuration = 2f; 
    public float landStunTime = 0.5f; 

    [Header("Patrol Settings")]
    public float leftBoundary = -10f;  
    public float rightBoundary = 10f; 
    private int targetDir = 1;        

    private Rigidbody2D rb;
    private Animator anim;
    private bool isHugging = false;
    private bool isDead = false;
    private bool isGrounded = true;
    private bool isLanding = false;
    private Vector3 originalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        originalScale = transform.localScale;
    }

    void OnEnable() 
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // 1. Reset các biến logic: Phải để FALSE để nó hiểu là đang rơi tự do
        isGrounded = false; 
        isLanding = false;
        isHugging = false;
        isDead = false;

        // 2. ÉP VẬT LÝ HOẠT ĐỘNG
        rb.bodyType = RigidbodyType2D.Dynamic; 
        rb.isKinematic = false; 
        rb.gravityScale = 3f; // Đảm bảo gravity đủ nặng để kéo nó xuống
        rb.velocity = Vector2.zero; 
        rb.WakeUp(); // Đánh thức Rigidbody ngay lập tức
    
        // 3. Bật lại Collider
        if(GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = true;

        // 4. KHÔNG gọi JumpNext ngay. Hãy để nó rơi chạm đất rồi mới nhảy cú đầu tiên
        CancelInvoke("JumpNext");
    }

    void Start()
    {
        targetDir = (transform.position.x > rightBoundary) ? -1 : 1;
        // Xóa Invoke JumpNext ở đây, để va chạm đất (OnCollisionEnter) tự kích hoạt cú nhảy
    }
    void Update()
    {
        if (isDead || isHugging) return;
        if (!isGrounded && !isLanding)
        {
            if (rb.velocity.y < -0.1f)
            {
                // Đang rơi xuống
                anim.SetBool("isInAir", true);
            }
        }
    }

    // --- HÀM NHẢY ---
    void JumpNext()
    {
        if (isHugging || isDead || isLanding) return;
        
        if (transform.position.x >= rightBoundary) targetDir = -1;
        else if (transform.position.x <= leftBoundary) targetDir = 1;

        transform.localScale = new Vector3(targetDir * originalScale.x, originalScale.y, originalScale.z);

        isGrounded = false;
        anim.SetTrigger("jump"); 
        anim.SetBool("isInAir", false);
        anim.SetBool("isLanding", false);
        
        rb.velocity = new Vector2(targetDir * jumpForceX, jumpForceY);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        // Chạm Player
        if (!isHugging && collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(HugPlayerRoutine(collision.gameObject));
            return;
        }
        
        // Chạm đất
        if (collision.gameObject.CompareTag("Ground"))
        {
            // Chỉ tiếp đất nếu đang RƠI XUỐNG (vận tốc Y <= 0) 
            // Điều này ngăn việc nó tự nhận đất ngay lúc vừa bấm nhảy
            if (!isGrounded && rb.velocity.y <= 0.1f) 
            {
                StartCoroutine(LandingRoutine());
            }
        }
    }

    IEnumerator LandingRoutine()
    {
        isLanding = true;
        isGrounded = true;
        rb.velocity = Vector2.zero; 

        anim.SetBool("isInAir", false);
        anim.SetBool("isLanding", true);

        yield return new WaitForSeconds(landStunTime);

        anim.SetBool("isLanding", false);
        isLanding = false;

        if (!isDead && !isHugging) JumpNext();
    }

    // --- TRẠNG THÁI ÔM MẶT ---
    IEnumerator HugPlayerRoutine(GameObject playerObj)
    {
        isHugging = true;
        StopCoroutine(LandingRoutine()); 

        // Khóa Player (Yêu cầu PlayerController có biến canMove)
        if(PlayerController.Instance != null) {
            //PlayerController.Instance.canMove = false;
        }
        playerObj.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        // Dính vào Player
        rb.isKinematic = true; 
        rb.velocity = Vector2.zero;
        transform.SetParent(playerObj.transform);
        transform.localPosition = new Vector3(0, 0.8f, 0); // Vị trí ngang mặt

        anim.SetBool("isHugging", true);

        yield return new WaitForSeconds(latchDuration);

        if (!isDead) Detach(playerObj);
    }

    void Detach(GameObject playerObj)
    {
        transform.SetParent(null);
        rb.isKinematic = false;
        isHugging = false;

        // Văng ngược ra sau khi thả
        float bounceDir = playerObj.transform.localScale.x > 0 ? -1 : 1;
        rb.AddForce(new Vector2(bounceDir * 5f, 5f), ForceMode2D.Impulse);

        //if(PlayerController.Instance != null) PlayerController.Instance.canMove = true;
        anim.SetBool("isHugging", false);
    }

    // --- SÁT THƯƠNG & CHẾT ---
    public void GetHit(int damage)
    {
        if (isDead) return;
        health -= damage;
        anim.SetTrigger("gethit");

        if (health <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        StopAllCoroutines();
        
        if (isHugging)
        {
            //if (PlayerController.Instance != null) PlayerController.Instance.canMove = true;
            transform.SetParent(null);
        }

        anim.SetTrigger("death");
        rb.isKinematic = false;
        rb.velocity = new Vector2(Random.Range(-2f, 2f), 5f);
        GetComponent<Collider2D>().enabled = false;

        Invoke("DestroySelf", 1.5f);
    }

    void DestroySelf() => GlobalPoolManager.Instance.Return(gameObject);

}