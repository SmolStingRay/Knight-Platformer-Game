using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
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
    [SerializeField] private LevelOption[] levelOptions;

    private PlayerResources currentPlayer;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        currentPlayer = FindPlayerResources(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        currentPlayer = FindPlayerResources(other);

        if (currentPlayer != null && Input.GetKeyDown(interactionKey))
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

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerResources resources = FindPlayerResources(other);
        if (resources != null && resources == currentPlayer)
        {
            currentPlayer = null;
            CloseMenu();
        }
    }

    private void LoadLevel(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName) || currentPlayer == null)
        {
            return;
        }

        CloseMenu();
        PlayerRuntimeState.Instance.SaveLevelEntrySnapshot(currentPlayer.gameObject);
        SceneManager.LoadScene(sceneName);
    }

    private void OpenMenu()
    {
        if (currentPlayer == null)
        {
            return;
        }

        isMenuOpen = true;

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
    }

    private static PlayerResources FindPlayerResources(Collider2D other)
    {
        if (other == null)
        {
            return null;
        }

        PlayerResources resources = other.GetComponent<PlayerResources>();
        if (resources != null)
        {
            return resources;
        }

        resources = other.GetComponentInParent<PlayerResources>();
        if (resources != null)
        {
            return resources;
        }

        return other.attachedRigidbody != null ? other.attachedRigidbody.GetComponent<PlayerResources>() : null;
    }
}
