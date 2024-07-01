using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSelectionButtons : MonoBehaviour
{
    public void SetHost(bool isHost)
    {
        NetworkManager.instance.SetHost(isHost);
    }

    public void Host()
    {
        SetHost(true);
        NetworkManager.instance.CreateNetwork();
        NetworkManager.instance.CreateLobby();
    }
}
