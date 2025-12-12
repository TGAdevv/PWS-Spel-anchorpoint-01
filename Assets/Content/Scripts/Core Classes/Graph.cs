using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Graph
{

    // The graph (graaf) consists of two elements:
    //      1: The Vertices
    //          ->represented as Vector2, useful for setting positions of vertices for rendering
    //      2: The Edges for every vertex
    //          ->represented as List of Edges, with one element per vertex respectively


    // ------------------------------
    //  Graph->VARIABLES
    // ------------------------------
    public List<Vector2> m_Vertices;
    public List<Edges> m_Connections;
    public List<Vector2Int> m_VertSizes;

    // ------------------------------
    //  Graph->CONSTRUCTORS
    // ------------------------------
    public Graph(List<Vector2> c_Vertices, List<Edges> c_Edges, List<Vector2Int> c_VertSizes = null)
    {
        if (c_VertSizes != null)
            m_VertSizes = new(c_VertSizes);
        else
            foreach (var _ in c_Vertices)
                m_VertSizes.Add(Vector2Int.one);

        m_Vertices = c_Vertices;
        m_Connections = c_Edges;
    }
    public Graph(List<Vector2> c_Vertices, List<Vector2Int> c_VertSizes = null)
    {
        if (c_VertSizes != null)
            m_VertSizes = new(c_VertSizes);
        else
            foreach (var _ in c_Vertices)
                m_VertSizes.Add(Vector2Int.one);

        m_Vertices = c_Vertices;
        m_Connections = new List<Edges>(new Edges[c_Vertices.Count]);

        for (int i = 0; i < m_Connections.Count; i++)
        {
            m_Connections[i] = new Edges();
        }
    }
    public Graph(Vector2[] c_Vertices, Vector2Int[] c_VertSizes = null)
    {
        m_VertSizes = new();
        if (c_VertSizes != null)
            m_VertSizes = new(c_VertSizes);
        else
            foreach (var _ in c_Vertices)
                m_VertSizes.Add(Vector2Int.one);

        m_Vertices = new(c_Vertices);
        m_Connections = new(new Edges[c_Vertices.Length]);

        for (int i = 0; i < m_Connections.Count; i++)
            m_Connections[i] = new();
    }
    public Graph() 
    {
        m_Vertices    = new();
        m_Connections = new();
        m_VertSizes   = new();
    }

    // ------------------------------
    //  Graph->FUNCTIONS
    // ------------------------------
    public void AddVertex(Vector2 position, Vector2Int size, Edges edges = null)
    {
        m_VertSizes.Add(size);
        m_Vertices.Add(position);
        m_Connections.Add(edges ?? new Edges());
    }
    public void AddVertices(Vector2[] position, Vector2Int[] sizes = null, Edges[] edges = null)
    {
        for (int i = 0; i < position.Length; i++)
        {
            m_VertSizes.Add(sizes == null ? Vector2Int.one : sizes[i]);
            m_Vertices.Add(position[i]);
            m_Connections.Add((edges != null) ? edges[i] : new Edges());
        }
    }
    public void AddEdge(int beginVert, int endVert, int weight = 1, Vector3[] bezierPoints = null)
    {
        m_Connections[beginVert].edges.Add(new Edge(endVert, weight, bezierPoints));
    }
    public string ExportGraph() 
    {
        string result = "";

        for (int i = 0; i < m_Vertices.Count; i++)
        {
            result += m_Vertices[i].x  + ",";
            result += m_Vertices[i].y  + ",";
            result += m_VertSizes[i].x + ",";
            result += m_VertSizes[i].y;
            if (i < m_Vertices.Count - 1)
                result += ";";
        }
        result += "_";
        for (int i = 0; i < m_Connections.Count; i++)
        {
            for (int j = 0; j < m_Connections[i].edges.Count; j++)
            {
                Edge curEdge = m_Connections[i].edges[j];

                result += curEdge.weight + ",";
                result += curEdge.vert + ",";

                for (int k = 0; k < curEdge.bezier_points.Length; k++)
                {
                    result += curEdge.bezier_points[k].x + ",";
                    result += curEdge.bezier_points[k].y + ",";
                    result += curEdge.bezier_points[k].z;
                    if (k < curEdge.bezier_points.Length - 1)
                        result += ",";
                }
                if (j < m_Connections[i].edges.Count - 1)
                    result += ";";
            }
            result += "/";
        }

        return result;
    }
    // ------------------------------
    //  Graph->OVERRIDES
    // ------------------------------
    override public string ToString()
    {
        string result = "Vertices:\n{ ";

        for (int i = 0; i < m_Vertices.Count; i++)
        {
            if (i != 0)
                result += ", ";

            result += "pos: " + m_Vertices[i] + ", size: " + m_VertSizes[i];
        }
        result += " }\nConnections:\n{ ";
        for (int i = 0; i < m_Connections.Count; i++)
        {
            if (i != 0)
                result += ", ";

            result += m_Connections[i].ToString();
        }

        return result + " }";
    }
    // =======================================


    // ------------------------------
    //  Graph->STRUCTS
    // ------------------------------
    public class Edge
    {
        // Represents single edge in a vertex
        // The begin vert of the edge is just the index in: List<Edge> edges

        // ------------------------------
        //  Edge->VARIABLES
        // ------------------------------
        public int vert;
        public int weight;
        public Vector3[] bezier_points;
        // ------------------------------


        // ------------------------------
        //  Edge->CONSTRUCTORS
        // ------------------------------
        public Edge(int c_Vert, int c_Weight = 1, Vector3[] c_BezierPonts = null)
        {
            vert = c_Vert;
            weight = c_Weight;
            bezier_points = c_BezierPonts;
        }
        // ------------------------------
    }

    // ------------------------------
    //  Graph->CLASSES
    // ------------------------------
    public class Edges
    {
        // Stores all edges of a certain vertex in the graph

        // ------------------------------
        //  Edges->VARIABLES
        // ------------------------------
        public List<Edge> edges;

        // ------------------------------
        //  Edges->CONSTRUCTORS
        // ------------------------------
        public Edges() 
        {
            edges = new List<Edge>();
        }
        public Edges(List<Edge> c_Edges)
        {
            edges = c_Edges;
        }

        // ------------------------------
        //  Edges->FUNCTIONS
        // ------------------------------
        public void Add(int Vert, int Weight = 1)
        {
            edges.Add(new Edge(Vert, Weight));
        }

        // ------------------------------
        //  Edges->OVERRIDES
        // ------------------------------
        override public string ToString() 
        {
            if (edges.Count == 0)
                return "NONE";

            string result = "[";

            for (int i = 0; i < edges.Count; i++)
            {
                if (i != 0)
                    result += ", ";

                result += "(" + edges[i].vert + ", " + edges[i].weight + ", bezier points: ";
                foreach (var point in edges[i].bezier_points)
                    result += point + ", ";
                result += ")";
            }

            return result + "]";
        }
    }
}
