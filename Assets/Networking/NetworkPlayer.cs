using Assets;
using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    ushort thisClientId;
    public void SetClientID(ushort id) => thisClientId = id;
    public static Transform networkTransform;

    private void Awake()
    {
        networkTransform = transform;
    }


    [MessageHandler((ushort)MessageHelper.messageTypes.PlayerSync, NetworkManager.networkHandlerId)]
    private static void GameStartMessageHandler(Message message)
    {
        ushort clientID = message.GetUShort();

        //If this packet doesnt effect this network player dont read it
        if (clientID != NetworkManager.GetClient().Id) return;

        Player.SyncableData syncableData = message.GetSerializable<Player.SyncableData>();

        networkTransform.position = syncableData.Position;
        networkTransform.localEulerAngles = syncableData.EulerAngles;
    }

}
