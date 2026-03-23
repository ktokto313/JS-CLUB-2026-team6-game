using _Game.Scripts.Core;
using UnityEngine;
using System.Collections;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private PlayerAttack playerAttack;

    private int lowKickStep = 1;
    private Coroutine blinkCoroutine;
    private Coroutine airSpinCoroutine;

    [Header("Blink Settings")]
    [SerializeField] private SpriteRenderer sr; 
    [SerializeField] private Color hitColor = Color.red; 
    [SerializeField] private float blinkDuration = 0.15f;
    
    [Header("Air Spin Visuals")]
    [SerializeField] private Transform airSpinPivot; 

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            EventManager.current.onPlayerHitAction += PlayHitBlink;
            EventManager.current.onPlayerDeadAction += PlayDeath;

            PlayerController.Instance.OnPerformLowAttack += PlayLowAttack;
            PlayerController.Instance.OnPerformSmash += PlaySmash;
            PlayerController.Instance.OnPerformJumpAttack += PlayJumpAttack;
            PlayerController.Instance.OnPerformRisingAttack += PlayRisingAttack;
            PlayerController.Instance.OnPerformAirSpin += PlayAirSpin;
            PlayerController.Instance.OnPerformUppercut += PlayUppercut;
            PlayerController.Instance.OnPerformAirAttack += PlayAirAttack; 
        }

        if (playerAttack != null)
        {
            playerAttack.OnComboAttackStep += PlayAttack;
            // 1. LẮNG NGHE LỆNH TRANG BỊ VŨ KHÍ
            playerAttack.OnWeaponEquipped += SetArmedState;
        }
    }

    private void Update()
    {
        if (PlayerController.Instance != null)
        {
            anim.SetInteger("State", (int)PlayerController.Instance.state);
        }
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
        {
            EventManager.current.onPlayerHitAction -= PlayHitBlink;
            if (EventManager.current != null) EventManager.current.onPlayerDeadAction -= PlayDeath;

            PlayerController.Instance.OnPerformLowAttack -= PlayLowAttack;
            PlayerController.Instance.OnPerformSmash -= PlaySmash;
            PlayerController.Instance.OnPerformJumpAttack -= PlayJumpAttack;
            PlayerController.Instance.OnPerformRisingAttack -= PlayRisingAttack;
            PlayerController.Instance.OnPerformAirSpin -= PlayAirSpin;
            PlayerController.Instance.OnPerformUppercut -= PlayUppercut;
            PlayerController.Instance.OnPerformAirAttack -= PlayAirAttack; 
        }

        if (playerAttack != null)
        {
            playerAttack.OnComboAttackStep -= PlayAttack;
            // NGẮT LẮNG NGHE
            playerAttack.OnWeaponEquipped -= SetArmedState;
        }
    }

    // --- HÀM DUY NHẤT ĐIỀU KHIỂN DÁNG CẦM VŨ KHÍ ---
    private void SetArmedState(bool isArmed)
    {
        if (anim != null)
        {
            anim.ResetTrigger("LowAttack1");
            anim.ResetTrigger("LowAttack2");
            anim.ResetTrigger("Attack1");
            anim.ResetTrigger("Attack2");
            anim.ResetTrigger("Attack3");
            anim.ResetTrigger("Attack4");
            anim.ResetTrigger("JumpAttack");
            anim.ResetTrigger("AirAttack");
            
            anim.SetBool("IsArmed", isArmed);
        }
    }

    // --- CÁC HÀM XỬ LÝ VISUAL VÀ ANIMATION TRIGGER GIỮ NGUYÊN ---
    private void PlayHitBlink(Vector3 pos)
    {
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        sr.color = hitColor;
        yield return new WaitForSecondsRealtime(blinkDuration);
        sr.color = Color.white; 
    }

    private void PlayDeath()
    {
        StopAllCoroutines();
        anim.Play("Death");
        anim.SetBool("IsDead", true);
    }

    private void PlayLowAttack()
    {
        anim.SetTrigger("LowAttack" + lowKickStep);
        lowKickStep = lowKickStep == 1 ? 2 : 1;
    }

    private void PlaySmash() => anim.SetTrigger("Smash");
    private void PlayJumpAttack() => anim.SetTrigger("JumpAttack");
    private void PlayRisingAttack() => anim.SetTrigger("RisingAttack");

    private void PlayAirSpin()
    {
        anim.SetTrigger("AirSpin");
        if (airSpinCoroutine != null) StopCoroutine(airSpinCoroutine);
        airSpinCoroutine = StartCoroutine(AirSpinRotateRoutine());
    }

    private IEnumerator AirSpinRotateRoutine()
    {
        float duration = 0.5f; 
        float elapsed = 0f;
        float spinSpeed = 1080f; 

        Vector3 playerEuler = transform.eulerAngles;

        while (elapsed < duration)
        {
            airSpinPivot.Rotate(0, 0, spinSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        airSpinPivot.localEulerAngles = Vector3.zero;
    }

    private void PlayAttack(int comboStep) => anim.SetTrigger("Attack" + comboStep);
    private void PlayUppercut() => anim.SetTrigger("Uppercut");
    private void PlayAirAttack() => anim.SetTrigger("AirAttack");
}