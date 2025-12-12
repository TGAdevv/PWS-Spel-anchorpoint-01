using UnityEngine;

public class CameraEditor : MonoBehaviour
{
    public float CameraSpeed;
    public float WalkSpeed;
    public float RunSpeed;
    Vector3 Angles;

    public void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            float rotationY = Input.GetAxis("Mouse Y") * CameraSpeed;
            float rotationX = Input.GetAxis("Mouse X") * CameraSpeed;
            if (rotationY > 0)
                Angles = new Vector3(Mathf.MoveTowards(Angles.x, -80, rotationY), Angles.y + rotationX, 0);
            else
                Angles = new Vector3(Mathf.MoveTowards(Angles.x, 80, -rotationY), Angles.y + rotationX, 0);
            transform.localEulerAngles = Angles;


            float speed = (Input.GetKey(KeyCode.LeftShift) ? RunSpeed : WalkSpeed);

            transform.position += speed * Input.GetAxisRaw("Vertical")   * Time.deltaTime * transform.forward;
            transform.position += speed * Input.GetAxisRaw("Horizontal") * Time.deltaTime * transform.right;

            if (Input.GetKey(KeyCode.E))
                transform.position += Time.deltaTime * speed * Vector3.up;
            if (Input.GetKey(KeyCode.Q))
                transform.position += Time.deltaTime * speed * Vector3.down;
        }
        else
            Cursor.lockState = CursorLockMode.None;
    }
}
