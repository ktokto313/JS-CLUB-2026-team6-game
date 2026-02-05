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

        // Chọn vị trí ngẫu nhiên trong danh sách điểm Spawn
        Vector3 pos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        
        // GỌI POOL: Lấy quái ra thay vì Instantiate
        GlobalPoolManager.Instance.Get(prefab, pos);
    }
}