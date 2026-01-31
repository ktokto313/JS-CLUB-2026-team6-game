using UnityEngine;

public class AxeMoster : EnemyBase
{
    public GameObject flyObjectPrefab;
    public Transform shootPoint;
    public float throwDistance = 10f;
    public float flySpeed = 15f;
    private bool hasThrown = false; 

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
            GameObject go = Instantiate(flyObjectPrefab, shootPoint.position, Quaternion.identity);
            FlyObject fly = go.GetComponent<FlyObject>();
            if (fly != null)
            {
                Vector2 dir = (player.position - shootPoint.position).normalized;
                fly.Launch(currentWeapon, dir, 15f, player, false);
            }
        }

        hasThrown = true; 
        Debug.Log("AxeMonster đã ném FlyObject!");
    }
}