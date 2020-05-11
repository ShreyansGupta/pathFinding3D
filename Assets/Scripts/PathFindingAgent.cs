using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathFindingAgent : MonoBehaviour {
	
	public GameObject finderGameObject;
	public Transform target;
	public Dictionary<GameObject,Vector3> perceivedNeighbors = new Dictionary<GameObject,Vector3>();
	
	private Astar astar;
	private Grid grid;
	void Start()
	{
		 astar = finderGameObject.GetComponent<Astar>();
		 grid = finderGameObject.GetComponent<Grid>();

	}
	public Vector3[] GetPath()
	{
		return astar.FindPath(transform.position, target.position);
	}

	// private void OnDrawGizmos()
	// {
	// 	if (grid != null)
	// 	{
	// 		// Debug.Log("seeker: "+agent.position);
	// 		Node s = grid.getNodeFromPos(transform.position);
	// 		Gizmos.DrawWireCube(s.worldPosition,100*Vector3.one);
	// 		Gizmos.DrawWireCube(transform.position,100*Vector3.one);
	// 	}
	// }

	public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "SideWall")
		{
			//Debug.Log(other.gameObject.name);
			Vector3 poc=other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
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

}
