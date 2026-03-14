using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float jumpAttackForce = 16f; 
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float defaultGravity = 2f;
    
    // Biến lưu trạng thái hướng hiện tại
    private Facing currentFacing = Facing.RIGHT;

    
    [Header("Air Combat Controls")]
    [SerializeField] private float airAttackHangTime = 0.15f; 
    [SerializeField] private float airSpinHangTime = 0.4f;

    private void Start()
    {
        // Nhận sự kiện từ Controller
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnPerformJumpAttack += PerformJump;
            PlayerController.Instance.OnPerformRisingAttack += PerformJump;

            PlayerController.Instance.OnPerformSmash += PerformSmash;

            PlayerController.Instance.OnPerformAirAttack += PerformAirAttackHang;
            PlayerController.Instance.OnPerformAirSpin += PerformAirSpinHang;

            PlayerController.Instance.OnHitAction += HandleKnockbackOnHit;
        }
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnPerformJumpAttack -= PerformJump;
            PlayerController.Instance.OnPerformRisingAttack -= PerformJump;

            PlayerController.Instance.OnPerformSmash -= PerformSmash;

            PlayerController.Instance.OnPerformAirAttack -= PerformAirAttackHang;
            PlayerController.Instance.OnPerformAirSpin -= PerformAirSpinHang;

            PlayerController.Instance.OnHitAction -= HandleKnockbackOnHit;
        }
    }

    private void HandleKnockbackOnHit()
    {
        ApplyKnockback(6f);
    }

    public void SetFacing(Facing newFacing)
    {
        if (currentFacing != newFacing)
        {
            currentFacing = newFacing;
        
            Vector3 scale = transform.localScale;
            
            float size = Mathf.Abs(scale.x);

            scale.x = (currentFacing == Facing.RIGHT) ? size : -size;
            
            transform.localScale = scale;
        }
    }

    public void PerformJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0); // Reset Y velocity before jump
        rb.AddForce(Vector2.up * jumpAttackForce, ForceMode2D.Impulse);
    }
    
    public void PerformSmash()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0); // Reset Y velocity
        rb.AddForce(Vector2.down * jumpAttackForce * 1.5f, ForceMode2D.Impulse); // Rơi xuống nhanh hơn lúc nhảy lên
    }
    
    public void ApplyKnockback(float knockbackForce = 10f)
    {
        // Vì trục X bị khoá (freeze x), knockback hợp lý nhất là làm người chơi nảy nhẹ lên trên 
        // để tạo cảm giác bị hất tung (Stagger) và ngắt nhịp rơi.
        rb.velocity = new Vector2(rb.velocity.x, 0); 
        rb.AddForce(Vector2.up * knockbackForce, ForceMode2D.Impulse);
    }

    public void PerformAirAttackHang()
    {
        StartCoroutine(AirHangRoutine(airAttackHangTime));
    }

    public void PerformAirSpinHang()
    {
        StartCoroutine(AirHangRoutine(airSpinHangTime));
    }

    private System.Collections.IEnumerator AirHangRoutine(float hangTime)
    {
        // Đình chỉ trọng lực và dừng rơi
        float currentGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(rb.velocity.x, 0f);

        yield return new WaitForSeconds(hangTime);

        // Trả lại trọng lực
        rb.gravityScale = currentGravity != 0 ? currentGravity : defaultGravity;
    }
}