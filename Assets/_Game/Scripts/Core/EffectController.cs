using System.Collections;
using System.Collections.Generic;
using _Game.Scripts.Core;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    public static EffectController current { get; private set; }
    [SerializeField] private GameObject effect;
    
    void Start()
    {
        current = this;
        EventManager.current.onHitAction += SpawnParticle;
        EventManager.current.onPlayerHitAction += SpawnParticle;
    }

    private void SpawnParticle(Vector3 position)
    {
        GlobalPoolManager.Instance.Get(effect, position);
    }
}
