using UnityEngine;

public class FlyObject : MonoBehaviour
{
    private WeaponTBScript _weaponTbScriptData;
    private Vector3 playerPos;
    private Rigidbody2D rb;
    
    private bool hasPassedPlayer = false;
    private bool isPlayerOwned = false; 
    private float leftBoundary = -20f;
    private float rightBoundary = 20f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void OnEnable()
    {
        hasPassedPlayer = false;
        isPlayerOwned = false;
    }

    public void Launch(WeaponTBScript data, Vector2 direction, float speed, Vector3 playerPos, bool fromPlayer)
    {
        _weaponTbScriptData = data;
        this.playerPos = playerPos;
        isPlayerOwned = fromPlayer; 

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.velocity = direction * speed;

        gameObject.tag = "FlyObject"; 
    }

    void Update()
    {
        transform.Rotate(0, 0, 1000 * Time.deltaTime);
        CheckMapBoundaries();

        if (!isPlayerOwned && !hasPassedPlayer) 
        {
            CheckIfPassedPlayer();
        }
    }
    
    public void GetCaught()
    {
        this.enabled = false; 
    }

    private void CheckMapBoundaries()
    {
        float currentX = transform.position.x;
        if (currentX > rightBoundary || currentX < leftBoundary)
        {
            GlobalPoolManager.Instance.Return(gameObject);
        }
    }

    private void CheckIfPassedPlayer()
    {
        float directionToPlayer = playerPos.x - transform.position.x;
        float moveDir = rb.velocity.x;
        
        if (Mathf.Sign(directionToPlayer) != Mathf.Sign(moveDir))
        {
            hasPassedPlayer = true;
            gameObject.tag = "DroppedWeapon"; 
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
            collision.GetComponent<EnemyBase>().GetHit(_weaponTbScriptData.damage, 3);
            GlobalPoolManager.Instance.Return(gameObject); 
        }
        else if (!isPlayerOwned && collision.CompareTag("Player") && !hasPassedPlayer)
        {
            // Logic Player trừ máu ở đây
            GlobalPoolManager.Instance.Return(gameObject); 
        }
    }
}