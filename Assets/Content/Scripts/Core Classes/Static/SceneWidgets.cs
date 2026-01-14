using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public static class SceneWidgets
{
    public struct Widget
    {
        public RectTransform rectTransform;
        public Vector3       worldSpacePos;

        public Widget (RectTransform Widget, Vector3 WorldPosition)
        {
            rectTransform = Widget;
            worldSpacePos = WorldPosition;
        }
    }

    static List<Widget> Widgets = new();
    static Camera camera = null;
    static RectTransform canvas = null;

    public static void Init(Camera _camera, RectTransform _canvas, bool removePreviousWidgets = false)
    {
        if (Widgets.Count > 0 && removePreviousWidgets)
            ClearAll();

        camera = _camera;
        canvas = _canvas;
    }
    public static void Tick()
    {
        foreach (Widget item in Widgets)
        {
            Vector2 screenPoint = camera.WorldToScreenPoint(item.worldSpacePos);
            screenPoint = new Vector2(screenPoint.x / Screen.width, screenPoint.y / Screen.height);

            item.rectTransform.anchorMax = screenPoint;
            item.rectTransform.anchorMin = screenPoint;
        }
    }

    private static T CheckForComponent<T>(GameObject OBJ)
    {
        T Comp = OBJ.GetComponent<T>();

        if (Comp == null)
            Debug.LogError("Widget Prefab \"" + OBJ.name + "\" Does Not Have " + typeof(T).FullName + " Component");

        return Comp;
    }
    private static T CheckForComponentInChildren<T>(GameObject OBJ)
    {
        T Comp = OBJ.GetComponentInChildren<T>();

        if (Comp == null)
            Debug.LogError("Widget Prefab \"" + OBJ.name + "\" Does Not Have " + typeof(T).FullName + " Component");

        return Comp;
    }

    public static RectTransform AddNew(GameObject Prefab, Vector3 WorldPosition) 
    {
        if (!camera || !canvas){
            Debug.LogError("SceneWidgets not properly initialized, please call Init() first!");
            return null; }

        GameObject WidgetOBJ = Object.Instantiate(Prefab, canvas);

        RectTransform rect = CheckForComponent<RectTransform>(WidgetOBJ);
        if (!rect) { return null; }

        Widgets.Add(new(rect, WorldPosition));
        return rect;
    }
    public static RectTransform AddNew(GameObject Prefab, Vector3 WorldPosition, string ChildText)
    {
        if (!camera || !canvas) {
            Debug.LogError("SceneWidgets not properly initialized, please call Init() first!");
            return null; }

        GameObject WidgetOBJ = Object.Instantiate(Prefab, canvas);

        RectTransform rect = CheckForComponent<RectTransform>(WidgetOBJ);
        if (!rect) { return null; }

        TMP_Text text = CheckForComponentInChildren<TMP_Text>(WidgetOBJ);
        if (!text) { return null; }
        text.text = ChildText;

        Widgets.Add(new(rect, WorldPosition));
        return rect;
    }
    public static RectTransform AddNew(GameObject Prefab, Vector3 WorldPosition, string ChildText, Color ParentImgColor)
    {
        if (!camera || !canvas) {
            Debug.LogError("SceneWidgets not properly initialized, please call Init() first!");
            return null; }

        GameObject WidgetOBJ = Object.Instantiate(Prefab, canvas);

        RectTransform rect = CheckForComponent<RectTransform>(WidgetOBJ);
        if (!rect) { return null; }

        TMP_Text text = CheckForComponentInChildren<TMP_Text>(WidgetOBJ);
        if (!text) { return null; }
        text.text = ChildText;

        Image img = CheckForComponent<Image>(WidgetOBJ);
        if (!img) { return null; }
        img.color = ParentImgColor;

        Widgets.Add(new(rect, WorldPosition));
        return rect;
    }

    public static void ClearAll()
    {
        foreach (var item in Widgets)
            Object.Destroy(item.rectTransform.gameObject);
        Widgets = new();
    }
}
