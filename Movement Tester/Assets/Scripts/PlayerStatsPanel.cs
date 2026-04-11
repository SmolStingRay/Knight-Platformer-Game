using TMPro;
using UnityEngine;

public class PlayerStatsPanel : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    [Header("References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private Health health;
    [SerializeField] private PlayerCombatStats combatStats;

    private bool isVisible;

    private void Awake()
    {
        if (health == null)
        {
            health = FindFirstObjectByType<Health>();
        }

        if (combatStats == null)
        {
            combatStats = FindFirstObjectByType<PlayerCombatStats>();
        }

        SetPanelVisible(false);
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.HealthChanged += HandleStatsChanged;
        }

        if (combatStats != null)
        {
            combatStats.StatsChanged += HandleCombatStatsChanged;
        }

        RefreshText();
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.HealthChanged -= HandleStatsChanged;
        }

        if (combatStats != null)
        {
            combatStats.StatsChanged -= HandleCombatStatsChanged;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            SetPanelVisible(!isVisible);
        }
    }

    private void HandleStatsChanged(int currentHealth, int maxHealth)
    {
        RefreshText();
    }

    private void HandleCombatStatsChanged()
    {
        RefreshText();
    }

    private void SetPanelVisible(bool visible)
    {
        isVisible = visible;

        if (panelRoot != null)
        {
            panelRoot.SetActive(visible);
        }

        if (visible)
        {
            RefreshText();
        }
    }

    private void RefreshText()
    {
        if (statsText == null)
        {
            return;
        }

        int currentHealth = health != null ? health.CurrentHealth : 0;
        int maxHealth = health != null ? health.MaxHealth : 0;
        int weaponLevel = combatStats != null ? combatStats.WeaponLevel : 1;
        int armorLevel = combatStats != null ? combatStats.ArmorLevel : 1;
        int attack = combatStats != null ? combatStats.AttackDamage : 0;
        int bonusHealth = combatStats != null ? combatStats.BonusMaxHealth : 0;

        statsText.text =
            "Weapon Lv. " + weaponLevel + "\n" +
            "Armor Lv. " + armorLevel + "\n" +
            "Attack " + attack + "\n" +
            "Bonus HP " + bonusHealth + "\n" +
            "HP " + currentHealth + "/" + maxHealth;
    }
}
