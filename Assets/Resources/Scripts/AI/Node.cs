using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool walkable;
    public Vector2 worldPosition;
    public int gridX, gridY;
    public int gCost, hCost;
    public Node parent;

    public Node(bool walkable, Vector2 worldPosition, int x, int y)
    {
        this.walkable = walkable;
        this.worldPosition = worldPosition;
        this.gridX = x;
        this.gridY = y;
    }

    public int fCost => gCost + hCost;
}
