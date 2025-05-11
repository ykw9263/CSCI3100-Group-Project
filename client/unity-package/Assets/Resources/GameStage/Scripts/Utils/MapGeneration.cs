using System.Collections;
using System.Collections.Generic;
using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;
using UnityEngine.U2D;
using UnityEngine;



public class MapGeneration : MonoBehaviour
{
    
    const int DEFAULT_HP = 100;
    struct MapPolygonMesh
    {
        public List<int> trangles;
        public List<IPoint> vertices;
        public int id;
    }
    // pixle size of the generated map
    public int mapSize = 100;
    // grid count, controls how many tiles are generated
    public int gridSize = 10;
    // a seed of 0 will be random
    public int seed = 10;

    IPoint[] points;
    Vector2[,] pointPositions;
    List<MapPolygonMesh> polygons;
    [SerializeField] GameObject TerrPrefab;

    public float mergeCoeff = .6f;
    public static float minDist = 3;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // TerrPrefab = Resources.Load<GameObject>("GameStage/Sprites/TerritoryPrefab");

        // generateMap();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void generateMap()
    {
        
        points = new IPoint[gridSize * gridSize];
        polygons = new List<MapPolygonMesh>();
        GenerateGrid();
        GenerateVoloni();

        GameState gamestate = GameState.GetGameState();

    }


    void GenerateVoloni()
    {

        List<IPoint> mergedVertices = new List<IPoint>();
        List<IPoint> mergedVerticesTo = new List<IPoint>();
        Delaunator delaunator = new Delaunator(points);
        IEnumerable<IEdge> voronoiEdges = delaunator.GetVoronoiEdgesBasedOnCircumCenter();


        // loop through every edge to check for too short edges
        foreach (var voroniEdge in voronoiEdges)
        {
            IPoint vertP = voroniEdge.P, vertQ = voroniEdge.Q;
            if (Vector2.Distance(
                    vertP.ToVector2(),
                    vertQ.ToVector2()
                ) < minDist
            )
            {
                if (vertP.ToVector2().magnitude < vertQ.ToVector2().magnitude)
                {
                    mergedVerticesTo.Add(vertP);
                    mergedVertices.Add(vertQ);
                }
                else
                {
                    mergedVerticesTo.Add(vertQ);
                    mergedVertices.Add(vertP);
                }
            }
        }
        
        Dictionary<IPoint, List<int>> adjDict = new();
        int polygonID = 0;

        IEnumerable<IVoronoiCell> voronoiCells = delaunator.GetVoronoiCellsBasedOnCircumcenters();
        foreach (var voroni in voronoiCells)
        {
            MapPolygonMesh tempPolygon = new MapPolygonMesh();
            tempPolygon.vertices = new List<IPoint>();
            bool abortPolygon = false;
            foreach (var vert in voroni.Points)
            {
                IPoint curVert = vert;
                if (
                    vert.X < 0 || vert.X > mapSize ||
                    vert.Y < 0 || vert.Y > mapSize
                )
                {
                    abortPolygon = true;
                    break;
                }
                if (mergedVertices.Contains(curVert))
                {
                    curVert = mergedVerticesTo[mergedVertices.IndexOf(vert)];
                }
                if (
                    tempPolygon.vertices.FindIndex((vert) => {
                        return curVert.Equals(vert);
                    }
                ) != -1)
                {
                    // vertice already included
                    continue;
                }
                tempPolygon.vertices.Add(curVert);
            }
            if (abortPolygon || tempPolygon.vertices.Count < 3)
            {
                continue;
            }
            tempPolygon.id = polygonID++;
            polygons.Add(tempPolygon);

            // polygons that shares a vertice is considered adjacent
            foreach (IPoint vert in tempPolygon.vertices) {
                if (!adjDict.ContainsKey(vert)) {
                    adjDict[vert] = new();
                };
                adjDict[vert].Add(tempPolygon.id);
            }

        };


        GameState gameState = GameState.GetGameState();
        int territoryHP = DEFAULT_HP;
        foreach (var poly in polygons)
        {

            GameObject terrShape1 = Instantiate<GameObject>(TerrPrefab);
            SpriteShapeController terrSC = terrShape1.GetComponent<SpriteShapeController>();

            List<int> adjList = new();
            Vector3 center = new(0,0,0);
            for (int i = 0; i < poly.vertices.Count; i++)
            {
                terrSC.spline.InsertPointAt(i, poly.vertices[i].ToVector3());

                // build adjacency list
                foreach (var neibourID in adjDict[poly.vertices[i]])
                {
                    if (!adjList.Contains(neibourID)) {
                        adjList.Add(neibourID);
                    }
                }

                // calcualte center of polygon
                center.x += (float)poly.vertices[i].X;
                center.y += (float)poly.vertices[i].Y;
            }

            center.x /= poly.vertices.Count;
            center.y /= poly.vertices.Count;

            terrShape1.transform.parent = this.transform;
            Territory territory = terrShape1.GetComponent<Territory>();
            territory.InitTerritory(poly.id, territoryHP, center, adjList);
            gameState.AddTerritory(territory);
            // gameState.AddTerritory(new Territory(poly.id, territoryHP, center, terrShape1, adjList));   
        }
    }

