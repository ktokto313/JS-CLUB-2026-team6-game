using UnityEngine;
using System.Collections;
using _Game.Scripts.Core;

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
    public Vector3 originalScale;
    public WeaponTBScript CurrentWeaponTbScript; 
    public GameObject weaponItemPrefab;
    protected bool isPerformingAction = false;
    protected Animator anim;
    private Coroutine stunCoroutine;
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
    
    public virtual void GetHit(int damage, int hitType) {
        Health -= damage;
        
        if (anim != null) {
            anim.SetTrigger("gethit");
        }

        if (Health <= 0) {
            if (anim != null) anim.SetTrigger("death");
            OnDeathAction(); 
            return;
        }

        // 3. Tính toán hướng bị đẩy
        float pushDir = transform.position.x < player.position.x ? -1f : 1f;
    
        // 4. Reset combo window
        if (Time.time - lastHitTime > comboWindow) comboCount = 0;
        comboCount++; 
        lastHitTime = Time.time;

        // 5. Xử lý Knockback và Stun
        switch (hitType) {
            case 0: // Đòn ngang
                if (isAirborne) {
                    rb.velocity = new Vector2(pushDir * 6f, rb.velocity.y);
                } else {
                    if (comboCount > 2) rb.AddForce(new Vector2(pushDir * 5f, 0f), ForceMode2D.Impulse);
                    ApplyStun(0.4f);
                }
                break;
            case 1: // Đòn hất tung
                isAirborne = true;
                rb.velocity = Vector2.zero; // Reset vận tốc trước khi hất
                rb.AddForce(new Vector2(pushDir * 2f, 6f), ForceMode2D.Impulse);
                break;
            case 2: // Đòn đập xuống
                if (isAirborne) {
                    rb.velocity = new Vector2(pushDir * 3f, -10f);
                    ApplyStun(0.8f);
                } else {
                    ApplyStun(0.5f);
                }
                break;
            case 3: // Fly object
                rb.velocity = Vector2.zero;
                rb.AddForce(new Vector2(pushDir * 6f, 2f), ForceMode2D.Impulse);
                ApplyStun(1.2f);
                break;
        }
        if (anim != null) anim.SetTrigger("stun");
    }

protected virtual void Update() {
        if (player == null || isAirborne || isStunned || isPerformingAction) {
            if (anim && isStunned) anim.SetBool("isWalking", false);
            return;
        }
        float currentRange = (CurrentWeaponTbScript != null) ? CurrentWeaponTbScript.attackRange : 1.5f;
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
            Vector3 impactPosition = (gameObject.transform.position + player.position) / 2;
            EventManager.current.onPlayerHit(impactPosition);
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


        yield return new WaitForSeconds(duration);
    
        isStunned = false;
        stunCoroutine = null; // Reset khi chạy xong
    }

// Trong hàm GetHit, thay vì Start trực tiếp, hãy dùng hàm quản lý này:
    private void ApplyStun(float duration) {
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    protected float LookAtPlayer() {
        if (player == null) return transform.localScale.x;
        float direction = player.position.x > transform.position.x ? 1 : -1;
        transform.localScale = new Vector3(direction * originalScale.x, originalScale.y, originalScale.z);
        return direction;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) { 
            isAirborne = false; 
            // Không nên ép rb.velocity = zero ở đây vì sẽ làm quái khựng lại mất tự nhiên
        }
    }
}