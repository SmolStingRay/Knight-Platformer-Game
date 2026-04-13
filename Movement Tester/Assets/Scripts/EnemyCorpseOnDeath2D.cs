using UnityEngine;

[DisallowMultipleComponent]
public class EnemyCorpseOnDeath2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer[] liveRenderers;
    [SerializeField] private SpriteRenderer corpseSpriteRenderer;

    [Header("Disable On Death")]
    [SerializeField] private Behaviour[] behavioursToDisable;
    [SerializeField] private Collider2D[] collidersToDisable;

    [Header("Corpse Look")]
    [SerializeField] private bool useDedicatedCorpseSprite = false;
    [SerializeField] private Sprite corpseSprite;
    [SerializeField] private Vector2 corpseSpriteLocalOffset = Vector2.zero;
    [SerializeField] private float corpseSpriteRotation = 0f;
    [SerializeField] private float fallenRotation = -90f;
    [SerializeField] private int sortingOrderOffset = -10;
    [SerializeField] private Color corpseTint = new(0.55f, 0.55f, 0.55f, 1f);
    [SerializeField] private bool freezeBodyOnDeath = true;
    [SerializeField] private bool alignCorpseToGround = true;

    private Color[] originalColors;
    private int[] originalSortingOrders;
    private Color corpseOriginalColor;
    private int corpseOriginalSortingOrder;

    private void Awake()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
        }

        if (health == null)
        {
            health = GetComponentInParent<Health>();
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (rb == null)
        {
            rb = GetComponentInParent<Rigidbody2D>();
        }

        if (liveRenderers == null || liveRenderers.Length == 0)
        {
            liveRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        if (collidersToDisable == null || collidersToDisable.Length == 0)
        {
            collidersToDisable = GetComponentsInChildren<Collider2D>(true);
        }

        if (behavioursToDisable == null || behavioursToDisable.Length == 0)
        {
            behavioursToDisable = GetComponents<Behaviour>();
        }

        CacheRendererState();
        SetupCorpseSpriteRenderer();
    }

    private void OnEnable()
    {
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

    private void HandleDied()
    {
        float groundedY = GetLowestBoundsY();
        DisableBehaviours();
        DisableColliders();
        FreezeRigidBody();
        ApplyCorpseLook(groundedY);

        if (alignCorpseToGround)
        {
            AlignCorpseToGround(groundedY);
        }
    }

    private void DisableBehaviours()
    {
        foreach (Behaviour behaviour in behavioursToDisable)
        {
            if (behaviour == null || behaviour == this)
            {
                continue;
            }

            behaviour.enabled = false;
        }
    }

    private void DisableColliders()
    {
        foreach (Collider2D targetCollider in collidersToDisable)
        {
            if (targetCollider != null)
            {
                targetCollider.enabled = false;
            }
        }
    }

    private void FreezeRigidBody()
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (freezeBodyOnDeath)
        {
            rb.simulated = false;
        }
    }

    private void ApplyCorpseLook(float groundedY)
    {
        if (useDedicatedCorpseSprite && corpseSpriteRenderer != null)
        {
            HideLiveRenderers();
            ShowDedicatedCorpseSprite(groundedY);
            return;
        }

        Vector3 euler = transform.eulerAngles;
        euler.z = fallenRotation;
        transform.eulerAngles = euler;

        for (int i = 0; i < liveRenderers.Length; i++)
        {
            SpriteRenderer renderer = liveRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            renderer.sortingOrder = originalSortingOrders[i] + sortingOrderOffset;
            renderer.color = MultiplyColor(originalColors[i], corpseTint);
        }
    }

    private void AlignCorpseToGround(float groundedY)
    {
        if (useDedicatedCorpseSprite && corpseSpriteRenderer != null)
        {
            Bounds dedicatedCorpseBounds = corpseSpriteRenderer.bounds;
            if (dedicatedCorpseBounds.size.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            float corpseOffsetY = groundedY - dedicatedCorpseBounds.min.y;
            corpseSpriteRenderer.transform.position += new Vector3(0f, corpseOffsetY, 0f);
            return;
        }

        Bounds corpseBounds = GetCombinedRendererBounds();
        if (corpseBounds.size.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        float offsetY = groundedY - corpseBounds.min.y;
        transform.position += new Vector3(0f, offsetY, 0f);
    }

    private void CacheRendererState()
    {
        originalColors = new Color[liveRenderers.Length];
        originalSortingOrders = new int[liveRenderers.Length];

        for (int i = 0; i < liveRenderers.Length; i++)
        {
            if (liveRenderers[i] == null)
            {
                continue;
            }

            originalColors[i] = liveRenderers[i].color;
            originalSortingOrders[i] = liveRenderers[i].sortingOrder;
        }

        if (corpseSpriteRenderer != null)
        {
            corpseOriginalColor = corpseSpriteRenderer.color;
            corpseOriginalSortingOrder = corpseSpriteRenderer.sortingOrder;
        }
    }

    private static Color MultiplyColor(Color a, Color b)
    {
        return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
    }

    private float GetLowestBoundsY()
    {
        bool hasBounds = false;
        float lowestY = float.PositiveInfinity;

        foreach (Collider2D targetCollider in collidersToDisable)
        {
            if (targetCollider == null)
            {
                continue;
            }

            lowestY = Mathf.Min(lowestY, targetCollider.bounds.min.y);
            hasBounds = true;
        }

        if (!hasBounds)
        {
            Bounds rendererBounds = GetCombinedRendererBounds();
            return rendererBounds.size.sqrMagnitude > Mathf.Epsilon ? rendererBounds.min.y : transform.position.y;
        }

        return lowestY;
    }

    private Bounds GetCombinedRendererBounds()
    {
        bool hasBounds = false;
        Bounds combinedBounds = default;

        foreach (SpriteRenderer renderer in liveRenderers)
        {
            if (renderer == null || !renderer.enabled)
            {
                continue;
            }

            if (!hasBounds)
            {
                combinedBounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(renderer.bounds);
            }
        }

        return combinedBounds;
    }

    private void SetupCorpseSpriteRenderer()
    {
        if (corpseSpriteRenderer == null)
        {
            return;
        }

        corpseSpriteRenderer.enabled = false;
    }

    private void HideLiveRenderers()
    {
        foreach (SpriteRenderer renderer in liveRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
    }

    private void ShowDedicatedCorpseSprite(float groundedY)
    {
        corpseSpriteRenderer.enabled = true;
        corpseSpriteRenderer.sprite = corpseSprite != null ? corpseSprite : corpseSpriteRenderer.sprite;
        corpseSpriteRenderer.color = MultiplyColor(corpseOriginalColor == default ? Color.white : corpseOriginalColor, corpseTint);
        corpseSpriteRenderer.sortingOrder = corpseOriginalSortingOrder + sortingOrderOffset;
        corpseSpriteRenderer.transform.localPosition = new Vector3(corpseSpriteLocalOffset.x, corpseSpriteLocalOffset.y, corpseSpriteRenderer.transform.localPosition.z);
        corpseSpriteRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, corpseSpriteRotation);
    }
}
