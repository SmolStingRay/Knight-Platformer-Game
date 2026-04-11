using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Base Health")]
    [SerializeField] private int baseMaxHealth = 10;

    [Header("Runtime Bonus")]
    [SerializeField] private int bonusMaxHealth = 0;

    [Header("Health State")]
    [SerializeField] private bool resetHealthOnEnable = true;

    [Header("Protection")]
    [SerializeField] private bool invulnerableOnStart = false;
    [SerializeField] private float damageProtectionDuration = 1f;

    public event Action<int, int> HealthChanged;
    public event Action Damaged;
    public event Action Died;

    public int BaseMaxHealth => baseMaxHealth;
    public int BonusMaxHealth => bonusMaxHealth;
    public int MaxHealth => baseMaxHealth + bonusMaxHealth;

    public int CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }
    public bool IsInvulnerable => invulnerableUntil > Time.time;

    private float invulnerableUntil;

    private void Awake()
    {
        CurrentHealth = MaxHealth;
    }

    private void OnEnable()
    {
        if (resetHealthOnEnable)
        {
            RestoreFullHealth();
        }

        if (invulnerableOnStart)
        {
            ProtectForSeconds(damageProtectionDuration);
        }
    }

    // =========================
    // DAMAGE SYSTEM
    // =========================

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead || IsInvulnerable)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

        Damaged?.Invoke();
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (CurrentHealth == 0)
        {
            IsDead = true;
            Died?.Invoke();
            return;
        }

        ProtectForSeconds(damageProtectionDuration);
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead)
            return;

        int previous = CurrentHealth;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);

        if (CurrentHealth != previous)
        {
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }
    }

    // =========================
    // HEALTH CONTROL
    // =========================

    public void RestoreFullHealth()
    {
        IsDead = false;
        CurrentHealth = MaxHealth;
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    // =========================
    // ARMOUR SYSTEM (IMPORTANT)
    // =========================

    public void AddMaxHealthBonus(int amount, bool healToFull = true)
    {
        bonusMaxHealth += Mathf.Max(0, amount);

        if (healToFull)
        {
            CurrentHealth = MaxHealth;
        }
        else
        {
            CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
        }

        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void SetMaxHealthBonus(int value, bool healToFull = true)
    {
        bonusMaxHealth = Mathf.Max(0, value);

        if (healToFull)
        {
            RestoreFullHealth();
        }
        else
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        }

        IsDead = CurrentHealth == 0;
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    // =========================
    // OPTIONAL DIRECT CONTROL
    // =========================

    public void SetCurrentHealth(int value)
    {
        CurrentHealth = Mathf.Clamp(value, 0, MaxHealth);
        IsDead = CurrentHealth == 0;
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    // =========================
    // INVULNERABILITY
    // =========================

    public void ProtectForSeconds(float seconds)
    {
        if (seconds <= 0f) return;

        invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + seconds);
    }
}