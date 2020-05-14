using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public float damping = 0.95f;
    // Update is called once per frame
    public Transform camera;
    void Update()
    {
        var newPos = transform.position - transform.forward * 15 + transform.up * 7;
        camera.position = damping * camera.transform.position + (1 - damping) * newPos; 
        
        camera.transform.LookAt(transform.position + transform.forward * 10);
    }
}