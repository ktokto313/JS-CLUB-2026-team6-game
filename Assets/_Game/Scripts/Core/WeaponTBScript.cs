using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WeaponSkin {
    public string skinName;
    public Sprite visualSprite; 
    public GameObject projectilePrefab;
}

public enum WeaponType { Melee, Ranged, Spear }

[CreateAssetMenu(fileName = "New Weapon", menuName = "Combat/Weapon")]
public class WeaponTBScript : ScriptableObject
{
    [Header("Combat Stats")]
    public WeaponType type;
    public int damage = 10;
    public float attackRange = 3f;
    public float attackSpeed = 1.0f;

    [Header("Projectile Settings (If Ranged)")]
    public float flySpeed = 15f;
    public float lifeTime = 5f; 
    
    [Header("Weapon Skins List")]
    public List<WeaponSkin> weaponSkins; 

    [Header("Current Active Skin (Internal)")]
    [HideInInspector] public Sprite currentVisual;
    [HideInInspector] public GameObject currentPrefab;

}