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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            OnInputUp?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.S) ||  Input.GetKeyDown(KeyCode.DownArrow))
        {
            OnInputDown?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.A) ||  Input.GetKeyDown(KeyCode.LeftArrow))
        {
            OnInputLeft?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.D)  ||  Input.GetKeyDown(KeyCode.RightArrow))
        {
            OnInputRight?.Invoke();
        }
    }
    
}