using UnityEngine;

public class JumpMoster : EnemyBase
{
    [Header("Spear Rider Settings")]
    public float jumpForce = 12f;
    public float jumpHorizontalSpeed = 5f;
    public float mapLimitLeft = -15f;
    public float mapLimitRight = 15f;
    
    private bool isRiding = true;
    private int jumpDirection = 1;

    protected override void Update()
    {
        if (Health <= 0) return;

        if (isRiding)
        {
            HandleJumpingLogic();
        }
        else
        {
            base.Update();
        }
    }

    void HandleJumpingLogic()
    {
        if (Mathf.Abs(rb.velocity.y) < 0.01f) 
        {
            Vector2 jumpForceVector = new Vector2(jumpDirection * jumpHorizontalSpeed, jumpForce);
            rb.AddForce(jumpForceVector, ForceMode2D.Impulse);
        }

        if (transform.position.x >= mapLimitRight) jumpDirection = -1;
        if (transform.position.x <= mapLimitLeft) jumpDirection = 1;

        transform.localScale = new Vector3(jumpDirection, 1, 1);
    }

    public override void GetHit(int damage, int hitType)
    {
        Health -= damage;
        if (Health <= 0) { onDeath(); return; }

        if (isRiding)
        {
            isRiding = false;
            base.Invoke("SpawnDroppedWeapon", 0f); 
            CurrentWeaponTbScript = null; 

            rb.velocity = Vector2.zero;
            float pushDir = transform.position.x < playerPos.x ? -1f : 1f;
            rb.AddForce(new Vector2(pushDir * 8f, 0f), ForceMode2D.Impulse);
            
            base.LookAtPlayer();
        }
        else
        {
            base.GetHit(damage, hitType); 
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Chỉ gây sát thương khi đang cưỡi giáo nhảy
        if (isRiding && collision.CompareTag("Player"))
        {
            Debug.Log("Player bị giáo đâm!");
            // Logic trừ máu player...
        }
    }
}