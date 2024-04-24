using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;

public class NetworkButtonRouter : MonoBehaviour
{
    [SerializeField] TMP_InputField NAME;
    [SerializeField] TMP_InputField IP;
    [SerializeField] TMP_InputField PORT;
    [SerializeField] GameObject NetworkConnectionMenu;
    [SerializeField] GameObject ConnectingMenu;

    public void OnJoinPressed()
    {
        if (IP.text.Length <= 1 || PORT.text.Length <= 1 || NAME.text.Length <= 1 && IPAddress.TryParse(IP.text,out IPAddress voidOutput)) return;

        NetworkManager.GetClientInstance().ConnectToServer(IP.text, PORT.text, NAME.text);
        NetworkConnectionMenu.SetActive(false);
        ConnectingMenu.SetActive(true);
    }
}
