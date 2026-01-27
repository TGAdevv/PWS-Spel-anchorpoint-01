using UnityEngine;
using UnityEngine.UI;

public class ChangeImageColor : MonoBehaviour
{
    Image img;
    RawImage img_raw;

    private void Start()
    {
        img = GetComponent<Image>();

        if (!img)
            img_raw = GetComponent<RawImage>();
    }

    public void ChangeColor(string colorStr)
    {
        if (!ColorUtility.TryParseHtmlString(colorStr, out Color newColor))
            return;

        if (img_raw)
        {
            img_raw.color = newColor;
            return;
        }
        if (img)
        {
            img.color = newColor;
        }
    }
}
