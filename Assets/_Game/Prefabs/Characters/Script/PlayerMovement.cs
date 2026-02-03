using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float jumpAttackForce = 16f; 
    [SerializeField] private Rigidbody2D rb;
    
    // Biến lưu trạng thái hướng hiện tại
    private Facing currentFacing = Facing.RIGHT;

    // --- CÁC HÀM PUBLIC (API) ĐỂ CONTROLLER GỌI ---
    
    public void SetFacing(Facing newFacing)
    {
        // Chỉ xử lý nếu có sự thay đổi hướng để tối ưu hiệu năng
        if (currentFacing != newFacing)
        {
            currentFacing = newFacing;
        
            Vector3 scale = transform.localScale;
            
            // Lấy giá trị tuyệt đối để đảm bảo scale gốc luôn dương
            float size = Mathf.Abs(scale.x);

            // Gán dấu: RIGHT là dương, LEFT là âm
            scale.x = (currentFacing == Facing.RIGHT) ? size : -size;
            
            transform.localScale = scale;
        }
    }

    public void PerformJump()
    {
        
        rb.velocity = new Vector2(rb.velocity.x, 0);

        rb.AddForce(Vector2.up * jumpAttackForce, ForceMode2D.Impulse);
    }
}