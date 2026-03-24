using UnityEngine;
using System.Collections;

public class DroppedWeapon : MonoBehaviour
{
    private WeaponTBScript _weaponTbScriptData;
    private GameObject _specificPrefab;
    private Transform playerTransform;
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
            rb.gravityScale = 0.2f;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f; 
        }
        transform.rotation = Quaternion.identity;
    }

public void Init(WeaponTBScript data, Transform player, GameObject specificPrefab = null)
    {
        _weaponTbScriptData = data;
        playerTransform = player;
        _specificPrefab = (specificPrefab != null) ? specificPrefab : data.currentPrefab;
        gameObject.tag = "DroppedWeapon";
        if (_weaponTbScriptData != null && _weaponTbScriptData.type == WeaponType.Spear)
        {
            transform.rotation = Quaternion.Euler(0, 0, 90f);
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }
        gameObject.tag = "DroppedWeapon";
        
        if (TryGetComponent(out FlyObject fly)) 
        {
            fly.UpdateRingVisual();
        }
    }
    
    // private IEnumerator EnableWeaponTagRoutine() 
    // {
    //     // Tạm thời để tag rỗng (hoặc Untagged) để Player không đấm trúng lúc nó đang rơi
    //     gameObject.tag = ""; 
        
    //     yield return new WaitForSeconds(0.8f); 
        
    //     // Sau 0.8s, chính thức biến thành đồ rớt dưới đất
    //     if (TryGetComponent(out FlyObject fly)) 
    //     {
    //         fly.UpdateRingVisual();
    //     }
    //     gameObject.tag = "DroppedWeapon";
    // }

    // Hàm này được FlyObject gọi để "mớm" Data sang khi quái vật ném vũ khí
    public void SetDataOnly(WeaponTBScript data, GameObject specificPrefab)
    {
        _weaponTbScriptData = data;
        _specificPrefab = specificPrefab;
    }

    // public WeaponTBScript GetWeaponData()
    // {
    //     if (_weaponTbScriptData != null) {
    //         _weaponTbScriptData.currentPrefab = _specificPrefab;
    //     }
    //     WeaponTBScript tempData = _weaponTbScriptData;
    //     // GlobalPoolManager.Instance.Return(gameObject);
    //     return tempData;
    // }

    public WeaponTBScript GetWeaponData()
    {
        if (_weaponTbScriptData != null) 
        {
            WeaponTBScript clonedData = Instantiate(_weaponTbScriptData);
            
            clonedData.currentPrefab = _specificPrefab;
            
            return clonedData;
        }
        
        return null;
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
        
        fly.Launch(_weaponTbScriptData, _specificPrefab, direction, speed, playerTransform, true);
    }

    private void Update()
    {
        if (isFalling && _weaponTbScriptData != null)
        {
            if (_weaponTbScriptData.type == WeaponType.Spear)
            {
                transform.rotation = Quaternion.Euler(0, 0, 90f);
                if (rb != null) rb.angularVelocity = 0f; 
            }
            else
            {
                transform.Rotate(0, 0, 180 * Time.deltaTime);
            }
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