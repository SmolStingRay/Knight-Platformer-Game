using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Enemy))]
[CanEditMultipleObjects]
public class EnemyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Preview Attack shows the enemy slash effect and attack hitbox in Scene view without entering Play mode.", MessageType.Info);

        if (GUILayout.Button("Preview Attack"))
        {
            foreach (Object selectedTarget in targets)
            {
                if (selectedTarget is Enemy enemy)
                {
                    enemy.PreviewAttackInEditor();
                }
            }
        }
    }
}
