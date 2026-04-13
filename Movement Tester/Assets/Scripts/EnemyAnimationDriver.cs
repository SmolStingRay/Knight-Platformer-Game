using UnityEngine;

[DisallowMultipleComponent]
public class EnemyAnimationDriver : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Health health;
    [SerializeField] private float moveThreshold = 0.05f;

    private bool isDead;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (health == null)
            health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        isDead = false;

        if (health != null)
        {
            health.Damaged += HandleDamaged;
            health.Died += HandleDied;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Damaged -= HandleDamaged;
            health.Died -= HandleDied;
        }
    }

    private void Update()
    {
        if (animator == null || rb == null || isDead)
            return;

        float speedX = Mathf.Abs(rb.linearVelocity.x);
        bool isMoving = speedX > moveThreshold;

        animator.SetBool("IsMoving", isMoving);
    }

    public void PlayAttack()
    {
        if (animator == null || isDead)
            return;

        animator.SetTrigger("Attack");
    }

    private void HandleDamaged()
    {
        if (animator == null || isDead)
            return;

        animator.SetTrigger("Hurt");
    }

    private void HandleDied()
    {
        if (animator == null)
            return;

        isDead = true;

        animator.SetBool("IsMoving", false);
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Hurt");
        animator.SetTrigger("Die");
    }
}