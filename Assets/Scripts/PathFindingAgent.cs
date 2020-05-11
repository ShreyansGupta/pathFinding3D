using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathFindingAgent : MonoBehaviour {
	
	public GameObject finderGameObject;
	public Transform target;

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
	
}
