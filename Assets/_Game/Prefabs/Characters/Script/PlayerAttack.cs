using System;
using System.Collections;
using System.Collections.Generic;
using _Game.Scripts.Core;
using UnityEngine;

public enum AttackHitboxType
{
    Normal,
    AirSpin,
    Smash
}

public class PlayerAttack : MonoBehaviour
{
    [Header("Combat Stats")]
    [SerializeField] private int baseDamage = 1;
    [UnityEngine.Serialization.FormerlySerializedAs("boxSize")]
    [SerializeField] private Vector2 baseBoxSize = new Vector2(1.5f, 1.5f);
    [SerializeField] private Vector2 punchOffset = new Vector2(0.8f, 1.0f);

    [Header("Air Spin Hitbox")]
    [SerializeField] private Vector2 airSpinBoxSize = new Vector2(3f, 3f);
    [SerializeField] private Vector2 airSpinOffset = new Vector2(0f, 1.0f);

    [Header("Smash Hitbox")]
    [SerializeField] private Vector2 smashBoxSize = new Vector2(1.5f, 1.5f);
    [SerializeField] private Vector2 smashOffset = new Vector2(1.2f, 1.0f);

    [Header("Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float showHitboxTime = 0.1f;
    
    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            // 1. NHÓM DUCK
            PlayerController.Instance.OnPerformLowAttack += PerformLowAttackWrapper;
            PlayerController.Instance.OnPerformSmash += PerformSmashAttack;

            // 2. NHÓM JUMP
            PlayerController.Instance.OnPerformJumpAttack += PerformUppercutAttack;
            PlayerController.Instance.OnPerformRisingAttack += PerformUppercutAttack;
            PlayerController.Instance.OnPerformAirSpin += PerformAirSpinWrapper;

            // 3. NHÓM ATTACK
            PlayerController.Instance.OnPerformAttack += PerformNormalComboAttack;
            PlayerController.Instance.OnPerformUppercut += PerformUppercutAttack;
            PlayerController.Instance.OnPerformAirAttack += PerformAirAttackWrapper; 
            
        }
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
        {
            // 1. NHÓM DUCK
            PlayerController.Instance.OnPerformLowAttack -= PerformLowAttackWrapper;
            PlayerController.Instance.OnPerformSmash -= PerformSmashAttack;

            // 2. NHÓM JUMP
            PlayerController.Instance.OnPerformJumpAttack -= PerformUppercutAttack;
            PlayerController.Instance.OnPerformRisingAttack -= PerformUppercutAttack;
            PlayerController.Instance.OnPerformAirSpin -= PerformAirSpinWrapper;

            // 3. NHÓM ATTACK
            PlayerController.Instance.OnPerformAttack -= PerformNormalComboAttack;
            PlayerController.Instance.OnPerformUppercut -= PerformUppercutAttack;
            PlayerController.Instance.OnPerformAirAttack -= PerformAirAttackWrapper; 
        }
    }

    // Các hàm wrapper tương ứng với từng loại đòn đánh
    private void PerformLowAttackWrapper() => PerformAttackWithType(baseDamage, 0, AttackHitboxType.Normal);
    private void PerformAirSpinWrapper() => PerformAttackWithType(baseDamage, 0, AttackHitboxType.AirSpin);
    private void PerformAirAttackWrapper() => PerformAttackWithType(baseDamage, 0, AttackHitboxType.Normal);

    private void PerformNormalComboAttack(int comboStep) 
    {
        PerformAttackWithType(baseDamage, 0, AttackHitboxType.Normal); 
    }

    private void PerformUppercutAttack() => PerformAttackWithType(baseDamage, 1, AttackHitboxType.Normal); // Đòn hất tung
    private void PerformSmashAttack() => PerformAttackWithType(baseDamage, 2, AttackHitboxType.Smash); // Đòn đập xuống

    private void PerformAttackWithType(int damage, int hitType, AttackHitboxType hitboxType = AttackHitboxType.Normal)
    {
        EventManager.current?.onPlayerAttack();
        StopAllCoroutines(); 
        StartCoroutine(PunchRoutine(damage, hitType, hitboxType));
    }
    
