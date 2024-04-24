using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Utils;
using Assets;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;

#nullable enable

[DisallowMultipleComponent]
public class NetworkManager : MonoBehaviour
{
    //Singleton instance of the network manager
    public static NetworkManager instance { get; private set; }

    //Used To Update The Instances Together depending on which is present
    public delegate void InstanceUpdateDelegate();
    public static InstanceUpdateDelegate InstanceUpdate;

    //Used To Dispose The Instances Together depending on which is present
    public delegate void InstanceDisposeDelegate();
    public static InstanceDisposeDelegate InstanceDispose;

    //Used When The Server Sendw The Client List
    public delegate void ClientListDelegate(Dictionary<ushort,string> clientList);
    public static ClientListDelegate OnClientListReceive;

    //Currently Serialized However This Will Be Controlled From The Menu
    [SerializeField] private bool _isHost = false;
    public bool getIsHost() => _isHost;

    //Depending on whether we are a client or a client-server, we may have one or both of these
    public ServerInstance _serverInstance;
    public ClientInstance _clientInstance;
    public PlayerManager _playerManager;



    /// <summary>
    /// Creates a singleton instance of the network manager ensuring only a single instance is present
    /// </summary>
    private void CreateSingltonInstance()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            //Initalize the singleton instance
            instance = this;

            _playerManager = transform.AddComponent<PlayerManager>(); 
            
            
            _clientInstance = transform.AddComponent<ClientInstance>();
            _clientInstance.OnClientInitalize();

            //Initalize the server if no other server exists and we are the host
            if (_serverInstance == null && _isHost)
            {
                _serverInstance = transform.AddComponent<ServerInstance>();
                _serverInstance.OnServerInitalize();
                _clientInstance.ConnectToServer("127.0.0.1", _serverInstance.GetPort().ToString(),_clientInstance.name); ;
            }




        }
    }

    public static Client? GetClient()
    {
        return NetworkManager.instance!._clientInstance!._instance;
    }

    
    public static ClientInstance? GetClientInstance()
    {
        return NetworkManager.instance!._clientInstance;
    }

    public static Server? GetServer()
    {
        return ServerInstance._instance;
    }


    /// <summary>
    /// Called when transitioning from the menu to the lobby
    /// </summary>
    public void CreateNetwork()
    {
        CreateSingltonInstance();
        DontDestroyOnLoad(this);
        Debug.developerConsoleEnabled = true;
    }

    /// <summary>
    /// Sets The Host Variable Of The Network Manager
    /// </summary>
    public void SetHost(bool isHost)
    {
        _isHost = isHost;
    }

    /// <summary>
    /// Update Client And Server Instances
    /// </summary>
    void FixedUpdate()
    {
        if(_clientInstance != null) InstanceUpdate();
    }

    /// <summary>
    /// Dispose Of Client And Server Instances
    /// </summary>
    private void OnApplicationQuit()
    {
        InstanceDispose?.Invoke();
    }

    /// <summary>
    /// Receive the clientlist from the server
    /// </summary>
    [MessageHandler((ushort)MessageHelper.messageTypes.ClientList)]
    private static void ClientList(Message message)
    {
        Dictionary<ushort, string> clients = new();
        int clientCount = message.GetInt();
        for(int i = 0; i < clientCount; i++)
        {
            ushort id = message.GetUShort();
            string name = message.GetString();

            clients.Add(id, name);
        }
        NetworkManager.OnClientListReceive?.Invoke(clients);

    }

}
