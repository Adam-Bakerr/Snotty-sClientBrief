using Assets;
using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;
using Riptide;

public class Player : MonoBehaviour
{
    public struct SyncableData : IMessageSerializable
    {
        public Vector3 Position;
        public Vector3 EulerAngles;

        public SyncableData(Vector3 position, Vector3 eulerAngles)
        {
            Position = position;
            EulerAngles = eulerAngles;
        }

        public void Serialize(Message message)
        {
            message.Add(Position);
            message.Add(EulerAngles);
        }

        public void Deserialize(Message message)
        {
            this.Position = message.GetVector3();
            this.EulerAngles = message.GetVector3();
        }
    }

    public void Update()
    {
        //Sends The Clients Position To The Server For Syncing
        //We Send It Unreliable due to the frequency
        Message message = MessageHelper.CreateMessage(MessageHelper.messageTypes.PlayerSync, MessageSendMode.Unreliable);

        message.Add(NetworkManager.GetClient().Id);
        message.Add(new SyncableData(transform.position, transform.localEulerAngles));

        NetworkManager.GetClient().Send(message);
    }
}

//[CustomEditor(typeof(Player))]
//public class PlayerEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        EditorGUILayout.LabelField("Syncs The Players Position With The Server");
//    }
//}