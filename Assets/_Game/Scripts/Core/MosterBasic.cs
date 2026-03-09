using UnityEngine;
using System.Collections;

public class EnemyBase : Entity {
    public float moveSpeed = 3f;
    private int comboCount = 0;  
    private float lastHitTime = 0f;  
    public float comboWindow = 0.5f;
    protected Vector3 playerPos;
    protected float attackTimer = 0f;
    protected bool isAirborne = false;
    protected Rigidbody2D rb;
    protected bool isStunned = false; 
    
    public WeaponTBScript CurrentWeaponTbScript; 
    public GameObject weaponItemPrefab;

    protected virtual void Awake() => rb = GetComponent<Rigidbody2D>();

    protected virtual void OnEnable() {
        isStunned = false;
        isAirborne = false;
        comboCount = 0;
        attackTimer = 0; 
        if (rb) rb.velocity = Vector2.zero;
        Health = 5;
    }

    protected virtual void Start()
    {
        playerPos = Vector3.zero;

        OnDeathAction = () => {
            // đang lỗi CheckAndDropWeapon();
            GlobalPoolManager.Instance.Return(gameObject);
        };
        LookAtPlayer();
    }
    
    public virtual void GetHit(int damage, int hitType) {
        Health -= damage;
        if (Health <= 0) { onDeath(); return; }
        if (Time.time - lastHitTime > comboWindow) comboCount = 0;
        comboCount++; 
        lastHitTime = Time.time;
        float pushDir = transform.position.x < playerPos.x ? -1f : 1f;
        
        switch (hitType) {
            case 0: 
                if (isAirborne) rb.AddForce(new Vector2(pushDir * 10f, 0f), ForceMode2D.Impulse);
                else if (comboCount > 2) rb.AddForce(new Vector2(pushDir * 3f, 0f), ForceMode2D.Impulse);
                else { StopCoroutine("StunRoutine"); StartCoroutine(StunRoutine(0.5f)); }
                break;
            case 1:
                if (isAirborne) rb.AddForce(new Vector2(pushDir * 10f, 0f), ForceMode2D.Impulse);
                else { isAirborne = true; rb.AddForce(new Vector2(0f, 4f), ForceMode2D.Impulse); }
                break;
            case 2:
                if (isAirborne) { rb.AddForce(new Vector2(pushDir * 5f, 0f), ForceMode2D.Impulse); StopCoroutine("StunRoutine"); StartCoroutine(StunRoutine(1f)); }
                else { StopCoroutine("StunRoutine"); StartCoroutine(StunRoutine(0.5f)); }
                break;
            case 3:
                rb.velocity = isAirborne ? Vector2.zero : rb.velocity;
                rb.AddForce(new Vector2(pushDir * (isAirborne ? 1f : 2f), 0f), ForceMode2D.Impulse);
                if (!isAirborne) { StopCoroutine("StunRoutine"); StartCoroutine(StunRoutine(0.3f)); }
                break;
        }
    }

protected virtual void Update() {
        if (isAirborne || isStunned) return;
        
        float currentRange = (CurrentWeaponTbScript != null) ? CurrentWeaponTbScript.attackRange : 3f;

        float distance = Vector2.Distance(new Vector2(transform.position.x, 0), new Vector2(playerPos.x, 0));

        if (distance > currentRange) {
            MoveTowardsPlayer();
        } else {
            HandleAttack();
        }
    }
    protected virtual void MoveTowardsPlayer() {
        float direction = playerPos.x > transform.position.x ? 1 : -1;
        transform.Translate(new Vector3(direction * moveSpeed * Time.deltaTime, 0, 0));
        transform.localScale = new Vector3(direction, 1, 1);
    }

    protected virtual void HandleAttack() {
        if (rb != null) rb.velocity = new Vector2(0, rb.velocity.y);
        float attackCooldown = (CurrentWeaponTbScript != null) ? CurrentWeaponTbScript.attackSpeed : 1.0f;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackCooldown)
        {
            PlayerController.Instance.TakeDamage();
            Debug.Log("Hit Playyer :########");
            attackTimer = 0;
        }
    }
    
    private void CheckAndDropWeapon() {
        if (CurrentWeaponTbScript == null) return;
        if (Random.value <= 0.5f) {
            GameObject dropObj = GlobalPoolManager.Instance.Get(weaponItemPrefab, transform.position + Vector3.up);//
            dropObj.GetComponent<DroppedWeapon>()?.Init(CurrentWeaponTbScript, playerPos);
        }
    }

    IEnumerator StunRoutine(float duration) {
        isStunned = true; 
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    protected void LookAtPlayer() {
        float direction = playerPos.x > transform.position.x ? 1 : -1;
        transform.localScale = new Vector3(direction, 1, 1);
    }

    protected void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) { isAirborne = false; rb.velocity = Vector2.zero; }
    }
}