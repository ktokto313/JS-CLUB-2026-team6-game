using UnityEngine;

public enum WeaponType { Melee, Ranged, Spear }

[CreateAssetMenu(fileName = "New Weapon", menuName = "Combat/Weapon")]
public class Weapon : ScriptableObject
{
    [Header("Thông tin cơ bản")] public string weaponName;
    public Sprite icon; // Hình ảnh hiển thị khi rơi hoặc trong kho đồ
    public WeaponType type;

    //[Header("Chỉ số chiến đấu")]
    public int damage = 10;
    //public float attackSpeed = 1f; // Tốc độ ra đòn
    //public float range = 1.5f;     // Tầm đánh

    //[Header("Cấu hình FlyObject (nếu là vũ khí ném)")]
    //public float flySpeed = 15f;
    //public float rotationSpeed = 1000f;

    //[Header("Prefab hiển thị")]
    public GameObject weaponModelPrefab;
}