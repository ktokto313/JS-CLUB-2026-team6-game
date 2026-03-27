using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    // SingleTon
    public static GameInput Instance { get; private set; }
    
    // Action Subject
    public event Action OnInputUp;
    public event Action OnInputDown;
    public event Action OnInputLeft;
    public event Action OnInputRight;
    
    private void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(this.gameObject); 
            return;
        }

        Instance = this;
    }

    [SerializeField] private float inputBufferTime = 0.2f;
    private float lastUpTime = -100f;
    private float lastDownTime = -100f;
    private float lastLeftTime = -100f;
    private float lastRightTime = -100f;

    private void Update()
    {   
        if (Time.timeScale == 0f) return;
        // 1. Record input presses
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            lastUpTime = Time.time;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            lastDownTime = Time.time;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            lastLeftTime = Time.time;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            lastRightTime = Time.time;

        // 2. Consume input if within buffer time
        if (Time.time - lastUpTime <= inputBufferTime)
        {
            OnInputUp?.Invoke();
            lastUpTime = -100f; // Consume
        }
        if (Time.time - lastDownTime <= inputBufferTime)
        {
            OnInputDown?.Invoke();
            lastDownTime = -100f;
        }
        if (Time.time - lastLeftTime <= inputBufferTime)
        {
            OnInputLeft?.Invoke();
            lastLeftTime = -100f;
        }
        if (Time.time - lastRightTime <= inputBufferTime)
        {
            OnInputRight?.Invoke();
            lastRightTime = -100f;
        }
    }
    
}