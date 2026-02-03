using UnityEngine;
using System.Collections;

public class EnemyBase : Entity {
    public float moveSpeed = 3f;
    public float stopDistance = 0.8f;
    private int comboCount = 0;  
    private float lastHitTime = 0f;  
    public float comboWindow = 0.5f; 
    protected Transform player;
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

    protected virtual void Start() {
        if (GameManager.Instance != null)  player = GameManager.Instance.PlayerTransform;

        OnDeathAction = () => {
            CheckAndDropWeapon();
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
        float pushDir = transform.position.x < player.position.x ? -1f : 1f;
        
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
        if (player == null || isAirborne || isStunned) return;
        
        float currentRange = (CurrentWeaponTbScript != null) ? CurrentWeaponTbScript.attackRange : 0.8f;

        float distance = Vector2.Distance(new Vector2(transform.position.x, 0), new Vector2(player.position.x, 0));

        if (distance > currentRange) {
            MoveTowardsPlayer();
        } else {
            HandleAttack();
        }
    }
    protected virtual void MoveTowardsPlayer() {
        float direction = player.position.x > transform.position.x ? 1 : -1;
        transform.Translate(new Vector3(direction * moveSpeed * Time.deltaTime, 0, 0));
        transform.localScale = new Vector3(direction, 1, 1);
    }

    protected virtual void HandleAttack() {
        float attackCooldown = (CurrentWeaponTbScript != null) ? CurrentWeaponTbScript.attackSpeed : 1.0f;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackCooldown) {
            // gọi hàm trừ máu player
            attackTimer = 0;
            // gọi Animation đánh của quái
        }
    }
    
    private void CheckAndDropWeapon() {
        if (CurrentWeaponTbScript == null) return;
        if (Random.value <= 0.5f) {
            GameObject dropObj = GlobalPoolManager.Instance.Get(weaponItemPrefab, transform.position + Vector3.up);
            dropObj.GetComponent<DroppedWeapon>()?.Init(CurrentWeaponTbScript, player);
        }
    }

    IEnumerator StunRoutine(float duration) {
        isStunned = true; 
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    protected void LookAtPlayer() {
        if (player == null) return;
        float direction = player.position.x > transform.position.x ? 1 : -1;
        transform.localScale = new Vector3(direction, 1, 1);
    }

    protected void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) { isAirborne = false; rb.velocity = Vector2.zero; }
    }
}