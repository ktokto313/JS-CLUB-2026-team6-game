using System;
using UnityEngine;

public class PlayerBodyCollider : MonoBehaviour
{
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private float duckHeightRatio = 0.5f;
    
    private Vector2 originSize;
    private Vector2 originOffset;
    private Vector2 duckingSize;
    private Vector2 duckingOffset;

    private void Awake()
    {
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();

        originSize = boxCollider.size;
        originOffset = boxCollider.offset;

        CalculateStat();
    }

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            // Nhóm đòn đánh bắt buộc phải cúi người (hitbox duck)
            PlayerController.Instance.OnPerformLowAttack += SetColliderDucking;
            
            // Tất cả các đòn khác đều làm đứng hitbox
            PlayerController.Instance.OnPerformSmash += SetColliderStanding;
            PlayerController.Instance.OnPerformJumpAttack += SetColliderStanding;
            PlayerController.Instance.OnPerformRisingAttack += SetColliderStanding;
            PlayerController.Instance.OnPerformAirSpin += SetColliderStanding;
            
            // --- SỬA Ở ĐÂY ---
            // Trỏ vào hàm có nhận tham số int
            PlayerController.Instance.OnPerformAttack += SetColliderStandingWithCombo; 
            
            PlayerController.Instance.OnPerformUppercut += SetColliderStanding;
            PlayerController.Instance.OnPerformAirAttack += SetColliderStanding; 
        }
    }

    // --- THÊM ONDESTROY ---
    // Bắt buộc phải có để dọn dẹp bộ nhớ khi Player chết/Reload Scene
    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnPerformLowAttack -= SetColliderDucking;
            
            PlayerController.Instance.OnPerformSmash -= SetColliderStanding;
            PlayerController.Instance.OnPerformJumpAttack -= SetColliderStanding;
            PlayerController.Instance.OnPerformRisingAttack -= SetColliderStanding;
            PlayerController.Instance.OnPerformAirSpin -= SetColliderStanding;
            
            PlayerController.Instance.OnPerformAttack -= SetColliderStandingWithCombo; 
            
            PlayerController.Instance.OnPerformUppercut -= SetColliderStanding;
            PlayerController.Instance.OnPerformAirAttack -= SetColliderStanding; 
        }
    }

    private void SetColliderDucking()
    {
        SetDucking(true);
    }

    private void SetColliderStanding()
    {
        SetDucking(false);
    }
    
    // Hàm phụ để khớp với Action<int> của OnPerformAttack
    private void SetColliderStandingWithCombo(int comboStep)
    {
        // Ta không quan tâm comboStep là mấy, chỉ cần biết nó đang đứng đánh
        SetDucking(false);
    }

    private void CalculateStat()
    {   
        float newHeight = originSize.y * duckHeightRatio;
        duckingSize = new Vector2(originSize.x, newHeight);
        
        float diff = originSize.y - duckingSize.y;
        
        duckingOffset = new Vector2(originOffset.x, originOffset.y - (diff / 2));
    }

    // --- HÀM PUBLIC ĐỂ NGƯỜI KHÁC GỌI ---
    public void SetDucking(bool isDucking)
    {
        if (isDucking)
        {
            boxCollider.size = duckingSize;
            boxCollider.offset = duckingOffset;
        }
        else
        {
            boxCollider.size = originSize;
            boxCollider.offset = originOffset;
        }
    }
}