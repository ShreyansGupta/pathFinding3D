using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.Vehicles.Aeroplane;

public class AIController : MonoBehaviour
{

    [SerializeField] private float m_RollSensitivity = .2f;         // How sensitively the AI applies the roll controls
    [SerializeField] private float m_PitchSensitivity = .5f;        // How sensitively the AI applies the pitch controls
    [SerializeField] private float m_LateralWanderDistance = 5;     // The amount that the plane can wander by when heading for a target
    [SerializeField] private float m_LateralWanderSpeed = 0.11f;    // The speed at which the plane will wander laterally
    [SerializeField] private float m_MaxClimbAngle = 45;            // The maximum angle that the AI will attempt to make plane can climb at
    [SerializeField] private float m_MaxRollAngle = 45;             // The maximum angle that the AI will attempt to u
    [SerializeField] private float m_SpeedEffect = 0.01f;           // This increases the effect of the controls based on the plane's speed.
    [SerializeField] private float m_TakeoffHeight = 20;            // the AI will fly straight and only pitch upwards until reaching this height
    public bool drawPath = false;
    
    private AeroplaneController m_Controller;
    private PathFindingAgent m_NavAgent;

    private Vector3[] m_Path;
    private Vector3 _startingPos;
    private float _prevPointDistance = Mathf.Infinity;
    private Rigidbody m_Rigidbody;
    
    private bool _takenOff;
    private float distanceThreshold = 120.0f;
    
    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
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
            if (m_Controller.Altitude > m_TakeoffHeight)
            {
                _takenOff = true;
                // Get the path to follow using path follower
                m_Path = m_NavAgent.GetPath();
                
                m_Path = m_NavAgent.Chaikin(m_Path);

                m_Path = movingAverage(m_Path, 10);
                
                print(m_Path.Length);
                StartCoroutine(MoveAgent());   
            }
        }
       
        // Clamp the speed to maxSpeed once the plane has taken off
        // Vector3.ClampMagnitude(m_Rigidbody.velocity, maxVelocity);
     
    }
    
    IEnumerator MoveAgent()
    {
        int i = 0;
        Vector3 currTarget = Vector3.zero;
        Vector3 nextTarget = Vector3.zero;
        float currTargetDist = 0;
        float nextTargetDist = 0;
        
        while (i < m_Path.Length)
        {
            // for every new point prev distance is set to infinity
            _prevPointDistance = Mathf.Infinity;

            currTarget = m_Path[i];
            if (i < m_Path.Length - 1)
            {
                nextTarget = m_Path[i + 1];    
            }
            else
            {
                nextTarget = currTarget + transform.forward * 100; // once on last point, set the next target to be in the line of currTarget
            }
            
            // print("starting towards: " + currTarget );

            currTargetDist = Vector3.Distance(transform.position, currTarget);
            nextTargetDist = Vector3.Distance(transform.position, nextTarget);
            
            // keep going while distance is decreasing and the plane hasn't overshot
            // Base assuption: the next point cannot be behind the plane - 50deg and distance is decreasing
            
            // print("Point: "+ i + ", dot: " + Vector3.Dot(transform.forward, (currTarget - transform.position).normalized));
            while ( (currTargetDist > distanceThreshold) && Vector3.Dot(transform.forward, (currTarget - transform.position).normalized) > 0.5 ) 
            {
                
                Debug.DrawRay(transform.position, transform.forward * 100, Color.blue);
                Debug.DrawLine(transform.position, currTarget);
                
                _prevPointDistance = currTargetDist;
                // using Lerp as an extra smoothening step
                // MoveTowards(Vector3.Lerp(currTarget, nextTarget, ));
                MoveTowards(currTarget);
                yield return new WaitForFixedUpdate();
                
                // recalculate the distances for next iteration Target distance
                currTargetDist = Vector3.Distance(transform.position, currTarget);
                nextTargetDist = Vector3.Distance(transform.position, nextTarget);

            }
            // move on to next point
            i += 12;
        }
        
        // reach final destination point
        while (currTargetDist < distanceThreshold)
        {
            MoveTowards(currTarget);
            yield return new WaitForFixedUpdate();
            // recalculate the current Target distance
            currTargetDist = Vector3.Distance(transform.position, currTarget);
        }

        yield return null;
    }
    
    #region PD_Controller

    void MoveTowards(Vector3 targetPos)
    {
         Vector3 localTarget = transform.InverseTransformPoint(targetPos);
        float targetAngleYaw = Mathf.Atan2(localTarget.x, localTarget.z);
        float targetAnglePitch = -Mathf.Atan2(localTarget.y, localTarget.z);


        // Set the target for the planes pitch, we check later that this has not passed the maximum threshold
        targetAnglePitch = Mathf.Clamp(targetAnglePitch, -m_MaxClimbAngle*Mathf.Deg2Rad,
                                       m_MaxClimbAngle*Mathf.Deg2Rad);

        // calculate the difference between current pitch and desired pitch
        float changePitch = targetAnglePitch - m_Controller.PitchAngle;

        // AI always applies gentle forward throttle
        const float throttleInput = 0.5f;

        // AI applies elevator control (pitch, rotation around x) to reach the target angle
        float pitchInput = changePitch*m_PitchSensitivity;

        // clamp the planes roll
        float desiredRoll = Mathf.Clamp(targetAngleYaw, -m_MaxRollAngle*Mathf.Deg2Rad, m_MaxRollAngle*Mathf.Deg2Rad);
        float yawInput = 0;
        float rollInput = 0;
        
        // now we have taken off to a safe height, we can use the rudder and ailerons to yaw and roll
        yawInput = targetAngleYaw;
        rollInput = -(m_Controller.RollAngle - desiredRoll)*m_RollSensitivity;
    
        // adjust how fast the AI is changing the controls based on the speed. Faster speed = faster on the controls.
        float currentSpeedEffect = 1 + (m_Controller.ForwardSpeed*m_SpeedEffect);
        rollInput *= currentSpeedEffect;
        pitchInput *= currentSpeedEffect;
        yawInput *= currentSpeedEffect;

        // pass the current input to the plane (false = because AI never uses air brakes!)
        m_Controller.Move(rollInput, pitchInput, yawInput, throttleInput, false);
    }
    
    #endregion
    
    private void OnDrawGizmos()
    {
        if (drawPath && m_Path != null)
        {
            for (int i = 0; i < m_Path.Length-1; i++)
            {
                Gizmos.color = Color.grey;
                Gizmos.DrawWireSphere(m_Path[i], 2);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(m_Path[i], m_Path[i+1]);
            }
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distanceThreshold);
    }

    Vector3[] movingAverage(Vector3[] path, int window=5)
    {
        Vector3[] newPath = new Vector3[path.Length - window + 1];
        for (int i = 0; i < path.Length - window; i++)
        {
            newPath[i] = Vector3.zero;
            for (int j = i; j < i + window; j++)
            {
                newPath[i] += path[j];
            }
            newPath[i] /= window;
        }
        return  newPath;
    }
}
