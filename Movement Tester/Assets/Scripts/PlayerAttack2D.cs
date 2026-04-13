using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerAttack2D : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode attackKey = KeyCode.J;

    [Header("Attack Shape")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector2 attackSize = new(1.2f, 0.8f);
    [SerializeField] private Vector2 attackOffset = new(0.9f, 0f);
    [SerializeField] private LayerMask hitLayers = ~0;

    [Header("Timing")]
    [SerializeField] private float attackCooldown = 0.3f;

    [Header("Feedback")]
    [SerializeField] private PlayerSlashVfx2D slashVfx;
    [SerializeField] private PlayerAnimationController animationController;

    [Header("Debug")]
    [SerializeField] private bool drawAttackGizmo = true;

    private readonly Collider2D[] hitBuffer = new Collider2D[12];

    private PlayerCombatStats combatStats;
    private Health selfHealth;
    private Rigidbody2D selfRigidbody;
    private float nextAttackTime;
    private bool editorPreviewActive;
    private double editorPreviewUntil;

    public int CurrentAttackDamage => combatStats != null ? combatStats.AttackDamage : 1;

    private void Awake()
    {
        combatStats = GetComponent<PlayerCombatStats>();
        selfHealth = GetComponent<Health>();
        selfRigidbody = GetComponent<Rigidbody2D>();
        slashVfx = slashVfx != null ? slashVfx : GetComponent<PlayerSlashVfx2D>();
        animationController = animationController != null ? animationController : GetComponent<PlayerAnimationController>();

        if (slashVfx != null)
        {
            slashVfx.SetAnchor(attackPoint);
        }
    }
    private void OnEnable()
    {
        nextAttackTime = 0f;
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
        {
            return;
        }

        if (Input.GetKeyDown(attackKey))
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;
        PerformAttack();
    }

    private void PerformAttack()
    {
        slashVfx?.PlaySlash();
        animationController?.PlayAttack();

        Vector2 center = GetAttackCenter();
        ContactFilter2D contactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = hitLayers,
            useTriggers = true
        };

        int hitCount = Physics2D.OverlapBox(center, attackSize, 0f, contactFilter, hitBuffer);
        if (hitCount <= 0)
        {
            return;
        }

        HashSet<Health> damagedTargets = new HashSet<Health>();
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hitCollider = hitBuffer[i];
            hitBuffer[i] = null;

            if (hitCollider == null)
            {
                continue;
            }

            if (BelongsToSelf(hitCollider))
            {
                continue;
            }

            Health targetHealth = FindHealth(hitCollider);
            if (targetHealth == null || targetHealth == selfHealth || damagedTargets.Contains(targetHealth))
            {
                continue;
            }

            damagedTargets.Add(targetHealth);
            targetHealth.TakeDamage(CurrentAttackDamage);
        }
    }

    public void PreviewAttackInEditor()
    {
#if UNITY_EDITOR
        slashVfx?.PreviewSlashInEditor();
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

        Health health = hitCollider.GetComponent<Health>();
        if (health != null)
        {
            return health;
        }

        health = hitCollider.GetComponentInParent<Health>();
        if (health != null)
        {
            return health;
        }

        return hitCollider.attachedRigidbody != null ? hitCollider.attachedRigidbody.GetComponent<Health>() : null;
    }

    private bool BelongsToSelf(Collider2D hitCollider)
    {
        if (hitCollider.transform == transform || hitCollider.transform.IsChildOf(transform))
        {
            return true;
        }

        if (selfRigidbody != null && hitCollider.attachedRigidbody == selfRigidbody)
        {
            return true;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!drawAttackGizmo || !editorPreviewActive)
        {
            return;
        }

        if (UnityEditor.EditorApplication.timeSinceStartup >= editorPreviewUntil)
        {
            editorPreviewActive = false;
            return;
        }

        DrawAttackGizmo(new Color(1f, 0.2f, 0.2f, 0.2f), Color.red);
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawAttackGizmo)
        {
            return;
        }

        DrawAttackGizmo(new Color(1f, 0.2f, 0.2f, 0.12f), Color.red);
    }

    private void DrawAttackGizmo(Color fillColor, Color wireColor)
    {
        Gizmos.color = fillColor;
        Gizmos.DrawCube(GetAttackCenter(), attackSize);
        Gizmos.color = wireColor;
        Gizmos.DrawWireCube(GetAttackCenter(), attackSize);
    }
}
