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
        ClearButtons();
        RefreshServerList();
    }

    private void Awake()
    {
        ClearButtons();
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
        ClearButtons();
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
            int playerCount = 1;
            go.GetComponentInChildren<TextMeshProUGUI>().text = $"{lobbyInfo.name}'s Lobby Players {playerCount}/4";
            go.GetComponent<Button>().onClick.AddListener(() => OnButtonClick(lobbyInfo));
            buttonPairs.Add(go,lobbyInfo);
        }
    }

    public void ClearButtons() {
        foreach(var button in buttonPairs)
        {
            Destroy(button.Key);
        }

        buttonPairs.Clear();
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
        NetworkManager.GetClientInstance().ConnectToHostID(hostId);
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
        NetworkManager.GetClientInstance().ConnectToHostID(hostId);
        currentLobbyInfomation = null;
        gameObject.SetActive(false);
        
    }
}
