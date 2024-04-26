using Riptide;
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

        private string _name, _connectedIp = "",_connectedPort = "";
        public string Name() => _name;
        public string ConnectedIP() => _connectedIp;
        public string ConnectedPort() => _connectedPort;


        /// <summary>
        /// Takes An Ip Address In The Form Of "127.0.0.1:7777" As A String
        /// </summary>
        public void ConnectToServer(string ServerIpAddress, string port, string name)
        {
            if(_instance == null) { Debug.LogError("Instance For Client Is Missing"); }
            if(ServerIpAddress == "") { Debug.LogError("Empty Ip Provided"); }


            _connectedIp = ServerIpAddress;
            _connectedPort = port;
            _name = name;

            _instance.Connect($"{ServerIpAddress}:{port}");
            var test = _instance.IsConnecting;


            //Subrscribe the client to notify the player manager of any connections
            _instance.Connected += NetworkManager.instance._playerManager.LocalPlayerConnected;
            _instance.Disconnected += NetworkManager.ResetInstance;
            _instance.ClientConnected += NetworkManager.instance._playerManager.ClientConnected;
            _instance.ClientDisconnected += NetworkManager.instance._playerManager.ClientDisconnected;
        }

        public void OnClientInitalize()
        {
            if (NetworkManager.instance == null) throw new System.Exception("Missing Server Instance");

            //Start the Client
            _instance = new Client();

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

    }
}
