using _Game.Scripts.Core;
using UnityEngine;

public class FlyObject : MonoBehaviour
{
    private WeaponTBScript _weaponTbScriptData;
    private Transform playerTransform; 
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

    public void Launch(WeaponTBScript data, Vector2 direction, float speed, Transform player, bool fromPlayer)
    {
        _weaponTbScriptData = data;
        playerTransform = player;
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
        
        if (!isPlayerOwned && !hasPassedPlayer && playerTransform != null)
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
        float moveDir = rb.velocity.x;

        // Nếu rìu bay sang phải (moveDir > 0) và tọa độ x đã > 0
        // HOẶC rìu bay sang trái (moveDir < 0) và tọa độ x đã < 0
        if ((moveDir > 0 && transform.position.x > 0) || (moveDir < 0 && transform.position.x < 0))
        {
            Debug.Log("<color=red>ĐÃ Doi trang thai!</color>");
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
        // TRƯỜNG HỢP 1: Rìu của QUÁI ném (Chưa bị Player phản đòn)
        if (!isPlayerOwned) 
        {
            // A. Đâm trúng Player (Chỉ tính khi CHƯA vượt qua x=0)
            if (collision.CompareTag("Player") && !hasPassedPlayer)
            {
                if (!hasPassedPlayer)
                {
                    Vector3 impactPosition = (gameObject.transform.position + playerTransform.position) / 2;
                    EventManager.current.onHit(impactPosition);
                    Debug.Log("<color=red>ĐÃ GÂY SÁT THƯƠNG CHO PLAYER!</color>");
                    GlobalPoolManager.Instance.Return(gameObject);
                }
            }

            // B. Đâm trúng Quái khác (Chỉ tính sau khi ĐÃ vượt qua x=0)
            if (collision.CompareTag("Enemy") && hasPassedPlayer)
            {
                HandleEnemyHit(collision);
            }
        }
        // TRƯỜNG HỢP 2: Rìu đã bị Player phản đòn (isPlayerOwned = true)
        else if (collision.CompareTag("Enemy"))
        {
            HandleEnemyHit(collision);
        }
    }

	// Tách hàm xử lý trúng quái để dùng chung, tránh bị trễ
    private void HandleEnemyHit(Collider2D collision)
    {
        EnemyBase enemy = collision.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            // Gây sát thương ngay lập tức
            enemy.GetHit(_weaponTbScriptData.damage, 3);
            Vector3 impactPosition = (gameObject.transform.position + enemy.transform.position) / 2;
            EventManager.current.onHit(impactPosition);
            Debug.Log("<color=red>ĐÃ GÂY SÁT THƯƠNG CHO Mosterrrrr!</color>");
            // Trả về Pool ngay lập tức để không bị hiện tượng "khựng" 1 giây
            GlobalPoolManager.Instance.Return(gameObject);
        }
    }
    public WeaponTBScript GetWeaponData()
    {
        return _weaponTbScriptData;
    }
}