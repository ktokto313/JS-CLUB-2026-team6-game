using System;
using System.Collections;
using System.Collections.Generic;
using _Game.Prefabs.Characters.Script;
using UnityEngine;

public class PlayerBodyCollider : MonoBehaviour
{
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private float duckHeightRadio;
    
    // Original Size
    private Vector2 OriginSize;
    private Vector2 OriginOffset;

    // Ducking Size
    private Vector2 DuckingSize;
    private Vector2 DuckingOffset;
    
    
    
    private bool isDucking;

    private void Awake()
    {
        OriginSize = boxCollider.size;
        OriginOffset = boxCollider.offset;

        CalculateStat();

    }

    private void CalculateStat()
    {   
        float newHeight = OriginSize.y * duckHeightRadio;
        
        DuckingSize = new Vector2(OriginSize.x, newHeight);
        
        float diff = OriginSize.y - DuckingSize.y;
        
        DuckingOffset = new Vector2(OriginOffset.x,OriginOffset.y - diff/2);
        
    }

    private void Update()
    {
        if (PlayerController.Instance != null)
        {
            if (PlayerController.Instance.state == PlayerState.DUCKING)
            {
                if (!isDucking)
                {
                    SetCollider();
                    isDucking = true;
                }
            }
            else
            {
                if (isDucking)
                {
                    ResetCollider();
                    isDucking = false;
                }
            }
        }
    }
    
    private void ResetCollider()
    {
        boxCollider.size = OriginSize;
        boxCollider.offset = OriginOffset;
        Debug.Log("Original size: " + boxCollider.size);
    }

    private void SetCollider()
    {
        boxCollider.size = DuckingSize;
        boxCollider.offset = DuckingOffset;
        Debug.Log("Ducking size: " + boxCollider.size);
    }
}