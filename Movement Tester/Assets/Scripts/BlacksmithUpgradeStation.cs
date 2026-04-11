using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BlacksmithUpgradeStation : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private float interactionRange = 1.25f;
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TMP_Text promptLabel;
    [SerializeField] private string promptMessage = "Press E to use blacksmith";

    [Header("Weapon Upgrade")]
    [SerializeField] private int weaponBaseCost = 10;
    [SerializeField] private int weaponCostStep = 5;

    [Header("Armor Upgrade")]
    [SerializeField] private int armorBaseCost = 10;
    [SerializeField] private int armorCostStep = 5;

    private PlayerResources playerResources;
    private PlayerCombatStats playerCombatStats;
    private SimpleRuntimeMenu runtimeMenu;
    private bool isMenuOpen;

    private void Awake()
    {
        runtimeMenu = GetComponent<SimpleRuntimeMenu>();
        if (runtimeMenu == null)
        {
            runtimeMenu = gameObject.AddComponent<SimpleRuntimeMenu>();
        }

        if (promptRoot == null)
        {
            Transform promptTransform = transform.Find("InteractionPromptAnchor");
            promptRoot = promptTransform != null ? promptTransform.gameObject : null;
        }

        if (promptLabel == null && promptRoot != null)
        {
            promptLabel = promptRoot.GetComponentInChildren<TMP_Text>(true);
        }

        HidePrompt();
    }

    private void Start()
    {
        playerResources = FindFirstObjectByType<PlayerResources>();
        playerCombatStats = FindFirstObjectByType<PlayerCombatStats>();
    }

    private void Update()
    {
        if (playerResources == null || playerCombatStats == null)
        {
            return;
        }

        bool isPlayerInRange = Vector2.Distance(transform.position, playerResources.transform.position) <= interactionRange;
        if (isPlayerInRange)
        {
            ShowPrompt();

            if (Input.GetKeyDown(interactionKey))
            {
                ToggleMenu();
            }
        }
        else
        {
            HidePrompt();
            CloseMenu();
        }
    }

    private void ToggleMenu()
    {
        if (isMenuOpen)
        {
            CloseMenu();
            return;
        }

        OpenMenu();
    }

    private void OpenMenu()
    {
        if (playerResources == null || playerCombatStats == null)
        {
            return;
        }

        isMenuOpen = true;
        runtimeMenu.Show(
            "Blacksmith",
            "Choose an upgrade.",
            new SimpleRuntimeMenu.MenuOption(GetWeaponOptionLabel(), TryUpgradeWeapon),
            new SimpleRuntimeMenu.MenuOption(GetArmorOptionLabel(), TryUpgradeArmor),
            new SimpleRuntimeMenu.MenuOption("Close", CloseMenu));
    }

    private void CloseMenu()
    {
        isMenuOpen = false;
        if (runtimeMenu != null)
        {
            runtimeMenu.Hide();
        }
    }

    private void TryUpgradeWeapon()
    {
        int cost = GetWeaponUpgradeCost();
        if (!playerResources.SpendGold(cost))
        {
            OpenMenu();
            return;
        }

        playerCombatStats.UpgradeWeapon();
        PlayerRuntimeState.Instance.CaptureCurrentState(playerResources.gameObject);
        OpenMenu();
    }

    private void TryUpgradeArmor()
    {
        int cost = GetArmorUpgradeCost();
        if (!playerResources.SpendGold(cost))
        {
            OpenMenu();
            return;
        }

        playerCombatStats.UpgradeArmor();
        PlayerRuntimeState.Instance.CaptureCurrentState(playerResources.gameObject);
        OpenMenu();
    }

    private string GetWeaponOptionLabel()
    {
        return "Upgrade Weapon Lv." + playerCombatStats.WeaponLevel + " -> Lv." + (playerCombatStats.WeaponLevel + 1) +
               " (" + GetWeaponUpgradeCost() + " gold)";
    }

    private string GetArmorOptionLabel()
    {
        return "Upgrade Armor Lv." + playerCombatStats.ArmorLevel + " -> Lv." + (playerCombatStats.ArmorLevel + 1) +
               " (" + GetArmorUpgradeCost() + " gold)";
    }

    private int GetWeaponUpgradeCost()
    {
        return weaponBaseCost + (playerCombatStats.WeaponLevel - 1) * weaponCostStep;
    }

    private int GetArmorUpgradeCost()
    {
        return armorBaseCost + (playerCombatStats.ArmorLevel - 1) * armorCostStep;
    }

    private void ShowPrompt()
    {
        if (promptLabel != null)
        {
            promptLabel.text = promptMessage;
        }

        if (promptRoot != null)
        {
            promptRoot.SetActive(true);
        }
    }

    private void HidePrompt()
    {
        if (promptRoot != null)
        {
            promptRoot.SetActive(false);
        }
    }
}
