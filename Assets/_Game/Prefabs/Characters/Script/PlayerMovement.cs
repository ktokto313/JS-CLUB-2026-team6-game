using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class PlayerMovement : MonoBehaviour
    {
        
        [SerializeField] private float jumpAttackForce = 16f; // Lực nhảy khi tấn công (thường mạnh hơn nhảy thường)

        [SerializeField] private Rigidbody2D rb;
        
        private Facing facing = Facing.RIGHT;
    
        
        private void Start()
        {
            if (PlayerController.Instance != null)
            {
                // 1. NHÓM DUCK (Phím S)
                PlayerController.Instance.OnPerformLowAttack += PlayLowAttack;
                PlayerController.Instance.OnPerformSmash += PlaySmash;

                // 2. NHÓM JUMP (Phím W)
                PlayerController.Instance.OnPerformJumpAttack += PlayJumpAttack;
                PlayerController.Instance.OnPerformRisingAttack += PlayRisingAttack;
                PlayerController.Instance.OnPerformAirSpin += PlayAirSpin;

                // 3. NHÓM ATTACK (Phím A/D)
                PlayerController.Instance.OnPerformAttack += PlayAttack;
                PlayerController.Instance.OnPerformUppercut += PlayAttack;
                PlayerController.Instance.OnPerformAirAttack += PlayAttack; 
            }
        }

        private void PlayLowAttack()
        {
        }

        private void PlaySmash()
        {
        }

        private void PlayJumpAttack()
        {
            // 1. Reset vận tốc Y về 0 để cú nhảy luôn có lực nhất quán 
            // (Dù đang rơi hay đang bay thì bấm là nảy lên ngay)
            rb.velocity = new Vector2(rb.velocity.x, 0);

            // 2. Thêm lực hướng lên trên (Impulse = Lực tức thời)
            rb.AddForce(Vector2.up * jumpAttackForce, ForceMode2D.Impulse);
        }

        private void PlayRisingAttack()
        {
        }

        private void PlayAirSpin()
        {
        }

        private void PlayAttack(Facing face)
        {
            SetFacing(face);
        }

        private void Update()
        {
            
        }

        private void OnDestroy()
        {
            if (PlayerController.Instance != null)
            {
                // 1. NHÓM DUCK
                PlayerController.Instance.OnPerformLowAttack -= PlayLowAttack;
                PlayerController.Instance.OnPerformSmash -= PlaySmash;

                // 2. NHÓM JUMP
                PlayerController.Instance.OnPerformJumpAttack -= PlayJumpAttack;
                PlayerController.Instance.OnPerformRisingAttack -= PlayRisingAttack;
                PlayerController.Instance.OnPerformAirSpin -= PlayAirSpin;

                // 3. NHÓM ATTACK
                PlayerController.Instance.OnPerformAttack -= PlayAttack;
                PlayerController.Instance.OnPerformUppercut -= PlayAttack;
                PlayerController.Instance.OnPerformAirAttack -= PlayAttack; 
            }
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
