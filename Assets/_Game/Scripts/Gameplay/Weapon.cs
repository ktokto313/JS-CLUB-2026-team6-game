using System;
using UnityEngine;

namespace _Game.Scripts.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class Weapon : MonoBehaviour
    {
        private WeaponType _weaponType;
        private WeaponData _weaponData;

        private void OnCollisionEnter2D(Collision2D other)
        {
            Entity entityHit = (Entity)(other.gameObject);
            entityHit.LowerHealth(_weaponData.getDamageOnWolf());
        }
    }

    public enum WeaponType
    {
        Ranged,
        Melee
    }
}