using Assets;
using Steamworks;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    // Start is called before the first frame update
    void Awake()
    {
        
    }

    private void OnLevelWasLoaded(int level)
    {
        AddLocalPlayer();
        AddPlayersFromClientList();
        PlayerInputManager.OnGamePaused += TogglePause;
        if (NetworkManager.instance.getIsHost()) ServerInstance.SendClientList();
    }


    void TogglePause(bool isPaused)
    {
        pauseMenu.SetActive(isPaused);
    }

    public void ReturnToMenu()
    {
        PlayerInputManager.OnGamePaused -= TogglePause;
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

    void AddLocalPlayer()
    {
        var mainCameraFollowScript = Camera.main.GetComponent<FollowTarget>();
        var playerPrefab = Resources.Load<GameObject>("Prefabs/Player");
        var player = Instantiate(playerPrefab, new Vector3(0,15,0), Quaternion.identity);
        var playerCameraPivot = player.transform.GetChild(0);
        mainCameraFollowScript.targetTransform = playerCameraPivot;
    }

    void AddPlayersFromClientList()
    {
        foreach(var player in PlayerManager.playerIds)
        {
            if (player.Key == NetworkManager.GetClient().Id) continue;
            if (!PlayerManager._players.ContainsKey(player.Key))
            {
                //Instantiate Player
                var playerPrefab = Resources.Load<GameObject>("Prefabs/NetworkPlayer");
                var networkPlayer = Instantiate(playerPrefab, new Vector3(0, 15, 0), Quaternion.identity);

                PlayerManager._players.Add(player.Key, networkPlayer);
            }
        }
    }

}
