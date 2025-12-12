using UnityEngine;

public class AxisScript : MonoBehaviour
{
    [SerializeField] BoxCollider x, y, z;

    [System.NonSerialized] public SplineEditor host;

    Vector3 prevPosition;
    int defaultSplineRes;
    Vector3 defaultAxisColSize;
    bool movingAxis;

    LineRenderer lineRenderer;

    private void Start()
    {
        defaultSplineRes = host.resolution;
        defaultAxisColSize = x.size;

        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (lineRenderer.positionCount == 2)
            lineRenderer.SetPosition(1, host.SamplePointInCurve(.5f) - transform.position);
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 10000) || (hit.collider != x && hit.collider != y && hit.collider != z))
        {
            if (movingAxis)
            {
                movingAxis = false;
                host.resolution = defaultSplineRes;
                host.GenerateSpline();
                host.RenderSpline();
            }
            return;
        }

        x.size = defaultAxisColSize.x * (hit.collider == x ? 5f : 0) * Vector3.one + defaultAxisColSize;
        y.size = defaultAxisColSize.x * (hit.collider == y ? 5f : 0) * Vector3.one + defaultAxisColSize;
        z.size = defaultAxisColSize.x * (hit.collider == z ? 5f : 0) * Vector3.one + defaultAxisColSize;

        if (Input.GetMouseButton(0))
        {
            movingAxis = true;

            if (hit.collider == x)
                transform.parent.position += Vector3.right * (hit.point - prevPosition).x;
            if (hit.collider == y)
                transform.parent.position += Vector3.up * (hit.point - prevPosition).y;
            if (hit.collider == z)
                transform.parent.position += Vector3.forward * (hit.point - prevPosition).z;

            host.resolution = 15;
            host.GenerateSpline();
            host.RenderSpline();
        }
        else if (movingAxis)
        {
            movingAxis = false;
            host.resolution = defaultSplineRes;
            host.GenerateSpline();
            host.RenderSpline();
        }

        prevPosition = hit.point;
    }
}
