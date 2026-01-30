using UnityEngine;

public class JumpMoster : EnemyBase
{
    [Header("Spear Rider Settings")]
    public float jumpForce = 12f;
    public float jumpHorizontalSpeed = 5f;
    public float mapLimitLeft = -10f;
    public float mapLimitRight = 10f;
    private bool isRiding = true;
    private int jumpDirection = 1;

    protected override void Update()
    {
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
    public override void GetHit(int hitType)
    {
        if (isRiding)
        {
            isRiding = false;
            isAirborne = true;
            float pushDir = transform.position.x < player.position.x ? -1f : 1f;
            rb.velocity = Vector2.zero;
            rb.AddForce(new Vector2(pushDir * 8f, 0f), ForceMode2D.Impulse);
            base.LookAtPlayer();
        }
        else
        {
            base.GetHit(hitType);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isRiding && collision.CompareTag("Player"))
        {
            Debug.Log("Player bị giáo đâm trúng!");
        }
    }
}