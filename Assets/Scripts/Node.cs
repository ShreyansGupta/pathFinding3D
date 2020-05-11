using System;
using UnityEngine;
using System.Collections;

public class Node  {
	
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public int gridZ;
    public double gCost;
    public double hCost;
    public Node parent;
	
    public Node(bool walkable, Vector3 worldPos, int gridX, int gridY, int gridZ) {
        this.walkable = walkable;
        this.worldPosition = worldPos;
        this.gridX = gridX;
        this.gridY = gridY;
        this.gridZ = gridZ;
    }
    public double fCost {
        get {
            return gCost + hCost;
        }
    }
    public double GetDistance(Node other) {
        return (Math.Sqrt(Math.Pow(this.gridX - other.gridX,2) + Math.Pow(this.gridY - other.gridY,2) 
                                                                                                   + Math.Pow(this.gridZ - other.gridZ,2)));
    }
}