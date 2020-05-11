using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocialForces : MonoBehaviour
{
    private Rigidbody rb;
    public float perceptionRadius;
    private PathFindingAgent pathFinding;
    // Start is called before the first frame update
    void Start()
    {
        pathFinding = GetComponent<PathFindingAgent>();
        rb = GetComponent<Rigidbody>();
        perceptionRadius=GetComponent<SphereCollider>().radius;
    }

    private void Update()
    {
        computeForce();
    }

    private void computeForce()
    {
        var force = Vector3.zero;
        var perceivedNeighbors = pathFinding.perceivedNeighbors;
        foreach (var obj in perceivedNeighbors)
        {
            force += calculateWallForce(obj.Key) * 0.0001f;
        }
        rb.AddForce(force * 10, ForceMode.Force);
    }

    private Vector3 calculateWallForce(GameObject wall)
    {
        var wallForce = Vector3.zero;

        var normal = transform.position - pathFinding.perceivedNeighbors[wall];
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
        Debug.DrawRay(pathFinding.perceivedNeighbors[wall], normal*100,Color.red);
        var dir = transform.position - pathFinding.perceivedNeighbors[wall];
        dir.y = 0;
        var projection = Vector3.Project(dir, normal);

        var exponent = Mathf.Exp(((perceptionRadius + 0.5f) - projection.magnitude) / 100f);
        wallForce += (2000f * exponent) * normal*100;
        //Debug.Log("Force" + wallForce.magnitude);

        return wallForce;
    }

}
