using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform target; // The car to be followed
    public float smoothness = 5f;
    public float rotationSpeed = 2f;
    public float distance = 5f;
    public float height = 2f;

    private Vector3 offset;
    private bool isGameRunning = false;

    void Start()
    {
        offset = new Vector3(0f, height, -distance);
        // Only lock cursor during actual gameplay
#if !UNITY_EDITOR
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isGameRunning = true;
#endif
    }

    void FixedUpdate()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position;
            transform.position = Vector3.Lerp(transform.position, targetPosition + offset, smoothness * Time.deltaTime);
            transform.LookAt(target.position);

            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            offset = Quaternion.Euler(0, mouseX, 0) * offset;
        }
    }
}