using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Health))]
public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private float respawnDelay = 1.5f;
    [SerializeField] private float respawnProtectionDuration = 2f;
    [SerializeField] private string townSceneName = "SampleScene";
    [SerializeField] private string townSpawnPointId = "TownSpawn";
    [SerializeField] private MonoBehaviour[] behavioursToDisableOnDeath;
    [SerializeField] private Collider2D[] collidersToDisableOnDeath;
    [SerializeField] private Rigidbody2D rb;

    private Health health;
    private Coroutine respawnRoutine;

    private void Awake()
    {
        health = GetComponent<Health>();

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }

    private void OnEnable()
    {
        health.Died += HandleDeath;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        health.Died -= HandleDeath;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    public void ConfigureTownRespawn(string sceneName, string spawnPointId)
    {
        townSceneName = sceneName;
        townSpawnPointId = spawnPointId;
    }

    public void ConfigureDeathStateTargets(MonoBehaviour[] behaviours, Collider2D[] colliders, Rigidbody2D targetRigidbody = null)
    {
        behavioursToDisableOnDeath = behaviours;
        collidersToDisableOnDeath = colliders;

        if (targetRigidbody != null)
        {
            rb = targetRigidbody;
        }
    }

    private void Start()
    {
        TryApplyPendingTownRespawn(SceneManager.GetActiveScene().name);
    }

    private void HandleDeath()
    {
        SetAliveState(false);

        if (respawnRoutine != null)
        {
            StopCoroutine(respawnRoutine);
        }

        respawnRoutine = StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        TownRespawnState.Schedule(townSceneName, townSpawnPointId, respawnProtectionDuration);
        SceneManager.LoadScene(townSceneName);
        respawnRoutine = null;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryApplyPendingTownRespawn(scene.name);
    }

    private void TryApplyPendingTownRespawn(string activeSceneName)
    {
        if (!TownRespawnState.HasPendingRespawn || TownRespawnState.TargetSceneName != activeSceneName)
        {
            return;
        }

        TownSpawnPoint spawnPoint = TownSpawnPoint.Find(TownRespawnState.TargetSpawnPointId);
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.SpawnPosition;
        }

        SetAliveState(true);
        health.RestoreFullHealth();
        health.ProtectForSeconds(TownRespawnState.RespawnProtectionDuration);
        TownRespawnState.Clear();
    }

    private void SetAliveState(bool alive)
    {
        foreach (MonoBehaviour behaviour in behavioursToDisableOnDeath)
        {
            if (behaviour != null)
            {
                behaviour.enabled = alive;
            }
        }

        foreach (Collider2D targetCollider in collidersToDisableOnDeath)
        {
            if (targetCollider != null)
            {
                targetCollider.enabled = alive;
            }
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}
