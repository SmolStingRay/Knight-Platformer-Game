using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DoorSceneLoader : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TMP_Text promptLabel;
    [SerializeField] private string promptMessage = "Press E to enter";
    [SerializeField] private string playerTag = "Player";

    private bool playerInRange;

    private void Start()
    {
        SetPromptVisible(false);

        if (promptLabel != null)
        {
            promptLabel.text = promptMessage;
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            SetPromptVisible(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            SetPromptVisible(false);
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