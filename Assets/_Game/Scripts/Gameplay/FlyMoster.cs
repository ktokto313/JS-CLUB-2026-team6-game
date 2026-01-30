using UnityEngine;

public class RocketRider : MonoBehaviour
{
    public float speed = 8f;
    public float mapLimitLeft = -15f;
    public float mapLimitRight = 15f;
    
    public float heightPhase1 = 5f; 
    public float heightPhase2 = 3f; 
    public float heightPhase3 = 1f; 
    
    public GameObject explosionEffect;
    public float explosionRadius = 2f;
    public int explosionDamage = 2;

    private int currentPhase = 1;
    private int direction = 1;
    private bool isDead = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        transform.position = new Vector3(transform.position.x, heightPhase1, 0);
    }

    void Update()
    {
        if (isDead)
        {
            if (rb.velocity != Vector2.zero)
            {
                float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
            return;
        }
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);
        CheckBoundaries();
    }

    void CheckBoundaries()
    {
        if ((direction == 1 && transform.position.x >= mapLimitRight) || 
            (direction == -1 && transform.position.x <= mapLimitLeft))
        {
            direction *= -1; 
            transform.localScale = new Vector3(direction, 1, 1); // Quay mặt

            if (currentPhase < 3)
            {
                currentPhase++;
                float nextHeight = (currentPhase == 2) ? heightPhase2 : heightPhase3;
                transform.position = new Vector3(transform.position.x, nextHeight, 0);
            }
        }
    }

    public void GetHit(int hitType)
    {
        if (isDead) return;
        Die();
    }

    void Die()
    {
        isDead = true;
        rb.gravityScale = 2; 
        float pushDir = direction; 
        rb.AddForce(new Vector2(pushDir * 3f, 3f), ForceMode2D.Impulse);
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Ground"))
        {
            Explode();
        }
    }

    void Explode()
    {
        if (explosionEffect != null) Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D obj in hitPlayers)
        {
            if (obj.CompareTag("Player"))
            {
                Debug.Log("Player dính bom nổ!");
            }
            else
            {
                Debug.Log("Quái bị trừ máu");
            }
        }

        Destroy(gameObject);
    }
}