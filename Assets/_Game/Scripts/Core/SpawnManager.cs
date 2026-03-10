using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {
    public static SpawnManager Instance { get; private set; }

    [Header("Wave Settings")]
    [SerializeField] private List<WaveData> waves; 
    [SerializeField] private float timeBetweenWaves = 5f;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        StartCoroutine(SpawnAllWavesRoutine());
    }

    private IEnumerator SpawnAllWavesRoutine() {
        foreach (WaveData wave in waves) {
            yield return new WaitForSeconds(timeBetweenWaves);
            
            for (int i = 0; i < wave.count; i++) {
                SpawnFromPool(wave.enemyPrefab);
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }
    }

private void SpawnFromPool(GameObject prefab) {
    if (spawnPoints.Length == 0) return;

    // 1. Chọn một điểm Spawn ngẫu nhiên trong danh sách
    Vector3 airPos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
    
    // 2. Bắn một tia Ray từ trên cao xuống để tìm mặt đất
    // Chú ý: LayerMask.GetMask("Ground") phải khớp với tên Layer mặt đất của bạn
    RaycastHit2D hit = Physics2D.Raycast(airPos, Vector2.down, 15f, LayerMask.GetMask("Ground"));

    Vector3 finalPos = airPos; // Mặc định nếu không thấy đất thì dùng vị trí cũ

    if (hit.collider != null) {
        // Nếu thấy đất, đặt quái ngay tại điểm chạm (hit.point)
        finalPos = hit.point; 
    } else {
        Debug.LogWarning("SpawnManager: Không tìm thấy mặt đất phía dưới điểm " + airPos);
    }

    // 3. Gọi Pool sinh quái tại vị trí chuẩn
    GlobalPoolManager.Instance.Get(prefab, finalPos);
}
}