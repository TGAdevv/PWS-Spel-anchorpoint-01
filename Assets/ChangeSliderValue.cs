using UnityEngine;
using UnityEngine.UI;

public class ChangeSliderValue : MonoBehaviour
{
    public Slider slider;

    public void ChangeValue(int change)
    {
        slider.value += change;
    }
    public void ChangeValue(float change)
    {
        slider.value += change;
    }
}
