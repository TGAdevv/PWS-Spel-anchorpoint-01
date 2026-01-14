using UnityEngine;
using UnityEngine.UI;

public class StartTutorial : MonoBehaviour
{
    [SerializeField] Image Panel, Mouse_ButDown_left, Mouse_Scroll;

    bool PanningTutorialDone = false;
    bool ScrollingTutorialDone = false;

    private void Update()
    {
        if (!PanningTutorialDone)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Panel.color -= Color.black;
                Mouse_ButDown_left.color -= Color.black;
            }

            if (Input.GetMouseButtonUp(0))
            {
                PanningTutorialDone = true;
                Panel.color += Color.black;
                Mouse_Scroll.color += Color.black;
            }
        }
        else if (!ScrollingTutorialDone)
        {
            if (Input.mouseScrollDelta.y != 0)
            {
                ScrollingTutorialDone = true;
                Panel.color -= Color.black;
                Mouse_Scroll.color -= Color.black;
            }
        }
    }
}
