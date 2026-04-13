using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelPortal : MonoBehaviour
{
    [System.Serializable]
    private class LevelOption
    {
        public string displayName = "New Level";
        public string sceneName = "SampleScene";
    }

    [SerializeField] private string menuTitle = "Choose Level";
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private float interactionRange = 1.25f;
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TMP_Text promptLabel;
    [SerializeField] private string promptMessage = "Press E to use portal";
    [SerializeField] private LevelOption[] levelOptions;

    private PlayerResources playerResources;
    private bool isMenuOpen;
    private SimpleRuntimeMenu runtimeMenu;

    private void Awake()
    {
        runtimeMenu = GetComponent<SimpleRuntimeMenu>();
        if (runtimeMenu == null)
        {
            runtimeMenu = gameObject.AddComponent<SimpleRuntimeMenu>();
        }
    }

    private void Start()
    {
        playerResources = FindFirstObjectByType<PlayerResources>();
        SetPromptVisible(false);
    }

    private void Update()
    {
        if (playerResources == null)
        {
            playerResources = FindFirstObjectByType<PlayerResources>();
        }

        bool inRange = IsPlayerInRange();
        if (!inRange)
        {
            SetPromptVisible(false);
            if (isMenuOpen)
            {
                CloseMenu();
            }
            return;
        }

        if (!isMenuOpen)
        {
            UpdatePrompt();
            SetPromptVisible(true);
        }
        else
        {
            SetPromptVisible(false);
        }

        if (Input.GetKeyDown(interactionKey))
        {
            if (isMenuOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }
    }

    private void LoadLevel(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName) || playerResources == null)
        {
            return;
        }

        CloseMenu();
        PlayerRuntimeState.Instance.SaveLevelEntrySnapshot(playerResources.gameObject);
        SceneManager.LoadScene(sceneName);
    }

    private void OpenMenu()
    {
        if (!IsPlayerInRange())
        {
            return;
        }

        isMenuOpen = true;
        SetPromptVisible(false);

        if (levelOptions == null || levelOptions.Length == 0)
        {
            runtimeMenu.Show(
                menuTitle,
                "No levels configured.",
                new SimpleRuntimeMenu.MenuOption("Close", CloseMenu));
            return;
        }

        SimpleRuntimeMenu.MenuOption[] options = new SimpleRuntimeMenu.MenuOption[levelOptions.Length + 1];
        for (int i = 0; i < levelOptions.Length; i++)
        {
            string sceneName = levelOptions[i].sceneName;
            string displayName = levelOptions[i].displayName;
            options[i] = new SimpleRuntimeMenu.MenuOption(displayName, () => LoadLevel(sceneName));
        }

        options[^1] = new SimpleRuntimeMenu.MenuOption("Close", CloseMenu);
        runtimeMenu.Show(menuTitle, "Choose a level to enter.", options);
    }

    private void CloseMenu()
    {
        isMenuOpen = false;
        if (runtimeMenu != null)
        {
            runtimeMenu.Hide();
        }

        if (IsPlayerInRange())
        {
            UpdatePrompt();
            SetPromptVisible(true);
        }
        else
        {
            SetPromptVisible(false);
        }
    }

    private bool IsPlayerInRange()
    {
        if (playerResources == null)
        {
            return false;
        }

        return Vector2.Distance(transform.position, playerResources.transform.position) <= interactionRange;
    }

    private void UpdatePrompt()
    {
        if (promptLabel != null)
        {
            promptLabel.text = promptMessage;
        }
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptRoot != null)
        {
            promptRoot.SetActive(visible);
        }
    }
}
