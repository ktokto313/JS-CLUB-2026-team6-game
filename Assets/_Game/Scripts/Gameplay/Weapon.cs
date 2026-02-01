using UnityEngine;

namespace _Game.Scripts.Gameplay
{
    [CreateAssetMenu(fileName = "FILENAME", menuName = "MENUNAME", order = 0)]
    public class Weapon : ScriptableObject
    {
        [SerializeField] private int damage = 0;
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            Entity entityHit = (other.gameObject.GetComponent<Entity>());
            entityHit?.LowerHealth(damage);
        }
    }
}