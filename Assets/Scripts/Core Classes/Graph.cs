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

    // ------------------------------
    //  Graph->CONSTRUCTORS
    // ------------------------------
    public Graph(List<Vector2> c_Vertices, List<Edges> c_Edges)
    {
        m_Vertices = c_Vertices;
        m_Connections = c_Edges;
    }
    public Graph(List<Vector2> c_Vertices)
    {
        m_Vertices = c_Vertices;
        m_Connections = new List<Edges>(new Edges[c_Vertices.Count]);

        for (int i = 0; i < m_Connections.Count; i++)
        {
            m_Connections[i] = new Edges();
        }
    }
    public Graph() 
    {
        m_Vertices = new List<Vector2>();
        m_Connections = new List<Edges>();
    }

    // ------------------------------
    //  Graph->FUNCTIONS
    // ------------------------------
    public void AddVertex(Vector2 position, Edges edges = null)
    {
        m_Vertices.Add(position);
        m_Connections.Add(edges ?? new Edges());
    }
    public void AddVertices(Vector2[] position, Edges[] edges = null)
    {
        for (int i = 0; i < position.Length; i++)
        {
            m_Vertices.Add(position[i]);
            m_Connections.Add((edges != null) ? edges[i] : new Edges());
        }
    }
    public void AddEdge(int beginVert, int endVert, float weight = 1)
    {
        m_Connections[beginVert].edges.Add(new Edge(endVert, weight));
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

            result += m_Vertices[i].ToString();
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
    public struct Edge
    {
        // Represents single edge in a vertex
        // The begin vert of the edge is just the index in: List<Edge> edges

        // ------------------------------
        //  Edge->VARIABLES
        // ------------------------------
        public int vert;
        public float weight;
        // ------------------------------


        // ------------------------------
        //  Edge->CONSTRUCTORS
        // ------------------------------
        public Edge(int c_Vert, float c_Weight = 1)
        {
            vert = c_Vert;
            weight = c_Weight;
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
        public void Add(int Vert, float Weight = 1)
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

                result += "(" + edges[i].vert + ", " + edges[i].weight + "f)";
            }

            return result + "]";
        }
    }
}
