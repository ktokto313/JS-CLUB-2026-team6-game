using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {
    public static SpawnManager Instance { get; private set; }

    [Header("Wave Settings")]
    [SerializeField] private List<WaveData> waves; 
    [SerializeField] private float timeBetweenWaves = 3f;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    private void Awake() { Instance = this; }

    private void Start() { StartCoroutine(SpawnAllWavesRoutine()); }

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

        // Chỉ lấy vị trí điểm Spawn (đảm bảo bạn đặt các điểm này trên không trung)
        Vector3 airPos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
    
        // Sinh quái ra và để trọng lực lo phần còn lại
        GlobalPoolManager.Instance.Get(prefab, airPos);
    }
}