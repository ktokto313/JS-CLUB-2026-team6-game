using UnityEngine;
public class FlyObject : MonoBehaviour
{
    private Weapon weaponData;
    private Transform playerTransform; 
    private Rigidbody2D rb;
    
    private bool hasPassedPlayer = false;
    private bool isPlayerOwned = false; 
    private float leftBoundary = -20f;
    private float rightBoundary = 20f;
    public void Launch(Weapon data, Vector2 direction, float speed, Transform player, bool fromPlayer)
    {
        weaponData = data;
        playerTransform = player;
        isPlayerOwned = fromPlayer; 

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.velocity = direction * speed;

        gameObject.tag = "FlyObject"; 
    }

    void Update()
    {
        transform.Rotate(0, 0, 1000 * Time.deltaTime);
        CheckMapBoundaries();
        if (!isPlayerOwned && !hasPassedPlayer && playerTransform != null)
        {
            CheckIfPassedPlayer();
        }
    }
    public void GetCaught()
    {
        Destroy(this); 
    }
    private void CheckMapBoundaries()
    {
        float currentX = transform.position.x;
        if (currentX > rightBoundary || currentX < leftBoundary)
        {
            Debug.Log("FlyObject bay ra khỏi map -> Tự hủy");
            Destroy(gameObject);
        }
    }
    private void CheckIfPassedPlayer()
    {
        float directionToPlayer = playerTransform.position.x - transform.position.x;
        float moveDir = rb.velocity.x;
        
        if (Mathf.Sign(directionToPlayer) != Mathf.Sign(moveDir))
        {
            hasPassedPlayer = true;
            gameObject.tag = "DroppedWeapon"; 
            Debug.Log("Chuyển sang tag DroppedWeapon");
        }
    }
    
    public void Reflect(Vector2 newDir)
    {
        isPlayerOwned = true;
        gameObject.tag = "FlyObject";
        rb.velocity = newDir * 15f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPlayerOwned && collision.CompareTag("Enemy"))
        {
            collision.GetComponent<EnemyBase>().GetHit(weaponData.damage, 3);
            Destroy(gameObject);
        }
        else if (!isPlayerOwned && collision.CompareTag("Player") && !hasPassedPlayer)
        {
            // Player trừ máu
            Destroy(gameObject);
        }
    }
}