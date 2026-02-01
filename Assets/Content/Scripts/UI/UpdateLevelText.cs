using UnityEngine;

public class UpdateLevelText : MonoBehaviour
{
    TMPro.TMP_Text lvlTMP;
    readonly string Alphabet = "abcdefghijklmnopqrstuvwxyz";

    private void Start()
    {
        lvlTMP = GetComponent<TMPro.TMP_Text>();

        if (!lvlTMP) { Debug.LogError("Could not find text component"); enabled = false; }
    }

    public void Tick()
    {
        int levelCategory = Mathf.FloorToInt(GlobalVariables.m_Level / 6);
        lvlTMP.text = "Level " + (GlobalVariables.m_Level % 6 + 1) + Alphabet[levelCategory];
    }
}
