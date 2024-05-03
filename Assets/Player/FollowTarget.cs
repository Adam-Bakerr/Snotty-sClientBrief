using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField] public Transform targetTransform;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if(!targetTransform) return;
        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
        transform.localScale = targetTransform.localScale;
    }
}
