using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] List<GameObject> players = new(); 
    [SerializeField] List<TextMeshProUGUI> playerTexts = new(); 

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.OnClientListReceive += OnClientListUpdate;
    }

    void OnClientListUpdate(Dictionary<ushort,string> ClientList)
    {
        foreach(var player in players) { player.SetActive(false); }

        var names = ClientList.ToList();
        for(int i = 0; i < ClientList.Count; i++)
        {
            players[i].SetActive(true);
            playerTexts[i].text = names[i].Value;
        }
    }
}
