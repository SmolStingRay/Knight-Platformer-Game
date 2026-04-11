using UnityEngine;

public class ResourcePickup2D : MonoBehaviour
{
    private enum ResourceType
    {
        Gold,
        Potion
    }

    [Header("Pickup")]
    [SerializeField] private ResourceType resourceType = ResourceType.Gold;
    [SerializeField] private int amount = 1;
    [SerializeField] private bool destroyOnPickup = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerResources resources = FindResources(other);
        if (resources == null || amount <= 0)
        {
            return;
        }

        switch (resourceType)
        {
            case ResourceType.Gold:
                resources.AddGold(amount);
                break;
            case ResourceType.Potion:
                resources.AddPotions(amount);
                break;
        }

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
    }

    private static PlayerResources FindResources(Collider2D other)
    {
        if (other == null)
        {
            return null;
        }

        PlayerResources resources = other.GetComponent<PlayerResources>();
        if (resources != null)
        {
            return resources;
        }

        resources = other.GetComponentInParent<PlayerResources>();
        if (resources != null)
        {
            return resources;
        }

        return other.attachedRigidbody != null ? other.attachedRigidbody.GetComponent<PlayerResources>() : null;
    }
}
