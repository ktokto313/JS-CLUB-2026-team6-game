using _Game.Scripts.Core;
using UnityEngine;

public class FlyObject : MonoBehaviour
{
    private WeaponTBScript _weaponTbScriptData;
    private GameObject _specificPrefab;
    private Transform playerTransform; 
    private Rigidbody2D rb;
    private bool hasPassedPlayer = false;
    private bool isPlayerOwned = false; 
    private bool isReturning = false;
    [Header("Visual Ring Settings")]
    private LineRenderer lineRenderer;
    [SerializeField] private float ringRadius = 0.1f;
    [SerializeField] private int segments = 6; 
    [SerializeField] private float dashSize = 0.2f; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SetupLineRenderer();
    }
    private void SetupLineRenderer()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null) lineRenderer = gameObject.AddComponent<LineRenderer>();
        
        lineRenderer.useWorldSpace = false; 
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = segments + 1;
        
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }
private void OnEnable()
    {
        isReturning = false;
        hasPassedPlayer = false;
        isPlayerOwned = false;
        gameObject.tag = "FlyObject";
        transform.rotation = Quaternion.identity;
        UpdateRingVisual(); 
        CancelInvoke();
    }

    // public void Launch(WeaponTBScript data, Vector2 direction, float speedOverride, Transform player, bool fromPlayer)
    // {   
    //     if (data.currentPrefab == null && data.weaponSkins != null && data.weaponSkins.Count > 0)
    //     {
    //         _specificPrefab = data.weaponSkins[0].projectilePrefab;
    //     }
    //     else
    //     {
    //         _specificPrefab = data.currentPrefab;
    //     }
    //     _weaponTbScriptData = data;
    //     playerTransform = player;
    //     isPlayerOwned = fromPlayer; 

    //     if (rb == null) rb = GetComponent<Rigidbody2D>();
    //     rb.gravityScale = 0;
    //     rb.velocity = Vector2.zero;
    //     rb.angularVelocity = 0;

    //     float finalSpeed = (data.flySpeed > 0) ? data.flySpeed : speedOverride;
    //     rb.velocity = direction * finalSpeed;

    //     gameObject.tag = "FlyObject"; 
    //     float lifeTime = (data.lifeTime > 0) ? data.lifeTime : 5f; 
    //     Invoke("ReturnToPool", lifeTime);

    //      if (TryGetComponent(out DroppedWeapon drop))
    //     {
    //         drop.SetDataOnly(data, _specificPrefab);
    //     }

    // }

    // Thêm tham số GameObject spawnedPrefab vào vị trí thứ 2
    public void Launch(WeaponTBScript data, GameObject spawnedPrefab, Vector2 direction, float speedOverride, Transform player, bool fromPlayer)
    { 
        _weaponTbScriptData = data;
        _specificPrefab = spawnedPrefab; 
        playerTransform = player;
        isPlayerOwned = fromPlayer; 

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;

        float finalSpeed = (data.flySpeed > 0) ? data.flySpeed : speedOverride;
        rb.velocity = direction * finalSpeed;

        if (data.type == WeaponType.Spear)
        {
            float targetAngle = (direction.x > 0) ? 0f : 180f;
            transform.rotation = Quaternion.Euler(0, 0, targetAngle);
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }

        gameObject.tag = "FlyObject"; 
        float lifeTime = (data.lifeTime > 0) ? data.lifeTime : 5f; 
        Invoke("ReturnToPool", lifeTime);

        if (TryGetComponent(out DroppedWeapon drop))
        {
            drop.SetDataOnly(data, _specificPrefab);
        }
    }

    void Update()
    {
        if (_weaponTbScriptData != null)
        {
            if (_weaponTbScriptData.type == WeaponType.Melee)
            {
                transform.Rotate(0, 0, 1000 * Time.deltaTime);
            }
            else if (_weaponTbScriptData.type == WeaponType.Spear)
            {
                {float targetZ = (rb.velocity.x >= 0) ? 0f : 180f;
                    transform.eulerAngles = new Vector3(0, 0, targetZ);
                }
            }
        }

        if (!isPlayerOwned && !hasPassedPlayer && playerTransform != null)
        {
            CheckIfPassedPlayer();
        }
    }

    private void ReturnToPool()
    {
        CancelInvoke();
        transform.SetParent(null); 
        if (rb != null) { rb.velocity = Vector2.zero; rb.angularVelocity = 0; }
        GlobalPoolManager.Instance.Return(gameObject);
    }
    

    private void CheckIfPassedPlayer()
    {
        float moveDir = rb.velocity.x;
        if ((moveDir > 0 && transform.position.x > playerTransform.position.x) || 
            (moveDir < 0 && transform.position.x < playerTransform.position.x))
        {
            hasPassedPlayer = true;
            gameObject.tag = "DroppedWeapon"; 
            UpdateRingVisual(); 
        }
    }
    
    public void Reflect(Vector2 newDir)
    {
        CancelInvoke(); 
        isPlayerOwned = true;
        gameObject.tag = "FlyObject";
        UpdateRingVisual(); 
        rb.velocity = newDir * (_weaponTbScriptData.flySpeed * 1.5f); 
        Invoke("ReturnToPool", _weaponTbScriptData.lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isPlayerOwned) 
        {
            if (collision.CompareTag("Player") && !hasPassedPlayer)
            {
                if (playerTransform != null && EventManager.current != null) {
                    //EventManager.current.onHit(playerTransform.position);
                    PlayerController.Instance.TakeDamage();
                    Debug.Log("FlyObject va trúng Player");
                }
                ReturnToPool();
            }

            if (collision.CompareTag("Enemy") && hasPassedPlayer)
            {
                HandleEnemyHit(collision);
            }
        }
        else if (collision.CompareTag("Enemy"))
        {
            HandleEnemyHit(collision);
        }
    }

    private void HandleEnemyHit(Collider2D collision)
    {
        if (_weaponTbScriptData == null) return; 

        EnemyBase enemy = collision.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            enemy.GetHit(_weaponTbScriptData.damage, 3);
            if (EventManager.current != null) {
                Vector3 impactPosition = (gameObject.transform.position + enemy.transform.position) / 2;
                //EventManager.current.onHit(impactPosition);
            }
        
            ReturnToPool();
        }
    }

    public WeaponTBScript GetWeaponData() 
    {
        if (_weaponTbScriptData != null)
        {
            _weaponTbScriptData.currentPrefab = _specificPrefab;
        }
        return _weaponTbScriptData;
    }
    
    public void UpdateRingVisual()
    {
        if (lineRenderer == null) return;

        Color ringColor;
        if (gameObject.CompareTag("FlyObject"))
        {
            ringColor = new Color(1f, 0f, 0f, 0.4f);
        }
        else 
        {
            ringColor = new Color(0f, 1f, 0f, 0.8f);
        }

        lineRenderer.startColor = ringColor;
        lineRenderer.endColor = ringColor;

        DrawDashedCircle();
    }

    private void DrawDashedCircle()
    {
        float deltaTheta = (2f * Mathf.PI) / segments;
        float theta = 0f;

        for (int i = 0; i < segments + 1; i++)
        {
            float x = ringRadius * Mathf.Cos(theta);
            float y = ringRadius * Mathf.Sin(theta);
            
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
            theta += deltaTheta;
        }
        lineRenderer.textureMode = LineTextureMode.RepeatPerSegment;
    }
}