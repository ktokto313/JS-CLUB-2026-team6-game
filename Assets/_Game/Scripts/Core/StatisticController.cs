using Unity.VisualScripting;
using UnityEngine;

namespace _Game.Scripts.Core
{
    public class StatisticController : MonoBehaviour
    {
        public static StatisticController current { get; private set; }
        
        private Time time;
        private int score = 0;
        private int hit = 0;
        private int attack = 0;
        private int bestCombo = 0;
        private int currentCombo = 0;

        private void Awake()
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