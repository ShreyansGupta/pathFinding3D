using System;
using UnityEngine;
using UnityStandardAssets.Vehicles.Aeroplane;

public class UserInputPlaneController: MonoBehaviour
{
    private AeroplaneController m_Controller;

    public float pitchInput = 0;
    public float rollInput = 0;

    private void Start()
    {
        m_Controller = GetComponent<AeroplaneController>();
    }

    void FixedUpdate()
    {
        pitchInput = -Input.GetAxis("Vertical");
        rollInput = Input.GetAxis("Horizontal");
        
        m_Controller.Move(rollInput, pitchInput, 1, 0.1f, false);
    }  
}
