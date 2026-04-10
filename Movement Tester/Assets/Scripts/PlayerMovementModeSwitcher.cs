using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementModeSwitcher : MonoBehaviour
{
    [SerializeField] private string townSceneName = "SampleScene";
    [SerializeField] private TopDownMovement topDownMovement;
    [SerializeField] private PlatformerMovement2D platformerMovement;
    [SerializeField] private PlayerRespawn playerRespawn;
    [SerializeField] private Collider2D[] collidersToDisableOnDeath;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (topDownMovement == null)
        {
            topDownMovement = GetComponent<TopDownMovement>();
        }

        if (platformerMovement == null)
        {
            platformerMovement = GetComponent<PlatformerMovement2D>();
        }

        if (playerRespawn == null)
        {
            playerRespawn = GetComponent<PlayerRespawn>();
        }
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

    private void ApplyMovementMode(string sceneName)
    {
        bool isTownScene = sceneName == townSceneName;

        if (topDownMovement != null)
        {
            topDownMovement.enabled = isTownScene;
        }

        if (platformerMovement != null)
        {
            platformerMovement.enabled = !isTownScene;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = isTownScene ? 0f : 3f;
            rb.freezeRotation = true;
        }

        if (playerRespawn != null)
        {
            MonoBehaviour activeMovement = isTownScene ? topDownMovement : platformerMovement;
            MonoBehaviour[] deathBehaviours = activeMovement != null ? new[] { activeMovement } : new MonoBehaviour[0];
            playerRespawn.ConfigureDeathStateTargets(deathBehaviours, collidersToDisableOnDeath, rb);
        }
    }
}
