using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(HitReaction2D))]
public class Enemy : MonoBehaviour
{
    private enum EnemyState
    {
        Patrolling,
        Chasing,
        Dead
    }

    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Health health;
    [SerializeField] private PlayerSlashVfx2D attackVfx;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private float patrolPauseDuration = 0.5f;
    [SerializeField] private bool startPatrolToRight = true;
    [SerializeField] private bool faceMoveDirection = true;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float loseTargetRange = 8f;
    [SerializeField] private float fieldOfView = 100f;
    [SerializeField] private LayerMask sightBlockerLayers;
    [SerializeField] private string playerTag = "Player";

    [Header("Attack")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackHeightTolerance = 1.25f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private Vector2 attackSize = new(1.2f, 0.8f);
    [SerializeField] private Vector2 attackOffset = new(0.9f, 0f);
    [SerializeField] private LayerMask attackTargetLayers = ~0;

    [Header("Debug")]
    [SerializeField] private bool drawDebugGizmos = true;

    private readonly Collider2D[] hitBuffer = new Collider2D[12];

    private EnemyState currentState;
    private Vector2 spawnPosition;
    private int patrolDirection = 1;
    private float patrolPauseUntil;
    private float nextAttackTime;
    private Vector3 initialScale;
    private bool editorPreviewActive;
    private double editorPreviewUntil;

    private void Awake()
    {
        rb = rb != null ? rb : GetComponent<Rigidbody2D>();
        health = health != null ? health : GetComponent<Health>();
        attackVfx = attackVfx != null ? attackVfx : GetComponent<PlayerSlashVfx2D>();
        spawnPosition = transform.position;
        initialScale = transform.localScale;

        if (rb != null)
        {
            rb.freezeRotation = true;
        }

        if (attackVfx != null)
        {
            attackVfx.SetAnchor(attackPoint);
        }
    }

    private void OnValidate()
    {
        rb = rb != null ? rb : GetComponent<Rigidbody2D>();
        health = health != null ? health : GetComponent<Health>();

#if UNITY_EDITOR
        if (!Application.isPlaying && GetComponent<HitReaction2D>() == null)
        {
            UnityEditor.Undo.AddComponent<HitReaction2D>(gameObject);
        }
#endif
    }

    private void OnEnable()
    {
        currentState = EnemyState.Patrolling;
        nextAttackTime = 0f;
        patrolPauseUntil = 0f;
        patrolDirection = startPatrolToRight ? 1 : -1;

        if (faceMoveDirection)
        {
            FaceDirection(patrolDirection);
        }

        if (health != null)
        {
            health.Died += HandleDied;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Died -= HandleDied;
        }
    }

    private void Update()
    {
        if (currentState == EnemyState.Dead)
        {
            return;
        }

        EnsureTarget();

        if (CanSeeTarget())
        {
            currentState = EnemyState.Chasing;
        }
        else if (ShouldStopChasing())
        {
            currentState = EnemyState.Patrolling;
        }

        if (currentState == EnemyState.Chasing)
        {
            TryAttack();
        }
    }

    private void FixedUpdate()
    {
        if (currentState == EnemyState.Dead || rb == null)
        {
            return;
        }

        switch (currentState)
        {
            case EnemyState.Patrolling:
                Patrol();
                break;
            case EnemyState.Chasing:
                Chase();
                break;
        }
    }

    private void Patrol()
    {
        float leftEdge = spawnPosition.x - patrolDistance;
        float rightEdge = spawnPosition.x + patrolDistance;
        float currentX = transform.position.x;

        if (currentX <= leftEdge)
        {
            patrolDirection = 1;
            patrolPauseUntil = Time.time + patrolPauseDuration;
            FaceDirection(patrolDirection);
        }
        else if (currentX >= rightEdge)
        {
            patrolDirection = -1;
            patrolPauseUntil = Time.time + patrolPauseDuration;
            FaceDirection(patrolDirection);
        }

        if (Time.time < patrolPauseUntil)
        {
            MoveHorizontally(0f);
            return;
        }

        FaceDirection(patrolDirection);
        MoveHorizontally(patrolDirection * patrolSpeed);
    }

    private void Chase()
    {
        if (target == null)
        {
            currentState = EnemyState.Patrolling;
            MoveHorizontally(0f);
            return;
        }

        float horizontalDelta = target.position.x - transform.position.x;
        float verticalDelta = Mathf.Abs(target.position.y - transform.position.y);

        if (Mathf.Abs(horizontalDelta) <= attackRange && verticalDelta <= attackHeightTolerance)
        {
            MoveHorizontally(0f);
            FaceDirection(Mathf.Sign(horizontalDelta));
            return;
        }

        MoveHorizontally(Mathf.Sign(horizontalDelta) * chaseSpeed);
    }

    private void MoveHorizontally(float speed)
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.x = speed;
        rb.linearVelocity = velocity;

        if (faceMoveDirection && Mathf.Abs(speed) > 0.01f)
        {
            FaceDirection(Mathf.Sign(speed));
        }
    }

    private void FaceDirection(float direction)
    {
        if (!faceMoveDirection || Mathf.Abs(direction) < 0.01f)
        {
            return;
        }

        transform.localScale = new Vector3(
            Mathf.Sign(direction) * Mathf.Abs(initialScale.x),
            initialScale.y,
            initialScale.z);
    }

