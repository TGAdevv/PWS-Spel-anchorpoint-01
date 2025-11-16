using UnityEngine;

public class OrbitalCamera : MonoBehaviour
{
    public Transform focusPoint;
    public float rotationSpeed;

    float curAngle = 0;
    float mag;

    private void Start()
    {
        curAngle = Mathf.Atan2(transform.position.z - focusPoint.position.z, transform.position.x - focusPoint.position.x);
        mag = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(focusPoint.position.x, focusPoint.position.z));
    }

    private void Update()
    {
        curAngle += Time.deltaTime * rotationSpeed;
        transform.position = new Vector3(Mathf.Cos(curAngle), 0, Mathf.Sin(curAngle)) * mag + Vector3.up * transform.position.y;
        transform.LookAt(focusPoint);
    }
}
