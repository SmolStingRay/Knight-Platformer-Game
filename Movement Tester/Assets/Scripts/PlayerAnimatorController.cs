using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    private TopDownMovement topDownMovement;
    private PlatformerMovement2D platformerMovement;
    private Health health;

    private Vector3 initialScale;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        topDownMovement = GetComponent<TopDownMovement>();
        platformerMovement = GetComponent<PlatformerMovement2D>();
        health = GetComponent<Health>();

        initialScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.Damaged += PlayHurt;
            health.Died += PlayDie;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Damaged -= PlayHurt;
            health.Died -= PlayDie;
        }
    }

    private void Update()
    {
        if (topDownMovement != null && topDownMovement.enabled)
        {
            animator.SetFloat("Speed", rb.linearVelocity.magnitude);
            animator.SetBool("IsGrounded", true);

            if (rb.linearVelocity.x > 0.01f)
            {
                transform.localScale = new Vector3(Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
            }
            else if (rb.linearVelocity.x < -0.01f)
            {
                transform.localScale = new Vector3(-Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
            }
        }
        else if (platformerMovement != null && platformerMovement.enabled)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            animator.SetBool("IsGrounded", platformerMovement.IsGrounded);
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