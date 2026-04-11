using UnityEngine;

public class player_dmg : MonoBehaviour
{
    [SerializeField] private KeyCode increaseKey = KeyCode.Y;
    [SerializeField] private int damageIncrease = 2;
    [SerializeField] private bool canOnlyUseOnce = true;

    private bool hasBeenUsed;

    private const string DMG_KEY = "Player_Damage";
    private const string DMG_VALUE_KEY = "Player_Damage_Value";

    public int CurrentDamageBonus { get; private set; }

    void Start()
    {
        // Load saved data
        hasBeenUsed = PlayerPrefs.GetInt(DMG_KEY, 0) == 1;
        CurrentDamageBonus = PlayerPrefs.GetInt(DMG_VALUE_KEY, 0);
    }

    void Update()
    {
        if (Input.GetKeyDown(increaseKey))
        {
            IncreaseDamage();
        }
    }

    void IncreaseDamage()
    {
        if (canOnlyUseOnce && hasBeenUsed)
            return;

        CurrentDamageBonus += damageIncrease;

        hasBeenUsed = true;

        // Save
        PlayerPrefs.SetInt(DMG_KEY, 1);
        PlayerPrefs.SetInt(DMG_VALUE_KEY, CurrentDamageBonus);
        PlayerPrefs.Save();

        Debug.Log("Damage increased! Bonus: +" + CurrentDamageBonus);
    }
}