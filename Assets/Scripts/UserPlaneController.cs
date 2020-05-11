using UnityEngine;
public class UserPlaneController: MonoBehaviour
{
    private PlaneDynamicsController m_Controller;

    void FixedUpdate()
    {
        var pitchInput = -Input.GetAxis("Vertical");
        var rollInput = Input.GetAxis("Horizontal");
        
        m_Controller.Move(rollInput, pitchInput, 0, 0.5f);
    }  
}
