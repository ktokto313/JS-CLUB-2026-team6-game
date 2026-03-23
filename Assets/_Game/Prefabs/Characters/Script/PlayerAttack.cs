using System;
using System.Collections;
using System.Collections.Generic;
using _Game.Scripts.Core;
using UnityEngine;

public enum AttackHitboxType { Normal, AirSpin, Smash }

public class PlayerAttack : MonoBehaviour
{
    [Header("Combat Stats")]
    [SerializeField] private int baseDamage = 1;
    [SerializeField] private Vector2 baseBoxSize = new Vector2(1.5f, 1.5f);
    [SerializeField] private Vector2 punchOffset = new Vector2(0.8f, 1.0f);

    [Header("Air Spin Hitbox")]
    [SerializeField] private Vector2 airSpinBoxSize = new Vector2(3f, 3f);
    [SerializeField] private Vector2 airSpinOffset = new Vector2(0f, 1.0f);

    [Header("Smash Hitbox")]
    [SerializeField] private Vector2 smashBoxSize = new Vector2(1.5f, 1.5f);
    [SerializeField] private Vector2 smashOffset = new Vector2(1.2f, 1.0f);

    [Header("Combo Settings")] 
    [SerializeField] private float comboTimeout = 0.8f;
    private int comboStep = 0;
    private float lastAttackTime = 0f;

    [Header("Weapon System")] 
    public WeaponTBScript currentWeapon;
    public bool weaponHasBeenUsedMelee = false;
    [SerializeField] private Transform handSocket;
    private GameObject currentWeaponObject;

