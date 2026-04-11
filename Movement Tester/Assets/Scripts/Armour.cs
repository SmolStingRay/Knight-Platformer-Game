using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerArmour : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode equipKey = KeyCode.T;

    [Header("Armour Settings")]
    [SerializeField] private int armourBonus = 10;
    [SerializeField] private bool canOnlyUseOnce = true;
    [SerializeField] private bool healToFullOnEquip = true;

    private Health health;
    private bool hasBeenUsed;

    private const string ARMOUR_SAVE_KEY = "PLAYER_ARMOUR_USED";

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void Start()
    {
        // Load saved armour state
        hasBeenUsed = PlayerPrefs.GetInt(ARMOUR_SAVE_KEY, 0) == 1;

        // Re-apply armour on scene load (important for persistence)
        if (hasBeenUsed)
        {
            ApplyArmour();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(equipKey))
        {
            TryEquipArmour();
        }
    }

    private void TryEquipArmour()
    {
        if (health == null || health.IsDead)
            return;

        if (canOnlyUseOnce && hasBeenUsed)
            return;

        hasBeenUsed = true;

        PlayerPrefs.SetInt(ARMOUR_SAVE_KEY, 1);
        PlayerPrefs.Save();

        ApplyArmour();

        Debug.Log("Armour equipped! +Max HP applied.");
    }

    private void ApplyArmour()
    {
        health.AddMaxHealthBonus(armourBonus, healToFullOnEquip);
    }
}