using Assets;
using Riptide;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Downable : MonoBehaviour
{
    [SerializeField] GameObject downableText;
    public bool isDown;

    public void Down()
    {
        isDown = true;
        PlayerManager._downablePlayers.Remove(gameObject);
        PlayerManager._downedPlayers.TryAdd(gameObject,this);
        downableText?.SetActive(true);

        //Tell The Server The Player Went Down
        Message playerDownMessage = MessageHelper.CreateMessage(MessageHelper.messageTypes.PlayerDowned, MessageSendMode.Reliable);
        playerDownMessage.Add(NetworkManager.GetClient().Id);
        NetworkManager.GetClient().Send(playerDownMessage);

        if(PlayerManager._downablePlayers.Count <= 0)
        {
            //Send Game Over Message To Server For Validation
            Message gameOverMessage = MessageHelper.CreateMessage(MessageHelper.messageTypes.GameCompleted, MessageSendMode.Reliable);
            gameOverMessage.Add(false);
            NetworkManager.GetClient().Send(gameOverMessage);
        }

    }

    public void NetworkPlayerDown()
    {
        isDown = true;
        PlayerManager._downablePlayers.Remove(gameObject);
        PlayerManager._downedPlayers.Add(gameObject, this);
        downableText?.SetActive(true);
    }

    public void Revive()    
    {
        isDown = false;
        PlayerManager._downablePlayers.TryAdd(gameObject, this);
        PlayerManager._downedPlayers.Remove(gameObject);
        downableText?.SetActive(false);

        //Tell The Server The Player Was Revived
        Message playerReviveMessage = MessageHelper.CreateMessage(MessageHelper.messageTypes.PlayerRevive, MessageSendMode.Reliable);
        playerReviveMessage.Add(PlayerManager._players.FirstOrDefault(x => x.Value == gameObject).Key);
        NetworkManager.GetClient().Send(playerReviveMessage);
    }

    public void NetworkRevive()
    {
        isDown = false;
        PlayerManager._downablePlayers.TryAdd(gameObject, this);
        PlayerManager._downedPlayers.Remove(gameObject);
        downableText?.SetActive(false);
    }

    public void OnTriggerStay(Collider other)
    {
        if (!PlayerInputManager.isInteracting) return;
        if(other.gameObject.layer == LayerMask.NameToLayer("Players"))
        {
            Debug.Log("Attempting Revive");
            if (!PlayerManager._downedPlayers.TryGetValue(other.gameObject, out Downable downedPlayerToRevive)) return;
            if (downedPlayerToRevive)
            {
                if (!downedPlayerToRevive.isDown) return;
                downedPlayerToRevive.Revive();
            }
        }
    }

    //Middle man for when a client goes down
    [MessageHandler((ushort)MessageHelper.messageTypes.PlayerDowned, NetworkManager.networkHandlerId)]
    public static void OnPlayerDowned(ushort clientid, Message message)
    {
        //Connection Message
        NetworkManager.GetServer().SendToAll(message,clientid);
    }

    [MessageHandler((ushort)MessageHelper.messageTypes.PlayerDowned, NetworkManager.networkHandlerId)]
    private static void OnPlayerDownedReceivedFromServer(Message message)
    {
        if (!PlayerManager._downablePlayers.TryGetValue(PlayerManager._players[message.GetUShort()], out Downable playerDownable)) return;
        playerDownable.NetworkPlayerDown();
    }


    //Middle man for when a gets revived
    [MessageHandler((ushort)MessageHelper.messageTypes.PlayerRevive, NetworkManager.networkHandlerId)]
    public static void OnPlayerRevived(ushort clientid, Message message)
    {
        //Connection Message
        NetworkManager.GetServer().SendToAll(message,clientid);
    }

    [MessageHandler((ushort)MessageHelper.messageTypes.PlayerRevive, NetworkManager.networkHandlerId)]
    private static void OnPlayerRevivedReceivedFromServer(Message message)
    {
        if(!PlayerManager._downedPlayers.TryGetValue(PlayerManager._players[message.GetUShort()], out Downable playerDownable))return;

        playerDownable.NetworkRevive();
    }

}
