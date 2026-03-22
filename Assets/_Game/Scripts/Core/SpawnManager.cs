using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {
    public static SpawnManager Instance { get; private set; }

    [Header("Global Settings")]
    [SerializeField] private List<WaveData> waves; 
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private Transform[] spawnPoints;

    private void Awake() { Instance = this; }

    private void Start() { 
        StartCoroutine(MasterWaveRoutine()); 
    }

    private IEnumerator MasterWaveRoutine() {
        foreach (WaveData wave in waves) {
            Debug.Log($"<color=cyan><b>START WAVE: {wave.waveName}</b></color>");
            
            // Chạy từng bước (Step) trong Wave
            foreach (var step in wave.spawnSteps) {
                if (step.delayAfterLastStep > 0)
                    yield return new WaitForSeconds(step.delayAfterLastStep);

                // Kích hoạt Coroutine spawn quái cho bước này (chạy song song để không chặn bước sau)
                StartCoroutine(ExecuteSpawnStep(step));
            }

            // Đợi wave kết thúc (tùy logic bạn muốn đợi thời gian cố định hay đợi quái chết hết)
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private IEnumerator ExecuteSpawnStep(EnemySpawnStep step) {
        for (int i = 0; i < step.count; i++) {
            SpawnEnemy(step);
            if (step.interval > 0)
                yield return new WaitForSeconds(step.interval);
        }
    }

    private void SpawnEnemy(EnemySpawnStep step) {
        if (spawnPoints.Length == 0 || step.enemyPrefab == null) return;

        // Chọn vị trí
        Vector3 spawnPos;
        if (step.useRandomPoint) {
            spawnPos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        } else {
            int index = Mathf.Clamp(step.specificPointIndex, 0, spawnPoints.Length - 1);
            spawnPos = spawnPoints[index].position;
        }

        GlobalPoolManager.Instance.Get(step.enemyPrefab, spawnPos);
    }
}