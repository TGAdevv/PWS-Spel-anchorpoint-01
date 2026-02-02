using System.Collections;
using System.Collections.Generic;
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

    public float ShowNewChapterDelay;
    MonoBehaviour camMono;

    HashSet<int> ChaptersAlreadySeen;

    IEnumerator ShowNewChapter()
    {
        yield return new WaitForSeconds(ShowNewChapterDelay);
        OnNewChapter.Invoke();
    }

    public void FlushChaptersAlreadySeen()
    {
        ChaptersAlreadySeen.Clear();
    }
    public void Tick()
    {
        if (!camMono)
            camMono = Camera.main.GetComponent<MonoBehaviour>();

        if (ChaptersAlreadySeen == null)
            ChaptersAlreadySeen = new(Chapters.Length);

        for (int i = 0; i < Chapters.Length; i++)
        {
            if (GlobalVariables.m_Level == Chapters[i].SetAtLevel && !ChaptersAlreadySeen.Contains(i))
            {
                ChaptersAlreadySeen.Add(i);
                camMono.StartCoroutine(ShowNewChapter());
            }
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
