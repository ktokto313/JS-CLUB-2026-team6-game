using Unity.VisualScripting;
using UnityEngine;

namespace _Game.Scripts.Core
{
    public class StatisticController : MonoBehaviour
    {
        public static StatisticController current { get; private set; }
        
        public int score { get; private set; }
        public int hit { get; private set; }
        public int attack { get; private set; }
        public int bestCombo { get; private set; }
        public int currentCombo { get; private set; }

        private void Awake()
        {
            current = this;
            EventManager.current.onDeadAction += onScoreIncrease;
            EventManager.current.onPlayerAttackAction += onPlayerAttack;
            EventManager.current.onHitAction += onHit;
            EventManager.current.onPlayerHitAction += resetCombo;
        }

        private void Reset() 
        {
            score = 0;
            hit = 0;
            attack = 0;
            bestCombo = 0;
            currentCombo = 0;
        }
        
        private void onScoreIncrease()
        {
            score++;
            EventManager.current.onPointUpdate(score);
        }

        private void onPlayerAttack()
        {
            attack++;
        }

        private void onHit(Vector3 position)
        {
            hit++;
            currentCombo++;
        }

        private void resetCombo(Vector3 position)
        {
            if (currentCombo > bestCombo)
            {
                bestCombo = currentCombo;
            }
            currentCombo = 0;
        }
    }
}