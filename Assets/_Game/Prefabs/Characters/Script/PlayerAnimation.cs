using _Game.Scripts.Core;
using UnityEngine;
using System.Collections;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator anim;

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            // 0. HIT EFFECT VÀ DEATH
            EventManager.current.onPlayerHitAction += PlayHitBlink;
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
           EventManager.current.onPlayerHitAction -= PlayHitBlink;
            if (EventManager.current != null)
            {
                EventManager.current.onPlayerDeadAction -= PlayDeath;
            }

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

    [Header("Blink Settings")]
[SerializeField] private SpriteRenderer sr; // Kéo thả SpriteRenderer của Player vào đây
[SerializeField] private Color hitColor = Color.red; // Thường dùng màu Đỏ hoặc Trắng
[SerializeField] private float blinkDuration = 0.15f;
private Coroutine blinkCoroutine;

private void PlayHitBlink(Vector3 pos)
{
    // 1. Dừng ngay hiệu ứng nháy cũ nếu bị đánh liên tục 2 hit quá nhanh
    if (blinkCoroutine != null) 
    {
        StopCoroutine(blinkCoroutine);
    }
    // 2. Chạy hiệu ứng nháy mới
    blinkCoroutine = StartCoroutine(BlinkRoutine());
}

private IEnumerator BlinkRoutine()
{
    // Chuyển sang màu bị thương
    sr.color = hitColor;
    
    // Đợi 0.15s (Dùng WaitForSecondsRealtime để nháy ngay cả khi game bị Hitstop/TimeScale = 0)
    yield return new WaitForSecondsRealtime(blinkDuration);
    
    // Trả về màu gốc
    sr.color = Color.white; 
}

    private void PlayDeath()
    {
        anim.Play("Death");
        anim.SetBool("IsDead", true);
    }
    private int lowKickStep = 1;
    

    // 1. NHÓM DUCK (Phím S)
    private void PlayLowAttack()
    {
        anim.SetTrigger("LowAttack" + lowKickStep);
        lowKickStep = lowKickStep == 1 ? 2 : 1;
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

    [Header("Air Spin Visuals (New Setup)")]
    // Kéo object "AirSpinPivot" (Object nằm ở ngực) vào đây
    [SerializeField] private Transform airSpinPivot; 
    private Coroutine airSpinCoroutine;

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
        float spinSpeed = 1080f; // Tốc độ xoay (3 vòng/0.5s)

        // 1. LƯU LẠI GÓC QUAY BAN ĐẦU (Để bảo toàn hướng X, Y đang lật trái/phải)
        // Chúng ta lấy góc của cha (Root) để chắc chắn hướng mặt chuẩn
        Vector3 playerEuler = transform.eulerAngles;

        while (elapsed < duration)
        {
            // 2. XOAY NỘI BỘ (Local): Xoay cục Pivot tại chỗ quanh trục Z
            // Cái Sprite con sẽ tự cuộn tròn hoàn hảo quanh ngực mà không văng X ra ngoài
            airSpinPivot.Rotate(0, 0, spinSpeed * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. RESET TUYỆT ĐỐI GÓC QUAY
        // Chỉ ép trục Z về 0, giữ nguyên X, Y (hướng mặt)
        airSpinPivot.localEulerAngles = new Vector3(0, 0, 0f);
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