using UnityEngine;

public class AxeMoster : EnemyBase {
    public float throwDistance = 7f;
    public float flySpeed = 2f;
    private bool hasThrown = false; 

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

    protected override void OnEnable()
    {
        anim.ResetTrigger("throw");
        hasThrown = false; 
        base.OnEnable();
    }
    public void ExecuteThrow() {
        if (selectedPrefab == null) return;

        float dir = Mathf.Sign(transform.localScale.x);
        Vector3 spawnPos = transform.position + new Vector3(dir * 0.5f, 1.3f, 0f);
    
        GameObject go = GlobalPoolManager.Instance.Get(selectedPrefab, spawnPos);
    
        if (go.TryGetComponent(out FlyObject fly)) {
            fly.Launch(CurrentWeaponTbScript, selectedPrefab, new Vector2(dir, 0), flySpeed, player, false);
        }
    }

    public void FinishThrowing() => isPerformingAction = false; 
}