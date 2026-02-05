using UnityEngine;

[CreateAssetMenu(fileName = "New Wave", menuName = "Combat/Wave Data")]
public class WaveData : ScriptableObject {
    public GameObject enemyPrefab; // Kéo Prefab con quái vào đây
    public int count;              // Số lượng quái trong đợt này
    public float spawnInterval;    // Thời gian giãn cách giữa mỗi con quái (giây)
}