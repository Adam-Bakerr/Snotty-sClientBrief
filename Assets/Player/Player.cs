using Assets;
using Riptide;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] PlayerInputManager inputManager;
    [SerializeField] Transform cameraPivot;

    public struct SyncableTransform : IMessageSerializable
    {
        public Vector3 Position;
        public Vector3 EulerAngles;
        public Vector3 Scale;

        public SyncableTransform(Vector3 position, Vector3 eulerAngles, Vector3 scale)
        {
            Position = position;
            EulerAngles = eulerAngles;
            Scale = scale;
        }

        public SyncableTransform(Transform transform)
        {
            Position = transform.position;
            EulerAngles = transform.eulerAngles;
            Scale = transform.localScale;
        }

        public void Serialize(Message message)
        {
            message.Add(Position);
            message.Add(EulerAngles);
            message.Add(Scale);
        }

        public void Deserialize(Message message)
        {
            this.Position = message.GetVector3();
            this.EulerAngles = message.GetVector3();
            this.Scale = message.GetVector3();
        }
    }
    public struct SyncableStats : IMessageSerializable
    {

        int CurrentLevel;
        int CurrentXP;
        int CurrentXPToNextRank;

        public SyncableStats(int currentLevel, int currentXP, int currentXPToNextRank)
        {
            CurrentLevel = currentLevel;
            CurrentXP = currentXP;
            CurrentXPToNextRank = currentXPToNextRank;
        }

        public void Serialize(Message message)
        {
            message.Add(CurrentLevel);
            message.Add(CurrentXP);
            message.Add(CurrentXPToNextRank);
        }

        public void Deserialize(Message message)
        {
            this.CurrentLevel = message.GetInt();
            this.CurrentXP = message.GetInt();
            this.CurrentXPToNextRank = message.GetInt();
        }
    }

    public void Update()
    {
        if (NetworkManager.instance.IsUnityNull()) return;

        //Sends The Clients Position To The Server For Syncing
        //We Send It Unreliable due to the frequency
        Message message = MessageHelper.CreateMessage(MessageHelper.messageTypes.PlayerTransformSync, MessageSendMode.Unreliable);

        message.Add(NetworkManager.GetClient().Id);
        message.Add(new SyncableTransform(transform.position, new Vector3(0, cameraPivot.eulerAngles.y,0), transform.localScale));

        NetworkManager.GetClient().Send(message);
    }

    public void SyncPlayerStats()
    {
        Message message = MessageHelper.CreateMessage(MessageHelper.messageTypes.StatSync, MessageSendMode.Reliable);

        message.Add(NetworkManager.GetClient().Id);
        message.Add(new SyncableStats(1,250,500));

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