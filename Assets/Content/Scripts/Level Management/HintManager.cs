using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Linq;
using System.Collections.Generic;
public class HintManager : MonoBehaviour
{
    public UnityEvent OnHintBoughtSuccesful;
    public CheckIfLevelFinished ChckLvlFinished;

    [Header("Panel used for displaying hints through text")]
    public UnityEvent ShowHintPanel;
    public TMP_Text   HintText;
    public List<List<Vector3Int>> AllRoutes;

    [Header("After how many req blocks should the hint switch " +
        "from revealing a single bridge to revealing the required amount of blocks")]
    public uint HintBridgeToTextThreshold;

    private void Start()
    {
        GlobalVariables.m_Coins = 999;
    }

    public void GenerateHint()
    {

        if (!GlobalVariables.PurchaseWithCoins(15)) return;
        OnHintBoughtSuccesful.Invoke();

        int req_blocks = GlobalVariables.m_requiredBlocks;

        switch (GlobalVariables.m_LevelGoal)
        {
            case LevelGoal.CONNECT_ALL_ISLANDS:
                if (req_blocks >= HintBridgeToTextThreshold)
                {
                    HintText.text = $"Er zijn in totaal {req_blocks} blokken nodig";
                    ShowHintPanel.Invoke();
                    return;
                }
                else
                {
                    List<uint> weights = new List<uint>();
                    List<bool> activated = new List<bool>();
                    foreach (var bridge in GlobalVariables.possibleBridges)
                    {
                        weights.Add(bridge.weight);
                        activated.Add(bridge.activated);
                    }
                    for (int i = 0; i < activated.Count; i++)
                    {
                        if (activated[i]) weights[i] = uint.MaxValue;
                        //to avoid picking an already active bridge
                    }
                    weights.Sort();
                    for (int i = 0; i < weights.Count; i++)
                    {
                        uint weightToCheck = weights[i];
                        for (int j = 0; j < GlobalVariables.possibleBridges.Length; j++)
                        {
                            var bridge = GlobalVariables.possibleBridges[j];
                            if (bridge.weight == weightToCheck && !bridge.activated && !bridge.inPuzzleMode)
                            {
                                //set internal value of bridge to active
                                GlobalVariables.possibleBridges[j] = new Bridge(bridge.startIsland, bridge.endIsland, bridge.weight, true);
                                //check if adding this bridge would create a circle
                                bool createsCircle = false;
                                void DFS(int currentIsland, HashSet<int> visited, int parentIsland)
                                {
                                    visited.Add(currentIsland);
                                    for (int k = 0; k < GlobalVariables.possibleBridges.Length; k++)
                                    {
                                        var b = GlobalVariables.possibleBridges[k];
                                        if (b.activated == false) continue;
                                        if (b.startIsland != currentIsland && b.endIsland != currentIsland) continue;

                                        int neighbor = -1;
                                        if (b.startIsland == currentIsland)
                                            neighbor = b.endIsland;
                                        else if (b.endIsland == currentIsland)
                                            neighbor = b.startIsland;

                                        if (!visited.Contains(neighbor))
                                        {
                                            DFS(neighbor, visited, currentIsland);
                                        }
                                        else if (neighbor != parentIsland)
                                        {
                                            createsCircle = true;
                                            return;
                                        }
                                    }
                                }
                                HashSet<int> visited = new HashSet<int>();
                                DFS(bridge.startIsland, visited, -1);
                                if (createsCircle) {
                                    //revert internal value of bridge to inactive
                                    GlobalVariables.possibleBridges[j] = new Bridge(bridge.startIsland, bridge.endIsland, bridge.weight, false);
                                    continue;
                                }
                                //reveal this bridge visually, internal value already set to active
                                GlobalVariables.m_Blocks += bridge.weight;
                                for (int k = 0; k < GlobalVariables.bridgeObjects.Count; k++)
                                {
                                    var bridgeObj = GlobalVariables.bridgeObjects[k];
                                    if (bridgeObj == null) continue;
                                    var bridgeScript = bridgeObj.GetComponent<ClickToCreateBridge>();
                                    if (bridgeScript == null) continue;
                                    if ((bridgeScript.startIsland == bridge.startIsland &&
                                        bridgeScript.endIsland == bridge.endIsland) ||
                                        (bridgeScript.startIsland == bridge.endIsland &&
                                        bridgeScript.endIsland == bridge.startIsland))
                                    {
                                        bridgeScript.targetBridgeActive = true;
                                    }
                                }
                                return;
                            }
                        }
                    }
                }
                break;
            case LevelGoal.FIND_SHORTEST_ROUTE:
                if (req_blocks >= HintBridgeToTextThreshold)
                {
                    HintText.text = $"Er zijn in totaal {req_blocks} blokken nodig";
                    ShowHintPanel.Invoke();
                    return;
                }
                else
                {
                    // Store bridge data in arrays
                    List<uint> weights = new List<uint>();
                    List<int> startIslands = new List<int>();
                    List<int> endIslands = new List<int>();
                    List<bool> activated = new List<bool>();
                    foreach (var bridge in GlobalVariables.possibleBridges)
                    {
                        weights.Add(bridge.weight);
                        startIslands.Add(bridge.startIsland);
                        endIslands.Add(bridge.endIsland);
                        activated.Add(bridge.activated);
                    }

                    // Variables to track shortest path
                    List<int> shortestPath = null;
                    int shortestDistance = int.MaxValue;

                    // DFS to find shortest route and track path
                    void ShortestRouteDFS(int currentIsland, int targetIsland, 
                                        int currentDistance, HashSet<int> visited, 
                                        List<int> currentPath)
                        {
                            visited.Add(currentIsland);
                            currentPath.Add(currentIsland);

                            if (currentIsland == targetIsland)
                            {
                                if (currentDistance < shortestDistance)
                                {
                                    shortestDistance = currentDistance;
                                    shortestPath = new List<int>(currentPath);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < GlobalVariables.possibleBridges.Length; i++)
                                {
                                    var bridge = GlobalVariables.possibleBridges[i];
                                    int neighbor = -1;

                                    if (bridge.startIsland == currentIsland)
                                        neighbor = bridge.endIsland;
                                    else if (bridge.endIsland == currentIsland)
                                        neighbor = bridge.startIsland;

                                    if (neighbor != -1 && !visited.Contains(neighbor))
                                    {
                                        int newDistance = currentDistance + (int)bridge.weight;
                                        if (newDistance < shortestDistance) // prune longer paths
                                        {
                                            ShortestRouteDFS(neighbor, targetIsland, newDistance, visited, currentPath);
                                        }
                                    }
                                }
                            }
                        visited.Remove(currentIsland);
                        currentPath.RemoveAt(currentPath.Count - 1);
                        }
                    ShortestRouteDFS(GlobalVariables.m_startIsland, GlobalVariables.m_endIsland, 0, new HashSet<int>(), new List<int>());
                    List<Bridge> bridgesInShortestPath = new List<Bridge>();
                    List<Bridge> inactiveBridgesInShortestPath = new List<Bridge>();
                    if (shortestPath != null)
                    {
                        for (int i = 0; i < shortestPath.Count - 1; i++)
                        {
                            int start = shortestPath[i];
                            int end = shortestPath[i + 1];
                            foreach (Bridge bridge in GlobalVariables.possibleBridges)
                            {
                                if ((bridge.startIsland == start && bridge.endIsland == end) ||
                                    (bridge.startIsland == end && bridge.endIsland == start))
                                {
                                    bridgesInShortestPath.Add(bridge);
                                    break;
                                }
                            }
                        }
                        // Reveal one of the bridges in the shortest path
                        for (int i = 0; i < bridgesInShortestPath.Count; i++)
                        {
                            Bridge bridgeIndex = bridgesInShortestPath[i];
                            if (!bridgeIndex.activated)
                            {
                                inactiveBridgesInShortestPath.Add(bridgesInShortestPath[i]);
                            }
                        }
                        Bridge bridgeToReveal = inactiveBridgesInShortestPath[Random.Range(0, inactiveBridgesInShortestPath.Count)];
                        for (int i = 0; i < GlobalVariables.possibleBridges.Length; i++)
                        {
                            if ((GlobalVariables.possibleBridges[i].startIsland == bridgeToReveal.startIsland &&
                                GlobalVariables.possibleBridges[i].endIsland == bridgeToReveal.endIsland) ||
                                (GlobalVariables.possibleBridges[i].startIsland == bridgeToReveal.endIsland &&
                                GlobalVariables.possibleBridges[i].endIsland == bridgeToReveal.startIsland))
                            {
                                GlobalVariables.possibleBridges[i] = new Bridge(bridgeToReveal.startIsland, bridgeToReveal.endIsland, bridgeToReveal.weight, true);
                                break;
                            }
                        }
                        GlobalVariables.m_Blocks += bridgeToReveal.weight;
                        for (int i = 0; i < GlobalVariables.bridgeObjects.Count; i++)
                        {
                            var bridgeObj = GlobalVariables.bridgeObjects[i];
                            if (bridgeObj == null) continue;
                            var bridgeScript = bridgeObj.GetComponent<ClickToCreateBridge>();
                            if (bridgeScript == null) continue;
                            if ((bridgeScript.startIsland == bridgeToReveal.startIsland &&
                                bridgeScript.endIsland == bridgeToReveal.endIsland) ||
                                (bridgeScript.startIsland == bridgeToReveal.endIsland &&
                                bridgeScript.endIsland == bridgeToReveal.startIsland))
                            {
                                bridgeScript.targetBridgeActive = true;
                            }
                        }
                    }
                break;
                }
            case LevelGoal.OPTIMIZE_PROCESS:
                //impossible to show bridge, always show text hint
                //code copied from CheckIfLevelFinished.cs
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
                            XTotalProcessWeight = System.Math.Max(totalWeight, XTotalProcessWeight);
                            return;
                        }

                        MaximumWeight = System.Math.Max(totalWeight, MaximumWeight);
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
                HintText.text = $"Het langste proces zonder de ingestelde waarde duurt {MaximumWeight} uur";
                ShowHintPanel.Invoke();
                break;
        }
    }
}