    [Header("Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float showHitboxTime = 0.1f;
    [SerializeField] private Vector2 dropForce = new Vector2(4f, 6f); // Lực ném vũ khí khi vứt

    // EVENT: Báo cho UI hoặc Animation biết
    public event Action<bool> OnWeaponEquipped;
    public event Action<int> OnComboAttackStep; // Thay thế cho event combo cũ của Controller

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnPerformLowAttack += PerformLowAttackWrapper;
            PlayerController.Instance.OnPerformSmash += PerformSmashAttack;
            PlayerController.Instance.OnPerformJumpAttack += PerformUppercutAttack;
            PlayerController.Instance.OnPerformRisingAttack += PerformUppercutAttack;
            PlayerController.Instance.OnPerformAirSpin += PerformAirSpinWrapper;
            PlayerController.Instance.OnPerformAttack += PerformNormalComboAttack; // Lắng nghe lệnh đánh thường
            PlayerController.Instance.OnPerformUppercut += PerformUppercutAttack;
            PlayerController.Instance.OnPerformAirAttack += PerformAirAttackWrapper; 
        }
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnPerformLowAttack -= PerformLowAttackWrapper;
            PlayerController.Instance.OnPerformSmash -= PerformSmashAttack;
            PlayerController.Instance.OnPerformJumpAttack -= PerformUppercutAttack;
            PlayerController.Instance.OnPerformRisingAttack -= PerformUppercutAttack;
            PlayerController.Instance.OnPerformAirSpin -= PerformAirSpinWrapper;
            PlayerController.Instance.OnPerformAttack -= PerformNormalComboAttack;
            PlayerController.Instance.OnPerformUppercut -= PerformUppercutAttack;
            PlayerController.Instance.OnPerformAirAttack -= PerformAirAttackWrapper; 
        }
    }

    // --- COMBO & ATTACK LOGIC ---
    private void PerformNormalComboAttack() 
    {
        // 1. Tính toán nhịp Combo ngay tại đây
        if (Time.time - lastAttackTime > comboTimeout) comboStep = 0;
        comboStep = (comboStep % 4) + 1;
        lastAttackTime = Time.time;

        // 2. Báo cho PlayerAnimation biết nhịp mấy để chạy Anim
        OnComboAttackStep?.Invoke(comboStep);

        // 3. Thực thi đòn đánh
        PerformAttackWithType(baseDamage, 0, AttackHitboxType.Normal); 
    }

    private void PerformLowAttackWrapper() => PerformAttackWithType(baseDamage, 0, AttackHitboxType.Normal);
    private void PerformAirSpinWrapper() => PerformAttackWithType(baseDamage, 0, AttackHitboxType.AirSpin);
    private void PerformAirAttackWrapper() => PerformAttackWithType(baseDamage, 0, AttackHitboxType.Normal);
    private void PerformUppercutAttack() => PerformAttackWithType(baseDamage, 1, AttackHitboxType.Normal);
    private void PerformSmashAttack() => PerformAttackWithType(baseDamage, 2, AttackHitboxType.Smash);

    private void PerformAttackWithType(int damage, int hitType, AttackHitboxType hitboxType = AttackHitboxType.Normal)
    {
        EventManager.current?.onPlayerAttack();
        StopAllCoroutines(); 
        StartCoroutine(PunchRoutine(damage, hitType, hitboxType));
    }
    
    private IEnumerator PunchRoutine(int damage, int hitType, AttackHitboxType hitboxType)
    {
        float direction = Mathf.Sign(transform.localScale.x);
        
        bool isThrowing = false;
        int finalDamage = damage;
        Vector2 currentBoxSize = baseBoxSize;
        Vector2 currentAirSpinBoxSize = airSpinBoxSize;
        Vector2 currentSmashBoxSize = smashBoxSize;

        ApplyWeaponModifiers(ref finalDamage, ref currentBoxSize, ref currentAirSpinBoxSize, ref currentSmashBoxSize, ref isThrowing);

        HashSet<Collider2D> hitObjects = GetHitObjects(hitboxType, direction, currentBoxSize, currentAirSpinBoxSize, currentSmashBoxSize);
        ProcessHitObjects(hitObjects, finalDamage, hitType);

        if (isThrowing) ThrowEquippedWeapon(direction);

        yield return new WaitForSeconds(showHitboxTime);
    }

    private void ApplyWeaponModifiers(ref int finalDamage, ref Vector2 currentBoxSize, ref Vector2 currentAirSpinBoxSize, ref Vector2 currentSmashBoxSize, ref bool isThrowing)
    {
        if (currentWeapon == null) return;

        if (!weaponHasBeenUsedMelee)
        {
            weaponHasBeenUsedMelee = true;

            float bonusRange = currentWeapon.attackRange;
            float bonusRangeY = bonusRange * 0.2f;

            finalDamage += currentWeapon.damage;
            currentBoxSize.x += bonusRange;
            currentBoxSize.y += bonusRangeY;
            
            currentAirSpinBoxSize.x += bonusRange;
            currentAirSpinBoxSize.y += bonusRange;

            currentSmashBoxSize.x += bonusRange;
            currentSmashBoxSize.y += bonusRangeY;
        }
        else
        {
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
            EnemyBase enemy = obj.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                HandleEnemyHit(enemy, finalDamage, hitType);
                continue;
            }

            if (obj.CompareTag("DroppedWeapon"))
            {
                HandleDroppedWeapon(obj);
            }
        }
    }

    private void HandleEnemyHit(EnemyBase enemy, int finalDamage, int hitType)
    {
        Vector3 impactPosition = (gameObject.transform.position + enemy.transform.position) / 2;
        if (EventManager.current != null) EventManager.current.onHit(impactPosition); 
        enemy.GetHit(finalDamage, hitType); 
    }

    // --- WEAPON INVENTORY LOGIC ---
    private void HandleDroppedWeapon(Collider2D obj)
    {
        WeaponTBScript caught = null;

        if (obj.TryGetComponent(out DroppedWeapon drop))
        {
            caught = drop.GetWeaponData();
        }
        else if (obj.TryGetComponent(out FlyObject fly))
        {
            caught = fly.GetWeaponData();
        }

        if (caught != null) 
        {
            EquipWeapon(caught, obj.gameObject);
            Debug.Log("<color=cyan>Đã bắt được vũ khí bằng đòn đánh!</color>");
        }
    }

    public void EquipWeapon(WeaponTBScript newWeapon, GameObject weaponObject = null)
    {
        if (currentWeaponObject != null)
        {
            currentWeaponObject.transform.SetParent(null);
            SetWeaponPhysics(currentWeaponObject, true); 

            if (GlobalPoolManager.Instance != null) GlobalPoolManager.Instance.Return(currentWeaponObject);
            else Destroy(currentWeaponObject);
                
            currentWeaponObject = null;
        }

        if (newWeapon == null) 
        {
            currentWeapon = null;
            weaponHasBeenUsedMelee = false;
            OnWeaponEquipped?.Invoke(false);
            return;
        }

        currentWeapon = newWeapon;
        weaponHasBeenUsedMelee = false;
        
        if (weaponObject != null) currentWeaponObject = weaponObject;
        else if (newWeapon.currentPrefab != null && handSocket != null)
        {
            currentWeaponObject = (GlobalPoolManager.Instance != null) 
                ? GlobalPoolManager.Instance.Get(newWeapon.currentPrefab, handSocket.position) 
                : Instantiate(newWeapon.currentPrefab, handSocket.position, Quaternion.identity);
        }

        if (currentWeaponObject != null && handSocket != null)
        {
            currentWeaponObject.transform.SetParent(handSocket);
            currentWeaponObject.transform.localPosition = Vector3.zero;
            currentWeaponObject.transform.localRotation = Quaternion.identity;
            SetWeaponPhysics(currentWeaponObject, false); 
        }
        
        OnWeaponEquipped?.Invoke(true);
    }

    public void DropWeapon()
    {
        if (currentWeapon == null || currentWeaponObject == null) return;

        currentWeaponObject.transform.SetParent(null);

        if (currentWeaponObject.TryGetComponent(out DroppedWeapon dropScript))
            dropScript.Init(currentWeapon, transform);

        SetWeaponPhysics(currentWeaponObject, true);

        if (currentWeaponObject.TryGetComponent(out Rigidbody2D rb))
        {
            rb.velocity = Vector2.zero; 
            float dropDir = Mathf.Sign(transform.localScale.x) * -1f; // Văng ngược lại
            rb.AddForce(new Vector2(dropForce.x * dropDir, dropForce.y), ForceMode2D.Impulse);
            rb.angularVelocity = UnityEngine.Random.Range(-360f, 360f); 
        }

        currentWeapon = null;
        currentWeaponObject = null;
        weaponHasBeenUsedMelee = false;
        OnWeaponEquipped?.Invoke(false);
    }

    private void ThrowEquippedWeapon(float direction)
    {
        if (currentWeapon != null && currentWeapon.currentPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(direction * 1.2f, punchOffset.y, 0f);
        
            GameObject go = GlobalPoolManager.Instance.Get(currentWeapon.currentPrefab, spawnPos);
        
            if (go.TryGetComponent(out FlyObject fly)) 
            {
                if (go.TryGetComponent(out Rigidbody2D rb)) rb.velocity = Vector2.zero;

                fly.Launch(currentWeapon, new Vector2(direction, 0), currentWeapon.flySpeed, transform, true);
            }
            EquipWeapon(null);
        }
    }

    private void SetWeaponPhysics(GameObject weapon, bool isSimulated)
    {
        if (weapon.TryGetComponent(out Rigidbody2D rb)) rb.simulated = isSimulated;
        if (weapon.TryGetComponent(out Collider2D col)) col.enabled = isSimulated;
        if (weapon.TryGetComponent(out FlyObject fly)) fly.enabled = isSimulated;
    }

    private void OnDrawGizmosSelected()
    {
        float direction = Mathf.Sign(transform.localScale.x);

        Vector2 drawBoxSize = baseBoxSize;
        Vector2 drawAirSpinBoxSize = airSpinBoxSize;
        Vector2 drawSmashBoxSize = smashBoxSize;

        if (Application.isPlaying && currentWeapon != null && !weaponHasBeenUsedMelee)
        {
            float bonusRange = currentWeapon.attackRange;
            float bonusRangeY = bonusRange * 0.2f;

            drawBoxSize.x += bonusRange; drawBoxSize.y += bonusRangeY;
            drawAirSpinBoxSize.x += bonusRange; drawAirSpinBoxSize.y += bonusRange;
            drawSmashBoxSize.x += bonusRange; drawSmashBoxSize.y += bonusRangeY;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)transform.position + new Vector2(punchOffset.x * direction, punchOffset.y), drawBoxSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube((Vector2)transform.position + airSpinOffset, drawAirSpinBoxSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + new Vector2(smashOffset.x, smashOffset.y), drawSmashBoxSize);
        Gizmos.DrawWireCube((Vector2)transform.position + new Vector2(-smashOffset.x, smashOffset.y), drawSmashBoxSize);
    }
}