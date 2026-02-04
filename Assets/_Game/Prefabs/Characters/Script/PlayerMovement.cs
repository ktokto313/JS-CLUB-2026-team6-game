using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float jumpAttackForce = 16f; 
    [SerializeField] private Rigidbody2D rb;
    
    // Biến lưu trạng thái hướng hiện tại
    private Facing currentFacing = Facing.RIGHT;

    
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
        
        rb.velocity = new Vector2(rb.velocity.x, 0);

        rb.AddForce(Vector2.up * jumpAttackForce, ForceMode2D.Impulse);
    }
    
    
}