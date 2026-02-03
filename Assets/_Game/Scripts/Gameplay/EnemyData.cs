using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Combat/Enemy Data")]
public class EnemyData : ScriptableObject {
    public string enemyName;
    public int maxHealth;
    public float moveSpeed;
    public float stopDistance;
    public GameObject prefab; 
}