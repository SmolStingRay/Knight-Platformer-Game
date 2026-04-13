using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerResources))]
public class PotionUser : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode usePotionKey = KeyCode.R;

    [Header("Potion")]
    [SerializeField] private int healAmount = 2;
    [SerializeField] private bool blockUseAtFullHealth = true;

    private Health health;
    private PlayerResources resources;

    private void Awake()
    {
        health = GetComponent<Health>();
        resources = GetComponent<PlayerResources>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(usePotionKey))
        {
            TryUsePotion();
        }
    }

    public bool TryUsePotion()
    {
        if (health == null || resources == null || health.IsDead)
        {
            return false;
        }

        if (blockUseAtFullHealth && health.CurrentHealth >= health.MaxHealth)
        {
            return false;
        }

        if (!resources.ConsumePotion())
        {
            return false;
        }

        health.Heal(healAmount);
        return true;
    }
}
