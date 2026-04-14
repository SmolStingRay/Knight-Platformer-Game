using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndTrigger : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "Level2";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool savePlayerState = true;

    private bool hasTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered)
            return;

        if (!other.CompareTag(playerTag))
            return;

        hasTriggered = true;

        if (savePlayerState && PlayerRuntimeState.Instance != null)
        {
            PlayerResources playerResources =
                other.GetComponent<PlayerResources>() ??
                other.GetComponentInParent<PlayerResources>();

            if (playerResources != null)
            {
                PlayerRuntimeState.Instance.SaveLevelEntrySnapshot(playerResources.gameObject);
            }
        }

        SceneManager.LoadScene(nextSceneName);
    }
}