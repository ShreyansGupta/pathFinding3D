using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class Target : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        var gObj = other.gameObject;
        // if the 
        if (gObj.tag.Equals("Player") && gObj.GetComponent<PathFindingAgent>().target == transform)
        {
            other.gameObject.SetActive(false);
        }
    }

    void OnDrawGizmos() 
    {
        
        Handles.Label(transform.position, name);
    }
    
    
}
