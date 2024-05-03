using Riptide.Utils;
using Riptide;

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using Riptide.Transports.Steam;

namespace Assets
{
    [System.Serializable]
    [DisallowMultipleComponent]
    public class ServerInstance : MonoBehaviour
    {
        //These will be changed dynamically 
        const int PORT = 7777;
        public int GetPort() => PORT;

        const int MAXPLAYERCOUNT = 10;

        //The client and server instance for the session
        //Server instance is only avalible to the host
        public Server _instance { get; private set; }
        public SteamServer _steamServerInstance;

        public void OnServerInitalize()
        {
            //Setup debug logger callback with timestaps
            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, true);

            //Create the steam server instance
            _steamServerInstance = new SteamServer();

            //Start the server
            _instance = new Server(_steamServerInstance);
            _instance.Start(0, MAXPLAYERCOUNT,NetworkManager.networkHandlerId);

            //Subscribe to the update loop
            NetworkManager.InstanceUpdate += _instance.Update;
            NetworkManager.InstanceDispose += _instance.Stop;
        }

        [MessageHandler((ushort)MessageHelper.messageTypes.LobbyReady, NetworkManager.networkHandlerId)]
        private static void PlayerReadyMessageHandler(ushort clientid, Message message)
        {
            ushort clientId = message.GetUShort();
            bool isClientReady = message.GetBool();

            Message toClientsMessage = MessageHelper.CreateMessage(MessageHelper.messageTypes.LobbyReady, MessageSendMode.Reliable);
            toClientsMessage.Add(clientId);
            toClientsMessage.Add(isClientReady);
            NetworkManager.GetServer().SendToAll(toClientsMessage);
        }


        [MessageHandler((ushort)MessageHelper.messageTypes.GameStart, NetworkManager.networkHandlerId)]
        private static void StartGame(ushort clientid, Message message)
        {
            Message gameStartMessage = MessageHelper.CreateMessage(MessageHelper.messageTypes.GameStart, MessageSendMode.Reliable);
            NetworkManager.GetServer().SendToAll(gameStartMessage);
        }

        [MessageHandler((ushort)MessageHelper.messageTypes.ClientConnection, NetworkManager.networkHandlerId)]
        private static void ClientConnection(ushort clientid, Message message)
        {
            string name = message.GetString();
            PlayerManager.playerIds.TryAdd(clientid, name);
            SendClientList();
        }


        public static void SendClientList()
        {
            Message message = MessageHelper.CreateMessage(MessageHelper.messageTypes.ClientList, MessageSendMode.Reliable);
            message.AddInt(PlayerManager.playerIds.Count);

            foreach(var keyvaluepair in PlayerManager.playerIds)
            {
                message.Add(keyvaluepair.Key);
                message.Add(keyvaluepair.Value);
            }

            NetworkManager.GetServer().SendToAll(message);
        }

        public void OnDestroy()
        {
            if (_instance != null) NetworkManager.InstanceUpdate -= _instance.Update;
            _instance?.Stop();
        }


    }
}
