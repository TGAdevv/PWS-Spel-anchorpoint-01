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
        if (LevelEditorParameters.currentConnection == null)
            return;

        if(currentConnection != LevelEditorParameters.currentConnection)
        {
            currentConnection = LevelEditorParameters.currentConnection;
            slider.value = currentConnection.weight;
        }
        if (slider.value != LevelEditorParameters.currentConnection.weight)
            LevelEditorParameters.currentConnection.weight = (int)slider.value;
    }
}
