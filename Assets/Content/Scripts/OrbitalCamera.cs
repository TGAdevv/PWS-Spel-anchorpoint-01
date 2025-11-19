using UnityEngine;

public class OrbitalCamera : MonoBehaviour
{
    public Transform focusPoint;
    public float rotationSpeed;
    public float smoothTime;
    public float scaleSpeed;

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
        curAngle += AngleVel;

        if (!Input.GetMouseButton(0))
        {
            AngleVel = Vector2.SmoothDamp(AngleVel, Vector2.zero, ref vel, smoothTime);
            moveCamera = false;
        }

        curAngle = new Vector2(Mathf.Clamp(curAngle.x, 10, 70), curAngle.y);
        focusPoint.transform.rotation = Quaternion.Euler(curAngle);

        if (Input.GetMouseButtonDown(0) && !Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), 1000, bridgeMask) && !Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), 1000, LayerMask.GetMask("UI")))
            moveCamera = true;
        if (moveCamera) 
        {
            Vector2 deltaMouse = Input.mousePositionDelta;
            AngleVel = new Vector2(-deltaMouse.y, deltaMouse.x) * rotationSpeed;
        }

        if (cam.orthographic)
            cam.orthographicSize -= Input.mouseScrollDelta.y * scaleSpeed;
        else
            transform.localPosition += Vector3.forward * Input.mouseScrollDelta.y * scaleSpeed;
    }
}
