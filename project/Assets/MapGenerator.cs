using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MapGenerator : MonoBehaviour
{
    //PHOTON MULTIPLAYER VAR
    private PhotonView PV;

    //dodano
    public LayerMask collisionLayerMask;

    public List<Block> path;

    //pozicja start/finish
    public Transform newStart;
    public Transform newFinish;
    public Vector3 startPositionPathfinding;
    public Vector3 finishPositionPathfinding;

    public Transform tilePrefab;
    public Transform startPrefab;
    public Transform finishPrefab;
    public Transform levelEndPrefab;
    public Transform obstaclePrefab;
    public Transform rotator1Prefab;
    public Transform rotator2Prefab;
    public Transform rotator3Prefab;
    public Transform rotator4Prefab;
    public Vector2 mapSize;
    //public Transform navmeshFloor;
    public Vector2 maxMapSize;

    //win condition
    public int[] winMap;
    public bool youWin = false;
    //matrica blokova
    public Block[,] blockMatrix;

    [Range(0, 1)]
    public float obstaclePercent;

    [Range(0, 1)]
    public float outlinePercent;

    public float tileSize;

    List<Coord> allTileCoords;

    Queue<Coord> shuffledTileCoords;

    public int seed = 10;
    Coord mapCentre;




    void Start()
    {
        PV = GetComponent<PhotonView>();
        GenerateMap();

    }

    private void Update()
    {
        /*if (youWin == true)
        {
            Debug.Log("-----YOU WIN!-----");
        }*/
    }

    public void GenerateMap()
    {
        blockMatrix = new Block[(int)mapSize.x, (int)mapSize.y];
        allTileCoords = new List<Coord>();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
                blockMatrix[x, y] = new Block(false, false, new Block.Coord(x, y), 0, 0, this);
            }
        }

        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), seed));
        mapCentre = new Coord((int)mapSize.x / 2, (int)mapSize.y / 2);
        //Debug.Log(mapCentre.x + " center" + mapCentre.y);//print

        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;

        string tileHolderName = "Tiles";
        Transform tileHolder = new GameObject(tileHolderName).transform;
        tileHolder.parent = mapHolder;
        mapHolder.parent = transform;

        string levelEndHolderName = "Level End";
        Transform levelEndHolder = new GameObject(levelEndHolderName).transform;
        levelEndHolder.parent = mapHolder;

        for (int x = -1; x < mapSize.x + 1; x++)
        {
            for (int y = -1; y < mapSize.y + 1; y++)
            {
                /*Vector3 tilePosition = CoordtoPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition + Vector3.up * 2.0f, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = tileHolder;*/
            }
        }

        for (int x = -1; x < mapSize.x + 1; x++)
        {
            for (int y = -1; y < mapSize.y + 1; y++)
            {
                if ((x == -1 || y == -1) || (x == mapSize.x || y == mapSize.y))
                {
                    Vector3 levelEndPosition = CoordtoPosition(x, y);
                    Transform newlevelEnd = Instantiate(levelEndPrefab, levelEndPosition + Vector3.up * 1.75f, Quaternion.Euler(Vector3.right * 0)) as Transform;
                    newlevelEnd.localScale = Vector3.one * (1 - outlinePercent) * 1.41f;
                    newlevelEnd.parent = levelEndHolder;//mora se stvorit block i onda mora bit obstacle da se broji za counter za rotatore
                }
            }
        }

        bool[,] obstacleMap = new bool[(int)mapSize.x, (int)mapSize.y];

        int obstacleCount = (int)(mapSize.x * mapSize.y * obstaclePercent);
        int currentObstacleCount = 0;

        string obstacleHolderName = "Obstacles";
        Transform obstacleHolder = new GameObject(obstacleHolderName).transform;
        obstacleHolder.parent = mapHolder;

        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            blockMatrix[randomCoord.x, randomCoord.y].Obstacle = true;
            currentObstacleCount++;
            if (randomCoord != mapCentre && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
            {
                Vector3 obstaclePosition = CoordtoPosition(randomCoord.x, randomCoord.y);
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * 2.5f, Quaternion.identity) as Transform;
                newObstacle.parent = obstacleHolder;
                newObstacle.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                /*Vector3 printPos = CoordtoPosition(randomCoord.x,randomCoord.y);
				Debug.Log(printPos+"  pozicija");//print*/

            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                blockMatrix[randomCoord.x, randomCoord.y].Obstacle = false;
                currentObstacleCount--;
            }
        }
        //navmeshFloor.localScale = new Vector3(maxMapSize.x,maxMapSize.y)*tileSize;


        //PLACEHOLDER, STAVLJA START NA NAJBLIŽI RUB DONJEM LIJEVOM KUTU
        bool stopStartGeneration = false;
        for (int x = 0; x < mapSize.x && !stopStartGeneration; x++)
        {
            for (int y = 0; y < mapSize.y && !stopStartGeneration; y++)
            {
                if ((x == 0 || y == 0) || (x == mapSize.x - 1 || y == mapSize.y - 1))
                {
                    if (obstacleMap[x, y] == false)
                    {
                        Vector3 startPosition = CoordtoPosition(x, y);
                        newStart = Instantiate(startPrefab, startPosition + Vector3.up * 2.25f, Quaternion.Euler(Vector3.right * 90)) as Transform;
                        newStart.tag = "winPath";
                        newStart.localScale = Vector3.one;
                        newStart.parent = mapHolder;
                        startPositionPathfinding = CoordtoPosition(x, y);
                        stopStartGeneration = true;
                    }
                }
            }
        }

        //PLACEHOLDER FINISH
        bool stopFinishGeneration = false;
        int a, b;
        for (a = (int)mapSize.x - 1; a > 0 && !stopFinishGeneration; a--)
        {
            for (b = (int)mapSize.y - 1; b > 0 && !stopFinishGeneration; b--)
            {
                if ((a == mapSize.x - 1 || b == mapSize.y - 1))
                {
                    if (obstacleMap[a, b] == false)
                    {
                        Vector3 finishPosition = CoordtoPosition(a, b);
                        newFinish = Instantiate(finishPrefab, finishPosition + Vector3.up * 2.25f, Quaternion.Euler(Vector3.right * 90)) as Transform;
                        newFinish.tag = "winPath";
                        newFinish.localScale = Vector3.one ;
                        newFinish.parent = mapHolder;
                        finishPositionPathfinding = CoordtoPosition(a, b);
                        stopFinishGeneration = true;
                    }
                }
            }
        }
        //KEAJ SVEGA

        generateRotators(path, mapHolder);
    }


    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(mapCentre);
        mapFlags[mapCentre.x, mapCentre.y] = true;
        int accessibleTileCount = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                    if (x == 0 || y == 0)
                    {
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                        {
                            if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                            {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new Coord(neighbourX, neighbourY));
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }

        int targetAccessibleTileCount = (int)(mapSize.x * mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    //nova fja winpath: path taga "winpath"
    public void setWinPath(List<Block> path)
    {
        for (int q = 0; q < path.Count - 1; q++)
        {
            Transform temp = path[q].physicalBlock;
            temp.tag = "winPath";
            if (temp.childCount == 2)
            {
                Transform child1 = temp.GetChild(0);
                child1.tag = "winPath";
                Transform child2 = temp.GetChild(1);
                child2.tag = "winPath";
            }
            else if(temp.childCount == 3){
                Transform child1 = temp.GetChild(0);
                child1.tag = "winPath";
                Transform child2 = temp.GetChild(1);
                child2.tag = "winPath";
                Transform child3 = temp.GetChild(2);
                child3.tag = "winPath";
            }
            else if (temp.childCount == 4)
            {
                Transform child1 = temp.GetChild(0);
                child1.tag = "winPath";
                Transform child2 = temp.GetChild(1);
                child2.tag = "winPath";
                Transform child3 = temp.GetChild(2);
                child3.tag = "winPath";
                Transform child4 = temp.GetChild(3);
                child4.tag = "winPath";
            }
            else
            {
                Transform temp2 = temp.GetChild(0);
                temp2.tag = "winPath";
            }
        }
    }

    public void setUnreachableRotatorsOff()
    {
        for (int a = 0; a < mapSize.x; a++)
        {
            for (int b = 0; b < mapSize.y; b++) //if(!myString.Equals("-1"))
            {
                if (blockMatrix[a, b].Rotator == true && !blockMatrix[a, b].physicalBlock.tag.Equals("winPath"))
                {
                    BoxCollider[] boxCollList = blockMatrix[a, b].physicalBlock.GetComponentsInChildren<BoxCollider>();
                    for (int i = 1; i < boxCollList.Length; i++)
                    {
                        boxCollList[i].enabled = false;
                    }
                }
            }
        }
    }

    //NOVA FJA ROTATORI
    void generateRotators(List<Block> path, Transform mapHolder)
    {
        string rotatorHolderName = "Rotators";
        Transform rotatorHolder = new GameObject(rotatorHolderName).transform;
        rotatorHolder.parent = mapHolder;

        for (int a = 0; a < mapSize.x; a++)
        {
            for (int b = 0; b < mapSize.y; b++)
            {
                int neighbourCounter = 0;
                bool flagLeft = false, flagRight = false, flagUp = false, flagDown = false;
                if (blockMatrix[a, b].Obstacle == false)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            int neighbourX = blockMatrix[a, b].Position.x + x;
                            int neighbourY = blockMatrix[b, b].Position.y + y;
                            if (x == 0 || y == 0)
                            {
                                if ((neighbourX == -1) || (neighbourX == mapSize.x + 1) || (neighbourY == -1) || (neighbourY == mapSize.y + 1))
                                {
                                    neighbourCounter++;
                                }
                                if (neighbourX >= 0 && neighbourX < mapSize.x && neighbourY >= 0 && neighbourY < mapSize.y)
                                {
                                    if (blockMatrix[neighbourX, neighbourY].Obstacle == true)
                                    {
                                        neighbourCounter++;
                                    }
                                    if (x == -1)
                                    {
                                        if (blockMatrix[neighbourX, neighbourY].Obstacle == true)
                                        {
                                            flagLeft = true;
                                        }
                                    }
                                    if (x == 1)
                                    {
                                        if (blockMatrix[neighbourX, neighbourY].Obstacle == true)
                                        {
                                            flagRight = true;
                                        }
                                    }
                                    if (y == -1)
                                    {
                                        if (blockMatrix[neighbourX, neighbourY].Obstacle == true)
                                        {
                                            flagDown = true;
                                        }
                                    }
                                    if (y == 1)
                                    {
                                        if (blockMatrix[neighbourX, neighbourY].Obstacle == true)
                                        {
                                            flagUp = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (neighbourCounter == 3 && blockMatrix[a, b].worldPosition != startPositionPathfinding && blockMatrix[a, b].worldPosition != finishPositionPathfinding)
                {
                    if (blockMatrix[a, b].Rotator == false)
                    {
                        //Transform newRotator1 = Instantiate(rotator1Prefab, blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators2())) as Transform;
                        GameObject newRotator1 = PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs", "Rotator_01"), blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators2()), 0);
                        newRotator1.transform.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                        blockMatrix[a, b].Rotator = true;
                        blockMatrix[a, b].physicalBlock = newRotator1.transform;
                        newRotator1.transform.parent = rotatorHolder;
                    }
                }
                if (((flagLeft == true && flagRight == true) || (flagUp == true && flagDown == true)) && blockMatrix[a, b].worldPosition != startPositionPathfinding && blockMatrix[a, b].worldPosition != finishPositionPathfinding)
                {
                    if (blockMatrix[a, b].Rotator == false)
                    {
                        //Transform newRotator1 = Instantiate(rotator1Prefab, blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators2())) as Transform;
                        GameObject newRotator1 = PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs", "Rotator_01"), blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators2()), 0);
                        newRotator1.transform.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                        blockMatrix[a, b].Rotator = true;
                        blockMatrix[a, b].physicalBlock = newRotator1.transform;
                        newRotator1.transform.parent = rotatorHolder;
                    }
                }
                if (((flagLeft == true && flagUp == true) || (flagDown == true && flagLeft == true) || (flagRight == true && flagDown == true) || (flagRight == true && flagUp == true)) && blockMatrix[a, b].worldPosition != startPositionPathfinding && blockMatrix[a, b].worldPosition != finishPositionPathfinding)
                {
                    if (blockMatrix[a, b].Rotator == false)
                    {
                        //Transform newRotator2 = Instantiate(rotator2Prefab, blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4())) as Transform;
                        GameObject newRotator2 = PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs", "Rotator_02"), blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4()), 0);
                        newRotator2.transform.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                        blockMatrix[a, b].Rotator = true;
                        blockMatrix[a, b].physicalBlock = newRotator2.transform;
                        newRotator2.transform.parent = rotatorHolder;
                    }
                }
                if (neighbourCounter == 0 && blockMatrix[a, b].worldPosition != startPositionPathfinding && blockMatrix[a, b].worldPosition != finishPositionPathfinding)
                {
                    if (blockMatrix[a, b].Obstacle == false && blockMatrix[a, b].Rotator == false)
                    {
                        //Transform newRotator4 = Instantiate(rotator4Prefab, blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.right * 0)) as Transform;
                        GameObject newRotator4 = PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs", "Rotator_04"), blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.right * 0), 0);
                        newRotator4.transform.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                        blockMatrix[a, b].Rotator = true;
                        blockMatrix[a, b].physicalBlock = newRotator4.transform;
                        newRotator4.transform.parent = rotatorHolder;
                    }
                }
                if (blockMatrix[a, b].Position.y == 0)
                {
                    if (blockMatrix[a, b].Rotator == false && blockMatrix[a, b].Obstacle == false && blockMatrix[a, b].worldPosition != startPositionPathfinding && blockMatrix[a, b].worldPosition != finishPositionPathfinding)
                    {
                        if (flagUp == true)
                        {
                            //Transform newRotator1 = Instantiate(rotator1Prefab, blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators2())) as Transform;
                            GameObject newRotator1 = PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs", "Rotator_01"), blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators2()), 0);
                            newRotator1.transform.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                            blockMatrix[a, b].Rotator = true;
                            blockMatrix[a, b].physicalBlock = newRotator1.transform;
                            newRotator1.transform.parent = rotatorHolder;
                        }
                        else if (flagUp == false)
                        {
                            //Transform newRotator2 = Instantiate(rotator2Prefab, blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4())) as Transform;
                            GameObject newRotator2 = PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs", "Rotator_02"), blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4()), 0);
                            newRotator2.transform.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                            blockMatrix[a, b].Rotator = true;
                            blockMatrix[a, b].physicalBlock = newRotator2.transform;
                            newRotator2.transform.parent = rotatorHolder;
                        }
                        else
                        {
                            //Transform newRotator3 = Instantiate(rotator3Prefab, blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4())) as Transform;
                            GameObject newRotator3 = PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs", "Rotator_03"), blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4()), 0);
                            newRotator3.transform.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                            blockMatrix[a, b].Rotator = true;
                            blockMatrix[a, b].physicalBlock = newRotator3.transform;
                            newRotator3.transform.parent = rotatorHolder;
                        }
                    }
                }
                if (blockMatrix[a, b].Position.y == mapSize.y - 1)
                {
                    if (blockMatrix[a, b].Rotator == false && blockMatrix[a, b].Obstacle == false && blockMatrix[a, b].worldPosition != startPositionPathfinding && blockMatrix[a, b].worldPosition != finishPositionPathfinding)
                    {
                        if (flagDown == true)
                        {
                            //Transform newRotator1 = Instantiate(rotator1Prefab, blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators2())) as Transform;
                            GameObject newRotator1 = PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs", "Rotator_01"), blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators2()), 0);
                            newRotator1.transform.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                            blockMatrix[a, b].Rotator = true;
                            blockMatrix[a, b].physicalBlock = newRotator1.transform;
                            newRotator1.transform.parent = rotatorHolder;
                        }
                        else if (flagDown == false)
                        {
                            //Transform newRotator2 = Instantiate(rotator2Prefab, blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4())) as Transform;
                            GameObject newRotator2 = PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs", "Rotator_02"), blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4()), 0);
                            newRotator2.transform.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                            blockMatrix[a, b].Rotator = true;
                            blockMatrix[a, b].physicalBlock = newRotator2.transform;
                            newRotator2.transform.parent = rotatorHolder;
                        }
                        else
                        {
                            //Transform newRotator3 = Instantiate(rotator3Prefab, blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4())) as Transform;
                            GameObject newRotator3 = PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs", "Rotator_03"), blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4()), 0);
                            newRotator3.transform.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                            blockMatrix[a, b].Rotator = true;
                            blockMatrix[a, b].physicalBlock = newRotator3.transform;
                            newRotator3.transform.parent = rotatorHolder;
                        }
                    }
                }
                if (blockMatrix[a, b].Position.x == 0)
                {
                    if (blockMatrix[a, b].Rotator == false && blockMatrix[a, b].Obstacle == false && blockMatrix[a, b].worldPosition != startPositionPathfinding && blockMatrix[a, b].worldPosition != finishPositionPathfinding)
                    {
                        if (flagRight == true)
                        {
                            //Transform newRotator1 = Instantiate(rotator1Prefab, blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators2())) as Transform;
                            GameObject newRotator1 = PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs", "Rotator_01"), blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators2()), 0);
                            newRotator1.transform.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                            blockMatrix[a, b].Rotator = true;
                            blockMatrix[a, b].physicalBlock = newRotator1.transform;
                            newRotator1.transform.parent = rotatorHolder;
  
                        }
                        else if (flagRight == false)
                        {
                            //Transform newRotator2 = Instantiate(rotator2Prefab, blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4())) as Transform;
                            GameObject newRotator2 = PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs", "Rotator_02"), blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4()), 0);
                            newRotator2.transform.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                            blockMatrix[a, b].Rotator = true;
                            blockMatrix[a, b].physicalBlock = newRotator2.transform;
                            newRotator2.transform.parent = rotatorHolder;
                        }
                        else
                        {
                            //Transform newRotator3 = Instantiate(rotator3Prefab, blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4())) as Transform;
                            GameObject newRotator3 = PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs", "Rotator_03"), blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4()), 0);
                            newRotator3.transform.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                            blockMatrix[a, b].Rotator = true;
                            blockMatrix[a, b].physicalBlock = newRotator3.transform;
                            newRotator3.transform.parent = rotatorHolder;
                        }
                    }
                }
                if (blockMatrix[a, b].Position.x == mapSize.x - 1)
                {
                    if (blockMatrix[a, b].Rotator == false && blockMatrix[a, b].Obstacle == false && blockMatrix[a, b].worldPosition != startPositionPathfinding && blockMatrix[a, b].worldPosition != finishPositionPathfinding)
                    {
                        if (flagLeft == true)
                        {
                            //Transform newRotator1 = Instantiate(rotator1Prefab, blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators2())) as Transform;
                            GameObject newRotator1 = PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs", "Rotator_01"), blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators2()), 0);
                            newRotator1.transform.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                            blockMatrix[a, b].Rotator = true;
                            blockMatrix[a, b].physicalBlock = newRotator1.transform;
                            newRotator1.transform.parent = rotatorHolder;
                        }
                        else if (flagLeft == false)
                        {
                            //Transform newRotator2 = Instantiate(rotator2Prefab, blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4())) as Transform;
                            GameObject newRotator2 = PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs", "Rotator_02"), blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4()), 0);
                            newRotator2.transform.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                            blockMatrix[a, b].Rotator = true;
                            blockMatrix[a, b].physicalBlock = newRotator2.transform;
                            newRotator2.transform.parent = rotatorHolder;
                        }
                        else
                        {
                            //Transform newRotator3 = Instantiate(rotator3Prefab, blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4())) as Transform;
                            GameObject newRotator3 = PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs", "Rotator_03"), blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4()), 0);
                            newRotator3.transform.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                            blockMatrix[a, b].Rotator = true;
                            blockMatrix[a, b].physicalBlock = newRotator3.transform;
                            newRotator3.transform.parent = rotatorHolder;
                        }
                    }
                }
                if (neighbourCounter == 1 && blockMatrix[a, b].worldPosition != startPositionPathfinding && blockMatrix[a, b].worldPosition != finishPositionPathfinding)
                {
                    if (blockMatrix[a, b].Rotator == false)
                    {
                        if (blockMatrix[a, b].Position.x != 0 && blockMatrix[a, b].Position.y != 0)
                        {
                            if (blockMatrix[a, b].Position.x != mapSize.x && blockMatrix[a, b].Position.y != mapSize.y)
                            {
                                //Transform newRotator3 = Instantiate(rotator3Prefab, blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4())) as Transform;
                                GameObject newRotator3 = PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs", "Rotator_03"), blockMatrix[a, b].worldPosition + Vector3.up * 2.5f, Quaternion.Euler(Vector3.up * randomRotateRotators4()), 0);
                                newRotator3.transform.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                                blockMatrix[a, b].Rotator = true;
                                blockMatrix[a, b].physicalBlock = newRotator3.transform;
                                newRotator3.transform.parent = rotatorHolder;
                            }
                        }
                    }
                }
            }
        }
    }


    public int randomRotateRotators2()
    {
        int[] angles = new int[2] { 0, 90 };
        System.Random r = new System.Random();
        int genRand = r.Next(0, 2);
        return angles[genRand];
    }
    public int randomRotateRotators4()
    {
        int[] angles = new int[4] { 0, 90, 180, 270 };
        System.Random r = new System.Random();
        int genRand = r.Next(0, 4);
        return angles[genRand];
    }
    IEnumerator waiter()
    {
        yield return new WaitForSecondsRealtime(5);
    }

    public Vector3 CoordtoPosition(int x, int y)
    {
        return new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y) * tileSize;
    }


    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }


    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }
    }
}