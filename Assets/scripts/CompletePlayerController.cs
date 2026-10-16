using UnityEngine;

public class CompletePlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;

    [Header("Camera (third person)")]
    public Transform cameraPivot;      // empty object, around chest/head height
    public float mouseSensitivity = 2f;

    private Rigidbody rb;
    private Animator animator;
    private float moveX, moveZ, xRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        moveX = Input.GetAxis("Horizontal");
        moveZ = Input.GetAxis("Vertical");

        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(Vector3.up * mx);

        if (cameraPivot != null)
        {
            xRotation = Mathf.Clamp(xRotation - my, -85f, 85f);
            cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        bool sprint = Input.GetKey(KeyCode.LeftShift);
        animator.SetFloat("MoveX", moveX);
        animator.SetFloat("MoveZ", moveZ);
        animator.SetBool("IsSprinting", sprint);
        animator.SetBool("IsMoving", moveX != 0 || moveZ != 0);

        if (Input.GetKeyDown(KeyCode.Escape)) Cursor.lockState = CursorLockMode.None;
    }

    void FixedUpdate()
    {
        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
        Vector3 move = (transform.forward * moveZ + transform.right * moveX).normalized * speed;
        move.y = rb.linearVelocity.y;
        rb.linearVelocity = move;
    }
}   