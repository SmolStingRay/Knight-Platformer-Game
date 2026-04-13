using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerAttack2D))]
[CanEditMultipleObjects]
public class PlayerAttack2DEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Preview Attack shows the slash effect and attack hitbox in Scene view without entering Play mode.", MessageType.Info);

        if (GUILayout.Button("Preview Attack"))
        {
            foreach (Object selectedTarget in targets)
            {
                if (selectedTarget is PlayerAttack2D attack)
                {
                    attack.PreviewAttackInEditor();
                }
            }
        }
    }
}
