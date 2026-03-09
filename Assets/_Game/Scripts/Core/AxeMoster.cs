using UnityEngine;

public class AxeMoster : EnemyBase
{
    public GameObject flyObjectPrefab; 
    public Transform shootPoint;
    public float throwDistance = 10f;
    public float flySpeed = 15f;
    private bool hasThrown = false; 

    protected override void OnEnable()
    {
        base.OnEnable();
        hasThrown = false; 
    }

    protected override void Update()
    {
        if (isAirborne || isStunned) return;
        
        float distance = Mathf.Abs(transform.position.x - playerPos.x);
        
        if (!hasThrown && distance <= throwDistance)
        {
            HandleOpeningAttack();
            return; 
        }
        
        base.Update();
    }

    void HandleOpeningAttack()
    {
        if (flyObjectPrefab != null)
        {
            GameObject go = GlobalPoolManager.Instance.Get(flyObjectPrefab, shootPoint.position);
            
            FlyObject fly = go.GetComponent<FlyObject>();
            if (fly != null)
            {
                Vector2 dir = (playerPos - shootPoint.position).normalized;
                fly.Launch(CurrentWeaponTbScript, dir, flySpeed, playerPos, false);
            }
        }

        hasThrown = true; 
    }
}