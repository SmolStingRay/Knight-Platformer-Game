using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRuntimeState : MonoBehaviour
{
    private struct PlayerStateSnapshot
    {
        public bool IsValid;
        public int Gold;
        public int PotionCount;
        public int WeaponLevel;
        public int ArmorLevel;
        public int CurrentHealth;
    }

    public static PlayerRuntimeState Instance
    {
        get
        {
            EnsureInstanceExists();
            return instance;
        }
    }

    private static PlayerRuntimeState instance;

    private PlayerStateSnapshot currentState;
    private PlayerStateSnapshot levelEntrySnapshot;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstanceExists()
    {
        if (instance != null)
        {
            return;
        }

        GameObject stateObject = new("PlayerRuntimeState");
        instance = stateObject.AddComponent<PlayerRuntimeState>();
        DontDestroyOnLoad(stateObject);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    public void CaptureCurrentState(GameObject playerObject = null)
    {
        GameObject target = playerObject != null ? playerObject : FindPlayerObject();
        if (target == null)
        {
            return;
        }

        currentState = BuildSnapshot(target);
    }

    public void SaveLevelEntrySnapshot(GameObject playerObject = null)
    {
        GameObject target = playerObject != null ? playerObject : FindPlayerObject();
        if (target == null)
        {
            return;
        }

        currentState = BuildSnapshot(target);
        levelEntrySnapshot = currentState;
    }

    public void RestoreLevelEntrySnapshot()
    {
        if (!levelEntrySnapshot.IsValid)
        {
            return;
        }

        currentState = levelEntrySnapshot;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject playerObject = FindPlayerObject();
        if (playerObject == null)
        {
            return;
        }

        if (!currentState.IsValid)
        {
            currentState = BuildSnapshot(playerObject);
            return;
        }

        ApplySnapshot(playerObject, currentState);
    }

    private static GameObject FindPlayerObject()
    {
        PlayerResources resources = FindFirstObjectByType<PlayerResources>();
        if (resources != null)
        {
            return resources.gameObject;
        }

        Health health = FindFirstObjectByType<Health>();
        return health != null ? health.gameObject : null;
    }

    private static PlayerStateSnapshot BuildSnapshot(GameObject playerObject)
    {
        PlayerStateSnapshot snapshot = new()
        {
            IsValid = true
        };

        PlayerResources resources = playerObject.GetComponent<PlayerResources>();
        if (resources != null)
        {
            snapshot.Gold = resources.Gold;
            snapshot.PotionCount = resources.PotionCount;
        }

        PlayerCombatStats combatStats = playerObject.GetComponent<PlayerCombatStats>();
        if (combatStats != null)
        {
            snapshot.WeaponLevel = combatStats.WeaponLevel;
            snapshot.ArmorLevel = combatStats.ArmorLevel;
        }
        else
        {
            snapshot.WeaponLevel = 1;
            snapshot.ArmorLevel = 1;
        }

        Health health = playerObject.GetComponent<Health>();
        snapshot.CurrentHealth = health != null ? health.CurrentHealth : 0;

        return snapshot;
    }

    private static void ApplySnapshot(GameObject playerObject, PlayerStateSnapshot snapshot)
    {
        PlayerCombatStats combatStats = playerObject.GetComponent<PlayerCombatStats>();
        if (combatStats != null)
        {
            combatStats.ApplyState(snapshot.WeaponLevel, snapshot.ArmorLevel);
        }

        Health health = playerObject.GetComponent<Health>();
        int maxHealth = health != null ? health.MaxHealth : 0;

        if (health != null && combatStats != null)
        {
            maxHealth = health.BaseMaxHealth + combatStats.BonusMaxHealth;
            health.SetMaxHealth(maxHealth, false);
        }

        PlayerResources resources = playerObject.GetComponent<PlayerResources>();
        if (resources != null)
        {
            resources.ApplyState(snapshot.Gold, snapshot.PotionCount);
        }

        if (health != null)
        {
            int clampedHealth = snapshot.CurrentHealth > 0 ? snapshot.CurrentHealth : maxHealth;
            health.SetCurrentHealth(clampedHealth);
        }
    }
}
