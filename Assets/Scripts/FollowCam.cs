using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public float damping = 0.95f;
    // Update is called once per frame
    void Update()
    {
        var newPos = transform.position - transform.forward * 15 + transform.up * 7;
        Camera.main.transform.position = damping * Camera.main.transform.position + (1 - damping) * newPos; 
        
        Camera.main.transform.LookAt(transform.position + transform.forward * 10);
    }
}