using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {
    public static SpawnManager Instance { get; private set; }

    [Header("Global Settings")]
    [SerializeField] private List<WaveData> waves; 
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private Transform[] spawnPoints;

    [HideInInspector] public int currentWaveIndex = 1; 

    private void Awake() { Instance = this; }

    private void Start() { 
        StartCoroutine(MasterWaveRoutine()); 
    }

    private IEnumerator MasterWaveRoutine() {
        int waveCounter = 1; 
        
        foreach (WaveData wave in waves) {
            currentWaveIndex = waveCounter; 
            
            Debug.Log($"<color=cyan><b>START WAVE {currentWaveIndex}: {wave.waveName}</b></color>");
            
            foreach (var step in wave.spawnSteps) {
                if (step.delayAfterLastStep > 0)
                    yield return new WaitForSeconds(step.delayAfterLastStep);

                StartCoroutine(ExecuteSpawnStep(step));
            }

            float waveDuration = GetLongestStepDuration(wave);
            yield return new WaitForSeconds(waveDuration + timeBetweenWaves);
            
            waveCounter++; 
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

        Vector3 spawnPos;
        if (step.useRandomPoint) {
            spawnPos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        } else {
            int index = Mathf.Clamp(step.specificPointIndex, 0, spawnPoints.Length - 1);
            spawnPos = spawnPoints[index].position;
        }

        GlobalPoolManager.Instance.Get(step.enemyPrefab, spawnPos);
    }

    // Hàm tính thời gian để MasterWaveRoutine biết khi nào Wave thực sự xong
    private float GetLongestStepDuration(WaveData wave) {
        float max = 0;
        foreach (var step in wave.spawnSteps) {
            float d = step.delayAfterLastStep + (step.count * step.interval);
            if (d > max) max = d;
        }
        return max;
    }
}