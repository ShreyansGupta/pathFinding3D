using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.Vehicles.Aeroplane;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AIController : MonoBehaviour
{

    [SerializeField] private float m_RollSensitivity = .2f;         // How sensitively the AI applies the roll controls
    [SerializeField] private float m_PitchSensitivity = .5f;        // How sensitively the AI applies the pitch controls
    [SerializeField] private float m_MaxClimbAngle = 45;            // The maximum angle that the AI will attempt to make plane can climb at
    [SerializeField] private float m_MaxRollAngle = 45;             // The maximum angle that the AI will attempt to u
    [SerializeField] private float m_SpeedEffect = 0.01f;           // This increases the effect of the controls based on the plane's speed.
    [SerializeField] private float m_TakeoffHeight = 20;            // the AI will fly straight and only pitch upwards until reaching this height
    [SerializeField] private float distanceThreshold = 120.0f;
    public bool drawPath = false;
    public bool drawWireSphere = false;
    
    private AeroplaneController m_Controller;
    private PathFindingAgent m_NavAgent;
    
    private Vector3[] m_Path;
    private Rigidbody m_Rigidbody;

    private bool _takenOff;
    private Vector3 _startingPos;
    private float _prevPointDistance = Mathf.Infinity;
    
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
            if (m_Controller.Altitude > m_TakeoffHeight) //&& m_NavAgent.grid.CheckInWorld(transform.position))
                //&& m_NavAgent.grid.checkInWorld(transform.position))
            {
                _takenOff = true;
                // Get the path to follow using path follower

              PathRequestManager.RequestPath(transform.position + transform.forward * 50 + transform.up * 20, m_NavAgent.target.position, OnPathFound);
            }
        }

        // Clamp the speed to maxSpeed once the plane has taken off
        // Vector3.ClampMagnitude(m_Rigidbody.velocity, maxVelocity);
        // StartCoroutine(SteerToAvoidCollisions());

    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            m_Path = newPath;
            // Do Path smoothing
            m_Path = m_NavAgent.Chaikin(m_Path);
            m_Path = movingAverage(m_Path, 10);
            
            // print("Path Count: " + m_Path.Length);
            // start moving the agent
            StartCoroutine(MoveAgent());
        }
        
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
                
                // Debug.DrawRay(transform.position, transform.forward * 100, Color.blue);
                // Debug.DrawLine(transform.position, currTarget);
                
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
            i += 20;
        }
        
        // reach final destination point
        while (currTargetDist < distanceThreshold / 3)
        {
            MoveTowards(currTarget);
            yield return new WaitForFixedUpdate();
            // recalculate the current Target distance
            currTargetDist = Vector3.Distance(transform.position, currTarget);
        }

        yield return null;
    }
 
    #region PD_Controller
    public void MoveTowards(Vector3 targetPos)
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
    
    private void OnDrawGizmos()
    {
        #if UNITY_EDITOR
        Handles.color = Color.black;
        Handles.Label(transform.position, name);
        #endif
        

        if (m_NavAgent != null)
        {
            var targetName = m_NavAgent.target.name;
            
            if (drawPath && m_Path != null)
            {
                for (int i = 0; i < m_Path.Length - 1; i++)
                {
                    Gizmos.color = Color.grey;
                    Gizmos.DrawWireSphere(m_Path[i], 2);
                    Gizmos.color = Color.grey;
                    Gizmos.DrawWireSphere(m_Path[i], 2);
                    if (targetName.Equals("RedTarget"))
                        Gizmos.color = Color.red;
                    else if (targetName.Equals("GreenTarget"))
                        Gizmos.color = Color.green;
                    else
                        Gizmos.color = Color.blue;
                    Gizmos.DrawLine(m_Path[i], m_Path[i + 1]);
                }
            }

            if (targetName.Equals("RedTarget")) 
                Gizmos.color = Color.red;
            else if(targetName.Equals("GreenTarget"))
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 30f);
        }
    }

   
}
