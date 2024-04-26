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
    [MessageHandler((ushort)MessageHelper.messageTypes.PlayerSync)]
    public static void OnPlayerSyncablePacketReceived(ushort clientid, Message message)
    {
        //Connection Message
        NetworkManager.GetServer().SendToAll(message,clientid);
    }


    /// <summary>
    /// Called when another client joins the server
    /// </summary>
    public void ClientConnected(object sender, ClientConnectedEventArgs e)
    {

    }


    /// <summary>
    /// Called when the local client connects to the server
    /// Handles Adding The Player To The Client List
    /// </summary>
    public void LocalPlayerConnected(object sender, System.EventArgs e)
    {
        NetworkManager.GetClient().Connected -= LocalPlayerConnected;

        string playerName = NetworkManager.GetClientInstance().Name();

        //Connection Message
        Message connectionMessage = MessageHelper.CreateMessage(MessageHelper.messageTypes.ClientConnection, MessageSendMode.Reliable);
        MessageHelper.AddToMessage(ref connectionMessage, MessageHelper.messageTypes.String, playerName);
        NetworkManager.GetClient().Send(connectionMessage);

    }

    public void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        //Removes the player id from our id list
        playerIds.Remove(e.Id);

        _players.TryGetValue(e.Id,out var go);
        if (go != null)
        {
            _players.Remove(e.Id);
            Destroy(go);
        }

        ServerInstance.SendClientList();
    }
}
