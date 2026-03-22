using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Combat/Enemy Data")]
public class EnemyData : ScriptableObject {
    public string enemyName;
    public int baseHealth = 10;
    public float baseMoveSpeed = 3f;
    public float baseDamage = 1f;
    
    [Header("Scaling - Tỉ lệ tăng tiến")]
    public float healthMultiplierPerWave = 0.2f;
    public float speedMultiplierPerWave = 0.05f;
}