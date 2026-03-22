using System;
using UnityEngine;

public class PlayerBodyCollider : MonoBehaviour
{
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private float duckHeightRatio = 0.5f;
    [SerializeField] private float spinSizeRatio = 0.5f;
    [SerializeField] private float spinDuration = 0.4f;
    
    private Vector2 originSize;
    private Vector2 originOffset;
    private Vector2 duckingSize;
    private Vector2 duckingOffset;
    private Vector2 spinningSize;

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
            PlayerController.Instance.OnPerformAirSpin += SetColliderSpinning;
            
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
            PlayerController.Instance.OnPerformAirSpin -= SetColliderSpinning;
            
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
        boxCollider.size = originSize;
        boxCollider.offset = originOffset;
    }
    
    private void SetColliderSpinning()
    {
        boxCollider.size = spinningSize;
        boxCollider.offset = originOffset; // Giữ nguyên tâm khi xoay
        
        StopAllCoroutines();
        StartCoroutine(ResetSpinRoutine());
    }

    private System.Collections.IEnumerator ResetSpinRoutine()
    {
        yield return new WaitForSeconds(spinDuration);
        SetColliderStanding();
    }
    
    // Hàm phụ để khớp với Action<int> của OnPerformAttack
    private void SetColliderStandingWithCombo()
    {
        // Ta không quan tâm comboStep là mấy, chỉ cần biết nó đang đứng đánh
        SetColliderStanding();
    }

    private void CalculateStat()
    {   
        float newHeight = originSize.y * duckHeightRatio;
        duckingSize = new Vector2(originSize.x, newHeight);
        
        float diff = originSize.y - duckingSize.y;
        
        duckingOffset = new Vector2(originOffset.x, originOffset.y - (diff / 2));

        float squareSize = Mathf.Max(originSize.x, originSize.y) * spinSizeRatio;
        spinningSize = new Vector2(squareSize, squareSize);
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
            SetColliderStanding();
        }
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider2D bc = boxCollider != null ? boxCollider : GetComponent<BoxCollider2D>();
        if (bc == null) return;

        // Lấy kích thước chuẩn (Nếu chưa chạy game thì lấy từ bc, nếu đang chạy thì lấy originSize đã lưu)
        Vector2 testOriginSize = Application.isPlaying ? originSize : bc.size;
        Vector2 testOriginOffset = Application.isPlaying ? originOffset : bc.offset;

        // Cúi người (Ducking) - Màu vàng
        float newDuckingHeight = testOriginSize.y * duckHeightRatio;
        Vector2 testDuckingSize = new Vector2(testOriginSize.x, newDuckingHeight);
        float diff = testOriginSize.y - testDuckingSize.y;
        Vector2 testDuckingOffset = new Vector2(testOriginOffset.x, testOriginOffset.y - (diff / 2));

        // Xoay (Spinning) - Màu lục lam
        float squareTestSize = Mathf.Max(testOriginSize.x, testOriginSize.y) * spinSizeRatio;
        Vector2 testSpinningSize = new Vector2(squareTestSize, squareTestSize);
        Vector2 testSpinningOffset = testOriginOffset;

        // Quy đổi để vẽ theo toạ độ tỷ lệ scale của object
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(testDuckingOffset, testDuckingSize);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(testSpinningOffset, testSpinningSize);
    }
}