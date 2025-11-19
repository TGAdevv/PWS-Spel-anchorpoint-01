using UnityEngine;
using UnityEngine.UI;

enum Property
{
    scale,
    imageTransparency
};

[System.Serializable]
class Animation
{
    public Property property;
    public AnimationCurve ScaleCurve;
    public float duration;

    [Tooltip("Keep null to not deactivate anything after animation stopped")]
    public GameObject DeactivateAfter;
}

public class AnimateUI : MonoBehaviour
{

    [SerializeField] Animation[] animations;

    RectTransform rect;

    public float Timer;
    float Duration;

    int currentID;

    Image img;

    void AnimationTick() 
    {
        Animation currentAnim = animations[currentID];

        if (Duration == 0)
            return;

        switch (currentAnim.property)
        {
            case Property.scale:
                if (!rect)
                    rect = GetComponent<RectTransform>();
                rect.localScale = Vector3.one * currentAnim.ScaleCurve.Evaluate(Timer / Duration);
                break;
            case Property.imageTransparency:
                if (!img)
                    img = GetComponent<Image>();
                img.color = new Color(img.color.r, img.color.g, img.color.b, currentAnim.ScaleCurve.Evaluate(Timer / Duration));
                break;
            default:
                break;
        }
    }

    void Update()
    {
        if (Timer <= Duration && Timer != -1) 
            AnimationTick();
        if (Timer > Duration && animations[currentID].DeactivateAfter) 
        {
            animations[currentID].DeactivateAfter.SetActive(false);
            Timer = -1;
        }

        if (Timer != -1)
            Timer += Time.deltaTime;
    }

    public void PlayAnimation(int id)
    {
        currentID = id;
        Timer = 0;
        Duration = animations[id].duration;
        AnimationTick();
    }
}