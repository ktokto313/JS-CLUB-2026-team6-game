using UnityEngine;

namespace _Game.Scripts.Gameplay
{
    [CreateAssetMenu(fileName = "weaponExample", menuName = "_Game/Scripts/Gameplay/weaponExample")]
    public class WeaponData : ScriptableObject
    {
        [SerializeField]
        private int damageOnWolf = 1;

        public int getDamageOnWolf()
        {
            return damageOnWolf;
        }
    }
}