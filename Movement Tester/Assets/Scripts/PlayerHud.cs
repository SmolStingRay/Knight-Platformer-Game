using TMPro;
using UnityEngine;

public class PlayerHud : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private PlayerResources playerResources;

    [Header("Text")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text potionText;
    [SerializeField] private TMP_Text goldText;

    [Header("Formatting")]
    [SerializeField] private string numberFormat = "D3";

    private void Awake()
    {
        if (playerResources == null)
        {
            playerResources = FindFirstObjectByType<PlayerResources>();
        }

        if (health == null)
        {
            if (playerResources != null)
            {
                health = playerResources.GetComponent<Health>();

                if (health == null)
                {
                    health = playerResources.GetComponentInParent<Health>();
                }
            }

            if (health == null)
            {
                health = FindFirstObjectByType<Health>();
            }
        }
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.HealthChanged += HandleHealthChanged;
        }

        if (playerResources != null)
        {
            playerResources.GoldChanged += HandleGoldChanged;
            playerResources.PotionCountChanged += HandlePotionCountChanged;
        }

        RefreshAll();
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.HealthChanged -= HandleHealthChanged;
        }

        if (playerResources != null)
        {
            playerResources.GoldChanged -= HandleGoldChanged;
            playerResources.PotionCountChanged -= HandlePotionCountChanged;
        }
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        if (healthText == null)
        {
            return;
        }

        healthText.text = FormatNumber(currentHealth) + "/" + FormatNumber(maxHealth);
    }

    private void HandleGoldChanged(int gold)
    {
        if (goldText == null)
        {
            return;
        }

        goldText.text = FormatNumber(gold);
    }

    private void HandlePotionCountChanged(int potionCount)
    {
        if (potionText == null)
        {
            return;
        }

        potionText.text = FormatNumber(potionCount);
    }

    public void RefreshAll()
    {
        if (health != null)
        {
            HandleHealthChanged(health.CurrentHealth, health.MaxHealth);
        }

        if (playerResources != null)
        {
            HandleGoldChanged(playerResources.Gold);
            HandlePotionCountChanged(playerResources.PotionCount);
        }
    }

    private string FormatNumber(int value)
    {
        return Mathf.Max(0, value).ToString(numberFormat);
    }
}
