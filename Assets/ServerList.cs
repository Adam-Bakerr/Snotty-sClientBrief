using Assets;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerList : MonoBehaviour
{

    [SerializeField] GameObject Content;
    [SerializeField] GameObject ButtonPrefab;
    [SerializeField] GameObject PasswordPane;
    [SerializeField] TMP_InputField PasswordInputField;

    NetworkManager.LobbyInfomation? currentLobbyInfomation;

    Dictionary<GameObject, NetworkManager.LobbyInfomation> buttonPairs = new();

    private void OnEnable()
    {
        RefreshServerList();
    }

    private void Awake()
    {
        NetworkManager.GetAvalibleLobbies();
    }

    private void OnDestroy()
    {
        NetworkManager.OnServerListUpdate -= ServerListUpdated;
    }

    private void Start()
    {
        NetworkManager.OnServerListUpdate += ServerListUpdated;
    }

    public void RefreshServerList()
    {
        NetworkManager.GetAvalibleLobbies();
    }

    public void ServerListUpdated(List<NetworkManager.LobbyInfomation> lobbyInfomations)
    {
        var keys = buttonPairs.Keys.ToArray();
        for (int i = 0; i < keys.Length; i++) Destroy(keys[i]);
        buttonPairs.Clear();

        foreach(var lobbyInfo in lobbyInfomations)
        {
            var go = Instantiate(ButtonPrefab, Content.transform);
            go.GetComponentInChildren<TextMeshProUGUI>().text = lobbyInfo.name;
            go.GetComponent<Button>().onClick.AddListener(() => OnButtonClick(lobbyInfo));
            buttonPairs.Add(go,lobbyInfo);
        }
    }

    private void OnButtonClick(NetworkManager.LobbyInfomation lobbyInfo)
    {
        currentLobbyInfomation = lobbyInfo;
        switch (lobbyInfo.password != "")
        {
            case (true):
                PasswordPane.SetActive(true);
                break;
            case (false):
                ConnectNoPassword();
                break;
        }
        
    }

    private void ConnectNoPassword()
    {
        SteamMatchmaking.JoinLobby(currentLobbyInfomation.Value.id);
        CSteamID hostId = SteamMatchmaking.GetLobbyOwner(currentLobbyInfomation.Value.id);
        NetworkManager.GetClientInstance().ConnectToHostID(hostId.ToString());
        gameObject.SetActive(false);
        currentLobbyInfomation = null;
    }

    public void PasswordPaneJoinPressed()
    {
        if(PasswordInputField.text != currentLobbyInfomation.Value.password)
        {
            currentLobbyInfomation = null;
            PasswordPane.SetActive(false);
            return;
        }


        SteamMatchmaking.JoinLobby(currentLobbyInfomation.Value.id);
        CSteamID hostId = SteamMatchmaking.GetLobbyOwner(currentLobbyInfomation.Value.id);
        NetworkManager.GetClientInstance().ConnectToHostID(hostId.ToString());
        currentLobbyInfomation = null;
        gameObject.SetActive(false);
        
    }
}
