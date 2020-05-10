using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour {

	public Transform agent, destination;
	public float moveSpeed = 20f;
	Grid grid;

	void Start() {
		grid = GetComponent<Grid>();
	}

	void Update() {
		Vector3[] path = FindPath(agent.position,destination.position);
		if (path != new Vector3[0])
		{
			object[] parms = {path,agent.gameObject};
			StartCoroutine("MoveAgent",parms);
		}
		StopCoroutine("MoveAgent");
	}
	
	IEnumerator MoveAgent(object[] parms) {
		Vector3[] path = (Vector3[])parms[0];
		GameObject objToMove = (GameObject)parms[1];
		
		Vector3 currentPoint = path[0];
		int count = 0;
		while (true) {
			if (objToMove.transform.position == currentPoint) {
				count ++;
				if (count >= path.Length) {
					yield break;
				}
				currentPoint = path[count];
			}

			objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position,
				currentPoint,moveSpeed * Time.deltaTime);
			yield return null;

		}
	}

	Vector3[] FindPath(Vector3 startPos, Vector3 targetPos) {

		Vector3[] path = new Vector3[0];
		bool pathFound = false;
		
		Node startNode = grid.getNodeFromPos(startPos);
		Node targetNode = grid.getNodeFromPos(targetPos);

		Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
		// Heap openSet = new Heap(grid.MaxSize);
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(startNode);

		while (openSet.Count > 0)
		{
			Node currentNode = openSet.RemoveFirst();
			closedSet.Add(currentNode);

			if (currentNode == targetNode)
			{
				pathFound = true;
				break;
			}

			foreach (Node neighbour in grid.GetNeighbours(currentNode))
			{
				if (!neighbour.walkable || closedSet.Contains(neighbour))
				{
					continue;
				}

				double newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
				if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
				{
					neighbour.gCost = newMovementCostToNeighbour;
					neighbour.hCost = GetDistance(neighbour, targetNode);
					neighbour.parent = currentNode;

					if (!openSet.Contains(neighbour))
						openSet.Add(neighbour);
				}
			}
		}

		if (pathFound)
		{
			path = RetracePath(startNode,targetNode);;				
		}
		return path;
	}

	Vector3[] RetracePath(Node startNode, Node endNode) {
		List<Node> path = new List<Node>();
		Node currentNode = endNode;

		while (currentNode != startNode) {
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}
		path.Reverse();
		grid.path = path;
		Vector3[] simplifiedPath = SimplifyPath(path);
		return simplifiedPath;
	}

	Vector3[] SimplifyPath(List<Node> path) {
		List<Vector3> waypoints = new List<Vector3>();
		Vector3 directionOld = Vector3.zero;
		
		for (int i = 1; i < path.Count; i ++) {
			Vector3 directionNew = new Vector3(path[i-1].gridX - path[i].gridX,path[i-1].gridY - path[i].gridY, path[i-1].gridZ - path[i].gridZ);
			if (directionNew != directionOld) {
				waypoints.Add(path[i].worldPosition);
			}
			directionOld = directionNew;
		}
		return waypoints.ToArray();
	}
	double GetDistance(Node nodeA, Node nodeB) {
		// int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		// int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
		// int dstZ = Mathf.Abs(nodeA.gridZ - nodeB.gridZ);
		return (Math.Sqrt(Math.Pow(nodeA.gridX - nodeB.gridX,2) + Math.Pow(nodeA.gridY - nodeB.gridY,2) 
		                                                        + Math.Pow(nodeA.gridZ - nodeB.gridZ,2)));
	}


	private void OnDrawGizmos()
	{
		if (grid != null)
		{
			Debug.Log("seeker: "+agent.position);
			Node s = grid.getNodeFromPos(agent.position);
			Gizmos.DrawWireCube(s.worldPosition,100*Vector3.one);
			Gizmos.DrawWireCube(agent.position,100*Vector3.one);
		}
	}
}
