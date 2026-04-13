using UnityEngine;
using UnityEngine.SceneManagement;

public static class CameraFollowBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        SetupCameraForActiveScene();
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupCameraForActiveScene();
    }

    private static void SetupCameraForActiveScene()
    {
        Camera targetCamera = FindBestCamera();
        if (targetCamera == null)
        {
            return;
        }

        CameraFollow2D follow = targetCamera.GetComponent<CameraFollow2D>();
        if (follow == null)
        {
            follow = targetCamera.gameObject.AddComponent<CameraFollow2D>();
        }

        Transform player = FindPlayerTarget();
        if (player != null)
        {
            follow.SetTarget(player, true);
        }

        follow.ApplyProfileForCurrentScene(player != null);
    }

    private static Camera FindBestCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            return mainCamera;
        }

        Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (Camera camera in cameras)
        {
            if (camera != null && camera.enabled && camera.name == "Main Camera")
            {
                return camera;
            }
        }

        foreach (Camera camera in cameras)
        {
            if (camera != null && camera.enabled)
            {
                return camera;
            }
        }

        return null;
    }

    private static Transform FindPlayerTarget()
    {
        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
        {
            return taggedPlayer.transform;
        }

        PlayerMovementModeSwitcher modeSwitcher = Object.FindFirstObjectByType<PlayerMovementModeSwitcher>();
        if (modeSwitcher != null)
        {
            return modeSwitcher.transform;
        }

        TopDownMovement topDownMovement = Object.FindFirstObjectByType<TopDownMovement>();
        if (topDownMovement != null)
        {
            return topDownMovement.transform;
        }

        PlatformerMovement2D platformerMovement = Object.FindFirstObjectByType<PlatformerMovement2D>();
        if (platformerMovement != null)
        {
            return platformerMovement.transform;
        }

        return null;
    }
}
