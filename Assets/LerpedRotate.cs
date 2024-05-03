using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpedRotate : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float lerpSpeed;
    // Update is called once per frame
    void Update()
    {
        transform.forward = Vector3.Slerp(transform.forward, target.forward, lerpSpeed * Time.deltaTime);
    }
}
