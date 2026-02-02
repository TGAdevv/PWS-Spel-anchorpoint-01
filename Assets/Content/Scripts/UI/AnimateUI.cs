using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

enum Property
{
    scale,
    imageTransparency,
    xPivot,
    yPivot
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
    [SerializeField] UnityEvent OnAnimationFinished;

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

        if (!rect && currentAnim.property != Property.imageTransparency)
            rect = GetComponent<RectTransform>();
        else if (!img)
            img = GetComponent<Image>();

        float t = Mathf.Clamp(Timer / Duration, 0, 1);

        switch (currentAnim.property)
        {
            case Property.scale:
                rect.localScale = Vector3.one * currentAnim.ScaleCurve.Evaluate(t);
                break;
            case Property.imageTransparency:
                img.color = new Color(img.color.r, img.color.g, img.color.b, currentAnim.ScaleCurve.Evaluate(t));
                break;
            case Property.xPivot:
                rect.pivot = new(currentAnim.ScaleCurve.Evaluate(t), rect.pivot.y);
                break;
            case Property.yPivot:
                rect.pivot = new(rect.pivot.x, currentAnim.ScaleCurve.Evaluate(t));
                break;
            default:
                break;
        }
    }

    void Update()
    {
        if (Timer <= Duration && Timer != -1) 
            AnimationTick();
        if (Timer > Duration) 
        {
            AnimationTick();
            OnAnimationFinished.Invoke();
            if (animations[currentID].DeactivateAfter)
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