using System.Collections.Generic;
using UnityEngine;

public enum LevelGoal
{
    CONNECT_ALL_ISLANDS,
    OPTIMIZE_PROCESS,
    FIND_SHORTEST_ROUTE,  
}

public static class GlobalVariables
{
    // ------------------------------
    //  GlobalVariables->VARIABLES
    // ------------------------------
    public static uint m_Coins = 0;
    public static uint m_Diamonds = 0;
    public static uint m_Blocks = 0;
    public static int m_requiredBlocks = -1;
    public static int  m_Level = 0;
    public static int m_totalIslands = 0;
    public static string[] possibleBridges = new string[0];
    public static int m_startIsland = -1;
    public static int m_endIsland = -1;
    public static List<GameObject> bridgeObjects = new List<GameObject>();
    public static LevelGoal m_LevelGoal = 0;
    
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
    public static bool PurchaseWithDiamonds(uint amount)
    {
        if (m_Diamonds < amount)
            return false;

        m_Diamonds -= amount;
        return true;
    }
    public static bool PurchaseWithBlocks(uint amount)
    {
        m_Blocks += amount;
        return true;
    }
    public static int[] IdentifyPossibletargetislands(int startIsland)
    {
        return new int[] { -1, -1 };
    }
    // =======================================
}
