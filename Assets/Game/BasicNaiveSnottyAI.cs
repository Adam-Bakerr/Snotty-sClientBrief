using Assets;
using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicNaiveSnottyAI : MonoBehaviour
{
    //The active transform being tracked by the nav mesh agent
    (float distance, Transform playerTransform) target;

    //Snottys nav mesh instance
    [SerializeField] NavMeshAgent agentInstace;

    //The Delay for the current navmesh agent before it attempts to find a new target
    [SerializeField] float TargetRecalcualationDelay = 1.0f;
                     float CurrentTargetRecalcualationDelay = 1.0f;

    [SerializeField] float TargetUpdateDelay = 0.1f;
                     float CurrentTargetUpdateDelay = 0;

    [SerializeField] float ReachDistance = 2f;

    static Transform thisTransform;

    private void Awake()
    {
        if (thisTransform == null) thisTransform = transform;
    }




    void FixedUpdate()
    {
        //Only the host needs to simulate the AI
        if (!NetworkManager.instance) return;
        if (!NetworkManager.instance.getIsHost()) return;

        //Only need if we want to pause on pause 
        /*if (PlayerInputManager.isPaused)
        {
            agentInstace.isStopped = true;
            return;
        }

        agentInstace.isStopped = false;*/

        //Update The Timers
        CurrentTargetRecalcualationDelay += Time.fixedDeltaTime;
        CurrentTargetUpdateDelay += Time.fixedDeltaTime;

        //If we should recalculate the closest player then do so
        //This is limited to perserve performance
        if (CurrentTargetRecalcualationDelay >= TargetRecalcualationDelay)
        {
            CurrentTargetRecalcualationDelay = 0;
            CalculateClosestPlayer();
        }

        //Set the agents destination
        if(target.playerTransform != null && CurrentTargetUpdateDelay >= TargetUpdateDelay)
        {
            CurrentTargetUpdateDelay = 0;
            agentInstace.isStopped = false;
            agentInstace.SetDestination(target.playerTransform.position);
        }else
        if(target.playerTransform == null)
        {
            agentInstace.isStopped = true;
        }
      
        //Tell ALl Other Players About Snottys new position
        Message syncSnottyMessage = MessageHelper.CreateMessage(MessageHelper.messageTypes.SnottyPositionUpdate, MessageSendMode.Unreliable);
        syncSnottyMessage.Add(transform.position);
        NetworkManager.GetServer().SendToAll(syncSnottyMessage,NetworkManager.GetClient().Id);
    }

    //TODO SET UP MIDDLE MAN FOR SNOTTY POSITION

    void CalculateClosestPlayer()
    {
        //Keep track of the closest player
        (float distance, Transform playerTransform) closestPlayer = (
            distance: float.MaxValue,
            playerTransform: null
        );

        //Itterate all players and check their distanc to the ai
        foreach (GameObject t in PlayerManager._downablePlayers.Keys)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, t.transform.position);
            if(!PlayerManager._downablePlayers.TryGetValue(t, out Downable downablePlayer))continue;
            if (distanceToPlayer <= closestPlayer.distance && !downablePlayer.isDown)
            {
                //Update the target if the player is closer than the last
                closestPlayer = (
                                distance: distanceToPlayer,
                                playerTransform: t.transform
                            );
            }
        }

        //If The closest player is really far away or not found dont path find anymore
        //This usually occurs when the player being tracked has gone down
        if (closestPlayer.distance > float.MaxValue / 2.0f) closestPlayer.playerTransform = null; ;

        //Define the target as either null or the closest
        target = closestPlayer;


    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Players"))
        {
            if (!PlayerManager._downablePlayers.ContainsKey(other.gameObject)) return;
            OnPlayerReached(PlayerManager._downablePlayers[other.gameObject]);
        }
    }

    void OnPlayerReached(Downable playerToDown)
    {
        playerToDown.Down();
    }

    [MessageHandler((ushort)MessageHelper.messageTypes.SnottyPositionUpdate, NetworkManager.networkHandlerId)]
    public static void OnPlayerSyncablePacketReceived(Message message)
    {
        thisTransform.position = message.GetVector3();
    }
}
