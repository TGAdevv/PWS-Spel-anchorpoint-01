using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class CheckIfLevelFinished : MonoBehaviour
{
    public GameObject LevelCompleet;
    public GameObject ster1obj, ster2obj, ster3obj;

    public AnimateUI LevelCompleetAnim;
    public AnimateUI BackgroundAnim;
    public AnimateUI ster1, ster2, ster3;

    public AudioSource LevelCompleteSound;

    public struct GraphEdge
    {
        public uint weight;
        public uint connectTo;

        public GraphEdge(uint _weight, uint _connectTo)
        {
            weight    = _weight;
            connectTo = _connectTo;
        }
    }
    public struct GraphVertex
    {
        public GraphEdge[] connections;

        public GraphVertex(GraphEdge[] _connections)
        {
            connections = _connections;
        }
        public GraphVertex(GraphEdge _connection)
        {
            connections = new GraphEdge[1] { _connection };
        }
    }

    public GraphVertex[] ActiveGraph;

    public bool Check()
    {
        //do not do this if level goal is optimize process
        if (GlobalVariables.m_LevelGoal != LevelGoal.OPTIMIZE_PROCESS)
        {
            string[] activeBridges = new string[0];
            //find active bridges
            for (int i = 0; i < GlobalVariables.possibleBridges.Length; i++)
            {
                if (GlobalVariables.possibleBridges[i].EndsWith(",1"))
                {
                    //remove the ,1 at the end
                    string simplifiedBridge = GlobalVariables.possibleBridges[i].Substring(0, GlobalVariables.possibleBridges[i].Length - 2);
                    activeBridges = activeBridges.Append(simplifiedBridge).ToArray();
                }   
            }
        
            //Build an adjacency list for all islands
            Dictionary<int, List<int>> adjacency = new Dictionary<int, List<int>>();
            //Initialize adjacency list
            for (int i = 0; i < GlobalVariables.m_totalIslands; i++)
            {
                adjacency[i] = new List<int>();
            }

            //Populate adjacency list from activeBridges
            foreach (string bridge in activeBridges)
            {
                string[] parts = bridge.Split(',');
                int start = int.Parse(parts[0]);
                int end = int.Parse(parts[1]);
            //Add connections in both directions
            adjacency[start].Add(end);
            adjacency[end].Add(start);
            }
            //Use depth first search (DFS)to traverse reachable islands
            HashSet<int> visited = new HashSet<int>();
    
            void DFS(int island)
            {
                if (visited.Contains(island)) return;
                visited.Add(island);

                foreach (int neighbor in adjacency[island])
                {
                    DFS(neighbor);
                }
            }
            switch (GlobalVariables.m_LevelGoal)
            {
                case LevelGoal.CONNECT_ALL_ISLANDS:
                    // Start DFS from any island (we use island 0)
                    DFS(0);
                    // Check if all islands were visited
                    if (visited.Count == GlobalVariables.m_totalIslands){
                        return true;
                    } else{
                        return false;
                    }

                case LevelGoal.FIND_SHORTEST_ROUTE:
                    //start DFS from start island
                    DFS(GlobalVariables.m_startIsland);
                    //check if end island is visited
                    if (visited.Contains(GlobalVariables.m_endIsland)){
                        return true;
                    } else{
                        return false;
                    }
            }
        
        } else{ //if level goal is optimize process, level is always finished for now, needs to change
            return true;
        }
        return false; //should never be called, but c# is wonky
    }

    public void LevelFinished()
    {
        if (!Check())
        {
            Debug.Log("Level not finished yet");
            return;
        }
        LevelCompleteSound.Play();
        LevelCompleet.SetActive(true);
        LevelCompleetAnim.PlayAnimation(0);
        BackgroundAnim.PlayAnimation(0);
        // first disable star gameobjects, only make them appear when earned
        ster1obj.SetActive(false);
        ster2obj.SetActive(false);
        ster3obj.SetActive(false);
        switch (GlobalVariables.m_LevelGoal)
        {
            case LevelGoal.CONNECT_ALL_ISLANDS:
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks)
                {
                    ster3obj.SetActive(true);
                    ster3.PlayAnimation(0);
                }
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks + 1)
                {
                    ster2obj.SetActive(true);
                    ster2.PlayAnimation(0);
                }
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks + 2)
                {
                    ster1obj.SetActive(true);
                    ster1.PlayAnimation(0);
                }
                break;
            case LevelGoal.FIND_SHORTEST_ROUTE:
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks)
                {
                    ster3obj.SetActive(true);
                    ster3.PlayAnimation(0);
                }
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks + 1)
                {
                    ster2obj.SetActive(true);
                    ster2.PlayAnimation(0);
                }
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks + 2)
                {
                    ster1obj.SetActive(true);
                    ster1.PlayAnimation(0);
                }
                break;

            case LevelGoal.OPTIMIZE_PROCESS:
                ster1obj.SetActive(true);
                ster2obj.SetActive(true);
                ster3obj.SetActive(true);
                ster1.PlayAnimation(0);
                ster2.PlayAnimation(0);
                ster3.PlayAnimation(0);
                break;

            default:
                break;
        }
    }
}
