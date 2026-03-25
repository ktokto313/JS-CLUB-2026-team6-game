using UnityEngine;
using System.Collections;

public class SwingingEnemy : EnemyBase {
    [Header("Swinging Settings")]
    [SerializeField] private float swingRadius = 7f; 
    [SerializeField] private float swingSpeed = 2f;   
    [SerializeField] private float sideLimit = 0.4f;
    [SerializeField] private SpriteRenderer ropeVisual;
    private Vector2 anchorPoint;  
    private float angle;       
    private bool isSwinging = true;
    private float swingDirection = 1f;
    private bool hasHitPlayerInThisSwing = false;
    [SerializeField] private float damageRadius = 0.6f;
    [SerializeField] private float damageOffset = 0f;
    protected override void OnEnable() {
        base.OnEnable();
        isSwinging = true;
        hasHitPlayerInThisSwing = false;
        if (ropeVisual != null) ropeVisual.enabled = true;
        if (anim != null) {
            anim.SetBool("fall", false);
        }
        if (GameManager.Instance != null && GameManager.Instance.PlayerTransform != null) {
            float playerX = GameManager.Instance.PlayerTransform.position.x;
            anchorPoint = new Vector2(playerX, transform.position.y + swingRadius);
            
            Vector2 relativePos = (Vector2)transform.position - anchorPoint;
            angle = Mathf.Atan2(relativePos.y, relativePos.x);
            swingDirection = (angle < -Mathf.PI / 2) ? 1f : -1f;
        }
    }

    protected override void Update() {
        if (isSwinging) {
            UpdateSwingingMovement();
            if (!hasHitPlayerInThisSwing) {
                CheckDamage();
            }
        } else {
            base.Update();
        }
    }

    private void UpdateSwingingMovement() {
        angle += swingDirection * swingSpeed * Time.deltaTime;

        if (angle > -sideLimit) {
            angle = -sideLimit;
            swingDirection = -1f;
            hasHitPlayerInThisSwing = false;
        }
        if (angle < -Mathf.PI + sideLimit) {
            angle = -Mathf.PI + sideLimit;
            swingDirection = 1f;
            hasHitPlayerInThisSwing = false;
        }
        float x = anchorPoint.x + Mathf.Cos(angle) * swingRadius;
        float y = anchorPoint.y + Mathf.Sin(angle) * swingRadius;
        transform.position = new Vector3(x, y, 0);
        float lookDir = (swingDirection > 0) ? 1f : -1f;
        transform.localScale = new Vector3(lookDir * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
    }

    public override void GetHit(int damage, int hitType) {
        if (isSwinging) {
            if (anim != null) {
                anim.SetBool("fall", true);
            }
            FallDown();
        }
        base.GetHit(damage, hitType);
    }

    private void FallDown() {
        isSwinging = false;
        isAirborne = true;
        hasHitPlayerInThisSwing = false;
        if (ropeVisual != null) ropeVisual.enabled = false;
        if (rb) {
            rb.gravityScale = 1.5f; 
            rb.velocity = new Vector2(swingDirection * 3f, 2f); 
        }
    }
    private void CheckDamage() {
        float facingDir = transform.localScale.x > 0 ? 1 : -1;
        Vector2 checkPos = (Vector2)transform.position + new Vector2(facingDir * damageOffset, 0);
        
        Collider2D hit = Physics2D.OverlapCircle(checkPos, damageRadius, LayerMask.GetMask("Player"));
        
        if (hit != null) {
            hasHitPlayerInThisSwing = true;
            PlayerController.Instance.TakeDamage();
            Debug.Log("<color=red>Rope Enemy hit Player!</color>");
        }
    }
    protected override void OnCollisionEnter2D(Collision2D collision) {
        if (!isSwinging && collision.gameObject.CompareTag("Ground")) {
            if (anim != null) anim.SetBool("fall", false);
            base.OnCollisionEnter2D(collision);
        }
    }
    
    private void OnDrawGizmos() {
        if (Application.isPlaying && isSwinging) {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(anchorPoint, transform.position);
            Gizmos.DrawWireSphere(anchorPoint, 0.2f);
        }
    }
}