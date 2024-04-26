using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Connecting : MonoBehaviour
{
    [SerializeField] GameObject FailedToConnectMenu;
    [SerializeField] GameObject LobyMenu;


    [SerializeField] TextMeshProUGUI m_TextMeshProUGUI;
    [SerializeField] float animationTime = .5f;
    [SerializeField] string baseText;

    int dotCount = 3;
    int currentDotCount = 0;
    float currentTime = 0;


    // Start is called before the first frame update
    void Awake()
    {
        NetworkManager.GetClient().ConnectionFailed += Failed;
        NetworkManager.GetClient().Connected += Connected;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        currentTime += Time.fixedDeltaTime;

        if((currentTime % (animationTime * 4)) > (animationTime * 3f))
        {
            m_TextMeshProUGUI.text = $"{baseText}...";
            return;
        }
        if ((currentTime % (animationTime * 4)) > (animationTime * 2f))
        {
            m_TextMeshProUGUI.text = $"{baseText}..";
            return;
        }
        if ((currentTime % (animationTime * 4)) > (animationTime))
        {
            m_TextMeshProUGUI.text = $"{baseText}.";
            return;
        }
        m_TextMeshProUGUI.text = baseText;

    }

    void Failed(object sender, Riptide.ConnectionFailedEventArgs e)
    {
        FailedToConnectMenu.SetActive(true);
    }

    void Connected(object sender, System.EventArgs e)
    {
        gameObject.SetActive(false);
        LobyMenu.SetActive(true);
    }
}
