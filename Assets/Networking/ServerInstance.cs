using Riptide.Utils;
using Riptide;

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;

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
        public static Server _instance { get; private set; }

        public void OnServerInitalize()
        {
            //Setup debug logger callback with timestaps
            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, true);

            //Start the server
            _instance = new Server();
            _instance.Start(PORT, MAXPLAYERCOUNT);
            _instance.ClientDisconnected += ClientDisconnected;

            //Subscribe to the update loop
            NetworkManager.InstanceUpdate += _instance.Update;
            NetworkManager.InstanceDispose += _instance.Stop;
        }

        private void ClientDisconnected(object sender, ServerDisconnectedEventArgs e)
        {
            
        }

        [MessageHandler((ushort)MessageHelper.messageTypes.ClientConnection)]
        private static void ClientConnection(ushort clientid, Message message)
        {
            string name = message.GetString();
            PlayerManager.playerIds.Add(clientid, name);
            Debug.Log($"Client Connected ID: {clientid} With Name {name}");
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

            _instance.SendToAll(message);
        }

        public void OnDestroy()
        {
            if (_instance != null) NetworkManager.InstanceUpdate -= _instance.Update;
            _instance?.Stop();
        }


    }
}
