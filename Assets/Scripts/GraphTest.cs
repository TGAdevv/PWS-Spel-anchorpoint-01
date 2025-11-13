// Used for testing functionality of the Graph class.

using System.Collections.Generic;
using UnityEngine;

public class GraphTest : MonoBehaviour
{
    Graph graph;

    List<T> ToList<T>(T[] array) 
    {
        return new List<T>(array);
    }

    void Start()
    {
        Vector2[] vertices = new Vector2[3] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1) };
        graph = new Graph();
        graph.AddVertices(vertices);

        graph.AddEdge(0, 1, 0.5f);
        graph.AddEdge(0, 2, 3.2f);

        print(graph.ToString());
    }
}
