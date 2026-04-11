using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private bool resetHealthOnEnable = true;

    [Header("Protection")]
    [SerializeField] private bool invulnerableOnStart = false;
    [SerializeField] private float damageProtectionDuration = 1f;

    public event Action<int, int> HealthChanged;
    public event Action Damaged;
    public event Action Died;

    public int BaseMaxHealth => maxHealth;
    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }
    public bool IsInvulnerable => invulnerableUntil > Time.time;

    private float invulnerableUntil;

    private void Awake()
    {
        CurrentHealth = Mathf.Clamp(maxHealth, 0, maxHealth);
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

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead || IsInvulnerable)
        {
            return;
        }

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
        {
            return;
        }

        int previousHealth = CurrentHealth;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);

        if (CurrentHealth != previousHealth)
        {
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }
    }

    public void RestoreFullHealth()
    {
        IsDead = false;
        CurrentHealth = maxHealth;
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void SetMaxHealth(int value, bool restoreFullHealth)
    {
        maxHealth = Mathf.Max(1, value);

        if (restoreFullHealth)
        {
            RestoreFullHealth();
            return;
        }

        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);
        IsDead = CurrentHealth == 0;
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void SetCurrentHealth(int value)
    {
        CurrentHealth = Mathf.Clamp(value, 0, maxHealth);
        IsDead = CurrentHealth == 0;
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void ProtectForSeconds(float seconds)
    {
        if (seconds <= 0f)
        {
            return;
        }

        invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + seconds);
    }
}
