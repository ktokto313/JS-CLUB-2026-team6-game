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
    public Action OnInputJump;
    public Action OnInputDuck;
    public Action<Facing> OnInputAttack;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            OnInputJump?.Invoke();
        }
        else if (Input.GetKey(KeyCode.S))
        {
            OnInputDuck?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            OnInputAttack?.Invoke(Facing.LEFT);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            OnInputAttack?.Invoke(Facing.RIGHT);
        }
    }
}