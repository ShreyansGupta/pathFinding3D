using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Priority_Queue;

public class Astar : MonoBehaviour {

	public Transform agent, destination;
	public float moveSpeed = 20f;
	Grid _grid;

	private void Start() {
		_grid = GetComponent<Grid>();
	}

	private void Update() {
		var path = FindPath(agent.position,destination.position);
		if (path != new Vector3[0])
		{
			object[] parms= {path,agent.gameObject};
			StartCoroutine(nameof(MoveAgent),parms);
		}
		StopCoroutine(nameof(MoveAgent));
	}

	private IEnumerator MoveAgent(object[] parms) {
		var path = (Vector3[])parms[0];
		var objToMove = (GameObject)parms[1];
		var count = 0;
		while (true) {
			if (++count >= path.Length) {
				yield break;
			}
			var currentPoint = path[count];
			objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position,
				currentPoint,moveSpeed * Time.deltaTime);
			yield return null;

		}
	}

	public Vector3[] FindPath(Vector3 startPos, Vector3 targetPos) {
		var path = new Vector3[0];
		var pathFound = false;
		var startNode = _grid.getNodeFromPos(startPos);
		var targetNode = _grid.getNodeFromPos(targetPos);
		var fringe = new SimplePriorityQueue<Node>();
		var visited = new HashSet<Node>();
		fringe.Enqueue(startNode,(float)(0 + startNode.GetDistance(targetNode)));
		
		while (fringe.Count > 0)
		{
			var currentNode = fringe.Dequeue();
			visited.Add(currentNode);
			if (Equals(currentNode , targetNode))
			{
				pathFound = true;
				break;
			}
			foreach (var neighbour in _grid.GetNeighbours(currentNode)
				.Where(neighbour => neighbour.walkable && !visited.Contains(neighbour))
				.Where(neighbour => !fringe.Contains(neighbour)))
			{
				neighbour.gCost = currentNode.gCost + currentNode.GetDistance( neighbour);
				neighbour.hCost = neighbour.GetDistance(targetNode);
				neighbour.parent = currentNode;
				fringe.Enqueue(neighbour,(float)neighbour.fCost);
			}
		}

		if (pathFound)
			path = GetPath(startNode,targetNode);
		return path;
	}

	private Vector3[] GetPath(Node startNode, Node endNode) {
		var nodesInPath = new List<Node>();
		var currentNode = endNode;
		while (currentNode != startNode) {
			nodesInPath.Add(currentNode);
			currentNode = currentNode.parent;
		}
		nodesInPath.Reverse();
		_grid.path = nodesInPath;
		return  PathFromNodeArray(nodesInPath);
	}

	private static Vector3[] PathFromNodeArray(List<Node> nodesInPath) {
		var path = new List<Vector3>();
		for (var i = 1; i < nodesInPath.Count; i ++) {
				path.Add(nodesInPath[i].worldPosition);
		}
		return path.ToArray();
	}

	
}