    private IEnumerator PunchRoutine(int damage, int hitType, AttackHitboxType hitboxType)
    {
        float direction = Mathf.Sign(transform.localScale.x);
        
        // 1. Setup stats and weapon state
        bool isThrowing = false;
        int finalDamage = damage;
        Vector2 currentBoxSize = baseBoxSize;
        Vector2 currentAirSpinBoxSize = airSpinBoxSize;
        Vector2 currentSmashBoxSize = smashBoxSize;

        ApplyWeaponModifiers(ref finalDamage, ref currentBoxSize, ref currentAirSpinBoxSize, ref currentSmashBoxSize, ref isThrowing);

        // 2. Get Hit Objects
        HashSet<Collider2D> hitObjects = GetHitObjects(hitboxType, direction, currentBoxSize, currentAirSpinBoxSize, currentSmashBoxSize);

        // 3. Process Hits (Enemies, Weapons)
        ProcessHitObjects(hitObjects, finalDamage, hitType);

        // 4. Handle Weapon Throwing
        if (isThrowing)
        {
            ThrowEquippedWeapon(direction);
        }

        yield return new WaitForSeconds(showHitboxTime);
    }

    private void ApplyWeaponModifiers(ref int finalDamage, ref Vector2 currentBoxSize, ref Vector2 currentAirSpinBoxSize, ref Vector2 currentSmashBoxSize, ref bool isThrowing)
    {
        if (PlayerController.Instance.currentWeapon == null) return;

        if (!PlayerController.Instance.weaponHasBeenUsedMelee)
        {
            // Sử dụng cận chiến 1 lần
            PlayerController.Instance.weaponHasBeenUsedMelee = true;

            float bonusRange = PlayerController.Instance.currentWeapon.attackRange;
            float bonusRangeY = bonusRange * 0.2f;

            finalDamage += PlayerController.Instance.currentWeapon.damage;
            currentBoxSize.x += bonusRange;
            currentBoxSize.y += bonusRangeY;
            
            currentAirSpinBoxSize.x += bonusRange;
            currentAirSpinBoxSize.y += bonusRange;

            currentSmashBoxSize.x += bonusRange;
            currentSmashBoxSize.y += bonusRangeY;
        }
        else
        {
            // Đã xài cận chiến, lần này ném đi
            isThrowing = true;
        }
    }

    private HashSet<Collider2D> GetHitObjects(AttackHitboxType hitboxType, float direction, Vector2 currentBoxSize, Vector2 currentAirSpinBoxSize, Vector2 currentSmashBoxSize)
    {
        List<Collider2D> hitObjectsList = new List<Collider2D>();

        if (hitboxType == AttackHitboxType.Normal)
        {
            Vector2 hitboxCenter = (Vector2)transform.position + new Vector2(punchOffset.x * direction, punchOffset.y);
            hitObjectsList.AddRange(Physics2D.OverlapBoxAll(hitboxCenter, currentBoxSize, 0));
        }
        else if (hitboxType == AttackHitboxType.AirSpin)
        {
            Vector2 hitboxCenter = (Vector2)transform.position + airSpinOffset;
            hitObjectsList.AddRange(Physics2D.OverlapBoxAll(hitboxCenter, currentAirSpinBoxSize, 0));
        }
        else if (hitboxType == AttackHitboxType.Smash)
        {
            Vector2 hitboxCenterRight = (Vector2)transform.position + new Vector2(smashOffset.x, smashOffset.y);
            Vector2 hitboxCenterLeft = (Vector2)transform.position + new Vector2(-smashOffset.x, smashOffset.y);
            hitObjectsList.AddRange(Physics2D.OverlapBoxAll(hitboxCenterRight, currentSmashBoxSize, 0));
            hitObjectsList.AddRange(Physics2D.OverlapBoxAll(hitboxCenterLeft, currentSmashBoxSize, 0));
        }

        return new HashSet<Collider2D>(hitObjectsList);
    }

