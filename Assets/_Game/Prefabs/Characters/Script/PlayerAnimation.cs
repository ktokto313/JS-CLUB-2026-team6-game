using Unity.VisualScripting;
using UnityEngine;


public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private PlayerController player;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        if (player != null)
        {
            player.OnAttackAction += PlayAttack;
            player.OnJumpAction += PlayJump;
        }
    }

    private void Update()
    {
        if (player)
        {
            anim.SetBool("IsDucking", player.IsDucking);
            anim.SetBool("IsOnAir",  player.IsOnAir);
        }
    }
    
    
    // PLAY METHOD

    private void PlayAttack()
    {
        anim.SetTrigger("Attack");
    }

    private void PlayJump()
    {
        anim.SetTrigger("Jump");
    }
    
}