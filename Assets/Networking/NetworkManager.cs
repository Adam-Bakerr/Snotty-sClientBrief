using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Utils;
using Assets;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using Riptide.Transports.Steam;
using Steamworks;
using TMPro;

#nullable enable

[DisallowMultipleComponent]
public class NetworkManager : MonoBehaviour
{
    public const byte networkHandlerId = 255;
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

    //Used When The Server List Is REceived
    public delegate void ServerListReceived(List<LobbyInfomation> lobbies);
    public static ServerListReceived OnServerListUpdate;

    //Currently Serialized However This Will Be Controlled From The Menu
    [SerializeField] private bool _isHost = false;
    [SerializeField] private TMP_InputField passwordField;

    //Depending on whether we are a client or a client-server, we may have one or both of these
    public ServerInstance _serverInstance;
    public ClientInstance _clientInstance;
    public PlayerManager _playerManager;
    public SteamManager _steamManager;


    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEnter;

    Callback<LobbyMatchList_t> m_LobbyMatchList;


    //Getter Functions
    public static Client? GetClient() => instance?._clientInstance?._instance;
    public static ClientInstance? GetClientInstance() => instance?._clientInstance;
    public static Riptide.Transports.Steam.SteamClient? GetSteamClient() => GetClientInstance()?._steamClientInstance;
    public static Server? GetServer() => instance?._serverInstance?._instance;
    public static ServerInstance? GetServerInstance() => instance?._serverInstance;
    public static SteamServer? GetSteamServer() => GetServerInstance()?._steamServerInstance;
    public bool getIsHost() => _isHost;


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

            _clientInstance = transform.AddComponent<ClientInstance>();
            _clientInstance.OnClientInitalize();

            m_LobbyMatchList = Callback<LobbyMatchList_t>.Create(ListAvalibleLobbies);
            lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnterCallback);
            gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        }
    }


    public static void GetAvalibleLobbies()
    {
        SteamMatchmaking.AddRequestLobbyListStringFilter("gameKey", "snotty's subway", ELobbyComparison.k_ELobbyComparisonEqual);
        SteamAPICall_t hSteamAPICall = SteamMatchmaking.RequestLobbyList();
    }

    public struct LobbyInfomation {
        public CSteamID id;
        public string password;
        public string? name;
        public string? nolevel;
        public int numberOfMembers;
        public int maxNumberOfMembers;
    }


    private static void ListAvalibleLobbies(LobbyMatchList_t list)
    {
        var outlist = new List<LobbyInfomation>();
        uint numLobbies = list.m_nLobbiesMatching;
        if (numLobbies <= 0)
        {
            Debug.Log("No lobbies");
        }
        else
        {
            for (var i = 0; i < numLobbies; i++)
            {
                var lobby = SteamMatchmaking.GetLobbyByIndex(i);
                var name = SteamMatchmaking.GetLobbyData(lobby, "name");
                var serverPassword = SteamMatchmaking.GetLobbyData(lobby, "password");
                var nolevel = SteamMatchmaking.GetLobbyData(lobby, "nolevel");
                var nplayers = SteamMatchmaking.GetNumLobbyMembers(lobby);
                var maxplayers = SteamMatchmaking.GetLobbyMemberLimit(lobby);

                if (name.Length == 0)
                    continue;

                outlist.Add(new LobbyInfomation() {
                    id = lobby,
                    name = name,
                    password = serverPassword,
                    nolevel = nolevel,
                    numberOfMembers = nplayers,
                    maxNumberOfMembers = maxplayers
                }); ;
            }
        }

        OnServerListUpdate?.Invoke(outlist);
    }

    private void Awake()
    {
        CreateSingltonInstance();
    }

    public void CreateLobby()
    {
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4);
    }

    void OnLobbyCreated(LobbyCreated_t callback)
    {
        if(callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.Log("Failed To Create Lobby");
            return;
        }

        lobbyID = new CSteamID(callback.m_ulSteamIDLobby);

        SteamMatchmaking.SetLobbyData(lobbyID, "name", SteamFriends.GetPersonaName());
        SteamMatchmaking.SetLobbyData(lobbyID, "gameKey", "snotty's subway");
        SteamMatchmaking.SetLobbyData(lobbyID, "password", passwordField.text);


        GetSteamClient()!.ChangeLocalServer(GetSteamServer());
        _clientInstance.ConnectToLocalServer();

        OnLobbyCreation?.Invoke(callback);
    }

    private void OnLobbyEnterCallback(LobbyEnter_t callback)
    {
        if (_isHost) return;

        CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        CSteamID hostId = SteamMatchmaking.GetLobbyOwner(lobbyId);

        _clientInstance.ConnectToHostID(hostId.ToString());
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        CSteamID hostId = SteamMatchmaking.GetLobbyOwner(callback.m_steamIDLobby);
        _clientInstance.ConnectToHostID(hostId.ToString());
    }

    /// <summary>
    /// Called when transitioning from the menu to the lobby
    /// </summary>
    public void CreateNetwork()
    {
        DontDestroyOnLoad(this);
        Debug.developerConsoleEnabled = true;

        //Initalize the server if no other server exists and we are the host
        if (_serverInstance == null)
        {
            _serverInstance = transform.AddComponent<ServerInstance>();
            _serverInstance.OnServerInitalize();
        }
    }

    public void SetHost(bool isHost) => _isHost = isHost;

    /// <summary>
    /// Update Client And Server Instances
    /// </summary>
    void FixedUpdate()
    {
        if(_clientInstance != null) InstanceUpdate();
        SteamAPI.RunCallbacks();
    }

    /// <summary>
    /// Dispose Of Client And Server Instances
    /// </summary>
    private void OnApplicationQuit()
    {
        SteamMatchmaking.LeaveLobby(lobbyID);
        GetSteamServer()?.Shutdown();
        GetServer()?.Stop();
        GetClient()?.Disconnect();
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
    [MessageHandler((ushort)MessageHelper.messageTypes.ClientList, NetworkManager.networkHandlerId)]
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
