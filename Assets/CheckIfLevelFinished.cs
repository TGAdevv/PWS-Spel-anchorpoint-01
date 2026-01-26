using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System;
using UnityEngine.Events;

public class CheckIfLevelFinished : MonoBehaviour
{
    public UnityEvent OnLevelNotFinished;
    public ShowLevelFinishedAnimation ShowLevelFinishedAnimation;

    [NonSerialized] public int currentLvlRoutes = 0;

    public List<List<Vector3Int>> AllRoutes;

    public bool Check()
    {
        //do not do this if level goal is optimize process
        if (GlobalVariables.m_LevelGoal != LevelGoal.OPTIMIZE_PROCESS)
        {
            List<Bridge> activeBridges = new();
            //find active bridges
            for (int i = 0; i < GlobalVariables.possibleBridges.Length; i++)
            {
                Debug.Log("Checking bridge between " + GlobalVariables.possibleBridges[i].startIsland + " and " + GlobalVariables.possibleBridges[i].endIsland + " activated: " + GlobalVariables.possibleBridges[i].activated);
                if (GlobalVariables.possibleBridges[i].activated)
                {
                    Debug.Log("Found active bridge between " + GlobalVariables.possibleBridges[i].startIsland + " and " + GlobalVariables.possibleBridges[i].endIsland);
                    activeBridges.Add(GlobalVariables.possibleBridges[i]);
                }   
            }
        
            //Build an adjacency list for all islands
            Dictionary<int, List<int>> adjacency = new();
            //Initialize adjacency list
            for (int i = 0; i < GlobalVariables.m_totalIslands; i++)
            {
                adjacency[i] = new List<int>();
            }

            //Populate adjacency list from activeBridges
            foreach (var bridge in activeBridges)
            {
                //Add connections in both directions
                adjacency[bridge.startIsland].Add(bridge.endIsland);
                adjacency[bridge.endIsland].Add(bridge.startIsland);
            }
            //Use depth first search (DFS)to traverse reachable islands
            HashSet<int> visited = new();
    
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

        // Maximum weight of process with x with x=0
        uint XTotalProcessWeight = 0;
        uint MaximumWeight = 0;

        // x = index of island it goes to, y = index of the bridge in possibleBridges array
        List<Vector2Int>[] bridges = new List<Vector2Int>[GlobalVariables.m_totalIslands];

        for (int i = 0; i < GlobalVariables.possibleBridges.Length; i++)
        {
            int startIsland = GlobalVariables.possibleBridges[i].startIsland;
            int endIsland   = GlobalVariables.possibleBridges[i].endIsland;

            if (bridges[startIsland] == null)
                bridges[startIsland] = new();
            bridges[startIsland].Add(new(endIsland, i));

            if (bridges[endIsland] == null)
                bridges[endIsland] = new();
            bridges[endIsland].Add(new(startIsland, i));
        }

        int totalSearches = 0;

        void WeightedDFS(int currentIsland, int prevIsland, uint totalWeight, bool containsX, List<Vector3Int> Route, bool firstSearch = false)
        {
            if (currentIsland == GlobalVariables.m_startIsland && !firstSearch)
                return;

            if (currentIsland == GlobalVariables.m_endIsland)
            {
                AllRoutes.Add(Route);

                if (containsX)
                {
                    XTotalProcessWeight = Math.Max(totalWeight, XTotalProcessWeight);
                    return;
                }

                MaximumWeight = Math.Max(totalWeight, MaximumWeight);
                return;
            }

            for (int i = bridges[currentIsland].Count - 1; i >= 0; i--)
            {
                Vector2Int bridge = bridges[currentIsland][i];

                if (bridge.x == prevIsland)
                    continue;

                uint newTotWeight = totalWeight;
                bool newContainsX = containsX;

                if (bridge.y == GlobalVariables.m_multiplechoiceconnection)
                    newContainsX = true;
                else
                    newTotWeight += GlobalVariables.possibleBridges[bridge.y].weight;

                int newPrevIsland = currentIsland;
                int newCurrentIsland = bridge.x;

                List<Vector3Int> newRoute;

                if (Route != null)
                    newRoute = Route;
                else
                    newRoute = new();

                newRoute.Add(new(newPrevIsland, newCurrentIsland));
                totalSearches++;

                WeightedDFS(newCurrentIsland, newPrevIsland, newTotWeight, newContainsX, newRoute);
            }
        }

        AllRoutes = new();

        //           Cur island                     Prev island  tot_weight  has_x   route  1st_search
        WeightedDFS(GlobalVariables.m_startIsland,      -1,          0,      false,  null,   true);

        if (GlobalVariables.SelectedWeightOption > MaximumWeight - XTotalProcessWeight)
            return false;
        GlobalVariables.neededweight = MaximumWeight - XTotalProcessWeight;
        return true;
    }

    float CalcAverageBridgeWeight()
    {
        uint TotalWeightInLevel = 0;
        foreach (var bridge in GlobalVariables.possibleBridges)
            TotalWeightInLevel += bridge.weight;
        return TotalWeightInLevel / (float)GlobalVariables.possibleBridges.Length;
    }

    public void LevelFinished()
    {
        if (!Check())
        {
            OnLevelNotFinished.Invoke();
            return;
        }

        int starCount = 0;

        switch (GlobalVariables.m_LevelGoal)
        {
            case LevelGoal.CONNECT_ALL_ISLANDS or LevelGoal.FIND_SHORTEST_ROUTE:
                float AverageBridgeLength = CalcAverageBridgeWeight();

                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks + AverageBridgeLength)
                {
                    starCount = 1;
                }
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks + AverageBridgeLength * .3f)
                {
                    starCount = 2;
                }
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks)
                {
                    starCount = 3;
                }

                break;

            case LevelGoal.OPTIMIZE_PROCESS:
                float offsetFromCorrectAnswer = Mathf.Abs(GlobalVariables.neededweight - GlobalVariables.SelectedWeightOption);

                if (offsetFromCorrectAnswer > 3 && offsetFromCorrectAnswer <= 5) {
                    starCount = 1;
                }
                else if (offsetFromCorrectAnswer > 0) {
                    starCount = 2;
                }
                else {
                    starCount = 3;
                }

                break;

            default:
                break;
        }

        if (starCount == 0)
        {
            OnLevelNotFinished.Invoke();
            return;
        }

        //if (GlobalVariables.m_LevelGoal == LevelGoal.OPTIMIZE_PROCESS) currentLvlRoutes++;

        StartCoroutine(ShowLevelFinishedAnimation.LevelFinished(starCount));
    }
}
