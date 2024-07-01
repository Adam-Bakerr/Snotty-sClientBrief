using Assets;
using Riptide;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using TMPro;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class CollectableManager : MonoBehaviour
{
    public static CollectableManager Instance;

    [SerializeField] TextMeshProUGUI score;

    [SerializeField] int collectibleCount = 10;
    [SerializeField] int totalCollectablesCollected = 0; 
    [SerializeField] public static int collectiblePickupRange = 10;
    [SerializeField] public List<Transform> collectableLocations = new List<Transform>();
    [SerializeField] public Dictionary<int,GameObject> collectables = new();

    //Called when all the players die or all collectables are collected
    public delegate void OnGameStateChange();
    public static OnGameStateChange OnGameWin;
    public UnityEvent OnGameWinEvent;
    public static OnGameStateChange OnGameFail;

    // Start is called before the first frame update
    void Start()
    {
        CreateSingletonInstance();

        //Check if steam is initalized, this will always be true unless starting the scene in editor directly
        if (NetworkManager.GetServer() == null) return;

        //Tell All Clients To Spawn Objects
        if (NetworkManager.instance.getIsHost())
        {
            //Create A Message To Send To The Clients
            Message message = MessageHelper.CreateMessage(MessageHelper.messageTypes.SpawnCollectables, MessageSendMode.Reliable);
            var collectablesToSpawnOnClients = GetCollectablesToSpawn();
            message.Add(collectablesToSpawnOnClients.Count);

            foreach(var collectable in collectablesToSpawnOnClients)
            {
                message.Add(collectable.Item1);
                message.Add(collectable.Item2);
            }

            NetworkManager.GetServer().SendToAll(message);
        }
        
    }

    private void CreateSingletonInstance()
    {
        if (CollectableManager.Instance == null)
        {
            CollectableManager.Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }
    }

    public void OnCollectableCollected(int objectGuid, ushort collector)
    {
        if (collectables.ContainsKey(objectGuid))
        {
            Destroy(collectables[objectGuid]);
            collectables.Remove(objectGuid);

            totalCollectablesCollected++;
            score.text = $"Current Score: {totalCollectablesCollected}/{collectibleCount}";

            if(totalCollectablesCollected >= collectibleCount)
            {
                OnGameWin?.Invoke();
                OnGameWinEvent?.Invoke();
            }

            //Create Object Collection Message
            Message message = MessageHelper.CreateMessage(MessageHelper.messageTypes.CollectablePickedUp, MessageSendMode.Reliable);
            message.AddInt(objectGuid); message.Add(collector);
            NetworkManager.GetClient().Send(message);
        }
    }

    public void AddLocation(Transform location)
    {
        collectableLocations.Add(location);
        for (int i = 0; i < collectableLocations.Count; i++)
        {
            if (collectableLocations[i].IsUnityNull()) collectableLocations.RemoveAt(i);
        }
    }

    //Function Returns A List Of Unique Elements In A List
    static List<T> GetRandomElements<T>(List<T> list, int count)
    {
        list.RemoveAll(item => item.IsUnityNull());

        if (count >= list.Count)
        {
            return list;
        }

        List<T> result = new List<T>();

        while (result.Count < count && list.Count != 0)
        {
            T element = list[Random.Range(0,list.Count)]; // Get a random element from the list
            if (!result.Contains(element))
            {
                result.Add(element);
                list.Remove(element);
            }
        }

        return result;
    }

    [MessageHandler((ushort)MessageHelper.messageTypes.SpawnCollectables, NetworkManager.networkHandlerId)]
    private static void SpawnCollectables(Message message)
    {
        int collectableCount = message.GetInt();
        var ListOfAllPrefabs = Resources.LoadAll<GameObject>("Prefabs/Collectables/").ToList();
        for (int i = 0; i < collectableCount; i++)
        {
            Player.SyncableTransform spawnLocation = message.GetSerializable<Player.SyncableTransform>();
            int objectID = message.GetInt();
            var prefabToSpawn = ListOfAllPrefabs[objectID];
            var spawnedCollectable = Instantiate(prefabToSpawn, spawnLocation.Position, Quaternion.Euler(spawnLocation.EulerAngles));
            spawnedCollectable.name = "Collctable " + i;
            spawnedCollectable.GetComponent<Collectible>().GUID = i;
            CollectableManager.Instance.collectables.Add(i, spawnedCollectable);
        }
    }


    //Middle man for when a client picks up an item, reroutes it to all other players aside the sender
    [MessageHandler((ushort)MessageHelper.messageTypes.CollectablePickedUp, NetworkManager.networkHandlerId)]
    public static void OnPlayerCollectedItem(ushort clientid, Message message)
    {
        //Connection Message
        NetworkManager.GetServer().SendToAll(message, clientid);
    }

    [MessageHandler((ushort)MessageHelper.messageTypes.CollectablePickedUp, NetworkManager.networkHandlerId)]
    private static void OnCollectablePickup(Message message)
    {
        int collectableIndex = message.GetInt();
        if (CollectableManager.Instance.collectables.ContainsKey(collectableIndex)){
            Destroy(CollectableManager.Instance.collectables[collectableIndex]);
            CollectableManager.Instance.collectables.Remove(collectableIndex);
            CollectableManager.Instance.totalCollectablesCollected++;
            CollectableManager.Instance.score.text = $"Current Score: {CollectableManager.Instance.totalCollectablesCollected}/{CollectableManager.Instance.collectibleCount}";

            if (CollectableManager.Instance.totalCollectablesCollected >= CollectableManager.Instance.collectibleCount)
            {
                OnGameWin?.Invoke();
                CollectableManager.Instance.OnGameWinEvent?.Invoke();
            }

        }
    }


    //Gets a list of spawn position and object GUIDs that each client should spawn
    List<(Player.SyncableTransform, int)> GetCollectablesToSpawn()
    {

        var randomSpawnLocations = GetRandomElements(CollectableManager.Instance.collectableLocations, CollectableManager.Instance.collectibleCount);

        var ListOfAllPrefabs = Resources.LoadAll<GameObject>("Prefabs/Collectables/").ToList();
        
        var randomCollectablePrefabs = GetRandomElements(ListOfAllPrefabs, CollectableManager.Instance.collectibleCount);
        var spawnedCollectables = new List<(Player.SyncableTransform, int)>();

        collectibleCount = Mathf.Min(randomSpawnLocations.Count, randomCollectablePrefabs.Count);

        for (int i = 0; i < Mathf.Min(randomSpawnLocations.Count, randomCollectablePrefabs.Count); i++)
        {
            Player.SyncableTransform transform = new(randomSpawnLocations[i]);
            spawnedCollectables.Add((transform, i));
        }

        return spawnedCollectables;
    }


    #if UNITY_EDITOR
    [MenuItem("GameObject/Create Collectible Spawn Infront Of Camera")]
    static void New()
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            return;
        }

        var sceneViewCameraTransform = SceneView.lastActiveSceneView.camera.transform;
        var posToSpawn = sceneViewCameraTransform.position + sceneViewCameraTransform.forward;
        var go = new GameObject();
        Selection.activeTransform = go.transform;
        go.transform.position = posToSpawn;
        go.transform.rotation = Quaternion.identity;


        var collectableManagerInstance = GameObject.FindObjectOfType<CollectableManager>();

        if (collectableManagerInstance == null)
        {
            var CollectableManager = new GameObject();
            CollectableManager.name = "Collectable Manager Instance";
            collectableManagerInstance = CollectableManager.AddComponent<CollectableManager>();
        }

        go.transform.parent = collectableManagerInstance.transform;
        go.name = "Collectable Location";
        collectableManagerInstance.AddLocation(go.transform);
    }
    #endif
}
