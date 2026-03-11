using UnityEngine;
using System.Collections;

public class ChargerEnemy : EnemyBase {
    [Header("Charger Settings")]
    public float dashSpeed = 15f;
    public float dashRange = 7f;
    public float prepTime = 2.0f; // Thời gian đứng im gồng
    
    private bool hasWeapon = true;

    protected override void Update() {
        // Nếu đã mất vũ khí, chạy logic cơ bản (đi bộ đến gần rồi đánh)
        if (!hasWeapon) {
            base.Update();
            return;
        }
        if (player == null || isAirborne || isStunned || isPerformingAction) return;

        float distance = Mathf.Abs(transform.position.x - player.position.x);

        // Bắt đầu chuỗi hành động khi vào tầm 7f
        if (distance <= dashRange) {
            StartCoroutine(ChargeSequence());
        } else {
            // Chưa tới tầm thì đi bộ tiếp cận
            if (anim) anim.SetBool("isWalking", true);
            MoveTowardsPlayer();
        }
    }

    IEnumerator ChargeSequence() {
        isPerformingAction = true;

        // 1. GIAI ĐOẠN LẤY ĐÀ 
        if (anim) {
            anim.SetBool("isWalking", false);
            anim.SetTrigger("prepDash"); 
        }
        rb.velocity = Vector2.zero;
        LookAtPlayer();
        
        yield return new WaitForSeconds(prepTime);

        // 2. GIAI ĐOẠN LAO
        if (anim) anim.SetTrigger("dash");
        
        float dashDir = player.position.x > transform.position.x ? 1 : -1;
        // Điểm đích cách vị trí hiện tại 14f (vượt qua player)
        float targetX = transform.position.x + (dashDir * dashRange * 2); 
        
        float dashTimer = 0;
        while (dashTimer < 0.8f) { // Timeout 0.8s tránh trường hợp bị kẹt
            rb.velocity = new Vector2(dashDir * dashSpeed, rb.velocity.y);
            
            // Check nếu đã vượt qua targetX
            if ((dashDir > 0 && transform.position.x >= targetX) || 
                (dashDir < 0 && transform.position.x <= targetX)) break;

            dashTimer += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        
        // Nghỉ một chút sau khi lao xong rồi mới mở khóa hành động
        yield return new WaitForSeconds(0.5f);
        isPerformingAction = false;
    }

    public override void GetHit(int damage, int hitType) {
        // Nếu dính đòn khi đang có vũ khí -> Rơi vũ khí
        if (hasWeapon) {
            LoseWeapon();
        }
        
        // Xử lý bị đánh (mất máu, văng ngược) như EnemyBase
        base.GetHit(damage, hitType);
    }

    private void LoseWeapon() {
        hasWeapon = false;
        
        // Nếu đang lao dở mà bị đánh trúng thì hủy ngay hành động đó
        isPerformingAction = false; 
        StopCoroutine("ChargeSequence"); 

        if (anim) anim.SetTrigger("loseWeapon");

        // Spawn vũ khí rơi (dùng logic của bạn)
        if (weaponItemPrefab != null && CurrentWeaponTbScript != null) {
            GameObject dropObj = GlobalPoolManager.Instance.Get(weaponItemPrefab, transform.position + Vector3.up);
            dropObj.GetComponent<DroppedWeapon>()?.Init(CurrentWeaponTbScript, player);
        }
        
        Debug.Log("Charger dropped weapon! Now acting as a normal enemy.");
    }
}