    void GenerateDelaunate()
    {

        List<int> includedTriangles = new List<int>();

        Delaunator delaunator = new Delaunator(points);
        //List<Vector3> tempVertices;
        // delaunator.GetVoronoiEdgesBasedOnCircumCenter();
        int[] triangles = delaunator.Triangles;
        for (int t = 0; t < triangles.Length / 3; t++)
        {
            if (includedTriangles.Contains(t)) continue;

            MapPolygonMesh tempPolygon = new MapPolygonMesh();
            tempPolygon.trangles = new List<int>();
            tempPolygon.vertices = new List<IPoint>();
            MakePolygonFromTriangle(delaunator, t * 3, includedTriangles, tempPolygon);
            polygons.Add(tempPolygon);

        };
        foreach (var poly in polygons)
        {
            GameObject terrShape1 = Instantiate<GameObject>(TerrPrefab);
            SpriteShapeController terrSC = terrShape1.GetComponent<SpriteShapeController>();

            for (int i = 0; i < poly.vertices.Count; i++)
            {
                terrSC.spline.InsertPointAt(i, poly.vertices[i].ToVector3());
            }
        }

    }

    int MakePolygonFromTriangle(Delaunator delaunator, int teIn, List<int> includedTriangles, MapPolygonMesh tempPolygon)
    {
        int te = teIn;
        int t = teIn / 3;
        if (includedTriangles.Contains(t)) return 1;
        includedTriangles.Add(t);
        tempPolygon.trangles.Add(t);

        for (int i = 0; i < 3; i++)
        {
            int mergeResult = 1;
            if (Random.Range(0f, 1f) > mergeCoeff)
            {

                int adjTriangleEdge = Delaunator.NextHalfedge(delaunator.Halfedges[te]);
                if (adjTriangleEdge >= 0)
                {
                    mergeResult = MakePolygonFromTriangle(delaunator, adjTriangleEdge, includedTriangles, tempPolygon);

                }

            }
            // merge failed(?
            if (mergeResult != 0)
            {
                int ptIndex = delaunator.Triangles[te];
                IPoint tempVertex = points[ptIndex];
                if (!tempPolygon.vertices.Contains(tempVertex))
                    tempPolygon.vertices.Add(tempVertex);
            }

            te = Delaunator.NextHalfedge(te);
        }
        return 0;
    }

    void GenerateGrid()
    {
        pointPositions = new Vector2[gridSize, gridSize];
        int cellSize = mapSize / gridSize;
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                int x = i * cellSize + Random.Range(cellSize / 10, cellSize / 10 * 9),
                    y = j * cellSize + Random.Range(cellSize / 10, cellSize / 10 * 9);
                pointPositions[i, j] = new Vector2(x, y);
                points[i + j * gridSize] = new Point(x, y);
                Vector3[] vertices = {
                    new(x+1, y, 0),
                    new(x+1, y+1, 0),
                    new(x, y, 0)
                };
                /*
                AddShape.AddShapeAsChild(
                    vertices,
                    this
                );
                */
            }
        }

    }
}
