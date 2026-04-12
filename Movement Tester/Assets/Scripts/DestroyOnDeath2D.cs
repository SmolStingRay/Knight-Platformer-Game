using UnityEngine;

[DisallowMultipleComponent]
public class DestroyOnDeath2D : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private float destroyDelay = 0f;

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
        GameObject target = health != null ? health.gameObject : gameObject;
        Destroy(target, Mathf.Max(0f, destroyDelay));
    }
}
