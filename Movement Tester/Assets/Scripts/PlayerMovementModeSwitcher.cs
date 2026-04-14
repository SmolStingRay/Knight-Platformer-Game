using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementModeSwitcher : MonoBehaviour
{
    [SerializeField] private string[] topDownSceneNames;
    [SerializeField] private TopDownMovement topDownMovement;
    [SerializeField] private PlatformerMovement2D platformerMovement;
    [SerializeField] private PlayerAttack2D playerAttack;
    [SerializeField] private PlayerRespawn playerRespawn;
    [SerializeField] private Collider2D[] collidersToDisableOnDeath;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (topDownMovement == null)
            topDownMovement = GetComponent<TopDownMovement>();

        if (platformerMovement == null)
            platformerMovement = GetComponent<PlatformerMovement2D>();

        if (playerAttack == null)
            playerAttack = GetComponent<PlayerAttack2D>();

        if (playerRespawn == null)
            playerRespawn = GetComponent<PlayerRespawn>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        ApplyMovementMode(SceneManager.GetActiveScene().name);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyMovementMode(scene.name);
    }

    private bool IsTopDownScene(string sceneName)
    {
        if (topDownSceneNames == null || topDownSceneNames.Length == 0)
            return false;

        for (int i = 0; i < topDownSceneNames.Length; i++)
        {
            if (topDownSceneNames[i] == sceneName)
                return true;
        }

        return false;
    }

    private void ApplyMovementMode(string sceneName)
    {
        bool isTopDownScene = IsTopDownScene(sceneName);

        if (topDownMovement != null)
            topDownMovement.enabled = isTopDownScene;

        if (platformerMovement != null)
            platformerMovement.enabled = !isTopDownScene;

        if (playerAttack != null)
            playerAttack.enabled = !isTopDownScene;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = isTopDownScene ? 0f : 3f;
            rb.freezeRotation = true;
        }

        if (playerRespawn != null)
        {
            MonoBehaviour activeMovement = isTopDownScene ? topDownMovement : platformerMovement;
            MonoBehaviour[] deathBehaviours = activeMovement != null ? new[] { activeMovement } : new MonoBehaviour[0];
            playerRespawn.ConfigureDeathStateTargets(deathBehaviours, collidersToDisableOnDeath, rb);
        }
    }
}