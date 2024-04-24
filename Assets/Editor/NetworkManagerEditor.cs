using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(NetworkManager))]
public class NetworkManagerEditor : Editor
{ 
    public override void OnInspectorGUI()
    {
        if (!Application.isPlaying)
        {
            base.OnInspectorGUI();
            return;
        }



        serializedObject.Update();

        NetworkManager script = (NetworkManager)target;
        if (script._clientInstance == null) return;
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("Network Manager Status");
        EditorGUILayout.BeginHorizontal();



        GUIStyle style = GUI.skin.box;
        style.normal.textColor = script._clientInstance._instance.IsConnected ? Color.green : Color.red;
        string networkStatus = NetworkManager.GetClient().IsConnected ? "Connected" : "Disconnected";
        EditorGUILayout.LabelField($"{networkStatus} IP: {NetworkManager.GetClientInstance().ConnectedIP()}:{NetworkManager.GetClientInstance().ConnectedPort()}", style);

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();


        ////Display User Connecton Status
        //GUIStyle style = GUI.skin.box;
        //style.normal.textColor = script._clientInstance._instance.IsConnected ? Color.green : Color.red;
        //EditorGUILayout.LabelField(script._clientInstance._instance.IsConnected ? "Connected" : "Disconnected", style);
        //EditorGUILayout.EndHorizontal();

        //GUILayout.Space(10);

        ////Display List Of Clients
        //EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        //EditorGUILayout.LabelField("Connected Clients:", EditorStyles.boldLabel);
        //if (ClientInstance.clientList != null)
        //{
        //    foreach (var item in ClientInstance.clientList)
        //    {
        //        EditorGUILayout.LabelField($"{item.Key} {item.Value}");
        //    }
        //}
        //EditorGUILayout.EndVertical();

        //EditorGUILayout.Space();


    }
}
