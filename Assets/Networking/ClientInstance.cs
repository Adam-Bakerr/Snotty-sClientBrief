using Riptide;
using Riptide.Transports;
using Riptide.Transports.Steam;
using Riptide.Utils;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;


namespace Assets
{
    [System.Serializable]
    [DisallowMultipleComponent]
    public class ClientInstance : MonoBehaviour
    {
        public Client _instance { get; private set; }
        public Riptide.Transports.Steam.SteamClient _steamClientInstance {  get; private set; }

        private string _name;
        CSteamID _connectedLobbyID;
        public string Name() => _name;
        public CSteamID ConnectedLobbyID() => _connectedLobbyID;


        /// <summary>
        /// Takes An Ip Address In The Form Of "127.0.0.1:7777" As A String
        /// </summary>
        public void ConnectToLocalServer(CSteamID lobbyID)
        {
            if(_instance == null) { Debug.LogError("Instance For Client Is Missing"); }
            _name = SteamFriends.GetPersonaName();
            _connectedLobbyID = lobbyID;
            _instance.Connect("127.0.0.1",messageHandlerGroupId: NetworkManager.networkHandlerId);
            _instance.Connected += OnConnectedToServer;
        }


        public void ConnectToHostID(CSteamID lobbyID)
        {
            _instance.Connect(lobbyID.ToString(), messageHandlerGroupId: NetworkManager.networkHandlerId);
            _connectedLobbyID = lobbyID;
            _name = SteamFriends.GetPersonaName();
            _steamClientInstance.Connected += OnConnectedToServer;
        }

        private void OnConnectedToServer(object sender, EventArgs e)
        {
            _instance.Connection.CanTimeout = false;

            Message ConnectionMessage = Message.Create(MessageSendMode.Reliable, (ushort)MessageHelper.messageTypes.ClientConnection);
            ConnectionMessage.Add(_name);
            _instance.Send(ConnectionMessage);
        }

        public void OnClientInitalize()
        {
            if (NetworkManager.instance == null) throw new System.Exception("Missing Server Instance");

            //Initalize the steam client not connected to any server
            _steamClientInstance = new (null);

            //Start the Client
            _instance = new Client(_steamClientInstance);

            //Subscribe the client to the update loop
            NetworkManager.InstanceUpdate += _instance.Update;

            //Subscribe the client to be disposed on shutdown
            NetworkManager.InstanceDispose += Dispose;

            _instance.ClientDisconnected += PlayerManager.ClientDisconnected;
        }

        /// <summary>
        /// Dispose Of The Network Before We Close The Session
        /// </summary>
        private void Dispose()
        {
            SteamMatchmaking.LeaveLobby(_connectedLobbyID);
            if (_instance != null) NetworkManager.InstanceUpdate -= _instance.Update;
            if (_instance != null) NetworkManager.InstanceDispose -= Dispose;
        }

    }
}
