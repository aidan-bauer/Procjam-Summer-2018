using System.Collections;
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
            seed = Time.time.ToString();

        System.Random pseudoRand = new System.Random(seed.GetHashCode());

        mapValues = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //mapValues[x, y] = coinFlip();
                mapValues[x, y] = (pseudoRand.Next(0, 100) < randFillPercent) ? 1 : 0;
            }
        }

        for (int i = 0; i < steps; i++)
        {
            smooth();
        }

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.OnGenerateMesh(mapValues, 1f);
    }

    int coinFlip()
    {
        float coin = Random.Range(0f, 1f);
        int headsOrTails = (coin < 0.5) ? 1 : 0;

        return headsOrTails;
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
                    //Gizmos.color = (mapValues[x, y] == 1) ? Color.black : Color.white;
                    Gizmos.color = Color.gray;
                    Vector3 pos = new Vector3(-width/2 + x, -height/2 + y);
                    Gizmos.DrawCube(pos, Vector3.one * 0.5f);
                }
            }
        }*/
    }
}