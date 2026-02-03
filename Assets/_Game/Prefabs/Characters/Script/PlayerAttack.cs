using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    // Value

    [SerializeField] private float boxX;
    [SerializeField] private float boxY;
    [SerializeField] private Vector2 punchOffset = new Vector2(0.8f, 1.0f);
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float showHitboxTime = 0.1f;
    
    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnPerformAttack += PerformAttack;
            PlayerController.Instance.OnPerformLowAttack += PerformAttack;
        }
        
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnPerformAttack -= PerformAttack;
            PlayerController.Instance.OnPerformLowAttack -= PerformAttack;
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

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(hitboxCenter, new Vector2(boxX, boxY), 0, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log($"Da dam trung: {enemy.name} o huong {direction}");
        }
        yield return new WaitForSeconds(showHitboxTime);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float direction = Mathf.Sign(transform.localScale.x);
        Vector2 drawCenter = (Vector2)transform.position + new Vector2(punchOffset.x * direction, punchOffset.y);
        Gizmos.DrawWireCube(drawCenter, new Vector2(boxX, boxY));
    }
}
