using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System;

public class CheckIfLevelFinished : MonoBehaviour
{
    public GameObject LevelCompleet;
    public GameObject ster1obj, ster2obj, ster3obj;

    public AnimateUI LevelCompleetAnim;
    public AnimateUI BackgroundAnim;
    public AnimateUI ster1, ster2, ster3;

    public AudioSource LevelCompleteSound;
    public TMP_Text CoinRewardTXT;

    //                              0 Stars->0 coins, 1 Star->2 coins etc
    uint[] CoinRewardPerStarCount = new uint[4] { 0, 2, 3, 5 };

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
                    }
                    return false;

                case LevelGoal.FIND_SHORTEST_ROUTE:
                    //start DFS from start island
                    DFS(GlobalVariables.m_startIsland);
                    //check if end island is visited
                    if (visited.Contains(GlobalVariables.m_endIsland)){
                        return true;
                    }
                    return false;
            }
        
        } 
        //for optimize process, we need to know all possible bridges
        string[] bridges = new string[0];
        for (int i = 0; i < GlobalVariables.possibleBridges.Length; i++)
        {
            bridges = bridges.Append(GlobalVariables.possibleBridges[i].Substring(0, GlobalVariables.possibleBridges[i].Length - 1)).ToArray();
        }
        //build adjacency list
        Dictionary<int, List<List<int>>> defaultAdjacentIslands = new Dictionary<int, List<List<int>>>();
        Dictionary<int, int> currentIslandWeight = new Dictionary<int, int>();
        Dictionary<int, int> islandWeightIfVisitedViaMultipleChoice = new Dictionary<int, int>();
        for (int i = 0; i < GlobalVariables.m_totalIslands; i++)
            {
                defaultAdjacentIslands[i] = new List<List<int>>();
                currentIslandWeight[i] = 9999;
                islandWeightIfVisitedViaMultipleChoice[i] = 9999;
            }
        foreach (string bridge in bridges)
        {
            string[] parts = bridge.Split(',');
            int start = int.Parse(parts[0]);
            int end = int.Parse(parts[1]);
            int weight = int.Parse(parts[2]);
            Debug.Log(start + " to " + end + " with weight " + weight);
            //Add connections in both directions
            defaultAdjacentIslands[start].Add(new List<int> { end, weight});
            defaultAdjacentIslands[end].Add(new List<int> { start, weight});
        }
        void WeightedDFS(int island, int weightToAdd, bool beenPastInput = false)
        {
            int currentWeight = currentIslandWeight[island];
            bool beenPast = beenPastInput;
            if (island == GlobalVariables.m_startIsland) 
            { 
                if (currentIslandWeight[GlobalVariables.m_startIsland] == 0) return;
                else
                {
                    currentIslandWeight[GlobalVariables.m_startIsland] = 0;
                    islandWeightIfVisitedViaMultipleChoice[GlobalVariables.m_startIsland] = 0;
                    Debug.Log(island + " updated to weight: 0 for all paths");
                }
            }
            if (!beenPast) {
                if (weightToAdd < currentWeight)
                {
                    currentIslandWeight[island] = weightToAdd;
                    Debug.Log(island + " updated to weight: " + weightToAdd);
                } else return;
            } else {
                if (weightToAdd < islandWeightIfVisitedViaMultipleChoice[island])
                {
                    islandWeightIfVisitedViaMultipleChoice[island] = weightToAdd;
                    Debug.Log(island + " updated to weight (via mc): " + weightToAdd);
                } else return;
            }
            currentWeight += weightToAdd;

            foreach (List<int> neighbor in defaultAdjacentIslands[island])
            {
                if (GlobalVariables.m_multiplechoiceconnection == island + "," + neighbor[0] || GlobalVariables.m_multiplechoiceconnection == neighbor[0] + "," + island)
                {
                    beenPast = true;
                }
                if (beenPast)
                    WeightedDFS(neighbor[0], islandWeightIfVisitedViaMultipleChoice[island] + neighbor[1], beenPast);
                else
                    WeightedDFS(neighbor[0], currentIslandWeight[island] + neighbor[1], beenPast);
            }
        }
        WeightedDFS(GlobalVariables.m_startIsland, 0);
        Debug.Log("Current island weight at start: " + currentIslandWeight[GlobalVariables.m_endIsland]);
        Debug.Log("Island weight if visited via multiple choice at start: " + islandWeightIfVisitedViaMultipleChoice[GlobalVariables.m_endIsland]);
        int maximumWeightNeeded = currentIslandWeight[GlobalVariables.m_endIsland] - islandWeightIfVisitedViaMultipleChoice[GlobalVariables.m_endIsland];
        Debug.Log("Maximum weight needed for shortest path: " + maximumWeightNeeded);
        if ((uint)maximumWeightNeeded < GlobalVariables.SelectedWeightOption)
            return false;
        GlobalVariables.neededweight = maximumWeightNeeded;
        return true;
    }

    public void LevelFinished()
    {
        if (!Check())
        {
            Debug.Log("Level not finished yet");
            return;
        }

        // first disable star gameobjects, only make them appear when earned
        ster1obj.SetActive(false);
        ster2obj.SetActive(false);
        ster3obj.SetActive(false);

        int starCount = 0;

        switch (GlobalVariables.m_LevelGoal)
        {
            case LevelGoal.CONNECT_ALL_ISLANDS:
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks + 2)
                {
                    starCount = 1;
                    ster1obj.SetActive(true);
                    ster1.PlayAnimation(0);
                }
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks + 1)
                {
                    starCount = 2;
                    ster2obj.SetActive(true);
                    ster2.PlayAnimation(0);
                }
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks)
                {
                    starCount = 3;
                    ster3obj.SetActive(true);
                    ster3.PlayAnimation(0);
                }
                break;
            case LevelGoal.FIND_SHORTEST_ROUTE:
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks + 2)
                {
                    starCount = 1;
                    ster1obj.SetActive(true);
                    ster1.PlayAnimation(0);
                }
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks + 1)
                {
                    starCount = 2;
                    ster2obj.SetActive(true);
                    ster2.PlayAnimation(0);
                }
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks)
                {
                    starCount = 3;
                    ster3obj.SetActive(true);
                    ster3.PlayAnimation(0);
                }
                break;

            case LevelGoal.OPTIMIZE_PROCESS:
                float offsetFromCorrectAnswer = Mathf.Abs(GlobalVariables.neededweight - GlobalVariables.SelectedWeightOption) / GlobalVariables.neededweight;
                if (offsetFromCorrectAnswer <= 0.5)
                {
                    starCount = 1;
                    ster1obj.SetActive(true);
                    ster1.PlayAnimation(0);
                }
                if (offsetFromCorrectAnswer <= 0.75)
                {
                    starCount = 2;
                    ster2obj.SetActive(true);
                    ster2.PlayAnimation(0);
                }
                if (GlobalVariables.SelectedWeightOption == GlobalVariables.neededweight)
                {
                    starCount = 3;
                    ster3obj.SetActive(true);
                    ster3.PlayAnimation(0);
                }
                break;

            default:
                break;
        }
        CoinRewardTXT.text = CoinRewardPerStarCount[starCount].ToString();
        GlobalVariables.m_Coins += CoinRewardPerStarCount[starCount];

        LevelCompleteSound.Play();
        LevelCompleet.SetActive(true);
        LevelCompleetAnim.PlayAnimation(0);
        BackgroundAnim.PlayAnimation(0);
    }
}
