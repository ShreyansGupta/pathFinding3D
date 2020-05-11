using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Target : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnDrawGizmos() 
    {
        Handles.Label(transform.position, "Target");
    }
}
