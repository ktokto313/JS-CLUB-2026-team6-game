using UnityEngine;

public class AxeMoster : EnemyBase {
    public GameObject flyObjectPrefab; 
    public float throwDistance = 10f;
    public float flySpeed = 2f;
    private bool hasThrown = false; 
    protected Animator anim;
    protected override void Awake() {
        base.Awake();
        anim = GetComponent<Animator>();
    }

    protected override void Update() {
        if (!hasThrown && player != null && Mathf.Abs(transform.position.x - player.position.x) <= throwDistance) {
            StartThrowing();
            return;
        }
        base.Update();
    }

    void StartThrowing() {
        hasThrown = true;
        isPerformingAction = true; 
        LookAtPlayer();
        if (anim) anim.SetTrigger("throw");
    }

    // Gán Animation Event tại frame vung tay
    public void ExecuteThrow() {
        float dir = Mathf.Sign(transform.localScale.x);
        Vector3 spawnPos = transform.position + new Vector3(dir * 0.5f, 1.3f, 0f);
        GameObject go = GlobalPoolManager.Instance.Get(flyObjectPrefab, spawnPos);
        
        if (go.TryGetComponent(out FlyObject fly))
            fly.Launch(CurrentWeaponTbScript, new Vector2(dir, 0), flySpeed, player, false);
    }

    // Gán Animation Event tại frame cuối cùng của clip ném
    public void FinishThrowing() => isPerformingAction = false; // Cho phép đi tiếp
}