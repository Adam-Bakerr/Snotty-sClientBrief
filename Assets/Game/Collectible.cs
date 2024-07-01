using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Linq;


public class Collectible : MonoBehaviour
{
    public int GUID;

    public void OnTriggerEnter(Collider other)
    {
        OnInteract(other.gameObject);
    }

    public void OnInteract(GameObject player)
    {
        //Find the id of the colliding player
        if (player.layer != LayerMask.NameToLayer("Players")) return;
        CollectableManager.Instance.OnCollectableCollected(GUID,PlayerManager._players.FirstOrDefault(x => x.Value == player).Key);
    }

    #if UNITY_EDITOR
    [MenuItem("Edit/Create Collectible From Object",priority = -1)]
    static void New()
    {
        //Gets the currently selected object from the hierarchy
        var objectsClicked = Selection.gameObjects;

        foreach (var objectClicked in objectsClicked)
        {
            //If there is no object selected do nothing
            if (objectClicked == null) continue;

            //add the script if it doesnt already have it
            if (objectClicked.transform.GetComponent<Collectible>() == null) objectClicked.transform.AddComponent<Collectible>();
            if (objectClicked.transform.GetComponent<BoxCollider>() == null)
            {
                var collectableCollider = objectClicked.transform.AddComponent<BoxCollider>();
                collectableCollider.isTrigger = true;
                collectableCollider.size = new Vector3(CollectableManager.collectiblePickupRange, CollectableManager.collectiblePickupRange, CollectableManager.collectiblePickupRange);
            }

                string localPath = Application.dataPath + "/Resources/Prefabs/Collectables/" + objectClicked.name + ".prefab";

            if (System.IO.File.Exists(localPath))
            {
                if (!EditorUtility.DisplayDialog("Prefab already exists", " Do you want to overwrite it?", "Yes", "No"))
                    continue;
                System.IO.File.Delete(localPath);
            }

            //Save the prefab
            var prefabObjectSaved = PrefabUtility.SaveAsPrefabAsset(objectClicked, localPath);

            //Register It With The Scenes Collectable Manager
            if (SceneManager.GetActiveScene().name != "Menu" && EditorUtility.DisplayDialog("Register With Manager?", "Do you want to automatically register this prefab as a collectable for the current scene?", "Yes", "No")) {
                var collectableManager = GameObject.FindObjectOfType<CollectableManager>();
                if (collectableManager==null)
                {
                    if (EditorUtility.DisplayDialog("No Manager", "Do you want to create a collectable manager for the current scene?", "Yes", "No"))
                    {
                        var collectableManagerObject = new GameObject("Collectable Manager");
                        collectableManager = collectableManagerObject.AddComponent<CollectableManager>();
                    }
                }
               
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
    }
    #endif


}
