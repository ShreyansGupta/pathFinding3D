using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineTest : MonoBehaviour
{
    private Vector3[] m_Path;

    // Start is called before the first frame update
    void Start()
    {
        m_Path = new Vector3[4];
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            m_Path[i] = transform.GetChild(i).position;
        }

        m_Path = GetSplinePath(m_Path);
        
        
    }
    
        public Vector3[] GetSplinePath(Vector3[] controlPointsList, float resolution=0.2f, float deviationThreshold=60)
    {
        // 0 1 2 3 4
        List<Vector3> splinePath = new List<Vector3>();
        splinePath.Add(controlPointsList[0]);

        for (int i = 1; i < controlPointsList.Length-2; i++)
        {
            //Cant draw between the endpoints
            //Neither do we need to draw from the second to the last endpoint
            //...if we are not making a looping line
            
            Vector3 p0 = controlPointsList[i - 1];
            Vector3 p1 = controlPointsList[i];
            Vector3 p2 = controlPointsList[(i + 1)];
            Vector3 p3 = controlPointsList[(i + 2)];
            
            //The start position of the line
            Vector3 lastPos = p1;
            if (i == 1)
            {
                // need to add p1 only the first time spline is run
                // because in next iteration p2 will be p1 which is already added
                splinePath.Add(lastPos);
            }

            //How many times should we loop?
            int loops = Mathf.FloorToInt(1f / resolution);
            for (int j = 1; j <= loops; j++)
            {
                //Which t position are we at?
                float t = i * resolution;

                //Find the coordinate between the end points with a Catmull-Rom spline
                Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);
                splinePath.Add(newPos); // add the calculated pos to splinePath
                
                //Save this pos so we can draw the next line segment
                lastPos = newPos;
            }
        }
        // Add the last two points
        splinePath.Add(controlPointsList[controlPointsList.Length - 2 ]);
        splinePath.Add(controlPointsList[controlPointsList.Length - 1 ]);

        return splinePath.ToArray();
    }

    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        //The cubic polynomial: a + b * t + c * t^2 + d * t^3
        Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

        return pos;
    }
    
    private void OnDrawGizmos()
    {
        if (m_Path != null)
        {
            for (int i = 0; i < m_Path.Length-1; i++)
            {
                Gizmos.color = Color.grey;
                Gizmos.DrawWireSphere(m_Path[i], 10);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(m_Path[i], m_Path[i+1]);
            }
        }
    }


}

