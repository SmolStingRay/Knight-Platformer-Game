using UnityEngine;

[DisallowMultipleComponent]
public class EnemyGoldDrop : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private GameObject goldPickupPrefab;
    [SerializeField] private int minGoldDrop = 1;
    [SerializeField] private int maxGoldDrop = 3;
    [SerializeField] private Vector2 spawnSpread = new(0.5f, 0.2f);

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
        if (goldPickupPrefab == null)
        {
            return;
        }

        int minDrop = Mathf.Max(0, minGoldDrop);
        int maxDrop = Mathf.Max(minDrop, maxGoldDrop);
        int dropCount = Random.Range(minDrop, maxDrop + 1);
        for (int i = 0; i < dropCount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-spawnSpread.x, spawnSpread.x),
                Random.Range(-spawnSpread.y, spawnSpread.y),
                0f);

            Instantiate(goldPickupPrefab, transform.position + offset, Quaternion.identity);
        }
    }
}
