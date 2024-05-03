using Assets;
using Riptide;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    //Called When All Players Are Ready And The Game Starts
    public delegate void GameStartDelegate();
    public static GameStartDelegate OnGameStart;

    //Lobby Players
    [SerializeField] List<GameObject> players = new(); 
    [SerializeField] List<TextMeshProUGUI> playerTexts = new();
    [SerializeField] TextMeshProUGUI lobbyCodeDisplay;

    //Main Menu Canvas
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject failedConnectionMenu;

    static Dictionary<ushort, TextMeshProUGUI> playersToTextDict = new();

    //Player Ready Status and count in the lobby
    static List<ushort> clientsReady = new();
    bool _localClientReady = false;

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.OnClientListReceive += OnClientListUpdate;
        NetworkManager.OnLobbyCreation += OnLobbyCreation;
        NetworkManager.GetClient().ClientConnected += LobbyManager_ClientConnected;
    }

    private void LobbyManager_ClientConnected(object sender, ClientConnectedEventArgs e)
    {
        NetworkManager.GetClient().ClientConnected -= LobbyManager_ClientConnected;
        SetMenuActive(false);
    }

    public void SetMenuActive(bool shouldHideMenu)
    {
        mainMenu.SetActive(shouldHideMenu);
    }

    public void FailedToConnect()
    {
        failedConnectionMenu.SetActive(true);
    }

    void OnLobbyCreation(LobbyCreated_t callback)
    {
        lobbyCodeDisplay.text = callback.m_ulSteamIDLobby.ToString();
    }

    void OnClientListUpdate(Dictionary<ushort,string> ClientList)
    {
        foreach(var player in players) { player.SetActive(false); }
        PlayerManager.playerIds.Clear();
        playersToTextDict.Clear();

        var names = ClientList.ToList();
        for(int i = 0; i < ClientList.Count; i++)
        {
            players[i].SetActive(true);
            playerTexts[i].text = names[i].Value;
            playersToTextDict.Add(names[i].Key, playerTexts[i]);
            PlayerManager.playerIds.Add(names[i].Key, names[i].Value);
        }
    }

    public void OnReadyPress()
    {
        Message clientReadyMessage = MessageHelper.CreateMessage(MessageHelper.messageTypes.LobbyReady, MessageSendMode.Reliable);
        clientReadyMessage.Add(NetworkManager.GetClient().Id);

        _localClientReady = !_localClientReady;
        clientReadyMessage.Add(_localClientReady);

        NetworkManager.GetClient().Send(clientReadyMessage);
    }


    [MessageHandler((ushort)MessageHelper.messageTypes.LobbyReady, NetworkManager.networkHandlerId)]
    private static void PlayerReadyMessageHandler(Message message)
    {
        ushort clientId = message.GetUShort();
        bool isClientReady = message.GetBool();

        Debug.Log($"Client with id: {clientId} Is {isClientReady}");
        if (isClientReady)
        {
            clientsReady.Add(clientId);
            playersToTextDict[clientId].color = Color.green;
            if (clientsReady.Count == PlayerManager.playerIds.Count)
            {
                Message startGameMessageToServer = MessageHelper.CreateMessage(MessageHelper.messageTypes.GameStart, MessageSendMode.Reliable);
                NetworkManager.GetClient().Send(startGameMessageToServer);
            }

        }
        else
        {
            playersToTextDict[clientId].color = Color.red;
            clientsReady.Remove(clientId);
        }
    }

    [MessageHandler((ushort)MessageHelper.messageTypes.GameStart, NetworkManager.networkHandlerId)]
    private static void GameStartMessageHandler(Message message)
    {
        Debug.Log("Game Starting");

        //Load The Game Scene ASync to prevent clients disconnecting
        NetworkManager.GetClientInstance().StartCoroutine(LobbyManager.LoadGameSceneAsync());
    }

    static IEnumerator LoadGameSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameScene");

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        //Adds a gameplay manager on the transition to the new scene
        NetworkManager.instance.transform.AddComponent<GameplayManager>();

        //Used To Notify Any Components Of The Scene Switch
        OnGameStart?.Invoke();
    }
}
