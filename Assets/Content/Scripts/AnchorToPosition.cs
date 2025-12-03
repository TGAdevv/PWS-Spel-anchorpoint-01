using UnityEngine;

public class AnchorToPosition : MonoBehaviour
{
    RectTransform rect;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    public Vector3 _AnchorToPosition(Vector2 anchor) 
    {
        print("Anchor func: " + anchor.ToString());

        rect.anchorMax = anchor;
        rect.anchorMin = anchor;

        return new Vector3(rect.position.x, 0, rect.position.y);
    }
}
