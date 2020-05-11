using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.Vehicles.Aeroplane;

public class AIController : MonoBehaviour
{
    public float maxPitchAgnleInDeg = 45;
    public float maxRollAngleInDeg = 45;

    [SerializeField] private float speedEffect = 0.5f;
    [SerializeField] private float rollSensitivity = 0.2f;
    [SerializeField] private float pitchSensitivity = 0.5f;
    [SerializeField] private float takeOffHeight = 10.0f;
    [SerializeField] private float maxVelocity = 200.0f;

    [SerializeField] private bool _takenOff = false;

    private AeroplaneController m_Controller;
    private PathFindingAgent m_NavAgent;

    private Vector3[] m_Path;
    private Vector3 _startingPos;
    private float _prevPointDistance = Mathf.Infinity;
    
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
            if (transform.position.y - _startingPos.y > takeOffHeight)
            {
                _takenOff = true;
                // Get the path to follow using path follower
                m_Path = m_NavAgent.GetPath();
                StartCoroutine(MoveAgent());
            }
        }
    }

    IEnumerator MoveAgent()
    {
        foreach (var nextPos in m_Path)
        {
            print("starting towards: " + nextPos );
            _prevPointDistance = Mathf.Infinity;
            while (Vector3.Distance(transform.position, nextPos) < _prevPointDistance)
            {
                // keep going while distance is decreasing, then move to next point once overshot
                _prevPointDistance = Vector3.Distance(transform.position, nextPos);
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
        
        Debug.DrawLine(transform.position, targetPos);
        // print("Target Angles: Pitch: " + targetAnglePitch * Mathf.Rad2Deg + " Yaw/Roll: " + targetAngleYaw * Mathf.Rad2Deg );
        // Set the target for the planes pitch, we check later that this has not passed the maximum threshold
        targetAnglePitch = Mathf.Clamp(targetAnglePitch, -maxPitchAgnleInDeg * Mathf.Deg2Rad,
            maxPitchAgnleInDeg * Mathf.Deg2Rad);
        // calculate the difference between current pitch and desired pitch to get the pitch input
        float changePitch = targetAnglePitch - m_Controller.PitchAngle;
        // for now let's try working with sensitivity 0.5 as Controller internally uses pitch input sensitivity
        float pitchinput = changePitch * pitchSensitivity;
        // roll
        float desiredRoll = Mathf.Clamp(targetAngleYaw, -maxRollAngleInDeg * Mathf.Deg2Rad, 
                                                                maxRollAngleInDeg * Mathf.Deg2Rad);
        float rollInput = -(m_Controller.RollAngle - desiredRoll) * rollSensitivity ;
        // yaw
        float yawInput = targetAngleYaw;
        // we linearly interpolate the distance to travel to calculate the throttle applied
        const float throttleInput = 0.3f; // for now, let's work with constant gentle throttle
        
        // AI modifies the controls based on speed 
        float currentSpeedEffect = 1 + (m_Controller.ForwardSpeed * speedEffect); // experiment with speed speed effect
        rollInput *= currentSpeedEffect;
        pitchinput *= currentSpeedEffect;
        yawInput *= currentSpeedEffect;
        
        m_Controller.Move(rollInput, pitchinput, yawInput, throttleInput, false);
    }

    // void LowPassFilter(int dimension = 3)
    // {
    //     // this returns less points in the end
    //     var newPath = new Vector3[m_Path.Length];
    //
    //     for (var i = 0; i <= m_Path.Length - dimension; i++)
    //     {
    //         newPath[i] = Vector3.zero;
    //         for (var j = i; j < i + dimension - 1; j++)
    //         {
    //             newPath[i] += m_Path[j];
    //         }
    //         newPath[i] /= dimension;
    //     }
    //     // replace old path with new low pass filtered path
    //     m_Path = newPath;
    // }

}