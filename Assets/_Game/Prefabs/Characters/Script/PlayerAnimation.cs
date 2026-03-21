using _Game.Scripts.Core;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator anim;

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            // 0. HIT EFFECT VÀ DEATH
            EventManager.current.onHitAction += PlayHitBlink;
            EventManager.current.onPlayerDeadAction += PlayDeath;

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
            // 0. HIT EFFECT VÀ DEATH
            EventManager.current.onHitAction += PlayHitBlink;
            EventManager.current.onPlayerDeadAction += PlayDeath;

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

    private void PlayHitBlink(Vector3 pos)
    {
        anim.SetTrigger("Hit"); 
    }

    private void PlayDeath()
    {
        anim.SetBool("IsDead", true);
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

    // 3. NHÓM ATTACK (Phím A/D)
    private void PlayAttack(int comboStep)
    {
        // Nhận trực tiếp comboStep từ Controller (1, 2, hoặc 3)
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