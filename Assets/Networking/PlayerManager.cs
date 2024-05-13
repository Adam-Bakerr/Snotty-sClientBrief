using Assets;
using Riptide;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    //Player List
    public static Dictionary<ushort,string> playerIds = new(); 
    public static Dictionary<ushort, GameObject> _players = new();


    //Middle man for client movement packets that pass throught the server
    //Doesnt Send The Data To The User That Sent It
    [MessageHandler((ushort)MessageHelper.messageTypes.PlayerTransformSync, NetworkManager.networkHandlerId)]
    public static void OnPlayerSyncablePacketReceived(ushort clientid, Message message)
    {
        //Connection Message
        NetworkManager.GetServer().SendToAll(message,clientid);
    }


    [MessageHandler((ushort)MessageHelper.messageTypes.PlayerTransformSync, NetworkManager.networkHandlerId)]
    public static void OnPlayerSyncablePacketReceived(Message message)
    {
        ushort clientID = message.GetUShort();

        Player.SyncableTransform messageTransform = message.GetSerializable<Player.SyncableTransform>();

        if(_players.TryGetValue(clientID,out GameObject go))
        {
            if (!go) return;
            Transform clientTransform = go.transform;
            clientTransform.position = messageTransform.Position;
            clientTransform.eulerAngles = messageTransform.EulerAngles;
            clientTransform.localScale = messageTransform.Scale;
        }
        else
        {
            Debug.Log($"Missing Network Player For ID: {clientID}");
        }
    }

    public static void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        //Removes the player id from our id list
        playerIds.Remove(e.Id);

        _players.TryGetValue(e.Id, out var go);
        if (go != null)
        {
            _players.Remove(e.Id);
            Destroy(go);
        }

        ServerInstance.SendClientList();
    }

}
