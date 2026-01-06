using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetTextToSlider : MonoBehaviour
{
    public Slider slider;
    TMP_Text txt;

    string origText;

    private void Start()
    {
        txt = GetComponent<TMP_Text>();
        origText = txt.text;
    }

    private void Update()
    {
        txt.text = origText + slider.value.ToString();
    }
}
