using UnityEngine;
using _Game.Scripts.Core;
using System.Collections;

public class RocketRider : EnemyBase 
{
    [Header("Settings")]
    [SerializeField] private float dashSpeed = 12f; 
    [SerializeField] private float explosionRadius = 3f; 
    [SerializeField] private float headOffset = 0.8f; 
    [SerializeField] private float[] phaseHeights = { 5f, 3f, 1f };
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float rotationSpeed = 5f;

    private int currentPhaseIndex = 0;
    private int dirX = 1;
    private bool isExploding = false;

    protected override void OnEnable() {
        base.OnEnable(); 
        isExploding = false;
        currentPhaseIndex = 0;
        if (rb) { 
            rb.gravityScale = 0; 
            rb.velocity = Vector2.zero; 
            rb.angularVelocity = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        if (player == null && GameManager.Instance != null) player = GameManager.Instance.PlayerTransform;
        dirX = (player != null && transform.position.x > player.position.x) ? -1 : 1;
        UpdateFacing();
        if (phaseHeights.Length > 0) transform.position = new Vector3(transform.position.x, phaseHeights[0], transform.position.z);
    }

    private void Update() {
        if (isExploding) {
            if (rb != null && rb.velocity.sqrMagnitude > 0.2f) {
                float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            return; 
        }
        rb.velocity = new Vector2(dirX * dashSpeed, 0);
        CheckBoundaries();
        Vector2 headPos = (Vector2)transform.position + new Vector2(dirX * headOffset, 0);
        if (Physics2D.OverlapCircle(headPos, 0.7f, LayerMask.GetMask("Player"))) {
            StartCoroutine(ExplosionSequence());
        }
    }

    public override void GetHit(int damage, int hitType) {
        if (isExploding) return;
        
        isExploding = true; 
        
        if (rb != null) {
            rb.gravityScale = 2f; 
            rb.velocity = Vector2.zero; 
            rb.constraints = RigidbodyConstraints2D.None; 
            float pushDir = (player != null && transform.position.x < player.position.x) ? -1f : 1f;
            rb.AddForce(new Vector2(pushDir * 5f, 5f), ForceMode2D.Impulse); 
        }

        StartCoroutine(ExplosionSequence());
    }

    private IEnumerator ExplosionSequence() {
        if (!isExploding) {
            rb.velocity = Vector2.zero; 
            isExploding = true;
            rb.gravityScale = 2f;
        }

        if (anim) anim.SetTrigger("boom");
        
        yield return new WaitForSeconds(1.5f);

        if (explosionEffect) Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D col in objectsInRange) {
            if (col.CompareTag("Player")) {
                PlayerController.Instance.TakeDamage();
                EventManager.current.onPlayerHit(transform.position);
            }
            else if (col.CompareTag("Enemy")) {
                EnemyBase otherEnemy = col.GetComponentInParent<EnemyBase>();
                if (otherEnemy != null && otherEnemy != this) {
                    otherEnemy.GetHit(3, 3); 
                }
            }
        }

        EventManager.current.onDead();
        GlobalPoolManager.Instance.Return(gameObject);
    }

    private void CheckBoundaries() {
        if ((dirX == 1 && transform.position.x >= 15f) || (dirX == -1 && transform.position.x <= -15f)) {
            dirX *= -1;
            UpdateFacing();
            if (currentPhaseIndex < phaseHeights.Length - 1) {
                currentPhaseIndex++;
                transform.position = new Vector3(transform.position.x, phaseHeights[currentPhaseIndex], transform.position.z);
            }
        }
    }

    private void UpdateFacing() {
        transform.localScale = new Vector3(dirX * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        transform.rotation = Quaternion.identity;
    }

    private void OnCollisionEnter2D(Collision2D col) {
        if (!isExploding && col.gameObject.CompareTag("Ground")) StartCoroutine(ExplosionSequence());
    }
}