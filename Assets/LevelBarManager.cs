using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class LevelBarManager : MonoBehaviour
{
    public enum LevelStatus
    {
        Locked,
        In_Progress,
        Incomplete,
        Completed
    }

    public UnityEvent OnLevelNotUnlockedYet;
    public UnityEvent OnLevelAlreadyCompleted;

    public LevelImporter levelImporter;

    [System.NonSerialized] public LevelStatus[] LevelStatusses = new LevelStatus[18];

    public Image[] LevelButtonImages = new Image[6];

    public Color Locked, In_progress, Incomplete, Completed;
    Color[] colors;

    bool setupComplete = false;

    public void ClickedOnLevel(int Level)
    {
        int LevelOffset = Mathf.FloorToInt(GlobalVariables.m_Level / 6) * 6;

        switch (LevelStatusses[LevelOffset + Level])
        {
            case LevelStatus.Locked:
                OnLevelNotUnlockedYet.Invoke();
                break;

            case LevelStatus.Incomplete:
                LevelStatusses[GlobalVariables.m_Level] = LevelStatus.Incomplete;
                levelImporter.ImportLevel(LevelOffset + Level);
                break;

            case LevelStatus.Completed:
                OnLevelAlreadyCompleted.Invoke();
                break;

            default:
                break;
        }
    }

    public void LevelCompleted(int StarCount)
    {
        LevelStatusses[GlobalVariables.m_Level] = (StarCount == 3) ? LevelStatus.Completed : LevelStatus.Incomplete;
    }
    public void OnLevelImport()
    {
        LevelStatusses[GlobalVariables.m_Level] = LevelStatus.In_Progress;
        int LevelOffset = Mathf.FloorToInt(GlobalVariables.m_Level / 6) * 6;

        if (!setupComplete)
        {
            colors = new Color[4] { Locked, In_progress, Incomplete, Completed };
            levelImporter.levelBarManager = this;

            for (int i = 0; i < LevelStatusses.Length; i++)
            {
                LevelStatusses[i] = LevelStatus.Locked;
            }
            LevelStatusses[GlobalVariables.m_Level - LevelOffset] = LevelStatus.In_Progress;

            setupComplete = true;
        }

        // Refresh UI
        for (int i = 0; i < 6; i++)
        {
            int Level = LevelOffset + i;
            LevelStatus lvlStatus = LevelStatusses[Level];
            TMP_Text txt = LevelButtonImages[i].GetComponentInChildren<TMP_Text>();

            LevelButtonImages[i].color = colors[(int)lvlStatus];
            LevelButtonImages[i].rectTransform.sizeDelta = Vector2.one * ((lvlStatus == LevelStatus.In_Progress) ? 40 : 30);
            txt.fontSize = (lvlStatus == LevelStatus.In_Progress) ? 40 : 17;
            txt.text     = (lvlStatus == LevelStatus.Locked) ? "" : (i + 1).ToString();
        }
    }
}
