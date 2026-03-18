using _Game.Scripts.Core;
using UnityEngine;

public class FlyObject : MonoBehaviour
{
    private WeaponTBScript _weaponTbScriptData;
    private Transform playerTransform; 
    private Rigidbody2D rb;
    private bool hasPassedPlayer = false;
    private bool isPlayerOwned = false; 
    private bool isReturning = false;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void OnEnable()
    {
        isReturning = false; // Reset cờ
        hasPassedPlayer = false;
        isPlayerOwned = false;
        gameObject.tag = "FlyObject";
        CancelInvoke();
    }

    public void Launch(WeaponTBScript data, Vector2 direction, float speedOverride, Transform player, bool fromPlayer)
    {
        _weaponTbScriptData = data;
        playerTransform = player;
        isPlayerOwned = fromPlayer; 

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        // DÙNG FLYSPEED TỪ SCRIPTABLE OBJECT (nếu có), nếu không thì dùng speed truyền vào
        float finalSpeed = (_weaponTbScriptData.flySpeed > 0) ? _weaponTbScriptData.flySpeed : speedOverride;
        rb.velocity = direction * finalSpeed;

        gameObject.tag = "FlyObject"; 

        // DÙNG LIFETIME ĐỂ TỰ HỦY THAY VÌ BIÊN
        float lifeTime = (_weaponTbScriptData.lifeTime > 0) ? _weaponTbScriptData.lifeTime : 5f; 
        Invoke("ReturnToPool", lifeTime);
    }

    void Update()
    {
        // Hiệu ứng xoay rìu
        transform.Rotate(0, 0, 1000 * Time.deltaTime);
        
        if (!isPlayerOwned && !hasPassedPlayer && playerTransform != null)
        {
            CheckIfPassedPlayer();
        }
    }

    private void ReturnToPool()
    {
        CancelInvoke();
    
        // Tách cha ngay tại đây, ĐỪNG đợi đến khi vào Pool mới tách
        transform.SetParent(null); 
    
        if (rb != null) {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
        }

        GlobalPoolManager.Instance.Return(gameObject);
    }
    

    private void CheckIfPassedPlayer()
    {
        // Logic đổi tag khi bay qua vị trí Player để tránh gây sát thương liên tục
        float moveDir = rb.velocity.x;
        if ((moveDir > 0 && transform.position.x > playerTransform.position.x) || 
            (moveDir < 0 && transform.position.x < playerTransform.position.x))
        {
            hasPassedPlayer = true;
            gameObject.tag = "DroppedWeapon"; 
        }
    }
    
    public void Reflect(Vector2 newDir)
    {
        CancelInvoke(); // Reset thời gian sống khi bị phản đòn
        isPlayerOwned = true;
        gameObject.tag = "FlyObject";
        
        // Có thể lấy tốc độ phản đòn từ data nếu muốn
        rb.velocity = newDir * (_weaponTbScriptData.flySpeed * 1.5f); 
        
        Invoke("ReturnToPool", _weaponTbScriptData.lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isPlayerOwned) 
        {
            // Khi bay trúng Player
            if (collision.CompareTag("Player") && !hasPassedPlayer)
            {
                // Kiểm tra playerTransform tránh lỗi khi Player bị Destroy/Null
                if (playerTransform != null && EventManager.current != null) {
                    EventManager.current.onHit(playerTransform.position);
                }
                ReturnToPool();
            }

            if (collision.CompareTag("Enemy") && hasPassedPlayer)
            {
                HandleEnemyHit(collision);
            }
        }
        else if (collision.CompareTag("Enemy"))
        {
            HandleEnemyHit(collision);
        }
    }

    private void HandleEnemyHit(Collider2D collision)
    {
        // Kiểm tra dữ liệu vũ khí có tồn tại không trước khi làm việc
        if (_weaponTbScriptData == null) return; 

        EnemyBase enemy = collision.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            enemy.GetHit(_weaponTbScriptData.damage, 3);
        
            // Kiểm tra EventManager cũng có thể null nếu chưa khởi tạo
            if (EventManager.current != null) {
                Vector3 impactPosition = (gameObject.transform.position + enemy.transform.position) / 2;
                EventManager.current.onHit(impactPosition);
            }
        
            ReturnToPool();
        }
    }

    public WeaponTBScript GetWeaponData() => _weaponTbScriptData;
}