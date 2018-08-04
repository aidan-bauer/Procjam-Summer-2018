using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBuilder : MonoBehaviour
{
    public bool isTwoDimensional = false;
    public bool debug_canSpawnLights = true;

    public int width = 250;
    public int height = 250;
    public int steps = 5;
    public int minRoomSize = 100;
    [Range(0,100)]
    public int randFillPercent;

    public string seed;
    public bool generateFromSeed;

    public GameObject player;
    public GameObject fungus;
    private List<GameObject> fungi = new List<GameObject>();

    //1 = solid, 0 = empty
    int[,] mapValues;
    List<List<Coord>> rooms = new List<List<Coord>>();

    // Use this for initialization
    void Awake () {
        generate();
    }

   /* private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            generate();
        }
    }*/

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

        //delete all existing lights
        for (int i = 0; i < fungi.Count; i++)
        {
            Destroy(fungi[i]);
        }

        fungi.Clear();

        spawnPlayer();

        if (debug_canSpawnLights)
            populateLights();
    }

    void processRegions()
    {
        List<Room> survingRooms = new List<Room>();
        rooms = getRegions(0);

        foreach (List<Coord> room in rooms)
        {
            //Debug.Log(room.Count);
            if (room.Count < minRoomSize)
            {
                foreach (Coord tile in room)
                {
                    mapValues[tile.tileX, tile.tileY] = 1;
                }

                Debug.Log("room closed");
            }

        }

        //reload rooms after removing all the small ones and sort by size
        rooms = getRegions(0);
        rooms.Sort(delegate (List<Coord> regionOne, List<Coord> regionTwo)
        {
            return regionOne.Count.CompareTo(regionTwo.Count);
        });

        foreach (List<Coord> room in rooms)
        {
            survingRooms.Add(new Room(room, mapValues));
        }

        survingRooms.Sort();

        survingRooms[0].isMainRoom = true;
        survingRooms[0].isAccessibleFromMainRoom = true;
        connectRooms(survingRooms);
    }

    void connectRooms(List<Room> allRooms, bool forceAccissibilityFromMainRoom = false)
    {
        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccissibilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        } else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccissibilityFromMainRoom)
            {
                possibleConnectionFound = false;

                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.isConnected(roomB))
                {
                    continue;
                }

                /*if (roomA.isConnected(roomB))
                {
                    possibleConnectionFound = false;
                    break;
                }*/

                

                //loop through all rooms and find the shortest connection beween two rooms
                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            if (possibleConnectionFound && !forceAccissibilityFromMainRoom)
            {
                createPassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccissibilityFromMainRoom)
        {
            createPassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            connectRooms(allRooms, true);
        }

        if (!forceAccissibilityFromMainRoom)
        {
            connectRooms(allRooms, true);
        }
    }


    void createPassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.connectRooms(roomA, roomB);
        Debug.DrawLine(coordToWorldPoint(tileA), coordToWorldPoint(tileB), Color.green, 1000);

        List<Coord> passage = getLine(tileA, tileB);
        foreach (Coord tile in passage)
        {
            drawCircle(tile, 1);
        }
    }

    //get all tiles in the radius of a tile and change them
    void drawCircle(Coord c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                int drawX = c.tileX + x;
                int drawY = c.tileY + y;

                if (isInMapRange(drawX, drawX))
                {
                    mapValues[drawX, drawY] = 0;
                }
            }
        }
    }

    List<Coord> getLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();
        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;     //change in x
        int dy = to.tileY - from.tileY;     //change in y

        bool inverted = false;
        int step = Math.Sign(dx);           //always increases by 1
        int gradientStep = Math.Sign(dy);   //

        int longest = Math.Abs(dx);
        int shortest = Math.Abs(dy);

        //if the change in y position is greater than the change in x
        //invert the x and y values in the formula
        if (longest < shortest)
        {
            inverted = true;
            longest = Math.Abs(dy);
            shortest = Math.Abs(dx);
            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;

        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            } else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                } else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    Vector3 coordToWorldPoint(Coord tile)
    {
        return new Vector3(-width / 2 + 0.5f + tile.tileX, -height / 2 + 0.5f + tile.tileY, 0);
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

    public bool isInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    //assemble the objects in the game world
    void spawnPlayer()
    {
        List<Coord> playerRoom = rooms[0];
        List<Coord> playerSpawnPlaces = new List<Coord>();
        int spawnZ = 0;

        if (!isTwoDimensional)
            spawnZ = 2;

        foreach (Coord tile in playerRoom)
        {
            int[,] neighborTiles = getSurroundingTiles(tile.tileX, tile.tileY);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //check to make sure we have an actual value to check
                    if (neighborTiles[i, j] != -1)
                    {
                        //if this tile has no ground beneath it and no walls on either side, add it for consideration
                        if (neighborTiles[1, 2] == 0 &&
                        neighborTiles[0, 1] == 0 &&
                        neighborTiles[2, 1] == 0 &&
                        neighborTiles[1, 0] == 1)
                        {
                            playerSpawnPlaces.Add(tile);
                        }
                    }
                }
            }
        }

        Coord playerSpawnLoc = playerSpawnPlaces[UnityEngine.Random.Range(0, playerSpawnPlaces.Count)];
        //add pos to -dimension / 2 to get accurate placement 
        /*GameObject playerInst = Instantiate(player, new Vector3((-width / 2) + playerSpawnLoc.tileX, 
            (-height / 2) + playerSpawnLoc.tileY, spawnZ), 
            Quaternion.identity);*/
        GameObject playerInst = Instantiate(player, coordToWorldPoint(playerSpawnLoc), Quaternion.identity);
        Camera.main.GetComponent<CameraFollow>().target = playerInst.transform;
    }

    void populateLights()
    {
        List<Coord> lightSpawnPlaces = new List<Coord>();

        foreach (List<Coord> room in rooms)
        {
            foreach (Coord tile in room)
            {
                int[,] neighborTiles = getSurroundingTiles(tile.tileX, tile.tileY);

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        //check to make sure we have an actual value to check
                        if (neighborTiles[i, j] != -1)
                        {
                            if (neighborTiles[1, 2] == 1)
                            {
                                lightSpawnPlaces.Add(tile);
                                //GameObject fungusInst = Instantiate(fungus, coordToWorldPoint(tile), Quaternion.identity);
                                //fungi.Add(fungusInst);
                            }
                        }
                    }
                }
            }
        }

        for (int i = 0; i < lightSpawnPlaces.Count; i += 15)
        {
            GameObject fungusInst = Instantiate(fungus, coordToWorldPoint(lightSpawnPlaces[i]), Quaternion.identity);
            fungi.Add(fungusInst);
        }
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

    int[,] getSurroundingTiles(int x, int y)
    {
        //[0, 0] [1, 0] [2, 0]
        //[0, 1] [1, 1] [2, 1]
        //[0, 2] [1, 2] [2, 2]

        int[,] surroundingTiles = new int[3, 3];

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (isInMapRange(x + i - 1, y + j - 1))
                    surroundingTiles[i, j] = mapValues[x + i - 1, y + j - 1];
                else
                    surroundingTiles[i, j] = -1;
            }
        }

        return surroundingTiles;
    }


    //hold map point coordinates
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

    //storage class for open areas aka rooms
    class Room : IComparable<Room>
    {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        //empty constructor
        public Room() { }

        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();
            edgeTiles = new List<Coord>();

            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            try
                            {
                                if (map[x, y] == 1)
                                {
                                    edgeTiles.Add(tile);
                                }
                            }
                            catch (System.IndexOutOfRangeException e)
                            {
                                Debug.Log("Edge tile assignment failed at "+ x + ", " + y);
                                //break;
                            }
                        }
                    }
                }
            }
        }

        //update this room, and have it check all the other rooms
        public void setAccessibleFromMainRoom ()
        {
            if (!isAccessibleFromMainRoom)
            {
                isAccessibleFromMainRoom = true;
                foreach (Room room in connectedRooms)
                {
                    room.setAccessibleFromMainRoom();
                }
            }
        }

        public static void connectRooms(Room roomA, Room roomB)
        {
            if (roomA.isAccessibleFromMainRoom)
            {
                roomB.setAccessibleFromMainRoom();
            }
            else if (roomB.isAccessibleFromMainRoom)
            {
                roomA.setAccessibleFromMainRoom(); 
            }

            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool isConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }

    /*private void OnDrawGizmos()
    {
        if (mapValues != null)
        {   
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
                    if (i == 0)
                    {
                        int[,] neighborTiles = getSurroundingTiles(tile.tileX, tile.tileY);
                        for (int k = 0; k < 3; k++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                //if this tile has no ground beneath it and no walls on either side, add it for consideration
                                if (neighborTiles[1, 0] == 1 &&
                                    neighborTiles[0, 1] == 0 &&
                                    neighborTiles[2, 1] == 0 &&
                                    neighborTiles[1, 2] == 0)
                                {
                                    Gizmos.color = Color.blue;
                                }
                                else
                                {
                                    Gizmos.color = Color.green;
                                }
                            }
                        }
                    }

                    Vector3 pos = new Vector3(-width / 2 + tile.tileX + 1f, -height / 2 + tile.tileY);
                    Gizmos.DrawCube(pos, Vector3.one * 0.5f);
                }
            }
        }
    }*/
}