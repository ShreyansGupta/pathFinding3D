using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AgentFollowCam : MonoBehaviour
{
    public float damping;
    
    void Update()
    {
        var name = "";
        if (Input.GetKey(KeyCode.R))
        {
            name += "R";
        }
        else if (Input.GetKey(KeyCode.G))
        {
            name += "G";
        }

        if (Input.GetKey(KeyCode.Alpha1))
        {
            name += "1";
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            name += "2";
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            name += "3";
        }
        else if (Input.GetKey(KeyCode.Alpha4))
        {
            name += "4";
        }
        else if (Input.GetKey(KeyCode.Alpha5))
        {
            name += "5";
        }else if (Input.GetKey(KeyCode.Alpha6))
        {
            name += "6";
        }
        
        if(name.Length == 2) {
            foreach (FollowCam fc in GetComponentsInChildren<FollowCam>())
            {
                fc.enabled = false;
                fc.GetComponent<AIController>().drawWireSphere = false;
                if (fc.gameObject != gameObject && fc.gameObject.name == name)
                {
                    fc.GetComponent<AIController>().drawWireSphere = true;
                    fc.damping = damping;
                    fc.enabled = true;
                }
            }
        }
    }
}
