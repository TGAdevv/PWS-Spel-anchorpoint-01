using UnityEngine;
using TMPro;
using UnityEngine.Events;

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
            case LevelGoal.CONNECT_ALL_ISLANDS or LevelGoal.FIND_SHORTEST_ROUTE:
                if (req_blocks >= HintBridgeToTextThreshold)
                {
                    HintText.text = $"Er zijn in totaal {req_blocks} blokken nodig";
                    ShowHintPanel.Invoke();
                }
                else
                    Debug.LogWarning("HINT SYSTEM FOR " + GlobalVariables.m_LevelGoal.ToString() + " NOT FULLY IMPLEMENTED YET");
                break;

            case LevelGoal.OPTIMIZE_PROCESS:
                //HintText.text = $"Het langste proces kost {ChckLvlFinished.LevelRoutes[ChckLvlFinished.currentLvlRoutes].MaxWeight} blokken";
                ShowHintPanel.Invoke();
                break;
        }
    }
}
