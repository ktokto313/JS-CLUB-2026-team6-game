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

    [Header("Weapon System (Path A - Object Attachment)")] 
    public WeaponTBScript currentWeapon;
    [SerializeField] private Transform handSocket;      // Vị trí gắn vũ khí trên tay
    private GameObject currentWeaponObject;             // Vật thể thật đang dùng để minh họa trên tay
    
    [Header("Throw Timers (For Coroutine)")]
    [SerializeField] private float throwDelay = 0.05f; 
    [SerializeField] private float recoverDelay = 0.1f;

    [Header("Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float showHitboxTime = 0.1f;
    [SerializeField] private Vector2 dropForce = new Vector2(4f, 6f); 

    public event Action<bool> OnWeaponEquipped;
    public event Action<int> OnComboAttackStep; 

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnPerformLowAttack += PerformLowAttackWrapper;
            PlayerController.Instance.OnPerformSmash += PerformSmashAttack;
            PlayerController.Instance.OnPerformJumpAttack += PerformUppercutAttack;
            PlayerController.Instance.OnPerformRisingAttack += PerformUppercutAttack;
            PlayerController.Instance.OnPerformAirSpin += PerformAirSpinWrapper;
            PlayerController.Instance.OnPerformAttack += PerformNormalComboAttack; 
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

    // --- GỌI ĐÒN ĐÁNH ---
    private void PerformNormalComboAttack() 
    {
        if (Time.time - lastAttackTime > comboTimeout) comboStep = 0;
        comboStep = (comboStep % 4) + 1;
        lastAttackTime = Time.time;

        OnComboAttackStep?.Invoke(comboStep);
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
    
    // --- BỘ NÃO LOGIC ---
    private IEnumerator PunchRoutine(int damage, int hitType, AttackHitboxType hitboxType)
    {
        float direction = Mathf.Sign(transform.localScale.x);
        
        // NHÁNH 1: ĐANG CẦM VŨ KHÍ LÀ QUĂNG ĐI NGAY
        if (currentWeapon != null)
        {
            yield return new WaitForSeconds(throwDelay);
            ThrowEquippedWeapon(direction);
            yield return new WaitForSeconds(recoverDelay);
            
            yield break; // Ép thoát để không gọi Hitbox cận chiến
        }

        // NHÁNH 2: TAY KHÔNG GÂY SÁT THƯƠNG
        HashSet<Collider2D> hitObjects = GetHitObjects(hitboxType, direction, baseBoxSize, airSpinBoxSize, smashBoxSize);
        ProcessHitObjects(hitObjects, damage, hitType);

        yield return new WaitForSeconds(showHitboxTime);
    }

    // --- LOGIC NÉM VŨ KHÍ (CÁCH A) ---
private void ThrowEquippedWeapon(float direction)
    {   
        if (currentWeapon == null) return;
        
        if (currentWeapon.currentPrefab == null)
        {
            Debug.LogError($"<color=red>Lỗi: {currentWeapon.name} không có prefab để ném!</color>");
            return;
        }

        Vector3 spawnPos = transform.position + new Vector3(direction * 1.2f, punchOffset.y, 0f);
        GameObject go = GlobalPoolManager.Instance.Get(currentWeapon.currentPrefab, spawnPos);
        
        if (go.TryGetComponent(out FlyObject fly)) 
        {
            if (go.TryGetComponent(out Rigidbody2D rb)) rb.velocity = Vector2.zero;

            fly.Launch(currentWeapon, new Vector2(direction, 0.1f), currentWeapon.flySpeed, transform, true);
        }
        
        EquipWeapon(null);
    }

    public void EquipWeapon(WeaponTBScript newWeapon, GameObject weaponObject = null)
    {
        if (currentWeaponObject != null)
        {
            currentWeaponObject.transform.SetParent(null);
            SetWeaponPhysics(currentWeaponObject, true); 

            if (GlobalPoolManager.Instance != null) 
                GlobalPoolManager.Instance.Return(currentWeaponObject);
            else 
                Destroy(currentWeaponObject);
                
            currentWeaponObject = null;
        }

        if (newWeapon == null) 
        {
            currentWeapon = null;
            OnWeaponEquipped?.Invoke(false);
            return;
        }

        currentWeapon = newWeapon;
        
        if (weaponObject != null)
        {
            currentWeaponObject = weaponObject;
        }
        else if (currentWeapon.currentPrefab != null)
        {
            currentWeaponObject = GlobalPoolManager.Instance.Get(currentWeapon.currentPrefab, handSocket.position);
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

    // Hàm dùng khi bị quái đánh rớt vũ khí (Nếu cần)
    public void DropWeapon()
    {
        if (currentWeapon == null || currentWeaponObject == null) return;

        currentWeaponObject.transform.SetParent(null);
        if (currentWeaponObject.TryGetComponent(out DroppedWeapon dropScript))
            dropScript.Init(currentWeapon, transform);

        // Bật lại vật lý và tác dụng lực văng
        SetWeaponPhysics(currentWeaponObject, true);
        if (currentWeaponObject.TryGetComponent(out Rigidbody2D rb))
        {
            rb.velocity = Vector2.zero; 
            float dropDir = Mathf.Sign(transform.localScale.x) * -1f; 
            rb.AddForce(new Vector2(dropForce.x * dropDir, dropForce.y), ForceMode2D.Impulse);
            rb.angularVelocity = UnityEngine.Random.Range(-360f, 360f); 
        }

        currentWeapon = null;
        currentWeaponObject = null;
        OnWeaponEquipped?.Invoke(false);
    }

    // --- LOGIC XỬ LÝ VA CHẠM (HITBOX) ---
    private void ProcessHitObjects(HashSet<Collider2D> hitObjects, int finalDamage, int hitType)
    {
        foreach (Collider2D obj in hitObjects)
        {   
            // Bắt bệnh Hitbox
            Debug.Log($"<color=yellow>Hitbox chạm vào: {obj.gameObject.name} (Tag: {obj.gameObject.tag})</color>");

            EnemyBase enemy = obj.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                HandleEnemyHit(enemy, finalDamage, hitType);
                continue; 
            }

            // Tìm vũ khí
            DroppedWeapon drop = obj.GetComponentInParent<DroppedWeapon>();
            FlyObject fly = obj.GetComponentInParent<FlyObject>();

            if (drop != null || fly != null)
            {
                HandleDroppedWeapon(drop, fly);
            }
        }
    }

    private void HandleDroppedWeapon(DroppedWeapon drop, FlyObject fly)
    {
        WeaponTBScript caught = null;
        GameObject weaponObject = null;

        // 1. Dò xem thằng FlyObject có cầm Data không (Ưu tiên đồ đang bay/quái ném)
        if (fly != null && fly.GetWeaponData() != null)
        {
            caught = fly.GetWeaponData();
            weaponObject = fly.gameObject;
        }
        // 2. Nếu không có, dò tiếp xem thằng DroppedWeapon có cầm Data không (Đồ rớt dưới đất)
        else if (drop != null && drop.GetWeaponData() != null)
        {
            caught = drop.GetWeaponData();
            weaponObject = drop.gameObject;
        }
        // 3. Fallback để lấy tên object log lỗi nếu cả 2 đều vô dụng
        else 
        {
            weaponObject = (drop != null) ? drop.gameObject : (fly != null ? fly.gameObject : null);
        }

        // KẾT LUẬN
        if (caught != null) 
        {
            EquipWeapon(caught, weaponObject);
            Debug.Log($"<color=cyan>ĐÃ BẮT ĐƯỢC VŨ KHÍ: {caught.name}!</color>");
        }
        else
        {
            Debug.LogError($"<color=red>LỖI: Đấm trúng [{weaponObject?.name}], nhưng cả script FlyObject và DroppedWeapon trên đó đều bị rỗng Data!</color>");
        }
    }

    private void HandleEnemyHit(EnemyBase enemy, int finalDamage, int hitType)
    {
        Vector3 impactPosition = (gameObject.transform.position + enemy.transform.position) / 2;
        if (EventManager.current != null) EventManager.current.onHit(impactPosition); 
        enemy.GetHit(finalDamage, hitType); 
    }

    private void SetWeaponPhysics(GameObject weapon, bool isSimulated)
    {
        if (weapon.TryGetComponent(out Rigidbody2D rb)) rb.simulated = isSimulated;
        if (weapon.TryGetComponent(out Collider2D col)) col.enabled = isSimulated;
        if (weapon.TryGetComponent(out FlyObject fly)) fly.enabled = isSimulated;
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

    private void OnDrawGizmosSelected()
    {
        float direction = Mathf.Sign(transform.localScale.x);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)transform.position + new Vector2(punchOffset.x * direction, punchOffset.y), baseBoxSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube((Vector2)transform.position + airSpinOffset, airSpinBoxSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + new Vector2(smashOffset.x, smashOffset.y), smashBoxSize);
        Gizmos.DrawWireCube((Vector2)transform.position + new Vector2(-smashOffset.x, smashOffset.y), smashBoxSize);
    }
}