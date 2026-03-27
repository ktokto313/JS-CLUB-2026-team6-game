using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core;

public class SpawnManager : MonoBehaviour {
    public static SpawnManager Instance { get; private set; }
    public static bool IsEndlessMode = false;

    [Header("Campaign Settings")]
    [SerializeField] private List<WaveData> waves; 
    [SerializeField] private float timeBetweenWaves = 5f;

    [Header("Endless Settings")]
    [SerializeField] private List<GameObject> enemyPrefabs; 
    [SerializeField] private float endlessSpawnInterval = 2f;

    [Header("Common")]
    [SerializeField] private Transform[] spawnPoints;
    [HideInInspector] public int currentWaveIndex = 1; 

    private void Awake() => Instance = this;

    private void Start() => StartCoroutine(MainRoutine());

    private IEnumerator MainRoutine() {
        if (IsEndlessMode) {
            yield return EndlessRoutine();
        } else {
            yield return CampaignRoutine();
        }
    }

    private IEnumerator CampaignRoutine() {
        foreach (WaveData wave in waves) {
            BackgroundController.Instance.OnNextWave();
            Debug.Log($"<color=cyan><b>START WAVE {currentWaveIndex}</b></color>");
            foreach (var step in wave.spawnSteps) {
                if (step.delayAfterLastStep > 0) yield return new WaitForSeconds(step.delayAfterLastStep);
                StartCoroutine(ExecuteStep(step));
            }
            yield return new WaitForSeconds(GetWaveDuration(wave) + timeBetweenWaves);
            currentWaveIndex++;
        }
        
        yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Enemy").Length == 0);
        EventManager.current.onWin();
    }

    private IEnumerator EndlessRoutine() {
        while (true) {
            int spawnCount = 5 + (currentWaveIndex * 3);
            for (int i = 0; i < spawnCount; i++) {
                var randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
                Spawn(randomPrefab, true);
                yield return new WaitForSeconds(Mathf.Max(0.5f, endlessSpawnInterval - (currentWaveIndex * 0.1f)));
            }
            yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Enemy").Length <= 2);
            yield return new WaitForSeconds(timeBetweenWaves);
            currentWaveIndex++;
        }
    }

    private IEnumerator ExecuteStep(EnemySpawnStep step) {
        for (int i = 0; i < step.count; i++) {
            Spawn(step.enemyPrefab, step.useRandomPoint, step.specificPointIndex);
            if (step.interval > 0) yield return new WaitForSeconds(step.interval);
        }
    }

    private void Spawn(GameObject prefab, bool isRandom, int index = 0) {
        if (spawnPoints.Length == 0 || prefab == null) return;
        int pIndex = isRandom ? Random.Range(0, spawnPoints.Length) : Mathf.Clamp(index, 0, spawnPoints.Length - 1);
        GlobalPoolManager.Instance.Get(prefab, spawnPoints[pIndex].position);
    }

    private float GetWaveDuration(WaveData wave) {
        float max = 0;
        wave.spawnSteps.ForEach(s => max = Mathf.Max(max, s.delayAfterLastStep + (s.count * s.interval)));
        return max;
    }
}