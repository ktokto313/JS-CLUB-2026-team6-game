using System;
using System.Collections;
using System.Collections.Generic;
using _Game.Scripts.Core;
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
            PlayerController.Instance.OnPerformLowAttack += PerformNormalAttack;
            PlayerController.Instance.OnPerformSmash += PerformSmashAttack;

            // 2. NHÓM JUMP
            PlayerController.Instance.OnPerformJumpAttack += PerformUppercutAttack;
            PlayerController.Instance.OnPerformRisingAttack += PerformUppercutAttack;
            PlayerController.Instance.OnPerformAirSpin += PerformNormalAttack;

            // 3. NHÓM ATTACK
            PlayerController.Instance.OnPerformAttack += PerformNormalAttack;
            PlayerController.Instance.OnPerformUppercut += PerformUppercutAttack;
            PlayerController.Instance.OnPerformAirAttack += PerformNormalAttack; 
            
        }
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
        {
            // 1. NHÓM DUCK
            PlayerController.Instance.OnPerformLowAttack -= PerformNormalAttack;
            PlayerController.Instance.OnPerformSmash -= PerformSmashAttack;

            // 2. NHÓM JUMP
            PlayerController.Instance.OnPerformJumpAttack -= PerformUppercutAttack;
            PlayerController.Instance.OnPerformRisingAttack -= PerformUppercutAttack;
            PlayerController.Instance.OnPerformAirSpin -= PerformNormalAttack;

            // 3. NHÓM ATTACK
            PlayerController.Instance.OnPerformAttack -= PerformNormalAttack;
            PlayerController.Instance.OnPerformUppercut -= PerformUppercutAttack;
            PlayerController.Instance.OnPerformAirAttack -= PerformNormalAttack; 
        }
    }

    // Các hàm wrapper tương ứng với từng loại đòn đánh
    private void PerformNormalAttack() => PerformAttackWithType(1, 0); // Đòn ngang
    private void PerformUppercutAttack() => PerformAttackWithType(1, 1); // Đòn hất tung
    private void PerformSmashAttack() => PerformAttackWithType(1, 2); // Đòn đập xuống

    private void PerformAttackWithType(int damage, int hitType)
    {
        StopAllCoroutines(); 
        StartCoroutine(PunchRoutine(damage, hitType));
    }
    
    private IEnumerator PunchRoutine(int damage, int hitType)
    {
        float direction = Mathf.Sign(transform.localScale.x);
        
        Vector2 hitboxCenter = (Vector2)transform.position + new Vector2(punchOffset.x * direction, punchOffset.y);

        // Quét tất cả object (enemy + vũ khí rơi) không lọc theo layer
        Collider2D[] hitObjects = Physics2D.OverlapBoxAll(hitboxCenter, boxSize, 0);

        foreach (Collider2D obj in hitObjects)
        {   
            // 1. Nếu là Kẻ địch
            EnemyBase enemy = obj.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                Vector3 impactPosition = (gameObject.transform.position + enemy.transform.position)/2;
                EventManager.current.onHit(impactPosition);
                enemy.GetHit(damage, hitType); // Truyền parameters vào đây
                continue;
            }

            // 2. Nếu là cái rìu 
            if (obj.CompareTag("DroppedWeapon"))
            {
                FlyObject fly = obj.GetComponent<FlyObject>();
                if (fly != null)
                {
                    // Lấy dữ liệu vũ khí
                    WeaponTBScript caughtData = fly.GetWeaponData();

                    PlayerController.Instance.EquipWeapon(caughtData);
                    
                    GlobalPoolManager.Instance.Return(obj.gameObject);
                
                    Debug.Log("<color=cyan>Đã bắt được vũ khí!</color>");
                    break; 
                }
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
