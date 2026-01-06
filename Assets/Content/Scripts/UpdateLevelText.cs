using UnityEngine;

public class UpdateLevelText : MonoBehaviour
{
    TMPro.TMP_Text lvlTMP;

    private void Start()
    {
        lvlTMP = GetComponent<TMPro.TMP_Text>();

        if (!lvlTMP) { Debug.LogError("Could not find text component"); enabled = false; }
    }

    public void Tick()
    {
        lvlTMP.text = "Level " + (GlobalVariables.m_Level + 1);
    }
}
