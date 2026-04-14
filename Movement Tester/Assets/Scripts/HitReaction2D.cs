using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class HitReaction2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer[] renderers;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 2.2f;
    [SerializeField] private float upwardForce = 0.4f;
    [SerializeField] private bool clearHorizontalVelocityOnHit = true;

    [SerializeField] private Enemy enemy;

    [Header("Flash")]
    [SerializeField] private Color flashColor = new(1f, 1f, 1f, 0.35f);
    [SerializeField] private int flashCount = 2;
    [SerializeField] private float flashInterval = 0.06f;

    private Coroutine flashRoutine;
    private Color[] originalColors;

    private void Awake()
    {
        if (enemy == null)
        {
            enemy = GetComponent<Enemy>();
        }

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

        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        CacheOriginalColors();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.Damaged += HandleDamaged;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Damaged -= HandleDamaged;
        }

        RestoreColors();
    }

    private void HandleDamaged()
    {
        enemy?.ApplyHitStun();
        ApplyKnockback();
        PlayFlash();
    }

    private void ApplyKnockback()
    {
        if (rb == null)
        {
            return;
        }

        Vector2 velocity = rb.linearVelocity;
        if (clearHorizontalVelocityOnHit)
        {
            velocity.x = 0f;
        }

        rb.linearVelocity = velocity;

        float facing = transform.localScale.x >= 0f ? 1f : -1f;
        Vector2 impulse = new Vector2(-facing * knockbackForce, upwardForce);
        rb.AddForce(impulse, ForceMode2D.Impulse);
    }

    private void PlayFlash()
    {
        if (renderers == null || renderers.Length == 0)
        {
            return;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            RestoreColors();
        }

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        int cycles = Mathf.Max(1, flashCount);
        float interval = Mathf.Max(0.01f, flashInterval);

        for (int i = 0; i < cycles; i++)
        {
            SetColors(flashColor);
            yield return new WaitForSeconds(interval);
            RestoreColors();
            yield return new WaitForSeconds(interval);
        }

        flashRoutine = null;
    }

    private void CacheOriginalColors()
    {
        if (renderers == null)
        {
            originalColors = System.Array.Empty<Color>();
            return;
        }

        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i] != null ? renderers[i].color : Color.white;
        }
    }

    private void SetColors(Color color)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].color = color;
            }
        }
    }

    private void RestoreColors()
    {
        if (renderers == null || originalColors == null)
        {
            return;
        }

        for (int i = 0; i < renderers.Length && i < originalColors.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].color = originalColors[i];
            }
        }
    }
}
