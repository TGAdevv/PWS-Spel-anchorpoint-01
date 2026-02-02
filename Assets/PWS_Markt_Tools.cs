using UnityEngine;
using UnityEngine.Events;

public class PWS_Markt_Tools : MonoBehaviour
{
    [SerializeField] UnityEvent enableTools;
    [SerializeField] UnityEvent disableTools;

    public float maxInterval;

    const string GoalCode = "QWEE";
    string CurCode = "";

    float timer;
    bool toolsEnabled = false;

    void Update()
    {
        if (timer <= 0)
        {
            CurCode = "";
            return;
        }

        timer -= Time.unscaledDeltaTime;
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (!e.isKey)
            return;

        if (e.keyCode == KeyCode.None)
            return;

        if (!Input.GetKeyDown(e.keyCode))
            return;

        timer = maxInterval;
        CurCode += e.keyCode.ToString();

        if (CurCode == GoalCode)
        {
            toolsEnabled = !toolsEnabled;

            if (toolsEnabled)
                enableTools.Invoke();
            else
                disableTools.Invoke();
        }
    }
}
