using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class PlayerMovement : MonoBehaviour
    {
        
        private Facing facing = Facing.RIGHT;

        private void Start()
        {
            
        }


        private void SetFacing(Facing newFacing)
        {
            if (facing != newFacing)
            {
                facing = newFacing;
            
                Vector3 scale = transform.localScale;
            
                float size = Mathf.Abs(scale.x);

                scale.x = (facing == Facing.RIGHT) ? size : -size;
                transform.localScale = scale;
            }
        }
    }
