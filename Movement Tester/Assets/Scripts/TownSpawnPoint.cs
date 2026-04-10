using UnityEngine;

[DisallowMultipleComponent]
public class TownSpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnPointId = "TownSpawn";
    [SerializeField] private Transform spawnPosition;

    public string SpawnPointId => spawnPointId;
    public Vector3 SpawnPosition => spawnPosition != null ? spawnPosition.position : transform.position;

    public static TownSpawnPoint Find(string spawnPointId)
    {
        TownSpawnPoint[] spawnPoints = Object.FindObjectsByType<TownSpawnPoint>(FindObjectsSortMode.None);

        foreach (TownSpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint.SpawnPointId == spawnPointId)
            {
                return spawnPoint;
            }
        }

        return spawnPoints.Length > 0 ? spawnPoints[0] : null;
    }
}
