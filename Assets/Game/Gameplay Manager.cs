using Assets;
using Riptide;
using Riptide.Utils;
using Steamworks;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    //Gameplay specific delegate events
    public delegate void GameEventDelegate();
    public static GameEventDelegate SendClientToMenu;

    //True means game won false means game failed
    public delegate void GameCompletedDelegate(bool value);
    public static GameCompletedDelegate GameCompleted;
    public static GameCompletedDelegate GameFailed;

    //Local Counter For Players In The Exit Zone
    private static int playersInExitZone = 0;

    //Reference to the games pause menu
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject gameOverMenu;
    [SerializeField] GameObject gameFailedMenu;
    [SerializeField] GameObject ui;


    void Start()
    {
        SendClientToMenu += ReturnToMenu;
        GameCompleted += GameWin;
        GameFailed += GameFail;
    }

    #region Scene Transitioning Code

    private void OnLevelWasLoaded(int level)
    {
        AddLocalPlayer();
        AddPlayersFromClientList();
        PlayerInputManager.OnGamePaused += TogglePause;
        if (NetworkManager.instance.getIsHost()) ServerInstance.SendClientList();
    }


    public void TogglePause(bool isPaused)
    {
        pauseMenu.SetActive(isPaused);
        ui.SetActive(!isPaused);
    }

    public void InvokeGamePause(bool isPaused)
    {
        PlayerInputManager.isPaused = isPaused;
        PlayerInputManager.OnGamePaused?.Invoke(isPaused);
    }

    public void ReturnToMenu()
    {
        PlayerInputManager.OnGamePaused -= TogglePause;
        SendClientToMenu -= ReturnToMenu;
        StartCoroutine(LoadGameSceneAsync());
    }

    IEnumerator LoadGameSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Menu");
        asyncLoad.allowSceneActivation = false;

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {

            if (asyncLoad.progress >= 0.9f)
            {
                NetworkManager.instance.ShutdownServer();
                asyncLoad.allowSceneActivation = true;

                break;
            }

            yield return null;
        }

        //Return To Menu Here

    }

    #endregion

    #region Player Loading Into Level Code

    void AddLocalPlayer()
    {
        var mainCameraFollowScript = Camera.main.GetComponent<FollowTarget>();
        var playerPrefab = Resources.Load<GameObject>("Prefabs/Player");
        var player = Instantiate(playerPrefab, new Vector3(0, 15, 0), Quaternion.identity);
        PlayerManager._players.Add(NetworkManager.GetClient().Id,player);
        PlayerManager._downablePlayers.Add(player,player.GetComponent<Downable>());
        var playerCameraPivot = player.transform.GetChild(0);
        mainCameraFollowScript.targetTransform = playerCameraPivot;
    }

    void AddPlayersFromClientList()
    {
        foreach (var player in PlayerManager.playerIds)
        {
            if (player.Key == NetworkManager.GetClient().Id) continue;
            if (!PlayerManager._players.ContainsKey(player.Key))
            {
                //Instantiate Player
                var playerPrefab = Resources.Load<GameObject>("Prefabs/NetworkPlayer");
                var networkPlayer = Instantiate(playerPrefab, new Vector3(0, 15, 0), Quaternion.identity);

                PlayerManager._players.Add(player.Key, networkPlayer);
                PlayerManager._downablePlayers.Add(networkPlayer, networkPlayer.GetComponent<Downable>());
            }
        }
    }

    #endregion

    #region Trigger Zone Code

    public void SendTriggerMessage(int didEnter)
    {
        Message message = MessageHelper.CreateMessage(MessageHelper.messageTypes.ExitZoneTriggered, MessageSendMode.Reliable);
        message.Add(didEnter);
        Debug.Log(didEnter);
        NetworkManager.GetClient().Send(message);
    }

    //Middle man for when a client picks up an item, reroutes it to all other players aside the sender
    [MessageHandler((ushort)MessageHelper.messageTypes.ExitZoneTriggered, NetworkManager.networkHandlerId)]
    public static void OnPlayerEnteredTriggerZone(ushort clientid, Message message)
    {
        //Connection Message
        NetworkManager.GetServer().SendToAll(message);
    }

    [MessageHandler((ushort)MessageHelper.messageTypes.ExitZoneTriggered, NetworkManager.networkHandlerId)]
    private static void OnOtherPlayerEnteredTriggerZone(Message message)
    {
        playersInExitZone += message.GetInt();

        if (playersInExitZone >= PlayerManager.playerIds.Count)
        {
            //Send Game Over Message To Server For Validation
            Message gameOverMessage = MessageHelper.CreateMessage(MessageHelper.messageTypes.GameCompleted, MessageSendMode.Reliable);
            gameOverMessage.Add(true);
            NetworkManager.GetClient().Send(gameOverMessage);
        }

    }

    #endregion

    #region GameOverCode

    //Middle man for when a client picks up an item, reroutes it to all other players aside the sender
    [MessageHandler((ushort)MessageHelper.messageTypes.GameCompleted, NetworkManager.networkHandlerId)]
    public static void OnServerReceivedGameOverFromClient(ushort clientid, Message message)
    {
        //Connection Message
        NetworkManager.GetServer().SendToAll(message);
    }

    [MessageHandler((ushort)MessageHelper.messageTypes.GameCompleted, NetworkManager.networkHandlerId)]
    private static void OnServerConfirmedGameOver(Message message)
    {
        bool isWin = message.GetBool();
        if(isWin)
        {
            GameCompleted?.Invoke(true);
        }
        else
        {
            GameFailed?.Invoke(true);
        }

    }

    public void GameWin(bool isWin)
    {
        GameCompleted -= GameWin;
        ui.SetActive(false);
        gameOverMenu.SetActive(true);

        PlayerInputManager.isPaused = true;
        PlayerInputManager.isGameOver = true;
    }

    public void GameFail(bool isWin)
    {
        GameFailed -= GameFail;
        ui.SetActive(false);
        gameFailedMenu.SetActive(true);
        PlayerInputManager.isPaused = true;
        PlayerInputManager.isGameOver = true;
    }

    #endregion


}
