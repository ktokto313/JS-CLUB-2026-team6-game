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
        if (player == null || isAirborne || isStunned) return;
        
        float distance = Mathf.Abs(transform.position.x - player.position.x);
        
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
                Vector2 dir = (player.position - shootPoint.position).normalized;
                fly.Launch(CurrentWeaponTbScript, dir, flySpeed, player, false);
            }
        }

        hasThrown = true; 
    }
}