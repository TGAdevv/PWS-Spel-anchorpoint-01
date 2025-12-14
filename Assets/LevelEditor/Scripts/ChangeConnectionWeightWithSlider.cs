using UnityEngine;
using UnityEngine.UI;

public class ChangeConnectionWeightWithSlider : MonoBehaviour
{
    Slider slider;
    Connection currentConnection;

    private void Start()
    {
        currentConnection = LevelEditorParameters.currentConnection;
        slider = GetComponent<Slider>();
    }

    private void Update()
    {
        if(currentConnection != null)
            if (!currentConnection.begin)
                currentConnection = null;
        if (LevelEditorParameters.currentConnection == null)
            return;

        if(currentConnection != LevelEditorParameters.currentConnection)
        {
            if (currentConnection != null)
                currentConnection.begin.parent.GetComponent<Image>().color = new(0.1908152f, 0.3495883f, 0.6037736f);
            currentConnection = LevelEditorParameters.currentConnection;
            if (!currentConnection.begin)
            {
                LevelEditorParameters.currentConnection = null;
                currentConnection = null;
                return;
            }
            currentConnection.begin.parent.GetComponent<Image>().color = new(0.8207547f, 0.4647784f, 0.3058473f);
            slider.value = currentConnection.weight;
        }
        if (slider.value != LevelEditorParameters.currentConnection.weight)
            LevelEditorParameters.currentConnection.weight = (int)slider.value;
    }
}
