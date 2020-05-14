using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SocialForces : MonoBehaviour
{
    private Rigidbody rb;
    private AIController m_AIController;
    private float perceptionRadius;
    private Dictionary<GameObject, Vector3> perceivedNeighbors = new Dictionary<GameObject, Vector3>();
    private float rayCastOffset = 10.0f;
    public float detectionDistance = 100.0f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        perceptionRadius = GetComponent<SphereCollider>().radius;
        m_AIController = GetComponent<AIController>();
    }

    private void FixedUpdate()
    {
        if (perceivedNeighbors.Count != 0)
        {
            computeForce();
            // rayCastForce();
            
            // Using AI MoveTowards
            // Vector3 turningDirection = Vector3.zero;
            // foreach (var obj in perceivedNeighbors)
            // {
            //     turningDirection += transform.position - obj.Value;
            // }
            //
            // m_AIController.MoveTowards((transform.position + turningDirection) * 0.01f);
        }
        
    }

    private void computeForce()
    {
        var force = Vector3.zero;
        
        foreach (var obj in perceivedNeighbors)
        {
            force += calculateWallForce(obj.Key) * 0.0001f;
            
            
        }
        rb.AddForce(force * 5, ForceMode.Force);
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
        wallForce += (2000f * exponent) * normal * 100;
        // Debug.Log("Force" + wallForce.magnitude);

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

        if (other.gameObject.tag.Equals("Player") && !perceivedNeighbors.ContainsKey(other.gameObject))
        {
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

    private void rayCastForce()
    {
        Vector3 left = transform.position - transform.right * rayCastOffset;
        Vector3 right = transform.position + transform.right * rayCastOffset;
        RaycastHit hit;
        Vector3 move = new Vector3();
        Debug.DrawRay(left, transform.forward*100, Color.magenta);
        Debug.DrawRay(right, transform.forward*100, Color.magenta);

        if(Physics.Raycast(left,transform.forward,out hit, detectionDistance))
        {
            move += transform.up;
        }
        else if (Physics.Raycast(right, transform.forward, out hit, detectionDistance))
        {
            move -= transform.up;
        }

        if (move != Vector3.zero)
        {
            transform.Rotate(move * 50f * Time.deltaTime);

        }
    }

}
