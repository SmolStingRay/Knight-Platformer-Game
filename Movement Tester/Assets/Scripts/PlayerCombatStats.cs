using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
public class PlayerCombatStats : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private int startingWeaponLevel = 1;
    [SerializeField] private int baseAttackDamage = 1;
    [SerializeField] private int attackDamagePerWeaponLevel = 1;

    [Header("Armor")]
    [SerializeField] private int startingArmorLevel = 1;
    [SerializeField] private int armorHealthBonusPerLevel = 2;

    public event Action StatsChanged;

    public int WeaponLevel { get; private set; }
    public int ArmorLevel { get; private set; }
    public int AttackDamage => Mathf.Max(1, baseAttackDamage + (WeaponLevel - 1) * attackDamagePerWeaponLevel);
    public int BonusMaxHealth => Mathf.Max(0, (ArmorLevel - 1) * armorHealthBonusPerLevel);

    private Health health;
    private int baseMaxHealth;

    private void Awake()
    {
        health = GetComponent<Health>();
        baseMaxHealth = health != null ? health.BaseMaxHealth : 1;
        WeaponLevel = Mathf.Max(1, startingWeaponLevel);
        ArmorLevel = Mathf.Max(1, startingArmorLevel);
        ApplyDerivedStats(true);
    }

    public void UpgradeWeapon(int amount = 1)
    {
        if (amount <= 0)
        {
            return;
        }

        WeaponLevel += amount;
        ApplyDerivedStats(false);
    }

    public void UpgradeArmor(int amount = 1)
    {
        if (amount <= 0)
        {
            return;
        }

        ArmorLevel += amount;
        ApplyDerivedStats(false);
    }

    public void ApplyState(int weaponLevel, int armorLevel)
    {
        WeaponLevel = Mathf.Max(1, weaponLevel);
        ArmorLevel = Mathf.Max(1, armorLevel);
        ApplyDerivedStats(false);
    }

    private void ApplyDerivedStats(bool restoreFullHealth)
    {
        if (health != null)
        {
            health.SetMaxHealth(baseMaxHealth + BonusMaxHealth, restoreFullHealth);
        }

        StatsChanged?.Invoke();
    }
}
