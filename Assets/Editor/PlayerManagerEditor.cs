using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerManager))]
public class PlayerManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (!Application.isPlaying)
        {
            base.OnInspectorGUI();
            return;
        }

        serializedObject.Update();

        PlayerManager script = (PlayerManager)target;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        if (PlayerManager.playerIds != null)
        {
            foreach(var player in PlayerManager.playerIds)
            {
                EditorGUILayout.LabelField($"{player.Key}  {player.Value}");
            }
        }
        EditorGUILayout.EndVertical();

    }
}
