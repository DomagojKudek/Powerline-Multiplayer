using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{

    public Transform seeker, target;
    public MapGenerator mapGenerator;

    void Start()
    {
        mapGenerator = GetComponent<MapGenerator>();
        seeker = mapGenerator.newStart;
        target = mapGenerator.newFinish;
        FindPath(seeker.position, target.position);
        mapGenerator.setWinPath(mapGenerator.path);
        mapGenerator.setUnreachableRotatorsOff();
        mapGenerator.winMap = new int[mapGenerator.path.Count-1];
        for (int i = 0; i < mapGenerator.path.Count-1; i++)
        {
            mapGenerator.winMap[i] = 0;
            //Debug.Log(i + " " + mapGenerator.winMap[i]);
        }

    }


    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Block startBlock = NodeFromWorldPoint(startPos);
        Block targetBlock = NodeFromWorldPoint(targetPos);

        List<Block> openSet = new List<Block>();
        HashSet<Block> closedSet = new HashSet<Block>();
        openSet.Add(startBlock);

        while (openSet.Count > 0)
        {
            Block block = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if ((openSet[i].G_cost + openSet[i].H_cost) < (block.G_cost + block.H_cost) || (openSet[i].G_cost + openSet[i].H_cost) == (block.G_cost + block.H_cost))
                {
                    if (openSet[i].H_cost < block.H_cost)
                        block = openSet[i];
                }
            }

            openSet.Remove(block);
            closedSet.Add(block);

            if (block == targetBlock)
            {
                RetracePath(startBlock, targetBlock);
                return;
            }

            foreach (Block neighbour in GetNeighbours(block))
            {
                if (neighbour.Obstacle || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newCostToNeighbour = block.G_cost + GetDistance(block, neighbour);
                if (newCostToNeighbour < neighbour.G_cost || !openSet.Contains(neighbour))
                {
                    neighbour.G_cost = newCostToNeighbour;
                    neighbour.H_cost = GetDistance(neighbour, targetBlock);
                    neighbour.Parent = block;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }
    }

    void RetracePath(Block startBlock, Block endBlock)
    {
        List<Block> path = new List<Block>();
        Block currentBlock = endBlock;

        while (currentBlock != startBlock)
        {
            path.Add(currentBlock);
            currentBlock = currentBlock.Parent;
        }
        path.Reverse();

        mapGenerator.path = path;

    }

    int GetDistance(Block blockA, Block blockB)
    {
        int dstX = Mathf.Abs(blockA.Position.x - blockB.Position.x);
        int dstY = Mathf.Abs(blockA.Position.y - blockB.Position.y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    public Block NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + (mapGenerator.mapSize.x * mapGenerator.tileSize) / 2) / (mapGenerator.mapSize.x * mapGenerator.tileSize);
        float percentY = (worldPosition.z + (mapGenerator.mapSize.y * mapGenerator.tileSize) / 2) / (mapGenerator.mapSize.y * mapGenerator.tileSize);
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((mapGenerator.mapSize.x - 1) * percentX);
        int y = Mathf.RoundToInt((mapGenerator.mapSize.y - 1) * percentY);
        return mapGenerator.blockMatrix[x, y];
    }

    public List<Block> GetNeighbours(Block block)
    {
        List<Block> neighbours = new List<Block>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                if (x == 0 || y == 0)
                {
                    int checkX = block.Position.x + x;
                    int checkY = block.Position.y + y;

                    if (checkX >= 0 && checkX < mapGenerator.mapSize.x && checkY >= 0 && checkY < mapGenerator.mapSize.y)
                    {
                        neighbours.Add(mapGenerator.blockMatrix[checkX, checkY]);
                    }
                }
            }
        }

        return neighbours;
    }

    IEnumerator waiter()
    {
        yield return new WaitForSecondsRealtime(5);
    }

    /* void OnDrawGizmos()
     {
         //Gizmos.DrawWireCube(transform.position, new Vector3(mapGenerator.mapSize.x, 1, mapGenerator.mapSize.y));

         if (mapGenerator.blockMatrix != null)
         {
             foreach (Block n in mapGenerator.blockMatrix)
             {
                 Gizmos.color = (n.Obstacle) ? Color.red : Color.white;
                 if (mapGenerator.path != null)
                     if (mapGenerator.path.Contains(n))
                         Gizmos.color = Color.black;
                 Gizmos.DrawCube(n.worldPosition + new Vector3(0,3,0), Vector3.one * (mapGenerator.tileSize - .1f));
             }
         }
     }*/
     
}