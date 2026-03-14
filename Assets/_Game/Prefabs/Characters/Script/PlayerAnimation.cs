using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator anim;

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            // 0. HIT EFFECT
            PlayerController.Instance.OnHitAction += PlayHitBlink;

            // 1. NHÓM DUCK (Phím S)
            PlayerController.Instance.OnPerformLowAttack += PlayLowAttack;
            PlayerController.Instance.OnPerformSmash += PlaySmash;

            // 2. NHÓM JUMP (Phím W)
            PlayerController.Instance.OnPerformJumpAttack += PlayJumpAttack;
            PlayerController.Instance.OnPerformRisingAttack += PlayRisingAttack;
            PlayerController.Instance.OnPerformAirSpin += PlayAirSpin;

            // 3. NHÓM ATTACK (Phím A/D)
            PlayerController.Instance.OnPerformAttack += PlayAttack;
            PlayerController.Instance.OnPerformUppercut += PlayUppercut;
            PlayerController.Instance.OnPerformAirAttack += PlayAirAttack; 
        }
    }

    private void Update()
    {
        if (PlayerController.Instance != null)
        {
            // (0: Standing, 1: Ducking, 2: Airborne, 3: Smashing)
            anim.SetInteger("State", (int)PlayerController.Instance.state);
        }
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
        {
            // 0. HIT EFFECT
            PlayerController.Instance.OnHitAction -= PlayHitBlink;

            // 1. NHÓM DUCK
            PlayerController.Instance.OnPerformLowAttack -= PlayLowAttack;
            PlayerController.Instance.OnPerformSmash -= PlaySmash;

            // 2. NHÓM JUMP
            PlayerController.Instance.OnPerformJumpAttack -= PlayJumpAttack;
            PlayerController.Instance.OnPerformRisingAttack -= PlayRisingAttack;
            PlayerController.Instance.OnPerformAirSpin -= PlayAirSpin;

            // 3. NHÓM ATTACK
            PlayerController.Instance.OnPerformAttack -= PlayAttack;
            PlayerController.Instance.OnPerformUppercut -= PlayUppercut;
            PlayerController.Instance.OnPerformAirAttack -= PlayAirAttack; 
        }
    }

    private void PlayHitBlink()
    {
        anim.SetTrigger("Hit"); // Cứ để sẵn nếu sau này cần Anim Hit
    }
    
    // 1. NHÓM DUCK (Phím S)
    private void PlayLowAttack()
    {
        anim.SetTrigger("LowAttack");
    }

    private void PlaySmash()
    {
        anim.SetTrigger("Smash");
    }

    // 2. NHÓM JUMP (Phím W)
    private void PlayJumpAttack()
    {
        anim.SetTrigger("JumpAttack");
    }

    private void PlayRisingAttack()
    {
        anim.SetTrigger("RisingAttack");
    }

    private void PlayAirSpin()
    {
        anim.SetTrigger("AirSpin");
    }

    [Header("Combo Settings")]
    [SerializeField] private float comboTimeout = 0.8f; 
    private int comboStep = 0;
    private float lastAttackTime = 0f;

    private void PlayAttack()
    {
        // Kiểm tra xem đã quá thời gian reset combo chưa
        if (Time.time - lastAttackTime > comboTimeout)
        {
            comboStep = 0;
        }

        comboStep++;
        
        // Chạy từ 1 đến 3 sau đó lặp lại
        if (comboStep > 3)
        {
            comboStep = 1; 
        }

        lastAttackTime = Time.time;

        // Gọi Trigger theo tên: Attack1, Attack2, Attack3
        // Đảm bảo trong Animator bạn đã tạo các Trigger này
        anim.SetTrigger("Attack" + comboStep);
    }

    private void PlayUppercut()
    {
        anim.SetTrigger("Uppercut");
    }

    private void PlayAirAttack()
    {
        anim.SetTrigger("AirAttack");
    }
}