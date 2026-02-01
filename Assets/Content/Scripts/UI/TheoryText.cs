using UnityEngine;
using UnityEngine.Events;

public class TheoryText : MonoBehaviour
{

    [System.Serializable]
    public struct Chapter
    {
        public string Title;

        [TextArea(0, 25)] 
        public string ChapterContent;

        public int SetAtLevel;
    }

    [SerializeField] Chapter[] Chapters;

    [SerializeField] TMPro.TMP_Text Title;
    [SerializeField] TMPro.TMP_Text ChapterContent;

    [SerializeField] UnityEvent OnNewChapter;

    public void Tick()
    {
        for (int i = 0; i < Chapters.Length; i++)
        {
            print(i);
            if (GlobalVariables.m_Level == Chapters[i].SetAtLevel)
                OnNewChapter.Invoke();
            if (i == Chapters.Length - 1)
            {
                Title.text = Chapters[i].Title;
                ChapterContent.text = Chapters[i].ChapterContent;
                return;
            }
            if (GlobalVariables.m_Level >= Chapters[i].SetAtLevel && GlobalVariables.m_Level < Chapters[i + 1].SetAtLevel)
            {
                Title.text = Chapters[i].Title;
                ChapterContent.text = Chapters[i].ChapterContent;
                return;
            }
        }
    }
}
