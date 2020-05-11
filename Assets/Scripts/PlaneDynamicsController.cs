using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlaneDynamicsController : MonoBehaviour
{
    public float maxEnginePower = 40.0f;
    public float maxSpeed = 200.0f; //Lift will stop acting once maxSpeed is reached
    public float maxLiftOffSpeed = 100.0f;
    public float aerodynamicFactor = 0.2f;
    public float liftFactor = 0.002f;

    public float throttleSensitivity = 0.4f;
    public float pitchSensitivity = 0.75f;
    public float rollSensitivity = 0.75f;
    public float yawSensitivity = 0.75f;
    public float bankSensitivity = 0.75f;

    private Rigidbody _rb;
    private Vector3 _totalForce;
    private float originalDrag;
    private float originalAngularDrag;

    public float RollInput;
    public float PitchInput;
    public float YawInput;
    public float ThrottleInput;
    public float Throttle;
    
    public float PitchAngle;
    public float RollAngle;
    public float bankedTurnAmount;

    public float forwardSpeed;
    
    // private bool _applyingForce = true;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        originalDrag = _rb.drag;
        originalAngularDrag = _rb.angularDrag;
    }

    // Update is called every Physics Frame
    void FixedUpdate()
    {
        // var roll = -Input.GetAxis("Horizontal");
        // var pitch = -Input.GetAxis("Vertical");
        // Debug.Log(pitch);
        // Move(roll, pitch, 0, 0.5f);
        
        // Add all kinds of Social forces from other planes and obstacles
        // _totalForce = GetTotalSocialForces(); //ma
        // _rb.AddForce(_totalForce, ForceMode.Acceleration); // apply accelerationg independent of mass
    }
    
    Vector3 GetTotalSocialForces()
    {
        // Calculates the total force to be applied to the plane 
        var force = Vector3.zero;
        
        return force;
    }
    
    public void Move(float rollInput, float pitchInput, float yawInput, float throttleInput)
    {    
        // Moves the plane with respect to the roll, pitch and yaw inputs
        RollInput = rollInput;
        PitchInput = pitchInput;
        YawInput = yawInput;
        ThrottleInput = throttleInput;
        
        ClampInputs();
        // calculate Throttle value: Adjust throttle based on throttle input (or immobilized state)
        Throttle = Mathf.Clamp01(Throttle + ThrottleInput * Time.deltaTime * throttleSensitivity); // slowly change throttle

        forwardSpeed = Vector3.Dot(_rb.velocity, transform.forward);
        
        CalculateRollAndPitchAngles();
        
        // bank amount is the Sin of roll angle, i.e. -1, 1
        bankedTurnAmount = Mathf.Sin(RollAngle);

        AutoLevel();
            
        CalculateDrag();
        
        CaluclateAerodynamicEffect();
        
        CalculateLinearForces();
        
        CalculateTorque();
        
        // clamp max forward speed
        _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, maxSpeed);

    }

    void ClampInputs()
    {
        RollInput = Mathf.Clamp(RollInput, -1, 1);
        PitchInput = Mathf.Clamp(PitchInput, -1, 1);
        YawInput = Mathf.Clamp(YawInput, -1, 1);
        ThrottleInput = Mathf.Clamp(ThrottleInput, -1, 1);
    }
    
    private void CalculateRollAndPitchAngles()
    {
        // Calculate roll & pitch angles
        // Calculate the flat forward direction (with no y component).
        var flatForward = transform.forward;
        flatForward.y = 0;
        // If the flat forward vector is non-zero (which would only happen if the plane was pointing exactly straight upwards)
        if (flatForward.sqrMagnitude > 0)
        {
            flatForward.Normalize();
            // calculate current pitch angle
            // Debug.DrawRay(transform.position, flatForward * 10, Color.magenta);
            var localFlatForward = transform.InverseTransformDirection(flatForward);
            // Debug.DrawRay(transform.position, localFlatForward * 10, Color.blue);
            
            PitchAngle = Mathf.Atan2(localFlatForward.y, localFlatForward.z);
            // calculate current roll angle
            var flatRight = Vector3.Cross(Vector3.up, flatForward);
            var localFlatRight = transform.InverseTransformDirection(flatRight);
            RollAngle = Mathf.Atan2(localFlatRight.y, localFlatRight.x);
        }
    }
    
    private void CalculateLinearForces()
    {
        // Now calculate forces acting on the aeroplane:
        var forces = Vector3.zero;
        // Add the engine power in the forward direction
        forces += (maxEnginePower * Throttle) * transform.forward;
        // The direction that the lift force is applied is at right angles to the plane's velocity (usually, this is 'up'!)
        var liftDirection = Vector3.Cross(_rb.velocity, transform.right).normalized;
        // The amount of lift drops off as the plane increases speed - in reality this occurs as the pilot retracts the flaps
        // shortly after takeoff, giving the plane less drag, but less lift. Because we don't simulate flaps, this is
        // a simple way of doing it automatically:
        var zeroLiftFactor = Mathf.InverseLerp(maxLiftOffSpeed, 0, forwardSpeed);
        // Calculate and add the lift power
        var liftPower = forwardSpeed * forwardSpeed * zeroLiftFactor * aerodynamicFactor * liftFactor;
        forces += liftPower * liftDirection;

        Debug.DrawRay(transform.position, forces.normalized * 10, Color.yellow);
        
        // Apply the calculated forces to the the Rigidbody
        _rb.AddForce(forces);
    }
    
    
    private void CaluclateAerodynamicEffect()
    {
        // "Aerodynamic" calculations. This is a very simple approximation of the effect that a plane
        // will naturally try to align itself in the direction that it's facing when moving at speed.
        // Without this, the plane would behave a bit like the asteroids spaceship!
        if (_rb.velocity.magnitude > 0)
        {
            // compare the direction we're pointing with the direction we're moving:
            aerodynamicFactor = Vector3.Dot(transform.forward, _rb.velocity.normalized);
            // multipled by itself results in a desirable rolloff curve of the effect
            aerodynamicFactor *= aerodynamicFactor;
            // Finally we calculate a new velocity by bending the current velocity direction towards
            // the the direction the plane is facing, by an amount based on this aeroFactor
            var newVelocity = Vector3.Lerp(_rb.velocity, transform.forward * forwardSpeed,
                aerodynamicFactor * forwardSpeed * aerodynamicFactor * Time.deltaTime);
            _rb.velocity = newVelocity;

            // also rotate the plane towards the direction of movement - this should be a very small effect, but means the plane ends up
            // pointing downwards in a stall
            _rb.rotation = Quaternion.Slerp(_rb.rotation,
                Quaternion.LookRotation(_rb.velocity, transform.up),
                aerodynamicFactor * Time.deltaTime);
        }
    }
    
    private void CalculateDrag()
    {
        // increase the drag based on speed, since a constant drag doesn't seem "Real" (tm) enough
        float extraDrag = _rb.velocity.magnitude * 0.0001f;
        _rb.drag = originalDrag + extraDrag;
        // Forward speed affects angular drag - at high forward speed, it's much harder for the plane to spin
        _rb.angularDrag =  originalAngularDrag * forwardSpeed;
    }
    
    private void CalculateTorque()
    {
        // We accumulate torque forces into this variable:
        var torque = Vector3.zero;
        // Add torque for the pitch based on the pitch input.
        torque += PitchInput * pitchSensitivity * transform.right;
        // Add torque for the yaw based on the yaw input.
        torque += YawInput * yawSensitivity * transform.up;
        // Add torque for the roll based on the roll input.
        torque += RollInput * rollSensitivity * transform.forward;
        // Add torque for banked turning.
        torque += bankedTurnAmount * bankSensitivity * transform.up;
        // The total torque is multiplied by the forward speed, so the controls have more effect at high speed,
        // and little effect at low speed, or when not moving in the direction of the nose of the plane
        // (i.e. falling while stalled)
        Debug.DrawRay(transform.position, torque * 20, Color.green);
        _rb.AddTorque(torque * (aerodynamicFactor * forwardSpeed));
    }

    private void AutoLevel()
    {
        // The banked turn amount (between -1 and 1) is the sine of the roll angle.
        // this is an amount applied to eAlevator input if the user is only using the banking controls,
        // because that's what people expect to happen in games!
        // auto level roll, if there's no roll input:
        if (RollInput == 0f)
        {
            RollInput = -RollAngle * 0.2f;
        }
        // auto correct pitch, if no pitch input (but also apply the banked turn amount)
        if (PitchInput == 0f)
        {
            PitchInput = -PitchAngle * 0.5f;
            PitchInput -= Mathf.Abs(bankedTurnAmount * bankedTurnAmount * 0.5f);
        }
    }

    
}
