using UnityEngine;

public class TheoryText : MonoBehaviour
{

    [System.Serializable]
    public struct Chapter
    {
        public string Title;
        [TextArea(0, 25)] public string ChapterContent;
        public int SetAtLevel;
    }

    [SerializeField] Chapter[] Chapters;
    int currentChapter = -1;

    [SerializeField] TMPro.TMP_Text Title;
    [SerializeField] TMPro.TMP_Text ChapterContent;

    public void Tick()
    {
        if (Chapters.Length <= currentChapter + 1)
            return;

        if (Chapters[currentChapter + 1].SetAtLevel == GlobalVariables.m_Level)
        {
            currentChapter++;

            Title.text          = Chapters[currentChapter].Title;
            ChapterContent.text = Chapters[currentChapter].ChapterContent;
        }
    }
}
