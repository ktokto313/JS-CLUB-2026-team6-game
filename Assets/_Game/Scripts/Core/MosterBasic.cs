using UnityEngine;
using System.Collections;
using _Game.Scripts.Core;

public class EnemyBase : Entity {
    [SerializeField] private WorldChatUI chatUI;
    [SerializeField] protected HealthBar healthBar;
    [SerializeField] private string[] spawnDialogues = {};
    [SerializeField] private string[] hitDialogues = {};
    [SerializeField] private string[] deathDialogues = {};
    protected int maxHealth;
    [Header("Current Instance Weapon Data")]
    protected Sprite selectedVisual;      
    protected GameObject selectedPrefab;
    [Header("Data Settings")]
    public EnemyData data;
    public WeaponTBScript CurrentWeaponTbScript; 
    [SerializeField] public GameObject visualAxe;
    protected Animator anim;
    protected Vector3 originalScale;
    protected Transform player;
    protected Rigidbody2D rb;
    
    protected int comboCount = 0;  
    protected float lastHitTime = 0f;  
    protected float comboWindow = 0.5f; 
    protected float attackTimer = 0f;
    
    protected bool isAirborne = false;
    protected bool isStunned = false; 
    protected bool isPerformingAction = false;
    protected Coroutine stunCoroutine;
    
    protected virtual void Awake() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        originalScale = transform.localScale;
    }
    protected virtual void OnEnable()
    {
        if (chatUI != null) chatUI.gameObject.SetActive(false); 
        if (Random.value < 0.5f && chatUI != null) {
            chatUI.ShowChat(spawnDialogues); 
        }
        else Debug.Log("Khong tim thay khung chat!");
        comboCount = 0;
        attackTimer = 0; 
        lastHitTime = 0f;  
        isStunned = false; 
        isPerformingAction = false;
        if (rb) rb.velocity = Vector2.zero;
        StopAllCoroutines(); 
        stunCoroutine = null;
        if (anim != null) {
            anim.ResetTrigger("death");
            anim.ResetTrigger("gethit");
            anim.SetBool("canAttack", false);
            anim.SetBool("isWalking", false);
            anim.Rebind();
            anim.Update(0f);
        }
        if (CurrentWeaponTbScript != null) {
            var skins = CurrentWeaponTbScript.weaponSkins;
            
            if (skins != null && skins.Count > 0) {
                int randomIndex = Random.Range(0, skins.Count);
                selectedVisual = skins[randomIndex].visualSprite;
                selectedPrefab = skins[randomIndex].projectilePrefab;
                if (visualAxe != null) {
                    visualAxe.SetActive(true);
                    SpriteRenderer sr = visualAxe.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sprite = selectedVisual;
                        sr.enabled = true;
                    }
                } 
            }
        } 
        if (data != null) {
            int wave = SpawnManager.Instance != null ? SpawnManager.Instance.currentWaveIndex : 1;
            float scaleFactor = 1f + (data.healthMultiplierPerWave * (wave - 1));
            maxHealth = Mathf.RoundToInt(data.baseHealth * scaleFactor);
            Health = maxHealth;
            moveSpeed = data.baseMoveSpeed * (1f + (data.speedMultiplierPerWave * (wave - 1)));
        
            Debug.Log($"<color=yellow>{gameObject.name} Spawned at Wave {wave} | HP: {Health} | Speed: {moveSpeed}</color>");
        } else {
            Health = 5;
            moveSpeed = 3f;
        }
        if (healthBar != null) {
            healthBar.gameObject.SetActive(true);
            healthBar.UpdateHealth(Health, maxHealth);
        }
    }

    protected virtual void Start() {
        if (GameManager.Instance != null) player = GameManager.Instance.PlayerTransform;

        OnDeathAction += () => {
            
            GlobalPoolManager.Instance.Return(gameObject);

        };
        LookAtPlayer();
    }
    
    public virtual void GetHit(int damage, int hitType) {
        Health -= damage;
        if (Random.value < 0.36f) {
            chatUI.ShowChat(hitDialogues, 0.5f); 
        }
        if (healthBar != null) {
            healthBar.UpdateHealth(Health, maxHealth);
        }
        isPerformingAction = false;
        if (anim != null) {
            anim.SetTrigger("gethit");
            anim.SetBool("isWalking", false); 
            anim.SetBool("canAttack", false);
        }

        if (Health <= 0) {
            if (Random.value < 0.5f) {
                chatUI.ShowChat(deathDialogues, 0.5f); 
            }
            if (healthBar != null) healthBar.gameObject.SetActive(false);
            if (anim != null) anim.SetTrigger("death");
            CheckAndDropWeapon();
            onDeath(); 
            return;
        }

        float pushDir = transform.position.x < player.position.x ? -1f : 1f;
    
        if (Time.time - lastHitTime > comboWindow) comboCount = 0;
        comboCount++; 
        lastHitTime = Time.time;
        float fallDir = (player != null && transform.position.x < player.position.x) ? 1f : -1f;
        
        switch (hitType) {
            case 0: // Đòn ngang
                if (isAirborne) {
                    rb.velocity = new Vector2(pushDir * 6f, rb.velocity.y);
                    transform.rotation = Quaternion.Euler(0, 0, fallDir * 69f);
                } else {
                    if (comboCount > 1) rb.AddForce(new Vector2(pushDir * 3f, 1f), ForceMode2D.Impulse);
                    ApplyStun(1f);
                }
                break;
            case 1: // Đòn hất tung
                isAirborne = true;
                rb.velocity = Vector2.zero; 
                rb.AddForce(new Vector2(pushDir * 0.5f, 7f), ForceMode2D.Impulse);
                break;
            case 2: // Đòn đập xuống
                if (isAirborne) {
                    transform.rotation = Quaternion.Euler(0, 0, fallDir * -90f);
                    rb.velocity = new Vector2(pushDir * 3f, -5f);
                    ApplyStun(2.5f);
                } else {
                    ApplyStun(1.5f);
                }
                break;
            case 3: // Fly object
                rb.velocity = Vector2.zero;isAirborne = true;
                rb.AddForce(new Vector2(pushDir * 5f, 2f), ForceMode2D.Impulse);
                transform.rotation = Quaternion.Euler(0, 0, fallDir * 69f);
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
            // EventManager.current.onPlayerHit(impactPosition);
            Debug.Log("Hit Playyer :########");
            attackTimer = 0;
        }
    }
    
    private void CheckAndDropWeapon() {
        if (selectedPrefab == null) return; 

        float direction = transform.position.x > player.position.x ? 1f : -1f;
        Vector3 spawnPosition = new Vector3(player.position.x + (direction * 1.2f), player.position.y + 3f, 0);
    
        GameObject dropObj = GlobalPoolManager.Instance.Get(selectedPrefab, spawnPosition);
    
        if (dropObj != null) {
            if (dropObj.TryGetComponent(out DroppedWeapon droppedWeaponComp)) {
                droppedWeaponComp.Init(CurrentWeaponTbScript, player, selectedPrefab);
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
        ExitStun();
    }

    private void ExitStun() {
        isStunned = false;
        stunCoroutine = null;
        if(rb) rb.velocity = new Vector2(0, rb.velocity.y); 
    }

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
            LookAtPlayer();
            isAirborne = false;
            transform.rotation = Quaternion.identity;
            ApplyStun(1f);
        }
    }
}