using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block 
{
    public MapGenerator mapGenerator;
    public Vector3 worldPosition;
    public bool Obstacle;
    public bool Rotator;
    public Coord Position;
    public int G_cost;
    public int H_cost;
    public Block Parent;
    public Transform physicalBlock;

    public Block(bool _Obstacle, bool _Rotator, Coord _Position, int _G_cost, int _H_cost, MapGenerator _mapGenerator)
    {
        Obstacle = _Obstacle;
        Rotator = _Rotator;
        Position = _Position;
        G_cost = _G_cost;
        H_cost = _H_cost;
        mapGenerator = _mapGenerator;
        worldPosition = mapGenerator.CoordtoPosition(Position.x, Position.y);
        
        
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
