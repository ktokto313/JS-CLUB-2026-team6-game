using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _Game.Scripts.Core;

[System.Serializable]
public class EnemyTutorialData {
    public string enemyName;     
    public List<Sprite> tutorialImages; 
    [TextArea]
    public List<string> descriptions;  
}

public class SpawnManager : MonoBehaviour {
    public static SpawnManager Instance { get; private set; }
    public static bool IsEndlessMode = false;

    [Header("Tutorial Settings")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Image tutorialUI_Image;
    [SerializeField] private TextMeshProUGUI tutorialUI_Text;
    [SerializeField] private List<EnemyTutorialData> tutorialList;

    [Header("Campaign Settings")]
    [SerializeField] private List<WaveData> waves; 
    [SerializeField] private float timeBetweenWaves = 5f;

    [Header("Endless Settings")]
    [SerializeField] private List<GameObject> enemyPrefabs; 
    [SerializeField] private float endlessSpawnInterval = 2f;

    [Header("Common")]
    [SerializeField] private Transform[] spawnPoints;
    [HideInInspector] public int currentWaveIndex = 1; 

    private HashSet<string> encounteredEnemies = new HashSet<string>();
    private bool isTutorialActive = false;
    private float canCloseTime = 0f;
    private EnemyTutorialData currentActiveData;
    private int currentPageIndex = 0;

    private void Awake() => Instance = this;

    private void Start() => StartCoroutine(MainRoutine());

    private void Update() {
        if (isTutorialActive && Time.unscaledTime > canCloseTime && Input.anyKeyDown) {
            HandleNextStep();
        }
    }

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
        if (prefab == null) return;
        
        CheckNewEnemyTutorial(prefab);

        if (spawnPoints.Length == 0) return;
        int pIndex = isRandom ? Random.Range(0, spawnPoints.Length) : Mathf.Clamp(index, 0, spawnPoints.Length - 1);
        GlobalPoolManager.Instance.Get(prefab, spawnPoints[pIndex].position);
    }

    private void CheckNewEnemyTutorial(GameObject prefab) {
        if (!encounteredEnemies.Contains(prefab.name)) {
            encounteredEnemies.Add(prefab.name);
            EnemyTutorialData data = tutorialList.Find(t => t.enemyName == prefab.name);
            
            if (data != null && tutorialPanel != null) {
                currentActiveData = data;
                currentPageIndex = 0;
                ShowPage(0);
            }
        }
    }

    private void ShowPage(int index) {
        if (currentActiveData.tutorialImages != null && index < currentActiveData.tutorialImages.Count)
            tutorialUI_Image.sprite = currentActiveData.tutorialImages[index];

        if (currentActiveData.descriptions != null && index < currentActiveData.descriptions.Count)
            tutorialUI_Text.text = currentActiveData.descriptions[index];
        
        isTutorialActive = true;
        canCloseTime = Time.unscaledTime + 0.3f; 
        tutorialPanel.SetActive(true);
        Time.timeScale = 0f; 
    }

    private void HandleNextStep() {
        currentPageIndex++;
        if (currentPageIndex < currentActiveData.descriptions.Count || currentPageIndex < currentActiveData.tutorialImages.Count) {
            ShowPage(currentPageIndex);
        } else {
            CloseTutorial();
        }
    }

    public void CloseTutorial() {
        isTutorialActive = false;
        currentActiveData = null;
        tutorialPanel.SetActive(false);
        Time.timeScale = 1f; 
    }

    private float GetWaveDuration(WaveData wave) {
        float max = 0;
        wave.spawnSteps.ForEach(s => max = Mathf.Max(max, s.delayAfterLastStep + (s.count * s.interval)));
        return max;
    }
}