using UnityEngine;

public class ClickToCreateBridge : MonoBehaviour
{
    SplineEditor spline;
    float bridgeActive = 0;
    float targetBridgeActive = 0;
    float speed = 3;

    void Start()
    {
        spline = GetComponent<SplineEditor>();

        if (!spline) {
            Debug.LogError("Could not find SplineEditor!");
            enabled = false;
        }
    }
    private void OnMouseDown()
    {
        print("Clicked " + gameObject.name);
        targetBridgeActive = 1 - targetBridgeActive;
    }

    float timer = 0;

    private void Update()
    {
        if (bridgeActive != targetBridgeActive) 
        {
            timer += Time.deltaTime * speed;
            bridgeActive = Mathf.Lerp(1 - targetBridgeActive, targetBridgeActive, timer);
        }

        if (timer > 1) 
        {
            bridgeActive = targetBridgeActive;
            timer = 0;
        }

        spline.percentage_transparent = 1 - bridgeActive;
    }
}
