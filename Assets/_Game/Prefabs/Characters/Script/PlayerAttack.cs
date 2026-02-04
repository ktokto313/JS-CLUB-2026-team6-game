using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    // Value
    [SerializeField] private Vector2 boxSize;
    [SerializeField] private Vector2 punchOffset = new Vector2(0.8f, 1.0f);
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float showHitboxTime = 0.1f;
    
    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            // 1. NHÓM DUCK
            PlayerController.Instance.OnPerformLowAttack += PerformAttack;
            PlayerController.Instance.OnPerformSmash += PerformAttack;

            // 2. NHÓM JUMP
            PlayerController.Instance.OnPerformJumpAttack += PerformAttack;
            PlayerController.Instance.OnPerformRisingAttack += PerformAttack;
            PlayerController.Instance.OnPerformAirSpin += PerformAttack;

            // 3. NHÓM ATTACK
            PlayerController.Instance.OnPerformAttack += PerformAttack;
            PlayerController.Instance.OnPerformUppercut += PerformAttack;
            PlayerController.Instance.OnPerformAirAttack += PerformAttack; 
            
        }
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
        {
            // 1. NHÓM DUCK
            PlayerController.Instance.OnPerformLowAttack -= PerformAttack;
            PlayerController.Instance.OnPerformSmash -= PerformAttack;

            // 2. NHÓM JUMP
            PlayerController.Instance.OnPerformJumpAttack -= PerformAttack;
            PlayerController.Instance.OnPerformRisingAttack -= PerformAttack;
            PlayerController.Instance.OnPerformAirSpin -= PerformAttack;

            // 3. NHÓM ATTACK
            PlayerController.Instance.OnPerformAttack -= PerformAttack;
            PlayerController.Instance.OnPerformUppercut -= PerformAttack;
            PlayerController.Instance.OnPerformAirAttack -= PerformAttack; 
        }
    }

    private void PerformAttack()
    {
        StopAllCoroutines(); 
        StartCoroutine(PunchRoutine());
    }

    private IEnumerator PunchRoutine()
    {
        float direction = Mathf.Sign(transform.localScale.x);
        
        Vector2 hitboxCenter = (Vector2)transform.position + new Vector2(punchOffset.x * direction, punchOffset.y);

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(hitboxCenter, boxSize, 0, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {   
            Entity entity = enemy.GetComponent<Entity>();
            if (entity != null)
            {
                int damage = 1; 
                int hitType = 0; 

                entity.onHit(damage, hitType); 
                
                Debug.Log($"Đã đánh trúng {enemy.name}!");
            }
        }
        yield return new WaitForSeconds(showHitboxTime);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float direction = Mathf.Sign(transform.localScale.x);
        Vector2 drawCenter = (Vector2)transform.position + new Vector2(punchOffset.x * direction, punchOffset.y);
        Gizmos.DrawWireCube(drawCenter, boxSize);
    }
}
