using System;
using UnityEngine;

namespace _Game.Scripts.Core
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager current { get; private set; }
        
        public event Action onHitAction;
        public event Action onPlayerHitAction;
        public event Action onDeadAction;
        public event Action onPlayerDeadAction;
        public event Action onPlayerAttackAction;
        public event Action<int> onPlayerHealthUpdateAction;
        public event Action<int> onPointUpdateAction;

        public void Awake()
        {
            current = this;
        }

        public void onHit()
        {
            onHitAction?.Invoke();
        }

        public void onPlayerHit()
        {
            onPlayerHitAction?.Invoke();
        }

        public void onDead()
        {
            onDeadAction?.Invoke();
        }

        public void onPlayerDead()
        {
            onPlayerDeadAction?.Invoke();
        }

        public void onPlayerAttack()
        {
            onPlayerAttackAction?.Invoke();
        }

        public void onPlayerHealthUpdate(int newHealth)
        {
            onPlayerHealthUpdateAction?.Invoke(newHealth);
        }

        public void onPointUpdate(int newPoint)
        {
            onPointUpdateAction?.Invoke(newPoint);
        }
    }
}