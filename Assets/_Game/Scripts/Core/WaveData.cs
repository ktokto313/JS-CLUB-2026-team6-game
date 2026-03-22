using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnStep {
    [Header("--- Enemy Config ---")]
    public string label;
    public GameObject enemyPrefab; 
    
    [Header("--- Timing ---")]
    public float delayAfterLastStep = 0f; 
    public int count = 1;
    public float interval = 0.5f; // Khoảng cách giữa các con trong CÙNG 1 đợt

    [Header("--- Position ---")]
    public bool useRandomPoint = true;
    public int specificPointIndex = 0; // Nếu tắt random, nó sẽ ra ở Spawn Point số này
}

[CreateAssetMenu(fileName = "New Wave", menuName = "Combat/Wave Data")]
public class WaveData : ScriptableObject {
    public string waveName;
    public List<EnemySpawnStep> spawnSteps; 
}