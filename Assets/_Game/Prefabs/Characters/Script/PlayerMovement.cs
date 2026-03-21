using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float jumpAttackForce = 16f; 
    [SerializeField] private Rigidbody2D rb;
    
    // Biến lưu trạng thái hướng hiện tại
    private Facing currentFacing = Facing.RIGHT;

    
    [Header("Air Combat Controls")]
    [SerializeField] private float airSpinUpwardForce = 12f;
    
    private void Start()
    {
        // Nhận sự kiện từ Controller
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnPerformJumpAttack += PerformJump;
            PlayerController.Instance.OnPerformRisingAttack += PerformJump;

            PlayerController.Instance.OnPerformSmash += PerformSmash;

            PlayerController.Instance.OnPerformAirSpin += PerformAirSpinHang;
        }
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnPerformJumpAttack -= PerformJump;
            PlayerController.Instance.OnPerformRisingAttack -= PerformJump;

            PlayerController.Instance.OnPerformSmash -= PerformSmash;

            PlayerController.Instance.OnPerformAirSpin -= PerformAirSpinHang;
        }
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

    public void PerformAirSpinHang()
    {
        HopInAir(airSpinUpwardForce);
    }

    private void HopInAir(float upwardForce)
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * upwardForce, ForceMode2D.Impulse);
    }
}