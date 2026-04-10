using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformerMovement2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float deceleration = 16f;
    [SerializeField] private bool faceMoveDirection = true;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;
    [SerializeField] private float fallGravityMultiplier = 2.2f;
    [SerializeField] private float lowJumpGravityMultiplier = 2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new(0.6f, 0.15f);
    [SerializeField] private LayerMask groundLayer = ~0;

    private Rigidbody2D rb;
    private float horizontalInput;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool jumpReleasedEarly;
    private bool isGrounded;
    private Vector3 initialScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialScale = transform.localScale;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        UpdateGroundedState();
        UpdateJumpTimers();

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }

        if (Input.GetButtonUp("Jump"))
        {
            jumpReleasedEarly = true;
        }

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            Jump();
        }

        if (faceMoveDirection && Mathf.Abs(horizontalInput) > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Sign(horizontalInput) * Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
        }
    }

    private void FixedUpdate()
    {
        float targetSpeed = horizontalInput * moveSpeed;
        float speedDifference = targetSpeed - rb.linearVelocity.x;
        float rate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float movement = speedDifference * rate;

        rb.AddForce(Vector2.right * movement, ForceMode2D.Force);
        ApplyBetterJumpGravity();
    }

    private void UpdateGroundedState()
    {
        Vector2 checkPosition = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position + Vector2.down;
        isGrounded = Physics2D.OverlapBox(checkPosition, groundCheckSize, 0f, groundLayer);

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            jumpReleasedEarly = false;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void UpdateJumpTimers()
    {
        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
        jumpReleasedEarly = false;
    }

    private void ApplyBetterJumpGravity()
    {
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && jumpReleasedEarly)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 checkPosition = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position + Vector2.down;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(checkPosition, groundCheckSize);
    }
}
