using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OrbitalCamera : MonoBehaviour
{
    public Transform focusPoint;
    public float rotationSpeed;
    public float smoothTime;
    public float scaleSpeed;

    [Range(0, 50)]
    public int minZoom = 5;
    [Range(100, 300)]
    public int maxZoom = 220;

    Vector2 curAngle = new(10, 0);
    Vector2 AngleVel = new(10, 0);

    Vector2 vel = new();

    public LayerMask bridgeMask;

    bool moveCamera;
    Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        // Check if any bridge is in puzzle mode
        bool inPuzzleMode = IsAnyBridgeInPuzzleMode();
        if (!inPuzzleMode)
        {
            curAngle += AngleVel;

            if (!Input.GetMouseButton(0))
            {
                AngleVel = Vector2.SmoothDamp(AngleVel, Vector2.zero, ref vel, smoothTime);
                moveCamera = false;
            }
            curAngle = new Vector2(Mathf.Clamp(curAngle.x, 10, 70), curAngle.y);
            focusPoint.transform.rotation = Quaternion.Euler(curAngle);
        
            if (Input.GetMouseButtonDown(0) && !Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), 1000, bridgeMask) && !Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), 1000, LayerMask.GetMask("UI")))
            {
                EventSystem currentEventSys = EventSystem.current;
                PointerEventData eventData = new(currentEventSys);
                eventData.position = Input.mousePosition;
                List<RaycastResult> results = new();
                currentEventSys.RaycastAll(eventData, results);
                if (results.Count == 0)
                    moveCamera = true;
            }
                
            if (moveCamera) 
            {
                Vector2 deltaMouse = Input.mousePositionDelta;
                AngleVel = new Vector2(-deltaMouse.y, deltaMouse.x) * rotationSpeed;
            }
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            EventSystem currentEventSys = EventSystem.current;
            PointerEventData eventData = new(currentEventSys);
            eventData.position = Input.mousePosition;
            List<RaycastResult> results = new();
            currentEventSys.RaycastAll(eventData, results);
            bool zoomingAllowed = true;
            if (results.Count == 0)
                cam.orthographicSize -= Input.mouseScrollDelta.y * scaleSpeed;
            else
            {
                foreach (var result in results)
                    if (result.gameObject.CompareTag("Menu"))
                    {
                        zoomingAllowed = false;
                        break;
                    }
                if (zoomingAllowed)
                    cam.orthographicSize -= Input.mouseScrollDelta.y * scaleSpeed;
            }
        }

        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }

    private bool IsAnyBridgeInPuzzleMode()
    {
        // Find all ClickToCreateBridge components and check if any are in puzzle mode
        ClickToCreateBridge[] bridges = FindObjectsByType<ClickToCreateBridge>(FindObjectsSortMode.None);
        foreach (var bridge in bridges)
        {
            if (bridge.IsInPuzzleMode())
                return true;
        }
        return false;
    }
}
