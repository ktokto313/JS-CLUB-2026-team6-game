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
    protected bool isPerformingAction = false;
    protected Animator anim;
    private Coroutine stunCoroutine;
    [SerializeField] public GameObject visualAxe;
    protected virtual void Awake() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }
    protected virtual void OnEnable() {
        comboCount = 0;
        attackTimer = 0; 
        if (rb) rb.velocity = Vector2.zero;
        Health = 5;
        isPerformingAction = false;
        isStunned = false; 
        
        
        if (anim != null) {
            anim.ResetTrigger("throw");
            anim.ResetTrigger("death");
            anim.ResetTrigger("gethit");
            anim.ResetTrigger("dash");
            anim.ResetTrigger("predash");
            anim.SetBool("canAttack", false);
            anim.SetBool("isWalking", false);
        
            anim.Rebind();
            anim.Update(0f);
        }
        if (visualAxe != null) {
            visualAxe.SetActive(true);
        
            // THÊM DÒNG NÀY: Ép trực tiếp Component Renderer bật lên
            SpriteRenderer sr = visualAxe.GetComponent<SpriteRenderer>();
            if (sr != null) {
                sr.enabled = true; 
            }
        }
    }

    protected virtual void Start() {
        originalScale = transform.localScale;
        if (GameManager.Instance != null) player = GameManager.Instance.PlayerTransform;

        OnDeathAction += () => {
            GlobalPoolManager.Instance.Return(gameObject);

        };
        LookAtPlayer();
    }
    
    public virtual void GetHit(int damage, int hitType) {
        Health -= damage;
        isPerformingAction = false;
        if (anim != null) {
            anim.SetTrigger("gethit");
            anim.SetBool("isWalking", false); 
            anim.SetBool("canAttack", false);
        }

        if (Health <= 0) {
            if (anim != null) anim.SetTrigger("death");
            CheckAndDropWeapon();
            onDeath(); 
            return;
        }

        float pushDir = transform.position.x < player.position.x ? -1f : 1f;
    
        if (Time.time - lastHitTime > comboWindow) comboCount = 0;
        comboCount++; 
        lastHitTime = Time.time;

        switch (hitType) {
            case 0: // Đòn ngang
                if (isAirborne) {
                    rb.velocity = new Vector2(pushDir * 6f, rb.velocity.y);
                } else {
                    if (comboCount > 1) rb.AddForce(new Vector2(pushDir * 3f, 1f), ForceMode2D.Impulse);
                    ApplyStun(1f);
                }
                break;
            case 1: // Đòn hất tung
                isAirborne = true;
                rb.velocity = Vector2.zero; // Reset vận tốc trước khi hất
                rb.AddForce(new Vector2(pushDir * 0.5f, 7f), ForceMode2D.Impulse);
                break;
            case 2: // Đòn đập xuống
                if (isAirborne) {
                    rb.velocity = new Vector2(pushDir * 3f, -5f);
                    ApplyStun(2.5f);
                } else {
                    ApplyStun(1.5f);
                }
                break;
            case 3: // Fly object
                rb.velocity = Vector2.zero;isAirborne = true;
                rb.AddForce(new Vector2(pushDir * 5f, 2f), ForceMode2D.Impulse);
                ApplyStun(1.2f);
                break;
        }
        if (visualAxe != null) visualAxe.SetActive(true);
    }

protected virtual void Update() {
        if (isAirborne || isStunned || isPerformingAction) {
            if (anim) {
                anim.SetBool("isWalking", false);
                anim.SetBool("canAttack", false); 
            }
            return;
        }
        float currentRange = (CurrentWeaponTbScript != null) ? CurrentWeaponTbScript.attackRange : 1.5f;
        float distance = Mathf.Abs(transform.position.x - player.position.x);

        if (distance > currentRange) {
            // Đang ở xa: Đi bộ
            if (anim) {
                anim.SetBool("isWalking", true); 
                anim.SetBool("canAttack", false); 
            }
            MoveTowardsPlayer();
        } else {
            if (anim) {
                anim.SetBool("isWalking", false);
                anim.SetBool("canAttack", true); 
            }
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
        // 1. Kiểm tra an toàn trước khi chạy logic
        if (CurrentWeaponTbScript == null || CurrentWeaponTbScript.projectilePrefab == null ) return;

        // 2. TÍNH TOÁN VỊ TRÍ SPAWN THEO HƯỚNG
        // Kiểm tra xem quái đang đứng bên phải hay bên trái Player
        float direction = transform.position.x > player.position.x ? 1f : -1f;
    
        // Vị trí spawn = Vị trí Player + (1.5f hoặc -1.5f tùy hướng)
        float offsetX = direction * 1.2f;
        Vector3 spawnPosition = new Vector3(player.position.x + offsetX, player.position.y + 3f, 0);

        // 3. LẤY VŨ KHÍ TỪ POOL
        Debug.Log($"Quái đang ở bên {(direction > 0 ? "Phải" : "Trái")}. Spawn vũ khí tại X: {spawnPosition.x}");
    
        GameObject dropObj = GlobalPoolManager.Instance.Get(CurrentWeaponTbScript.projectilePrefab, spawnPosition);

        if (dropObj != null) {
            dropObj.tag = "Untagged"; 

            DroppedWeapon droppedWeaponComp = dropObj.GetComponent<DroppedWeapon>();
            if (droppedWeaponComp != null) {
                droppedWeaponComp.Init(CurrentWeaponTbScript, player);
            
                // Chạy Coroutine đổi Tag trên chính cái vũ khí (để tránh bị hủy khi quái biến mất)
                droppedWeaponComp.StartCoroutine(EnableWeaponTag(dropObj));

                Rigidbody2D weaponRb = dropObj.GetComponent<Rigidbody2D>();
                if (weaponRb != null) {
                    // Cho vũ khí nảy lên một chút cho đẹp
                    weaponRb.velocity = new Vector2(0, 5f); 
                }
            }
        }
    }

    private IEnumerator EnableWeaponTag(GameObject obj) {
        yield return new WaitForSeconds(0.8f); 
        if (obj != null && obj.activeSelf) {
            obj.tag = "DroppedWeapon";
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
        }
    }
}