using UnityEngine;
using System.Collections;

public class EnemyBase : Entity {
    public float moveSpeed = 3f;
    private int comboCount = 0;  
    private float lastHitTime = 0f;  
    public float comboWindow = 0.5f; 
    public Transform player;
    protected float attackTimer = 0f;
    protected bool isAirborne = false;
    protected Rigidbody2D rb;
    protected bool isStunned = false; 
    private Vector3 originalScale;
    public WeaponTBScript CurrentWeaponTbScript; 
    public GameObject weaponItemPrefab;
    protected bool isPerformingAction = false;
    protected Animator anim;
    protected virtual void Awake() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }
    protected virtual void OnEnable() {
        isStunned = false;
        isAirborne = false;
        comboCount = 0;
        attackTimer = 0; 
        if (rb) rb.velocity = Vector2.zero;
        Health = 5;
    }

    protected virtual void Start() {
        originalScale = transform.localScale;
        if (GameManager.Instance != null)  player = GameManager.Instance.PlayerTransform;

        OnDeathAction = () => {
            // đang lỗi CheckAndDropWeapon();
            GlobalPoolManager.Instance.Return(gameObject);
        };
        LookAtPlayer();
    }
    
    public virtual void GetHit(int damage, int hitType) {// Hướng: 0 = Ngang, 1 = Up , 2 = Down, 3 Fly object
        Health -= damage;
        if (anim != null) anim.SetTrigger("gethit");
        if (Health <= 0)
        {
            if (anim != null) anim.SetTrigger("death");
            onDeath(); return;
        }
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
                if (!isAirborne) { StopCoroutine("StunRoutine"); StartCoroutine(StunRoutine(1.5f)); }
                break;
        }
    }

protected virtual void Update() {
        if (player == null || isAirborne || isStunned || isPerformingAction) {
            if (anim && isStunned) anim.SetBool("isWalking", false);
            return;
        }
        float currentRange = (CurrentWeaponTbScript != null) ? CurrentWeaponTbScript.attackRange : 2.5f;
        float distance = Mathf.Abs(transform.position.x - player.position.x);

        if (distance > currentRange) {
            // Đang ở xa: Đi bộ
            if (anim) anim.SetBool("isWalking", true); 
            MoveTowardsPlayer();
        } else {
            // Đã đến gần: Dừng đi bộ và tấn công
            if (anim) anim.SetBool("isWalking", false);
            HandleAttack();
        }
    }
    protected virtual void MoveTowardsPlayer() {
        float direction = player.position.x > transform.position.x ? 1 : -1;
        transform.Translate(new Vector3(direction * moveSpeed * Time.deltaTime, 0, 0));
        transform.localScale = new Vector3(direction * originalScale.x, originalScale.y, originalScale.z);
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
            dropObj.GetComponent<DroppedWeapon>()?.Init(CurrentWeaponTbScript, player);
        }
    }

    IEnumerator StunRoutine(float duration) {
        isStunned = true; 
        if (anim) anim.SetBool("stun", true);
        yield return new WaitForSeconds(duration);
        if (anim) anim.SetBool("stun", false);
        isStunned = false;
    }

    protected float LookAtPlayer() {
        if (player == null) return transform.localScale.x;
        float direction = player.position.x > transform.position.x ? 1 : -1;
        transform.localScale = new Vector3(direction * originalScale.x, originalScale.y, originalScale.z);
        return direction;
    }

    protected void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) { isAirborne = false; rb.velocity = Vector2.zero; }
    }
}