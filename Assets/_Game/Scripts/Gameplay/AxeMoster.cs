using UnityEngine;

public class AxeMoster: EnemyBase
{
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float throwDistance = 10f;
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
        if (projectilePrefab != null)
        {
            GameObject go = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
            Projectile p = go.GetComponent<Projectile>();
            
            Vector3 dir = (player.position - shootPoint.position).normalized;
            p.Launch(dir);
        }

        hasThrown = true; 
        Debug.Log("Đã ném xong");
    }
}