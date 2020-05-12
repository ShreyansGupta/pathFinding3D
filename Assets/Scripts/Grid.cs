using System;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {

	public bool drawPath;
	public bool drawGrid;
	public LayerMask unwalkableMask;
	public Vector3 gridWorldSize;
	public float nodeRadius;
	private Node[,,] _grid;
	public List<Node> path;
	private float _nodeDiameter;
	public int _gridSizeX, _gridSizeY, _gridSizeZ;

	private int noFreeSpace = 1;
	private HashSet<Tuple<int, int, int>> extraUnwalkable = new HashSet<Tuple<int, int, int>>();
	void Start() {
		_nodeDiameter = nodeRadius*2;
		_gridSizeX = Mathf.RoundToInt(gridWorldSize.x/_nodeDiameter);
		_gridSizeY = Mathf.RoundToInt(gridWorldSize.y/_nodeDiameter);
		_gridSizeZ = Mathf.RoundToInt(gridWorldSize.z/_nodeDiameter);
		CreateGrid();
	}

	public int MaxSize {
		get {
			return _gridSizeX * _gridSizeY * _gridSizeZ;
		}
	}

	void CreateGrid() {
		_grid = new Node[_gridSizeX,_gridSizeY,_gridSizeZ];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.up * gridWorldSize.y/2 
		                          - Vector3.forward * gridWorldSize.z/2;
		
		for (int x = 0; x < _gridSizeX; x ++) {
			for (int y = 0; y < _gridSizeY; y++) {
				for(int z=0; z < _gridSizeZ; z++) {
					Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiameter + nodeRadius) + 
					                     Vector3.up * (y * _nodeDiameter + nodeRadius) + 
					                     Vector3.forward * (z * _nodeDiameter + nodeRadius);
					
					bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
					_grid[x,y,z] = new Node(walkable,worldPoint,x,y,z);
					// mark all neighbors as extraFreeUnwalkaleSpace
					if (!walkable) {
						for (int i = -noFreeSpace; i <= noFreeSpace; i++) 
						{
							for (int j = -noFreeSpace; j <= noFreeSpace; j++)
							{
								for (int k = -noFreeSpace; k <= noFreeSpace; k++)
								{
									if (i==0 && j==0 && k==0) continue;
									if ((x + i >= 0 && x + i < _gridSizeX) &&
									    (y + j >= 0 && y + j < _gridSizeY) &&
									    (z + k >= 0 && z + k < _gridSizeZ))
									{
										extraUnwalkable.Add(Tuple.Create(x+i, y+j, z+k));
									}
								}
							}
						}
					}
				}
			}
		}
		// mark all extra unwalkable white space
		foreach(var t in extraUnwalkable)
		{
			_grid[t.Item1, t.Item2, t.Item3].walkable = false;
		}
	}

	public List<Node> GetNeighbours(Node node)
	{
		List<Node> neighbours = new List<Node>();

		// n^3-needs optimization
		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				for (int z = -1; z <= 1; z++) {
					if (x == 0 && y == 0 && z == 0)
						continue;
					if (((node.gridX + x) >= 0) && ((node.gridX + x) < _gridSizeX) && 
					    ((node.gridY + y) >= 0) && ((node.gridY + y) < _gridSizeY) &&
					    ((node.gridZ + z) >= 0) && ((node.gridZ + z) < _gridSizeZ))
						neighbours.Add(_grid[(node.gridX + x), (node.gridY + y), (node.gridZ + z)]);
				}
			}
		}
		// Get only the adjacent nodes and not diagonals
		// int[,] possible  = {{-1, 0, 0}, {1, 0, 0}, {0, 0, -1}, {0, 0, 1}, {0, -1, 0}, {0, 1, 0}};
		//
		// // Debug.Log("possible :"+ possible.GetLength(0));
		// for (int i = 0; i < possible.GetLength(0); i++)
		// {
		// 	int x = possible[i,0];
		// 	int y = possible[i,1];
		// 	int z = possible[i,2];
		// 	int checkX = node.gridX + x;
		// 	int checkY = node.gridY + y;
		// 	int checkZ = node.gridZ + z;
		// 	if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY && 
		// 	    checkZ >= 0 && checkZ < gridSizeZ)
		// 	{
		// 		neighbours.Add(grid[checkX, checkY, checkZ]);
		// 	}
		// }
		//
		return neighbours;
	}
	

	public Node getNodeFromPos(Vector3 worldPosition) {
		float percentX = (worldPosition.x + gridWorldSize.x/2) / gridWorldSize.x;
		float percentY = (worldPosition.y) / gridWorldSize.y;
		float percentZ = (worldPosition.z + gridWorldSize.z/2) / gridWorldSize.z;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);
		percentZ = Mathf.Clamp01(percentZ);
		int x = Mathf.RoundToInt((_gridSizeX-1) * percentX);
		int y = Mathf.RoundToInt((_gridSizeY-1) * percentY);
		int z = Mathf.RoundToInt((_gridSizeZ-1) * percentZ);
		return _grid[x,y,z];
	}

	
	void OnDrawGizmos() {
		Gizmos.DrawWireCube(transform.position,
			new Vector3(gridWorldSize.x,gridWorldSize.y,gridWorldSize.z));

		if (drawPath) {
			if (path != null) {
				foreach (Node n in path) {
					Gizmos.color = Color.yellow;
					Gizmos.DrawCube(n.worldPosition, Vector3.one * (_nodeDiameter-.1f));
				}
			}
		}
		if(drawGrid) {

			if (_grid != null) {
				foreach (Node n in _grid) {
					Gizmos.color = (n.walkable)?Color.white:Color.red;
					if (path != null)
						if (path.Contains(n))
							Gizmos.color = Color.black;
					if (!n.walkable)
						Gizmos.DrawCube(n.worldPosition, Vector3.one * (_nodeDiameter-.1f));
				}
			}
		}
	}
}