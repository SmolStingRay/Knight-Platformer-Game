using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MissingScriptCleaner
{
    [MenuItem("Tools/Cleanup/Remove Missing Scripts In Open Scene")]
    public static void RemoveMissingScriptsInOpenScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid())
        {
            Debug.LogWarning("No valid active scene is open.");
            return;
        }

        int removedCount = 0;
        int affectedObjects = 0;

        foreach (GameObject rootObject in activeScene.GetRootGameObjects())
        {
            RemoveMissingScriptsRecursive(rootObject, ref removedCount, ref affectedObjects);
        }

        if (removedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
        }

        Debug.Log($"Removed {removedCount} missing script component(s) from {affectedObjects} object(s) in scene '{activeScene.name}'.");
    }

    [MenuItem("Tools/Cleanup/Log Objects With Missing Scripts In Open Scene")]
    public static void LogObjectsWithMissingScriptsInOpenScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid())
        {
            Debug.LogWarning("No valid active scene is open.");
            return;
        }

        List<string> objectPaths = new List<string>();

        foreach (GameObject rootObject in activeScene.GetRootGameObjects())
        {
            LogMissingScriptsRecursive(rootObject, objectPaths);
        }

        if (objectPaths.Count == 0)
        {
            Debug.Log($"No objects with missing scripts were found in scene '{activeScene.name}'.");
            return;
        }

        Debug.Log($"Found {objectPaths.Count} object(s) with missing scripts in scene '{activeScene.name}':\n - {string.Join("\n - ", objectPaths)}");
    }

    [MenuItem("Tools/Cleanup/Log Prefabs With Missing Scripts In Assets")]
    public static void LogPrefabsWithMissingScriptsInAssets()
    {
        List<string> prefabPaths = new List<string>();

        foreach (string prefabPath in AssetDatabase.GetAllAssetPaths())
        {
            if (!prefabPath.StartsWith("Assets/") || !prefabPath.EndsWith(".prefab"))
            {
                continue;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                if (PrefabHasMissingScripts(prefabRoot))
                {
                    prefabPaths.Add(prefabPath);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        if (prefabPaths.Count == 0)
        {
            Debug.Log("No prefabs with missing scripts were found in Assets.");
            return;
        }

        Debug.Log($"Found {prefabPaths.Count} prefab(s) with missing scripts:\n - {string.Join("\n - ", prefabPaths)}");
    }

    [MenuItem("Tools/Cleanup/Remove Missing Scripts In Prefabs In Assets")]
    public static void RemoveMissingScriptsInPrefabsInAssets()
    {
        int removedCount = 0;
        int affectedPrefabs = 0;

        foreach (string prefabPath in AssetDatabase.GetAllAssetPaths())
        {
            if (!prefabPath.StartsWith("Assets/") || !prefabPath.EndsWith(".prefab"))
            {
                continue;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                int prefabRemovedCount = 0;
                int prefabAffectedObjects = 0;
                RemoveMissingScriptsRecursive(prefabRoot, ref prefabRemovedCount, ref prefabAffectedObjects);

                if (prefabRemovedCount > 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                    removedCount += prefabRemovedCount;
                    affectedPrefabs++;
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Removed {removedCount} missing script component(s) from {affectedPrefabs} prefab(s) in Assets.");
    }

    private static void RemoveMissingScriptsRecursive(GameObject gameObject, ref int removedCount, ref int affectedObjects)
    {
        int before = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
        if (before > 0)
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
            removedCount += before;
            affectedObjects++;
        }

        foreach (Transform child in gameObject.transform)
        {
            RemoveMissingScriptsRecursive(child.gameObject, ref removedCount, ref affectedObjects);
        }
    }

    private static void LogMissingScriptsRecursive(GameObject gameObject, List<string> objectPaths)
    {
        if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject) > 0)
        {
            objectPaths.Add(GetHierarchyPath(gameObject.transform));
        }

        foreach (Transform child in gameObject.transform)
        {
            LogMissingScriptsRecursive(child.gameObject, objectPaths);
        }
    }

    private static bool PrefabHasMissingScripts(GameObject gameObject)
    {
        if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject) > 0)
        {
            return true;
        }

        foreach (Transform child in gameObject.transform)
        {
            if (PrefabHasMissingScripts(child.gameObject))
            {
                return true;
            }
        }

        return false;
    }

    private static string GetHierarchyPath(Transform current)
    {
        string path = current.name;
        while (current.parent != null)
        {
            current = current.parent;
            path = $"{current.name}/{path}";
        }

        return path;
    }
}
