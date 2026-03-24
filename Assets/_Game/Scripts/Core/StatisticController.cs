using Unity.VisualScripting;
using UnityEngine;

namespace _Game.Scripts.Core
{
    public class StatisticController : MonoBehaviour
    {
        public static StatisticController current { get; private set; }
        
        public Time time { get; private set; }
        public int score { get; private set; } = 0;
        public int hit { get; private set; } = 0;
        public int attack { get; private set; } = 0;
        public int bestCombo { get; private set; } = 0;
        public int currentCombo { get; private set; } = 0;

        private void Start()
        {
            current = this;
            EventManager.current.onDeadAction += onScoreIncrease;
            EventManager.current.onPlayerAttackAction += onPlayerAttack;
            EventManager.current.onHitAction += onHit;
            EventManager.current.onPlayerHitAction += resetCombo;
            EventManager.current.onPlayerDeadAction += Reset;
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