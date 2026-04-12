using UnityEngine;

public class DamageSource2D : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private bool destroyAfterHit;
    [SerializeField] private bool useCombatStatsDamage = true;
    [SerializeField] private PlayerCombatStats combatStatsSource;

    private void Awake()
    {
        if (combatStatsSource == null)
        {
            combatStatsSource = GetComponent<PlayerCombatStats>();
        }

        if (combatStatsSource == null)
        {
            combatStatsSource = GetComponentInParent<PlayerCombatStats>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDealDamage(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDealDamage(collision.collider);
    }

    private void TryDealDamage(Collider2D other)
    {
        Health health = FindHealth(other);
        if (health == null)
        {
            return;
        }

        health.TakeDamage(GetDamageAmount());

        if (destroyAfterHit)
        {
            Destroy(gameObject);
        }
    }

    private int GetDamageAmount()
    {
        if (useCombatStatsDamage && combatStatsSource != null)
        {
            return combatStatsSource.AttackDamage;
        }

        return damage;
    }

    private static Health FindHealth(Collider2D other)
    {
        if (other == null)
        {
            return null;
        }

        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            return health;
        }

        health = other.GetComponentInParent<Health>();
        if (health != null)
        {
            return health;
        }

        return other.attachedRigidbody != null ? other.attachedRigidbody.GetComponent<Health>() : null;
    }
}
