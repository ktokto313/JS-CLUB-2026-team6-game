using UnityEngine;

public enum WeaponType { Melee, Ranged, Spear }

[CreateAssetMenu(fileName = "New Weapon", menuName = "Combat/Weapon")]
public class WeaponTBScript : ScriptableObject
{
    [Header("UI & Identity")]
    public string weaponName;
    public Sprite icon; // Dùng cho UI kho đồ
    public Sprite worldSprite; // Dùng cho DroppedWeapon hiện trên mặt đất

    [Header("Combat Stats")]
    public WeaponType type;
    public int damage = 10;
    public float attackRange = 3f;
    public float attackSpeed = 1.0f;

    [Header("Projectile Settings (If Ranged)")]
    public float flySpeed = 15f;
    public float lifeTime = 5f; // Thời gian tự hủy nếu không trúng mục tiêu
    public GameObject projectilePrefab; // Prefab dùng cho FlyObject

    [Header("Visuals")]
    public GameObject weaponModelPrefab; // Prefab dùng khi Player cầm trên tay
}