﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBuilder : MonoBehaviour {

    public int width = 250;
    public int height = 250;
    public int steps = 5;
    [Range(0,100)]
    public int randFillPercent;

    public string seed;
    public bool generateFromSeed;

    //1 = solid, 0 = empty
    int[,] mapValues;

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
        /*for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //not everything is the same because of weird stuff going on in MeshGenerator
                //pls make note to fix it
                if (x == 0 || y == 1 || x == (width - 2) || y == (height - 1))
                {
                    mapValues[x, y] = 1;
                }
            }
        }*/

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.OnGenerateMesh(borderMap, 1f);
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
        for (int i = x-1; i <= x+1; i++)
        {
            for (int j = y-1; j <= y+1; j++)
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

    private void OnDrawGizmos()
    {
        /*if (mapValues != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (mapValues[x, y] == 1) ? Color.black : Color.white;
                    //Gizmos.color = Color.gray;
                    Vector3 pos = new Vector3(-width/2 + x, -height/2 + y);
                    Gizmos.DrawCube(pos, Vector3.one * 0.5f);
                }
            }
        }*/
    }
}