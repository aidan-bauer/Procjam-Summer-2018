using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {

    public SquareGrid squareGrid;
    public MeshFilter walls;
    public MeshCollider wallCollider;
    public float wallHeight = 5;

    List<Vector3> vertices;
    List<Vector2> uv;
    List<Vector3> normals;
    List<int> triangles;

    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checkedVertices = new HashSet<int>();

    Mesh mesh;
    MeshCollider meshCollider;

    public void OnGenerateMesh(int[,] map, float squareSize)
    {
        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();

        //create the list of nodes
        squareGrid = new SquareGrid(map, squareSize);

        vertices = new List<Vector3>();
        uv = new List<Vector2>();
        normals = new List<Vector3>();
        triangles = new List<int>();

        mesh = GetComponent<MeshFilter>().mesh = new Mesh();
        mesh.name = "Procedural Cave";
        //surface = GetComponent<NavMeshSurface>();

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                FillSquare(squareGrid.squares[x, y]);
            }
        }

        meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        CreateWallMesh();
    }

    void createWallMesh()
    {
        calculateMeshOutlines();

        List<Vector3> wallVerts = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();
        wallMesh.name = "Cave Walls";

        foreach(List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVerts.Count;

                //generate the four vertices
                wallVerts.Add(vertices[outline[i]]);        //left vertex
                wallVerts.Add(vertices[outline[i + 1]]);    //right vertex
                wallVerts.Add(vertices[outline[i]] - Vector3.forward * wallHeight);        //bottom left vertex
                wallVerts.Add(vertices[outline[i + 1]] - Vector3.forward * wallHeight);    //bottom right vertex

                //triangle 1
                wallTriangles.Add(startIndex + 0);      //topleft
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                //triangle 2
                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }

        wallMesh.vertices = wallVerts.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        walls.sharedMesh = wallCollider.sharedMesh = wallMesh;
        walls.transform.position = Vector3.forward * wallHeight;
    }

    void FillSquare(Square square)
    {
        switch (square.config)
        {
            case 0:
                break;
            case 1:
                AssignVertices(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                createTriangle(square.topLeft, square.topRight, square.bottomRight);
                createTriangle(square.topLeft, square.bottomRight, square.bottomLeft);
                break;
        }
    }

    //create vertices and assign them in order in the mesh
    void AssignVertices(params Node[] nodes)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].vertexIndex == -1)
            {
                nodes[i].vertexIndex = vertices.Count;
                vertices.Add(nodes[i].pos);
            }
        }
    }

    void createTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        addTriangleToDictionary(triangle.vertexIndexA, triangle);
        addTriangleToDictionary(triangle.vertexIndexB, triangle);
        addTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    //add to array or override an existing triangle
    void addTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (triangleDictionary.ContainsKey(vertexIndexKey))
        {
            triangleDictionary[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    //go through every vertex in the map, see if it an outline vertex
    //if it is, follow that outline until it meets up with itself again
    void calculateMeshOutlines()
    {
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
        {
            if (!checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = getConnectedOutlineVertex(vertexIndex);

                if (newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int> ();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    followOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }


    void followOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = getConnectedOutlineVertex(vertexIndex);

        if (nextVertexIndex != -1)
        {
            followOutline(nextVertexIndex, outlineIndex);
        }
    }

    //if given a vertex, find another vertex that forms an outline edge (two vertexes that only share one triangle)
    int getConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesWithVertexIndex = triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesWithVertexIndex.Count; i++)
        {
            Triangle triangle = trianglesWithVertexIndex[i];

            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];

                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexIndex))
                    if (isOutlineEdge(vertexIndex, vertexB))
                        return vertexB;
            }
        }

        return -1;
    }

    //if two vertexes share only one triangle then they are an edge
    bool isOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesWithVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesWithVertexA.Count; i++)
        {
            if (trianglesWithVertexA[i].Contains(vertexB)) {
                sharedTriangleCount++;

                if (sharedTriangleCount > 1)
                    break;
            }
        }

        return sharedTriangleCount == 1;
    }

    //Seb's functions, they're different because they're capitalized
    void CreateWallMesh()
    {

        CalculateMeshOutlines();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();
        wallMesh.name = "Seb's Cave Walls";
        //float wallHeight = 5;

        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++) {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]); // left
                wallVertices.Add(vertices[outline[i + 1]]); // right
                wallVertices.Add(vertices[outline[i]] - Vector3.forward * wallHeight); // bottom left
                wallVertices.Add(vertices[outline[i + 1]] - Vector3.forward * wallHeight); // bottom right

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        walls.mesh = wallCollider.sharedMesh = wallMesh;
        walls.transform.position = Vector3.forward * wallHeight;
    }

    void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (triangleDictionary.ContainsKey(vertexIndexKey))
        {
            triangleDictionary[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    void CalculateMeshOutlines()
    {

        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++) {
            if (!checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    void FollowOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if (nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++) {
            Triangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j++) {
                int vertexB = triangle[j];
                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB)) {
                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }

        return -1;
    }

    bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++) {
            if (trianglesContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1) {
                    break;
                }
            }
        }
        return sharedTriangleCount == 1;
    }


    private void OnDrawGizmos()
    {
        /*if (squareGrid != null)
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
        }*/
    }
}

struct Triangle
{
    public int vertexIndexA;
    public int vertexIndexB;
    public int vertexIndexC;
    int[] vertices;

    public Triangle(int a, int b, int c)
    {
        vertexIndexA = a;
        vertexIndexB = b;
        vertexIndexC = c;

        vertices = new int[3];
        vertices[0] = a;
        vertices[1] = b;
        vertices[2] = c;
    }

    public int this[int i]
    {
        get
        {
            return vertices[i];
        }
    }


    public bool Contains(int vertexIndex)
    {
        return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
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
    public int config;

    public Square(Node _topLeft, Node _topRight, Node _bottomRight, Node _bottomLeft)
    {
        topLeft = _topLeft;
        topRight = _topRight;
        bottomRight = _bottomRight;
        bottomLeft = _bottomLeft;

        if (topLeft.active)
            config = 1;
        else
            config = 0;
    }
}

public class Node
{
    public Vector3 pos;
    public bool active;
    public int vertexIndex = -1;

    public Node(Vector3 _pos, bool _active)
    {
        pos = _pos;
        active = _active;
    }
}
