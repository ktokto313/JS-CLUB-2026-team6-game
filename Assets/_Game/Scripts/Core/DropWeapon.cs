using System.Collections;
using UnityEngine;

public class DroppedWeapon : MonoBehaviour
{
    private Weapon weaponData;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private Collider2D col;
    
    private bool isFalling = true; 
    private bool isHeld = false;    
    private bool isFlying = false;  

    public void Init(Weapon data, Transform player)
    {
        weaponData = data;
        playerTransform = player;
        
        rb = gameObject.AddComponent<Rigidbody2D>();
        col = gameObject.AddComponent<CapsuleCollider2D>();
        col.isTrigger = true; 

        gameObject.tag = "DroppedWeapon"; 
        rb.gravityScale = 0.5f; 
    }

    public void GetInteract(bool playerHasWeapon) // game controller gọi sau
    {
        if (!playerHasWeapon)
        {
            if (isFalling || !isHeld) {
                StartCoroutine(HoldWeapon());
            } else if (isHeld || weaponData.type == WeaponType.Ranged) {
                LaunchWeapon(); 
            }
        }
        else 
        {
            ReflectWeapon();
        }
    }

    private IEnumerator HoldWeapon()
{
    FlyObject fly = GetComponent<FlyObject>();
    if (fly != null) fly.GetCaught(); 

    isFalling = false;
    isHeld = true;
    
    rb.velocity = Vector2.zero;
    rb.gravityScale = 0;
    col.isTrigger = true;

    while (isHeld)
    {
        float dir = playerTransform.localScale.x;
        transform.position = playerTransform.position + new Vector3(dir * 0.6f, 0, 0);
        yield return null;
    }
}

    private void LaunchWeapon()
    {
        isHeld = false;
        ExecuteLaunch(10f);
    }

    private void ReflectWeapon()
    {
        isFalling = false;
        ExecuteLaunch(15f);
    }
    private void ExecuteLaunch(float speed)
    {
        FlyObject fly = GetComponent<FlyObject>();
        if (fly == null) fly = gameObject.AddComponent<FlyObject>();

        float dir = playerTransform.localScale.x;
        fly.Launch(weaponData, new Vector2(dir, 0), speed, playerTransform, true);
        Destroy(this);
    }
    private void Update()
    {
        if (isFlying)
        {
            transform.Rotate(0, 0, 1000 * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isFalling && collision.CompareTag("Ground"))
        {
            Destroy(gameObject, 0.1f);
        }
        if (isFlying && collision.CompareTag("Enemy"))
        {
            var enemy = collision.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.GetHit(weaponData.damage, 3);
            }
            Destroy(gameObject);
        }
    }
}