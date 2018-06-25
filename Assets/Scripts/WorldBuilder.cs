using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBuilder : MonoBehaviour {

    public int width = 250;
    public int height = 250;
    public int steps = 5;
    public int minRoomSize = 100;
    [Range(0,100)]
    public int randFillPercent;

    public string seed;
    public bool generateFromSeed;

    //1 = solid, 0 = empty
    int[,] mapValues;

    bool drawOnce = false;

    // Use this for initialization
    void Awake () {
        generate();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            generate();
        }
    }

    void generate()
    {
        if (generateFromSeed)
            seed = System.DateTime.Now.ToString();

        System.Random pseudoRand = new System.Random(seed.GetHashCode());

        mapValues = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                mapValues[x, y] = (pseudoRand.Next(0, 100) < randFillPercent) ? 1 : 0;
            }
        }

        for (int i = 0; i < steps; i++)
        {
            smooth();
        }

        processRegions();

        int borderWidth = 10;
        int[,] borderMap = new int[width + borderWidth * 2, height + borderWidth * 2];

        //make sure the outer edges of the map are closed off
        for (int x = 0; x < borderMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderMap.GetLength(1); y++)
            {
                if (x >= borderWidth && x < width + borderWidth && y >= borderWidth && y < height + borderWidth) {
                    borderMap[x, y] = mapValues[x - borderWidth, y - borderWidth];
                }
                else
                {
                    borderMap[x, y] = 1;
                }
            }
        }

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.OnGenerateMesh(borderMap, 1f);
    }

    void processRegions()
    {
        List<List<Coord>> rooms = getRegions(0);
        rooms.Sort(delegate (List<Coord> regionOne, List<Coord> regionTwo)
        {
            return regionOne.Count.CompareTo(regionTwo.Count);
        });

        foreach (List<Coord> room in rooms)
        {
            if (room.Count < minRoomSize)
            {
                foreach (Coord tile in room)
                {
                    mapValues[tile.tileX, tile.tileY] = 1;
                }
                Debug.Log("room closed");
                //rooms.Remove(room);
            }
        }
    }

    List<List<Coord>> getRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && mapValues[x, y] == tileType)
                {
                    List<Coord> newRegion = getRegionTiles(x, y);
                    regions.Add(newRegion);

                    //mark every tile as checked
                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    //find out how many same-type tiles are in a specific region of the world
    List<Coord> getRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];   //determine if we've already checked an value
        int tileType = mapValues[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            //loop through the surrounding tiles
            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (isInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && mapValues[x, y] == tileType)   //if tile hasn't been checked and is the same type as the origin tile
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            } 
        }

        return tiles;
    }

    bool isInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    //smooth out the random noise
    void smooth()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int numWalls = countWalls(x, y);

                if (numWalls > 4)
                {
                    mapValues[x, y] = 1;
                } else if (numWalls < 4)
                {
                    mapValues[x, y] = 0;
                }
            }
        }
    }

    int countWalls(int x, int y)
    {
        int changedValue = 0;

        //loop through all surrounding values
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                //add up values here
                if (i >= 0 && i < width && j >= 0 && j < height)
                {
                    if (i != x || j != y)
                    {
                        changedValue += mapValues[i, j];
                    }
                }
                //have the boundaries count as walls
                else
                {
                    changedValue++;
                }
            }
        }

        return changedValue;
    }

    struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    private void OnDrawGizmos()
    {
        if (mapValues != null)
        {
            /*for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (mapValues[x, y] == 1) ? Color.black : Color.white;
                    //Gizmos.color = Color.gray;
                    Vector3 pos = new Vector3(-width/2 + x, -height/2 + y);
                    Gizmos.DrawCube(pos, Vector3.one * 0.5f);
                }
            }*/
            
            List<List<Coord>> rooms = getRegions(0);
            rooms.Sort(delegate(List<Coord> regionOne, List<Coord> regionTwo)
            {
                return regionOne.Count.CompareTo(regionTwo.Count);
            });

            for (int i = 0; i < rooms.Count; i++)
            {
                if (i == 0)
                    Gizmos.color = Color.green;
                else if (i == rooms.Count - 1)
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = Color.yellow;
                //Gizmos.color = Random.ColorHSV();

                foreach (Coord tile in rooms[i])
                {
                    Vector3 pos = new Vector3(-width / 2 + tile.tileX + 0.5f, -height / 2 + tile.tileY + 0.5f);
                    Gizmos.DrawCube(pos, Vector3.one * 0.5f);
                }
            }
            
        }
    }
}