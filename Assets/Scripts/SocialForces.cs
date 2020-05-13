using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocialForces : MonoBehaviour
{
    private Rigidbody rb;
    private float perceptionRadius;
    private Dictionary<GameObject, Vector3> perceivedNeighbors = new Dictionary<GameObject, Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        perceptionRadius = GetComponent<SphereCollider>().radius;
    }

    private void Update()
    {
        if (perceivedNeighbors.Count!=0)
            computeForce();
    }

    private void computeForce()
    {
        var force = Vector3.zero;
        
        foreach (var obj in perceivedNeighbors)
        {
            force += calculateWallForce(obj.Key) * 0.0001f;
            
            
        }
        rb.AddForce(force * 10, ForceMode.Force);
    }

    private Vector3 calculateWallForce(GameObject wall)
    {
        var wallForce = Vector3.zero;

        var normal = transform.position - perceivedNeighbors[wall];
        if (Mathf.Abs(normal.x) > Mathf.Abs(normal.z))
        {
            normal.z = 0;
        }
        else
        {
            normal.x = 0;
        }
        normal = normal.normalized;

        normal.y = 0;
        Debug.DrawRay(perceivedNeighbors[wall], normal * 100, Color.red);
        var dir = transform.position - perceivedNeighbors[wall];
        dir.y = 0;
        var projection = Vector3.Project(dir, normal);

        var exponent = Mathf.Exp(((perceptionRadius + 0.5f) - projection.magnitude) / 100f);
        wallForce += (2000f * exponent) * normal * 250;
        //Debug.Log("Force" + wallForce.magnitude);

        return wallForce;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "SideWall" && !perceivedNeighbors.ContainsKey(other.gameObject))
        {
            //Debug.Log(other.gameObject.name);
            Vector3 poc = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            perceivedNeighbors.Add(other.gameObject, poc);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (perceivedNeighbors.ContainsKey(other.gameObject))
        {
            perceivedNeighbors.Remove(other.gameObject);
        }
    }

}
