/**
* Provide simple path-finding algorithm with support in penalties.
* Heavily based on code from this tutorial: https://www.youtube.com/watch?v=mZfyt03LDH4
* This is just a Unity port of the code from the tutorial + option to set penalty + nicer API.
*
* Original Code author: Sebastian Lague.
* Modifications & API by: Ronen Ness.
* Since: 2016.
*/
using UnityEngine;
using System.Collections.Generic;

/**
* Main class to find the best path from A to B.
* Use like this:
* Grid grid = new Grid(width, height, tiles_costs);
* List<Point> path = Pathfinding.FindPath(grid, from, to);
*/
public abstract class Pathfinding
{
    // The API you should use to get path
    // grid: grid to search in.
    // startPos: starting position.
    // targetPos: ending position.
    public static List<Area> FindPath(Area startPos, Area targetPos)
    {
        // find path
        List<Area> locationInstancesPath = _ImpFindPath(startPos, targetPos);

        // convert to a list of points and return
        List<Area> ret = new List<Area>();
        if (locationInstancesPath != null)
        {
            return locationInstancesPath;
        }
        return ret;
    }

    // internal function to find path, don't use this one from outside
    private static List<Area> _ImpFindPath(Area startNode, Area targetNode)
    {

        List<Area> openSet = new List<Area>();
        HashSet<Area> closedSet = new HashSet<Area>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Area currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (Area neighbour in currentNode.Neighbours)//GetNeighbours(grid, currentNode))
            {
                if (closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) ;
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

        return null;
    }

    private static List<Area> RetracePath(Area startNode, Area endNode)
    {
        List<Area> path = new List<Area>();
        Area currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Add(startNode);

        path.Reverse();
        return path;
    }

    private static int GetDistance(Area LocationInstanceA, Area LocationInstanceB)
    {
        int dstX = Mathf.Abs(LocationInstanceA.X - LocationInstanceB.X);
        int dstY = Mathf.Abs(LocationInstanceA.Y - LocationInstanceB.Y);
        
        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

}
    