    private void EnsureTarget()
    {
        if (target != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            target = player.transform;
            return;
        }

        PlayerMovementModeSwitcher modeSwitcher = FindFirstObjectByType<PlayerMovementModeSwitcher>();
        if (modeSwitcher != null)
        {
            target = modeSwitcher.transform;
            return;
        }

        PlatformerMovement2D platformerMovement = FindFirstObjectByType<PlatformerMovement2D>();
        if (platformerMovement != null)
        {
            target = platformerMovement.transform;
            return;
        }

        TopDownMovement topDownMovement = FindFirstObjectByType<TopDownMovement>();
        if (topDownMovement != null)
        {
            target = topDownMovement.transform;
        }
    }

    private bool CanSeeTarget()
    {
        if (target == null)
        {
            return false;
        }

        Vector2 origin = transform.position;
        Vector2 toTarget = (Vector2)target.position - origin;
        float distance = toTarget.magnitude;

        if (distance > detectionRange || distance <= Mathf.Epsilon)
        {
            return false;
        }

        Vector2 facing = patrolDirection >= 0 ? Vector2.right : Vector2.left;
        if (faceMoveDirection)
        {
            facing = transform.localScale.x >= 0f ? Vector2.right : Vector2.left;
        }

        float angle = Vector2.Angle(facing, toTarget.normalized);
        if (angle > fieldOfView * 0.5f)
        {
            return false;
        }

        if (sightBlockerLayers.value != 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, toTarget.normalized, distance, sightBlockerLayers);
            if (hit.collider != null && !hit.transform.IsChildOf(target))
            {
                return false;
            }
        }

        return true;
    }

    private bool ShouldStopChasing()
    {
        if (currentState != EnemyState.Chasing || target == null)
        {
            return false;
        }

        float horizontalDelta = Mathf.Abs(target.position.x - transform.position.x);
        float verticalDelta = Mathf.Abs(target.position.y - transform.position.y);

        return horizontalDelta > loseTargetRange || verticalDelta > detectionRange;
    }

    private void TryAttack()
    {
        if (target == null || Time.time < nextAttackTime)
        {
            return;
        }

        float horizontalDelta = Mathf.Abs(target.position.x - transform.position.x);
        float verticalDelta = Mathf.Abs(target.position.y - transform.position.y);
        if (horizontalDelta > attackRange || verticalDelta > attackHeightTolerance)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;
        attackVfx?.PlaySlash();

        ContactFilter2D contactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = attackTargetLayers,
            useTriggers = true
        };

        int hitCount = Physics2D.OverlapBox(GetAttackCenter(), attackSize, 0f, contactFilter, hitBuffer);
        if (hitCount <= 0)
        {
            return;
        }

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hitBuffer[i];
            hitBuffer[i] = null;

            Health targetHealth = FindHealth(hit);
            if (targetHealth == null || targetHealth == health || targetHealth.IsDead)
            {
                continue;
            }

            targetHealth.TakeDamage(attackDamage);
            return;
        }
    }

    public void PreviewAttackInEditor()
    {
#if UNITY_EDITOR
        attackVfx?.PreviewSlashInEditor();
        editorPreviewActive = true;
        editorPreviewUntil = UnityEditor.EditorApplication.timeSinceStartup + Mathf.Max(0.2f, attackCooldown);
        UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    private Vector2 GetAttackCenter()
    {
        float facingSign = transform.localScale.x >= 0f ? 1f : -1f;
        Vector2 worldOffset = new Vector2(attackOffset.x * facingSign, attackOffset.y);

        if (attackPoint != null)
        {
            return (Vector2)attackPoint.position + worldOffset;
        }

        return (Vector2)transform.position + worldOffset;
    }

    private static Health FindHealth(Collider2D hitCollider)
    {
        if (hitCollider == null)
        {
            return null;
        }

        Health hitHealth = hitCollider.GetComponent<Health>();
        if (hitHealth != null)
        {
            return hitHealth;
        }

        hitHealth = hitCollider.GetComponentInParent<Health>();
        if (hitHealth != null)
        {
            return hitHealth;
        }

        return hitCollider.attachedRigidbody != null ? hitCollider.attachedRigidbody.GetComponent<Health>() : null;
    }

    private void HandleDied()
    {
        currentState = EnemyState.Dead;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!drawDebugGizmos || !editorPreviewActive)
        {
            return;
        }

        if (UnityEditor.EditorApplication.timeSinceStartup >= editorPreviewUntil)
        {
            editorPreviewActive = false;
            return;
        }

        DrawDebugGizmos(new Color(1f, 0.2f, 0.2f, 0.2f), Color.red);
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawDebugGizmos)
        {
            return;
        }

        DrawDebugGizmos(new Color(1f, 0.2f, 0.2f, 0.12f), Color.red);
    }

    private void DrawDebugGizmos(Color attackFillColor, Color attackWireColor)
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            new Vector3(transform.position.x - patrolDistance, transform.position.y, transform.position.z),
            new Vector3(transform.position.x + patrolDistance, transform.position.y, transform.position.z));

        Gizmos.color = attackFillColor;
        Gizmos.DrawCube(GetAttackCenter(), attackSize);

        Gizmos.color = attackWireColor;
        Gizmos.DrawWireCube(GetAttackCenter(), attackSize);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
