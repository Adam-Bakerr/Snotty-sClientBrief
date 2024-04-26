using Riptide;
using Riptide.Transports.Steam;
using Riptide.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets
{
    [System.Serializable]
    [DisallowMultipleComponent]
    public class ClientInstance : MonoBehaviour
    {
        public Client _instance { get; private set; }
        public SteamClient _steamClientInstance {  get; private set; }

        private string _name, _connectedIp = "",_connectedPort = "";
        public string Name() => _name;
        public string ConnectedIP() => _connectedIp;
        public string ConnectedPort() => _connectedPort;


        /// <summary>
        /// Takes An Ip Address In The Form Of "127.0.0.1:7777" As A String
        /// </summary>
        public void ConnectToServer(string name)
        {
            if(_instance == null) { Debug.LogError("Instance For Client Is Missing"); }

            _name = name;

            _instance.Connect("127.0.0.1",messageHandlerGroupId: NetworkManager.instance.networkHandlerId);
            Debug.Log(_instance.IsConnecting);


            //Subrscribe the client to notify the player manager of any connections
            _instance.Connected += NetworkManager.instance._playerManager.LocalPlayerConnected;
            _instance.Disconnected += NetworkManager.ResetInstance;
            _instance.ClientConnected += NetworkManager.instance._playerManager.ClientConnected;
            _instance.ClientDisconnected += NetworkManager.instance._playerManager.ClientDisconnected;
        }

        public void OnClientInitalize()
        {
            if (NetworkManager.instance == null) throw new System.Exception("Missing Server Instance");

            _steamClientInstance = new SteamClient(NetworkManager.instance._steamServerInstance);

            //Start the Client
            _instance = new Client(_steamClientInstance);

            //Subscribe the client to the update loop
            NetworkManager.InstanceUpdate += _instance.Update;

            //Subscribe the client to be disposed on shutdown
            NetworkManager.InstanceDispose += Dispose;
        }

        /// <summary>
        /// Dispose Of The Network Before We Close The Session
        /// </summary>
        private void Dispose()
        {
            if (_instance != null) NetworkManager.InstanceUpdate -= _instance.Update;
            if (_instance != null) NetworkManager.InstanceDispose -= Dispose;
            _instance?.Disconnect();
        
        
        }

        private void Update()
        {
            Debug.Log("Is Connected = "+_instance.IsConnected);
        }

    }
}
