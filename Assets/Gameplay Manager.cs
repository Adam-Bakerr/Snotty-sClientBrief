using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        AddLocalPlayer();
    }

    void AddLocalPlayer()
    {
        var mainCameraFollowScript = Camera.main.GetComponent<FollowTarget>();
        var playerPrefab = Resources.Load<GameObject>("Prefabs/Player");
        var player = Instantiate(playerPrefab, new Vector3(0,15,0), Quaternion.identity);
        var playerCameraPivot = player.transform.GetChild(0);
        mainCameraFollowScript.targetTransform = playerCameraPivot;
    }

    void AddNetworkPlayers(ushort id)
    {
        var playerPrefab = Resources.Load<GameObject>("Prefabs/NetworkPlayer");
        var networkPlayer = Instantiate(playerPrefab, new Vector3(0, 15, 0), Quaternion.identity);
        networkPlayer.GetComponent<NetworkPlayer>().SetClientID(id);
    }
}
