using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.Vehicles.Aeroplane;

public class AIController : MonoBehaviour
{
    public float maxPitchAgnleInDeg = 45;
    public float maxRollAngleInDeg = 55;

    [SerializeField] private float speedEffect = 0.75f;
    [SerializeField] private float rollSensitivity = 0.65f;
    [SerializeField] private float pitchSensitivity = 0.65f;
    [SerializeField] private float takeOffHeight = 10.0f;
    [SerializeField] private float maxVelocity = 200.0f;

    [SerializeField] private float distanceThreshold = 50.0f;

    [SerializeField] private bool _takenOff = false;
    [SerializeField] private bool airBrakes = false;
    
    private AeroplaneController m_Controller;
    private PathFindingAgent m_NavAgent;

    private Vector3[] m_Path;
    private Vector3 _startingPos;
    private float _prevPointDistance = Mathf.Infinity;
    private Rigidbody m_Rigidbody;
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
            if (transform.position.y - _startingPos.y > takeOffHeight)
            {
                _takenOff = true;
                // Get the path to follow using path follower
                m_Path = m_NavAgent.GetPath();
                print(m_Path.Length);
                StartCoroutine(MoveAgent());
            }
        }
        else
        {
            // Clamp the speed to maxSpeed once the plane has taken off
            Vector3.ClampMagnitude(m_Rigidbody.velocity, maxVelocity);
        }
    }

    IEnumerator MoveAgent()
    {
        int i = 0;
        Vector3 currTarget = Vector3.zero;
        Vector3 nextTarget = Vector3.zero;
        float currDist = 0;
        float nextDist = 0;
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

            currDist = Vector3.Distance(transform.position, currTarget);
            nextDist = Vector3.Distance(transform.position, nextTarget);
            
            // still approaching the point and hasn't overshot
            while (( currDist < _prevPointDistance) && (nextDist > currDist))  
            {
                // keep going while distance is decreasing, keep moving towards this point
                _prevPointDistance = currDist;
                MoveTowards(Vector3.Lerp(currTarget, nextTarget, Mathf.Clamp01(m_Controller.GetVelocity().magnitude * Time.deltaTime)));
                yield return new WaitForFixedUpdate();
                // recalculate the current Target distance
                currDist = Vector3.Distance(transform.position, currTarget);
            }
            // move on to next point
            i += 1;
        }
        
        // reach final destination point
        while (currDist < distanceThreshold)
        {
            MoveTowards(currTarget);
            yield return new WaitForFixedUpdate();
            // recalculate the current Target distance
            currDist = Vector3.Distance(transform.position, currTarget);
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
        
        Debug.DrawRay(transform.position, transform.forward * 100, Color.blue);
        
        // if the plane needs to change it's direction by more than 90 degrees, apply air brakes immediately
        if ((Vector3.Dot(transform.forward, (targetPos - transform.position).normalized) <= 0.2) || (localTarget.magnitude > 2 * distanceThreshold))
        {
            airBrakes = true; // indicator to see if it's working
            // apply large amount of drag using airbrakes and increase the control sensitivity to allow for immediate correction
            m_Controller.Move(rollInput * 1.2f, pitchinput * 1.2f, yawInput, throttleInput, true);
        }
        else
        {
            airBrakes = false;
            m_Controller.Move(rollInput, pitchinput, yawInput, throttleInput, false);
            
        }
        
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
