using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private TopDownMovement topDownMovement;
    private PlatformerMovement2D platformerMovement;
    private Vector3 initialScale;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        topDownMovement = GetComponent<TopDownMovement>();
        platformerMovement = GetComponent<PlatformerMovement2D>();
        initialScale = transform.localScale;
    }

    void Update()
    {
        if (topDownMovement != null && topDownMovement.enabled)
        {
            float speed = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y).magnitude;
            anim.SetFloat("Speed", speed);
            anim.SetBool("IsGrounded", true);

            if (Mathf.Abs(rb.linearVelocity.x) > 0.01f)
            {
                transform.localScale = new Vector3(
                    Mathf.Sign(rb.linearVelocity.x) * Mathf.Abs(initialScale.x),
                    initialScale.y,
                    initialScale.z
                );
            }
        }
        else if (platformerMovement != null && platformerMovement.enabled)
        {
            float speed = Mathf.Abs(rb.linearVelocity.x);
            anim.SetFloat("Speed", speed);
            anim.SetBool("IsGrounded", platformerMovement.IsGrounded);
        }
        else
        {
            anim.SetFloat("Speed", 0f);
            anim.SetBool("IsGrounded", true);
        }
    }

    public void PlayAttack()
    {
        anim.SetTrigger("Attack");
    }

    public void PlayHurt()
    {
        anim.SetTrigger("Hurt");
    }

    public void PlayDie()
    {
        anim.SetTrigger("Die");
    }
}