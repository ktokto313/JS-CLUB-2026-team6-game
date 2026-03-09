using UnityEngine;

public class DroppedWeapon : MonoBehaviour
{
    private WeaponTBScript _weaponTbScriptData;
    private Vector3 playerPos;
    private Rigidbody2D rb;
    private Collider2D col;
    
    private bool isFalling = true; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        isFalling = true;
        this.enabled = true; 
        if (col != null) col.isTrigger = true;
        if (rb != null) {
            rb.gravityScale = 0.5f;
            rb.velocity = Vector2.zero;
        }
    }

    public void Init(WeaponTBScript data, Vector3 player)
    {
        _weaponTbScriptData = data;
        this.playerPos = playerPos;
        gameObject.tag = "DroppedWeapon"; 
    }

    // --- HÀM CHO PLAYER GỌI ---
    /// Player gọi hàm này khi muốn NHẶT vũ khí.
    public WeaponTBScript Collect()
    {
        GlobalPoolManager.Instance.Return(gameObject);
        return _weaponTbScriptData;
    }

    public void Reflect(Vector2 launchDirection, float speed)
    {
        isFalling = false;
        ExecuteLaunch(speed, launchDirection);
    }
    

    private void ExecuteLaunch(float speed, Vector2 direction)
    {
        FlyObject fly = GetComponent<FlyObject>();
        if (fly == null) fly = gameObject.AddComponent<FlyObject>();
        else fly.enabled = true;
        
        fly.Launch(_weaponTbScriptData, direction, speed, playerPos, true);
        
        this.enabled = false; 
    }

    private void Update()
    {
        if (isFalling)
        {
            transform.Rotate(0, 0, 180 * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isFalling && collision.CompareTag("Ground"))
        {
            GlobalPoolManager.Instance.Return(gameObject);
        }
    }
}