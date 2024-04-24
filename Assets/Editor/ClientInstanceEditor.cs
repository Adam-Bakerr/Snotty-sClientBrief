using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ClientInstance))]
public class ClientInstanceEditor : Editor
{
    SerializedProperty listProperty;

    public override void OnInspectorGUI()
    {

    }
}
