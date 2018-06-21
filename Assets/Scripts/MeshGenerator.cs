using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {

    public SquareGrid squareGrid;

    public void OnGenerateMesh(int[,] map, float squareSize)
    {
        squareGrid = new SquareGrid(map, squareSize);
    }

    private void OnDrawGizmos()
    {
        if (squareGrid != null)
        {
            for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
            {
                for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
                {
                    Gizmos.color = squareGrid.squares[x, y].topLeft.active ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].topLeft.pos, Vector3.one * 0.25f);

                    Gizmos.color = squareGrid.squares[x, y].topRight.active ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].topRight.pos, Vector3.one * 0.25f);

                    Gizmos.color = squareGrid.squares[x, y].bottomRight.active ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].bottomRight.pos, Vector3.one * 0.25f);

                    Gizmos.color = squareGrid.squares[x, y].bottomLeft.active ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].bottomLeft.pos, Vector3.one * 0.25f);
                }
            }
        }
    }
}

public class SquareGrid
{
    public Square[,] squares;

    public SquareGrid(int [,] map, float squareSize)
    {
        int nodeCountX = map.GetLength(0);
        int nodeCountY = map.GetLength(1);
        float mapWidth = nodeCountX * squareSize;
        float mapHeight = nodeCountY * squareSize;

        //creat the nodes
        Node[,] nodes = new Node[nodeCountX, nodeCountY];
        for (int x = 0; x < nodeCountX; x++)
        {
            for (int y = 0; y < nodeCountY; y++)
            {
                Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize * 0.5f, 
                    -mapHeight / 2 + y * squareSize + squareSize * 0.5f, 0);
                nodes[x, y] = new Node(pos, map[x, y] == 1);    //second parameter will return a bool
            }
        }

        //assign the nodes to their correct squares
        squares = new Square[nodeCountX - 1, nodeCountY - 1];
        for (int x = 0; x < squares.GetLength(0); x++)
        {
            for (int y = 0; y < squares.GetLength(1); y++)
            {
                squares[x, y] = new Square(nodes[x, y + 1], nodes[x + 1, y + 1], nodes[x + 1, y], nodes[x, y]);
            }
        }
    }
}

public class Square
{
    public Node topLeft, topRight, bottomRight, bottomLeft;

    public Square(Node _topLeft, Node _topRight, Node _bottomRight, Node _bottomLeft)
    {
        topLeft = _topLeft;
        topRight = _topRight;
        bottomRight = _bottomRight;
        bottomLeft = _bottomLeft;
    }
}

public class Node
{
    public Vector3 pos;
    public bool active;
    public int vertexInt = -1;

    public Node(Vector3 _pos, bool _active)
    {
        pos = _pos;
        active = _active;
    }
}
