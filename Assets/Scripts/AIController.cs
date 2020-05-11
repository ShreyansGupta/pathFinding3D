using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.Vehicles.Aeroplane;

public class AIController : MonoBehaviour
{
    public float maxPitchAgnleInDeg = 45;
    public float maxRollAngleInDeg = 45;

    private AeroplaneController m_Controller;
    private PathFindingAgent m_NavAgent;

    private Vector3[] m_Path;
    private Vector3 _startingPos;
    
    [SerializeField] private bool _takenOff = false;
    void Start()
    {
        m_Controller = GetComponent<AeroplaneController>();
        m_NavAgent = GetComponent<PathFindingAgent>();
        _startingPos = transform.position;
    }


    void FixedUpdate()
    {
        // Navigation only kicks of once the plane has taken off
        if (!_takenOff)
        {
            // apply full throttle for takeoff
            m_Controller.Move(0,0,0, 1, false);
            
            if (transform.position.y - _startingPos.y > 10)
            {
                _takenOff = true;
            }
        }
        else
        {
            // Get the path to follow using path follower
            m_Path = m_NavAgent.GetPath();
            Debug.Log(m_Path.Length);
            StartCoroutine(MoveAgent());
        }
    }

    IEnumerator MoveAgent()
    {
        foreach (var nextPos in m_Path)
        {
            while (Vector3.Distance(transform.position, nextPos) < 3.0f)
            {
                MoveTowards(nextPos);
                yield return new WaitForFixedUpdate();
            }
        }

        yield return null;
    }

    void MoveTowards(Vector3 targetPos)
    {
        // adjust the yaw and pitch towards the target
        Vector3 localTarget = transform.InverseTransformPoint(targetPos);
        float targetAngleYaw = Mathf.Atan2(localTarget.x, localTarget.z);
        float targetAnglePitch = -Mathf.Atan2(localTarget.y, localTarget.z);
        
        // Set the target for the planes pitch, we check later that this has not passed the maximum threshold
        targetAnglePitch = Mathf.Clamp(targetAnglePitch, -maxPitchAgnleInDeg * Mathf.Deg2Rad,
            maxPitchAgnleInDeg * Mathf.Deg2Rad);
        // calculate the difference between current pitch and desired pitch to get the pitch input
        float changePitch = targetAnglePitch - m_Controller.PitchAngle;
        // for now let's try working with sensitivity 1 as Controller internally uses pitch input sensitivity
        float pitchinput = changePitch * 1.0f;
        // roll
        float desiredRoll = Mathf.Clamp(targetAngleYaw, -maxRollAngleInDeg * Mathf.Deg2Rad, 
                                                                maxRollAngleInDeg * Mathf.Deg2Rad);
        float rollInput = -(m_Controller.RollAngle - desiredRoll) * 1.0f ;
        // yaw
        float yawInput = targetAngleYaw;
        // we linearly interpolate the distance to travel to calculate the throttle applied
        const float throttleInput = 0.5f; // for now, let's work with constant gentle throttle
        
        // AI modifies the controls based on speed 
        float currentSpeedEffect = 1 + (m_Controller.ForwardSpeed * 0.01f); // experiment with speed speed effect
        rollInput *= currentSpeedEffect;
        pitchinput *= currentSpeedEffect;
        yawInput *= currentSpeedEffect;

        m_Controller.Move(rollInput, pitchinput, yawInput, throttleInput, false);
    }
    
}
