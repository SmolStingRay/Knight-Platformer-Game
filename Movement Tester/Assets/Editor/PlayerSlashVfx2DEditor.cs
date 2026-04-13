using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerSlashVfx2D))]
public class PlayerSlashVfx2DEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        PlayerSlashVfx2D slashVfx = (PlayerSlashVfx2D)target;
        if (GUILayout.Button("Preview Slash"))
        {
            slashVfx.PreviewSlashInEditor();
        }
    }
}
