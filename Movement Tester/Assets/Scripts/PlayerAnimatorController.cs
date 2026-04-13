using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private TopDownMovement topDownMovement;
    private PlatformerMovement2D platformerMovement;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        topDownMovement = GetComponent<TopDownMovement>();
        platformerMovement = GetComponent<PlatformerMovement2D>();
    }

    private void Update()
    {
        if (topDownMovement != null && topDownMovement.enabled)
        {
            animator.SetFloat("Speed", rb.linearVelocity.magnitude);
            animator.SetBool("IsGrounded", true);
        }
        else if (platformerMovement != null && platformerMovement.enabled)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            animator.SetBool("IsGrounded", platformerMovement.IsGrounded);
        }

        if (rb.linearVelocity.x > 0.01f)
        {
            spriteRenderer.flipX = false;
        }
        else if (rb.linearVelocity.x < -0.01f)
        {
            spriteRenderer.flipX = true;
        }
    }

    public void PlayAttack()
    {
        animator.SetTrigger("Attack");
    }

    public void PlayHurt()
    {
        animator.SetTrigger("Hurt");
    }

    public void PlayDie()
    {
        animator.SetTrigger("Die");
    }
}