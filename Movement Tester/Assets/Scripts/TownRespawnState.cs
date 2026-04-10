public static class TownRespawnState
{
    public static bool HasPendingRespawn { get; private set; }
    public static string TargetSceneName { get; private set; } = string.Empty;
    public static string TargetSpawnPointId { get; private set; } = string.Empty;
    public static float RespawnProtectionDuration { get; private set; }

    public static void Schedule(string sceneName, string spawnPointId, float protectionDuration)
    {
        HasPendingRespawn = true;
        TargetSceneName = sceneName;
        TargetSpawnPointId = spawnPointId;
        RespawnProtectionDuration = protectionDuration;
    }

    public static void Clear()
    {
        HasPendingRespawn = false;
        TargetSceneName = string.Empty;
        TargetSpawnPointId = string.Empty;
        RespawnProtectionDuration = 0f;
    }
}
