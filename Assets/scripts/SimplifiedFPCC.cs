// ==========================================
// SimplifiedFPCC.cs
// Clean, easy-to-understand FPC controller
// ==========================================

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimplifiedFPCC : MonoBehaviour
{
    [Header("=== REFERENCES ===")]
    public Transform cameraTransform;
    private CharacterController controller;

    [Header("=== MOUSE LOOK ===")]
    public float mouseSensitivity = 2f;
    public float maxLookUp = 90f;
    public float maxLookDown = 90f;
    private float verticalLookAngle = 0f;

    [Header("=== MOVEMENT ===")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float crouchSpeed = 2.5f;
    public float groundDrag = 5f;
    private Vector3 currentVelocity = Vector3.zero;

    [Header("=== JUMPING ===")]
    public bool allowJump = false;
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    private float verticalVelocity = 0f;

    [Header("=== GROUND CHECK ===")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.2f;
    private bool isGrounded = false;

    [Header("=== CROUCH ===")]
    public float crouchHeight = 1f;
    public float normalHeight = 2f;
    public float crouchSpeed_transition = 5f;
    private bool isCrouching = false;
    private float targetHeight;

    [Header("=== CAMERA BOB ===")]
    public bool enableHeadBob = true;
    public float bobSpeed = 5f;
    public float bobAmount = 0.1f;
    private float bobTimer = 0f;
    private Vector3 cameraStartPos;

    [Header("=== CURSOR ===")]
    public bool lockCursorOnStart = true;
    private bool cursorLocked = true;

    void Start()
    {
        // Setup references
        controller = GetComponent<CharacterController>();
        
        if (!cameraTransform)
        {
            Debug.LogError("Camera not assigned!");
            return;
        }

        cameraStartPos = cameraTransform.localPosition;
        targetHeight = normalHeight;
        controller.height = normalHeight;

        // Lock cursor
        if (lockCursorOnStart)
        {
            LockCursor(true);
        }
    }

    void Update()
    {
        // Check if grounded
        CheckGrounded();

        // Handle all inputs
        HandleMouseLook();
        HandleMovement();
        HandleCrouch();
        HandleJump();
        HandleCursorToggle();

        // Apply gravity
        ApplyGravity();

        // Apply movement to controller
        controller.Move(currentVelocity * Time.deltaTime);

        // Camera bob
        if (enableHeadBob)
        {
            UpdateCameraBob();
        }
    }

    // ===== MOUSE LOOK =====
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate body left/right
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera up/down
        verticalLookAngle -= mouseY;
        verticalLookAngle = Mathf.Clamp(verticalLookAngle, -maxLookUp, maxLookDown);

        cameraTransform.localRotation = Quaternion.Euler(verticalLookAngle, 0f, 0f);
    }

    // ===== MOVEMENT =====
    void HandleMovement()
    {
        // Get input
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        // Determine speed
        float targetSpeed = walkSpeed;
        if (Input.GetKey(KeyCode.LeftShift) && isGrounded && !isCrouching)
        {
            targetSpeed = sprintSpeed;
        }
        if (isCrouching)
        {
            targetSpeed = crouchSpeed;
        }

        // Create movement direction relative to player rotation
        Vector3 moveDirection = (transform.right * inputX + transform.forward * inputY);

        // Apply drag
        if (moveDirection.magnitude > 0)
        {
            currentVelocity = Vector3.Lerp(currentVelocity, moveDirection.normalized * targetSpeed, groundDrag * Time.deltaTime);
        }
        else
        {
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, groundDrag * Time.deltaTime);
        }

        // Only move horizontally (keep vertical velocity separate)
        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        currentVelocity = horizontalVelocity + new Vector3(0, verticalVelocity, 0);
    }

    // ===== CROUCH =====
    void HandleCrouch()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            isCrouching = true;
            targetHeight = crouchHeight;
        }
        else
        {
            isCrouching = false;
            targetHeight = normalHeight;
        }

        // Smoothly transition height
        controller.height = Mathf.Lerp(controller.height, targetHeight, crouchSpeed_transition * Time.deltaTime);

        // Adjust camera height with crouch
        float heightDifference = (controller.height - normalHeight) / 2f;
        Vector3 newCamPos = cameraStartPos;
        newCamPos.y += heightDifference;
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newCamPos, crouchSpeed_transition * Time.deltaTime);
    }

    // ===== JUMP =====
    void HandleJump()
    {
        if (!allowJump) return;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isCrouching)
        {
            verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    // ===== GRAVITY =====
    void ApplyGravity()
    {
        if (isGrounded)
        {
            verticalVelocity = -2f; // Small negative to keep grounded
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    // ===== GROUND CHECK =====
    void CheckGrounded()
    {
        Vector3 spherePosition = transform.position + Vector3.down * (controller.height / 2f);
        isGrounded = Physics.CheckSphere(spherePosition, groundCheckDistance, groundLayer);
    }

    // ===== CAMERA BOB =====
    void UpdateCameraBob()
    {
        float speed = new Vector3(currentVelocity.x, 0, currentVelocity.z).magnitude;

        if (speed > 0.1f && isGrounded)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float bobY = Mathf.Sin(bobTimer) * bobAmount;
            float bobX = Mathf.Cos(bobTimer * 0.5f) * bobAmount * 0.5f;

            Vector3 bobOffset = cameraStartPos;
            bobOffset.y += bobY;
            bobOffset.x += bobX;

            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, bobOffset, Time.deltaTime * 5f);
        }
        else
        {
            bobTimer = 0f;
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, cameraStartPos, Time.deltaTime * 5f);
        }
    }

    // ===== CURSOR CONTROL =====
    void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cursorLocked = !cursorLocked;
            LockCursor(cursorLocked);
        }
    }

    void LockCursor(bool lockIt)
    {
        if (lockIt)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        cursorLocked = lockIt;
        Debug.Log("Cursor locked: " + lockIt);
    }

    public bool IsCursorLocked() => cursorLocked;
    public bool IsGrounded() => isGrounded;
    public bool IsCrouching() => isCrouching;
}
