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
        public List<Vector3> vertices;

    }
    // pixle size of the generated map
    public int mapSize = 100;
    // grid count
    public int gridSize = 3;
    public int seed = 10;

    IPoint[] points;
    Vector2[,] pointPositions;
    List<MapPolygonMesh> polygons;
    SpriteShape shapeInnerEdge;
    Material TerrBorderMaterial;
    Material TerrFillMaterial;

    public float mergeCoeff = .6f;
    public static float minDist = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shapeInnerEdge = Resources.Load<SpriteShape>("GameStage/ShapeShapes/TerritoryShapeProfile");
        TerrBorderMaterial = Resources.Load<Material>("GameStage/Materials/TerrBorderMaterial");
        TerrFillMaterial = Resources.Load<Material>("GameStage/Materials/TerrFillMaterial");


        generateMap();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void generateMap()
    {
        if (seed != 0)
            Random.InitState(seed);
        Debug.Log(seed);
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
        List<Material> shapeMaterials = new List<Material>
            {
                TerrFillMaterial,
                TerrBorderMaterial
            };
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

        IEnumerable<IVoronoiCell> voronoiCells = delaunator.GetVoronoiCellsBasedOnCircumcenters();
        foreach (var voroni in voronoiCells)
        {
            MapPolygonMesh tempPolygon = new MapPolygonMesh();
            tempPolygon.vertices = new List<Vector3>();
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
                    tempPolygon.vertices.FindIndex((vect) => {
                        return vect == curVert.ToVector3();
                    }
                ) != -1)
                {
                    // vertice already included
                    continue;
                }

                tempPolygon.vertices.Add(curVert.ToVector3());
            }
            if (abortPolygon || tempPolygon.vertices.Count < 3)
            {
                continue;
            }
            polygons.Add(tempPolygon);


        };
        GameState gameState = GameState.GetGameState();
        int territoryID = 0, territoryHP = DEFAULT_HP;
        foreach (var poly in polygons)
        {
            GameObject territoryShape = ClosedShapeBuilder.AddShapeAsChild(
                poly.vertices.ToArray(),
                this,
                shapeInnerEdge
            );
            Vector2 center = new(0,0);
            poly.vertices.ForEach((vert) => {
                center.x += vert.x;
                center.y += vert.y;
            });
            center.x /= poly.vertices.Count;
            center.y /= poly.vertices.Count;

            SpriteShapeRenderer shapeRender = territoryShape.GetComponent<SpriteShapeRenderer>();
            shapeRender.SetMaterials(shapeMaterials);

            gameState.AddTerritory(new Territory(territoryID++, territoryHP, center, territoryShape));
            
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
            tempPolygon.vertices = new List<Vector3>();
            MakePolygonFromTriangle(delaunator, t * 3, includedTriangles, tempPolygon);
            polygons.Add(tempPolygon);


        };
        foreach (var poly in polygons)
        {
            ClosedShapeBuilder.AddShapeAsChild(
                poly.vertices.ToArray(),
                this
           );
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
                Vector3 tempVertex = new Vector3(
                    (float)points[ptIndex].X, (float)points[ptIndex].Y, 0
                );
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
