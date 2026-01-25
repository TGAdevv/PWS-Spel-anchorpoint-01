using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ShowLevelFinishedAnimation : MonoBehaviour
{
    public TMP_Text CoinRewardTXT;
    public UnityEvent OnAnimationFinished;
    public CheckIfLevelFinished checkIfLevelFinished;

    [Header("Index 0 -> hide all stars\n Index 1 -> show star 1\n Index 2 -> show star 2\n etc")]
    public UnityEvent[] ShowStar;

    //                                     0 Stars->0 coins, 1 Star->2 coins etc
    readonly uint[] CoinRewardPerStarCount = new uint[4] { 0, 2, 3, 5 };

    public IEnumerator LevelFinished(int StarCount)
    {
        ShowStar[0].Invoke();
        for (int i = StarCount; i >= 1; i--) {
            ShowStar[i].Invoke(); 
        }

        // Play animation here
        if (GlobalVariables.m_LevelGoal == LevelGoal.OPTIMIZE_PROCESS)
        {

        }

        CoinRewardTXT.text = CoinRewardPerStarCount[StarCount].ToString();
        GlobalVariables.m_Coins += CoinRewardPerStarCount[StarCount];
        OnAnimationFinished.Invoke();
        yield return new();
    }
}