    private void ProcessHitObjects(HashSet<Collider2D> hitObjects, int finalDamage, int hitType)
    {
        foreach (Collider2D obj in hitObjects)
        {   
            // 1. Nếu là Kẻ địch
            EnemyBase enemy = obj.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                HandleEnemyHit(enemy, finalDamage, hitType);
                continue;
            }

            // 2. Nếu là cái rìu 
            if (obj.CompareTag("DroppedWeapon"))
            {
                HandleDroppedWeapon(obj);
            }
        }
    }

    private void HandleEnemyHit(EnemyBase enemy, int finalDamage, int hitType)
    {
        Vector3 impactPosition = (gameObject.transform.position + enemy.transform.position) / 2;
        if (EventManager.current != null)
        {
            EventManager.current.onHit(impactPosition); 
        }
        else
        {
            Debug.LogWarning("Chưa có EventManager trong Scene!");
        }
        enemy.GetHit(finalDamage, hitType); // Sử dụng sát thương được cộng dồn vũ khí
    }

    private void HandleDroppedWeapon(Collider2D obj)
    {
        DroppedWeapon drop = obj.GetComponent<DroppedWeapon>();
        FlyObject fly = obj.GetComponent<FlyObject>();
        
        if (fly != null || drop != null)
        {
            WeaponTBScript fl = fly != null ? fly.GetWeaponData() : null;
            WeaponTBScript dr = drop != null ? drop.GetWeaponData() : null;
            WeaponTBScript caught = fl != null ? fl : dr;

            if (caught != null) 
            {
                PlayerController.Instance.EquipWeapon(caught);
                GlobalPoolManager.Instance.Return(obj.gameObject);
                Debug.Log("<color=cyan>Đã bắt được vũ khí!</color>");
            }
            else 
            {
                Debug.LogError("Thís Weapon không mang theo dữ liệu WeaponTBScript!");
            }
        }
    }

    private void ThrowEquippedWeapon(float direction)
    {
        if (PlayerController.Instance.currentWeapon != null && PlayerController.Instance.currentWeapon.projectilePrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(direction * 0.5f, punchOffset.y, 0f);
            GameObject go = GlobalPoolManager.Instance.Get(PlayerController.Instance.currentWeapon.projectilePrefab, spawnPos);
            
            if (go.TryGetComponent(out FlyObject fly)) 
            {
                fly.Launch(PlayerController.Instance.currentWeapon, new Vector2(direction, 0), PlayerController.Instance.currentWeapon.flySpeed, transform, true);
            }

            // Mất vũ khí sau khi đã đánh và ném
            PlayerController.Instance.EquipWeapon(null);
        }
    }
    private void OnDrawGizmosSelected()
    {
        float direction = Mathf.Sign(transform.localScale.x);

        Vector2 drawBoxSize = baseBoxSize;
        Vector2 drawAirSpinBoxSize = airSpinBoxSize;
        Vector2 drawSmashBoxSize = smashBoxSize;

        if (Application.isPlaying && PlayerController.Instance != null && PlayerController.Instance.currentWeapon != null && !PlayerController.Instance.weaponHasBeenUsedMelee)
        {
            float bonusRange = PlayerController.Instance.currentWeapon.attackRange;
            float bonusRangeY = bonusRange * 0.2f;

            drawBoxSize.x += bonusRange;
            drawBoxSize.y += bonusRangeY;

            drawAirSpinBoxSize.x += bonusRange;
            drawAirSpinBoxSize.y += bonusRange;

            drawSmashBoxSize.x += bonusRange;
            drawSmashBoxSize.y += bonusRangeY;
        }

        // 1. Normal Hitbox (Đỏ)
        Gizmos.color = Color.red;
        Vector2 normalCenter = (Vector2)transform.position + new Vector2(punchOffset.x * direction, punchOffset.y);
        Gizmos.DrawWireCube(normalCenter, drawBoxSize);

        // 2. Air Spin Hitbox (Xanh dương)
        Gizmos.color = Color.blue;
        Vector2 airSpinPt = (Vector2)transform.position + airSpinOffset;
        Gizmos.DrawWireCube(airSpinPt, drawAirSpinBoxSize);

        // 3. Smash Hitbox (Xanh lá)
        Gizmos.color = Color.green;
        Vector2 smashPtRight = (Vector2)transform.position + new Vector2(smashOffset.x, smashOffset.y);
        Vector2 smashPtLeft = (Vector2)transform.position + new Vector2(-smashOffset.x, smashOffset.y);
        Gizmos.DrawWireCube(smashPtRight, drawSmashBoxSize);
        Gizmos.DrawWireCube(smashPtLeft, drawSmashBoxSize);
    }
}
