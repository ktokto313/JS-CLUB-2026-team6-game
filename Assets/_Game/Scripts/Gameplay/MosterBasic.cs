using UnityEngine;
using System.Collections;

public class EnemyBase : Entity
{
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
    
    IEnumerator StunRoutine(float duration)
    {
        isStunned = true; 
        yield return new WaitForSeconds(duration);
        isStunned = false;
        Debug.Log("Quái đã hết choáng!");
    }
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        LookAtPlayer();
        OnDeathAction = () => { Destroy(gameObject); };
    }

    // Hướng: 0 = Ngang, 1 = Up , 2 = Down ... { Chắc phải thêm nhiều tại phải chuyển cái đòn đánh kết hợp khác đó từ class player sang}
    public virtual void GetHit(int hitType)
    {
        Health -= 1;
        if (Health <= 0) { onDeath(); return; }
        if (Time.time - lastHitTime > comboWindow) {
            comboCount = 0;
        }
        comboCount++; 
        lastHitTime = Time.time;
        float pushDir = transform.position.x < player.position.x ? -1f : 1f;
        switch (hitType)
        {
            case 0: 
                if (isAirborne) {
                    rb.AddForce(new Vector2(pushDir * 10f, 0f), ForceMode2D.Impulse);
                } else if (comboCount>2) {
                    rb.AddForce(new Vector2(pushDir * 3f, 0f), ForceMode2D.Impulse);
                }
                else
                {
                    StopCoroutine("StunRoutine");
                    StartCoroutine(StunRoutine(0.5f));
                }
                break;
            
            case 1:
                if (isAirborne)
                {
                    rb.AddForce(new Vector2(pushDir * 10f, 0f), ForceMode2D.Impulse);
                }
                else
                {
                    isAirborne = true;
                    rb.AddForce(new Vector2(pushDir * 0f, 4f), ForceMode2D.Impulse);
                }
                break;

            case 2:
                if (isAirborne)
                {
                    rb.AddForce(new Vector2(pushDir * 5f, 0f), ForceMode2D.Impulse);
                    StopCoroutine("StunRoutine"); 
                    StartCoroutine(StunRoutine(1f));
                }
                else
                {
                    StopCoroutine("StunRoutine");
                    StartCoroutine(StunRoutine(0.5f));
                }
                
                break;
        }

        OnHitAction?.Invoke(); 
    }

    protected virtual void Update()
    {
        if (player == null || isAirborne || isStunned) return;

        float distance = Vector2.Distance(new Vector2(transform.position.x, 0), new Vector2(player.position.x, 0));

        if (distance > stopDistance) {
            MoveTowardsPlayer();
        } else {
            HandleAttack();
        }
    }

    protected virtual void MoveTowardsPlayer()
    {
        float direction = player.position.x > transform.position.x ? 1 : -1;
        transform.Translate(new Vector3(direction * moveSpeed * Time.deltaTime, 0, 0));
    }

    protected virtual void HandleAttack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= 1.0f) {
            Debug.Log("Gây sát thương cho Player!");
            attackTimer = 0;
        }
    }
    
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) {
            isAirborne = false;
            rb.velocity = Vector2.zero;
        }
    }
    protected void LookAtPlayer()
    {
        if (player == null) return;
        float direction = player.position.x > transform.position.x ? 1 : -1;
    
        // Giả sử model chuẩn đã được xoay nhìn sang PHẢI trong Unity
        transform.localScale = new Vector3(direction, 1, 1);
    }
}