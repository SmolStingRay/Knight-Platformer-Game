using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerSlashVfx2D : MonoBehaviour
{
    [Header("Attachment")]
    [SerializeField] private Transform effectAnchor;
    [SerializeField] private Vector3 localOffset = new(0.1f, 0f, 0f);

    [Header("Look")]
    [SerializeField] private Sprite customSlashSprite;
    [SerializeField] private Color slashColor = new(1f, 1f, 1f, 0.9f);
    [SerializeField] private Vector2 slashScale = new(1.8f, 1.4f);
    [SerializeField] private int sortingOrderOffset = 1;

    [Header("Timing")]
    [SerializeField] private float slashDuration = 0.16f;
    [SerializeField] private float startAngle = -35f;
    [SerializeField] private float endAngle = 28f;
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0.8f, 1f, 1.1f);

    private Coroutine activeRoutine;
    private GameObject slashVisual;
    private SpriteRenderer slashRenderer;
    private Sprite generatedSlashSprite;

    private void Awake()
    {
        EnsureVisual();
    }

    private void OnDisable()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        if (slashVisual != null)
        {
            slashVisual.SetActive(false);
        }
    }

    public void SetAnchor(Transform anchor)
    {
        effectAnchor = anchor;

        if (slashVisual == null)
        {
            return;
        }

        Transform parent = effectAnchor != null ? effectAnchor : transform;
        slashVisual.transform.SetParent(parent, false);
        slashVisual.transform.localPosition = localOffset;
    }

    public void PlaySlash()
    {
        EnsureVisual();

        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        activeRoutine = StartCoroutine(PlaySlashRoutine());
    }

    private void EnsureVisual()
    {
        if (slashVisual == null)
        {
            slashVisual = new GameObject("SlashVfx");
            slashVisual.hideFlags = HideFlags.DontSaveInEditor;
            slashVisual.transform.SetParent(effectAnchor != null ? effectAnchor : transform, false);
            slashRenderer = slashVisual.AddComponent<SpriteRenderer>();
        }

        if (slashRenderer == null)
        {
            slashRenderer = slashVisual.GetComponent<SpriteRenderer>();
        }

        slashRenderer.sprite = customSlashSprite != null ? customSlashSprite : GetOrCreatePlaceholderSprite();

        SpriteRenderer ownerRenderer = GetComponent<SpriteRenderer>();
        if (ownerRenderer != null)
        {
            slashRenderer.sortingLayerID = ownerRenderer.sortingLayerID;
            slashRenderer.sortingOrder = ownerRenderer.sortingOrder + sortingOrderOffset;
        }

        slashVisual.transform.localPosition = localOffset;
        slashVisual.transform.localScale = Vector3.one;
        slashVisual.SetActive(false);
    }

    private IEnumerator PlaySlashRoutine()
    {
        slashVisual.transform.SetParent(effectAnchor != null ? effectAnchor : transform, false);
        slashVisual.transform.localPosition = localOffset;
        slashRenderer.color = new Color(slashColor.r, slashColor.g, slashColor.b, 0f);
        slashVisual.SetActive(true);

        float duration = Mathf.Max(0.01f, slashDuration);
        Vector3 baseScale = new Vector3(slashScale.x, slashScale.y, 1f);

        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Sin(t * Mathf.PI) * Mathf.Clamp01(alphaCurve.Evaluate(t)) * slashColor.a;
            float scaleMultiplier = Mathf.Max(0.01f, scaleCurve.Evaluate(t));

            slashRenderer.color = new Color(slashColor.r, slashColor.g, slashColor.b, alpha);
            slashVisual.transform.localScale = baseScale * scaleMultiplier;
            slashVisual.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(startAngle, endAngle, t));

            yield return null;
        }

        slashRenderer.color = new Color(slashColor.r, slashColor.g, slashColor.b, 0f);
        slashVisual.SetActive(false);
        activeRoutine = null;
    }

    private Sprite GetOrCreatePlaceholderSprite()
    {
        if (generatedSlashSprite != null)
        {
            return generatedSlashSprite;
        }

        const int textureSize = 256;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        texture.name = "GeneratedSlashPlaceholder";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        float outerRadius = 0.96f;
        float innerRadius = 0.56f;
        float startArc = -72f;
        float endArc = 72f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                Vector2 point = new Vector2(
                    ((x + 0.5f) / textureSize) * 2f - 1f,
                    ((y + 0.5f) / textureSize) * 2f - 1f);

                float radius = point.magnitude;
                float angle = Mathf.Atan2(point.y, point.x) * Mathf.Rad2Deg;
                float radialMask = SmoothBand(radius, innerRadius, outerRadius, 0.08f);
                float angularMask = SmoothRange(angle, startArc, endArc, 10f);
                float taper = Mathf.Lerp(0.65f, 1f, Mathf.InverseLerp(startArc, endArc, angle));
                float alpha = radialMask * angularMask * taper;

                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();

        generatedSlashSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.35f, 0.5f),
            128f);

        return generatedSlashSprite;
    }

    private static float SmoothBand(float value, float min, float max, float feather)
    {
        float outer = 1f - Mathf.SmoothStep(max - feather, max, value);
        float inner = Mathf.SmoothStep(min, min + feather, value);
        return Mathf.Clamp01(inner * outer);
    }

    private static float SmoothRange(float value, float min, float max, float feather)
    {
        float left = Mathf.SmoothStep(min - feather, min + feather, value);
        float right = 1f - Mathf.SmoothStep(max - feather, max + feather, value);
        return Mathf.Clamp01(left * right);
    }
}
