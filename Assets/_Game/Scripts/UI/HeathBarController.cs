using System.Collections;
using System.Collections.Generic;
using _Game.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;

public class HeathBarController : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    
    // Start is called before the first frame update
    void Start()
    {
        EventManager.current.onPlayerHealthUpdateAction += SetHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMaxHeath(int health)
    {
        healthSlider.maxValue  = health;
        healthSlider.value = health;
    }

    public void SetHealth(int health)
    {
        if (health > healthSlider.maxValue) healthSlider.maxValue = health;
        healthSlider.value = health;
        
    }
}
