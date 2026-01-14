using UnityEngine;

public class UpdateGoalText : MonoBehaviour
{
    [System.Serializable]
    public struct GoalText
    {
        public LevelGoal Goal;
        
        [TextArea(0, 5)]
        public string Text;
    }

    public GoalText[] goalTexts;

    public TMPro.TMP_Text Text;

    public void Tick()
    {
        foreach (var goal_txt in goalTexts)
        {
            if (GlobalVariables.m_LevelGoal == goal_txt.Goal)
            {
                Text.text = goal_txt.Text;
                return;
            }
        }

        Debug.LogWarning("Could not find goal text for: " + GlobalVariables.m_LevelGoal.ToString());
    }
}
