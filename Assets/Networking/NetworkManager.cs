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
using UnityEngine.SceneManagement;
using Riptide.Transports.Steam;
using Steamworks;
using Riptide.Demos.Steam.PlayerHosted;

#nullable enable

[DisallowMultipleComponent]
public class NetworkManager : MonoBehaviour
{
    public byte networkHandlerId = 255;
    public CSteamID lobbyID;

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

    //Used When The Lobby is created
    public delegate void LobyCreationDelegate(LobbyCreated_t callback);
    public static LobyCreationDelegate OnLobbyCreation;

    //Used When The Lobby is created
    public delegate void LobyEnterDelegate(LobbyEnter_t callback);
    public static LobyEnterDelegate OnLobbyEnter;

    //Currently Serialized However This Will Be Controlled From The Menu
    [SerializeField] private bool _isHost = false;
    public bool getIsHost() => _isHost;

    //Depending on whether we are a client or a client-server, we may have one or both of these
    public SteamServer _steamServerInstance;
    public ServerInstance _serverInstance;
    public ClientInstance _clientInstance;
    public PlayerManager _playerManager;
    public SteamManager _steamManager;


    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEnter;

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
            _steamManager = transform.AddComponent<SteamManager>();

            if (!SteamManager.Initialized)
            {
                Debug.LogError("Steam is not initialized!");
                return;
            }

            _steamServerInstance = new SteamServer();

            //Initalize the server if no other server exists and we are the host
            if (_serverInstance == null && _isHost)
            {
                _serverInstance = transform.AddComponent<ServerInstance>();
                _serverInstance.OnServerInitalize();

                lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
                SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly,4);

                //_clientInstance.ConnectToServer("127.0.0.1", _serverInstance.GetPort().ToString(),_clientInstance.name); ;
            }

            _clientInstance = transform.AddComponent<ClientInstance>();
            _clientInstance.OnClientInitalize();

            lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnterCallback);

        }
    }

    void OnLobbyCreated(LobbyCreated_t callback)
    {
        if(callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.Log("Failed To Create Lobby");
            return;
        }

        lobbyID = new CSteamID(callback.m_ulSteamIDLobby);

        _clientInstance.ConnectToServer("Host");

        OnLobbyCreation?.Invoke(callback);
    }

    private void OnLobbyEnterCallback(LobbyEnter_t callback)
    {
        if (_isHost) return;

        CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        CSteamID hostId = SteamMatchmaking.GetLobbyOwner(lobbyId);

        _clientInstance._instance.Connect(hostId.ToString(), messageHandlerGroupId: NetworkManager.instance.networkHandlerId);
        OnLobbyEnter?.Invoke(callback);
        Debug.Log("This is a connection message");
    }

    void JoinLobby(ulong lobbyId)
    {
        SteamMatchmaking.JoinLobby(new CSteamID(lobbyId));
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
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
    /// Resets the game instance back to its inital state
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public static void ResetInstance(object sender, DisconnectedEventArgs e)
    {
        Transform NetworkTransform = NetworkManager.instance.transform;
        Destroy(NetworkTransform.GetComponent<ClientInstance>());
        Destroy(NetworkTransform.GetComponent<ServerInstance>());
        Destroy(NetworkTransform.GetComponent<PlayerManager>());
        SceneManager.LoadScene("Menu");
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
