using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerArea : MonoBehaviour
{
    public UnityEvent TriggerEntered;
    public UnityEvent TriggerExited;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            TriggerEntered?.Invoke();
            Debug.Log("Player Entered Trigger");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            TriggerExited?.Invoke();
            Debug.Log("Player Exited Trigger");
        }
    }
}
