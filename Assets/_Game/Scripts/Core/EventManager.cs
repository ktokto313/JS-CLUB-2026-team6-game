using System;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace _Game.Scripts.Core
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager current { get; private set; }
        
        public event Action<Vector3> onHitAction;
        public event Action<Vector3> onPlayerHitAction;
        public event Action onDeadAction;
        public event Action onPlayerDeadAction;
        public event Action onPlayerAttackAction;
        public event Action<int> onPlayerHealthUpdateAction;
        public event Action<int> onPointUpdateAction;

        public event Action<float> onVolumeSliderUpdateAction;

        public void Awake()
        {
            current = this;
        }

        public void onHit(Vector3 hitPosition)
        {
            onHitAction?.Invoke(hitPosition);
        }

        public void onPlayerHit(Vector3 hitPosition)
        {
            onPlayerHitAction?.Invoke(hitPosition);
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

        public void onVolumeSliderUpdate(float volume)
        {
            onVolumeSliderUpdateAction?.Invoke(volume);
        }
    }
}