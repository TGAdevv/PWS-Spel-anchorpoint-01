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
                    //add all weights to a list, sort from low to high and check for the lowest weigts if bridge is active. 
                    //if not active, check if the bridge would make a circle (no need if the index of the weightslist is 0 or 1 (cant make a circle with 2 values)).
                    //Find and reveal a bridge that is part of the optimal solution
                    Debug.LogWarning("HINT SYSTEM FOR " + GlobalVariables.m_LevelGoal.ToString() + " NOT FULLY IMPLEMENTED YET");
                    return;
                }
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
                //HintText.text = $"Het langste proces kost {ChckLvlFinished.LevelRoutes[ChckLvlFinished.currentLvlRoutes].MaxWeight} blokken";
                ShowHintPanel.Invoke();
                break;
        }
    }
}
