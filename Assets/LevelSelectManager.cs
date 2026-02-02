using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectManager : MonoBehaviour
{
    Button[] levelButtons = new Button[18];
    TMP_Text[] levelText  = new TMP_Text[18];

    public LevelImporter levelImporter;
    public LevelBarManager LevelBarManager;

    void Start()
    {
        int childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Transform trans = transform.GetChild(i);
            levelButtons[i] = trans.GetComponent<Button>();
            levelText[i]    = trans.GetComponentInChildren<TMP_Text>();

            int levelIndex = i;
            int levelOffset = Mathf.FloorToInt(i / 6f) * 6;
            int indexInCategory = levelIndex - levelOffset;

            levelText[i].text = (i + 1).ToString();

            levelButtons[i].onClick.AddListener(delegate {
                for (int j = 0; j < LevelBarManager.LevelStatusses.Length; j++)
                    LevelBarManager.LevelStatusses[j] = LevelBarManager.LevelStatus.Locked;
                for (int j = 0; j < indexInCategory; j++)
                    LevelBarManager.LevelStatusses[j + levelOffset] = LevelBarManager.LevelStatus.Completed;

                levelImporter.ImportLevel(levelIndex);
            });
        }

        Destroy(this);
    }
}
