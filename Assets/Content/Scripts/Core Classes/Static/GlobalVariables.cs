using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Burst.CompilerServices;
using UnityEngine;

public enum LevelGoal
{
    CONNECT_ALL_ISLANDS,
    OPTIMIZE_PROCESS,
    FIND_SHORTEST_ROUTE,  
}

// ------------------------------
//  GlobalVariables->STRUCTS
// ------------------------------
[Serializable]
public struct Bridge
{
    public bool activated;
    public uint weight;
    public int startIsland;
    public int endIsland;
    public bool inPuzzleMode;
    public Bridge(int _startIsland, int _endIsland, uint _weight, bool _activated, bool _puzzleMode = false)
    {
        activated = _activated;
        weight = _weight;
        startIsland = _startIsland;
        endIsland = _endIsland;
        inPuzzleMode = _puzzleMode;
    }

    public static bool operator ==(Bridge b1, Bridge b2)
    {
        return (b1.startIsland == b2.startIsland && b1.endIsland == b2.endIsland) ||
            (b1.startIsland == b2.endIsland && b1.endIsland == b2.startIsland);
    }
    public static bool operator !=(Bridge b1, Bridge b2)
    {
        return !(b1.startIsland == b2.startIsland && b1.endIsland == b2.endIsland) &&
            !(b1.startIsland == b2.endIsland && b1.endIsland == b2.startIsland);
    }



    public override bool Equals(object obj)
    {
        return obj is Bridge bridge &&
               activated == bridge.activated &&
               weight == bridge.weight &&
               startIsland == bridge.startIsland &&
               endIsland == bridge.endIsland;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(activated, weight, startIsland, endIsland);
    }
}
// =======================================

public static class GlobalVariables
{
    // ------------------------------
    //  GlobalVariables->VARIABLES
    // ------------------------------

    // UI Variables
    public static uint m_Coins = 0;
    public static uint m_Blocks = 0;
    public static int m_Level = 0;
    public static LevelGoal m_LevelGoal = 0;

    // Island Variables
    public static int m_startIsland = -1;
    public static int m_endIsland = -1;

    // Bridge Variables
    public static string[] allWeights = new string[0];
    public static Dictionary<GameObject, List<GameObject>> bridgeBuildParts = new();
    public static Dictionary<GameObject, Vector3> bridgeSegmentOriginalPositions = new();
    public static Bridge[] possibleBridges = new Bridge[0];
    public static List<GameObject> bridgeObjects = new();
    public static List<GameObject> bridgeSegments = new();
    public static int m_totalIslands = 0;

    // LevelCompleteCheck and puzzle Variables
    public static int m_requiredBlocks = -1;
    public static int m_multiplechoiceconnection = -1;
    public static uint neededweight = 0;
    public static uint SelectedWeightOption = 0;
    public static bool puzzleModeActive = false;

    
    // ------------------------------
    //  GlobalVariables->FUNCTIONS
    // ------------------------------
    public static bool PurchaseWithCoins(uint amount) 
    {
        if (m_Coins < amount)
            return false;

        m_Coins -= amount;
        return true;
    }
    public static bool PurchaseWithBlocks(uint amount)
    {
        m_Blocks += amount;
        return true;
    }
    // =======================================
}
