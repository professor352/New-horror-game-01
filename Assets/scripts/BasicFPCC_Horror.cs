// ------------------------------------------
// BasicFPCC.cs
// Horror-style first person character controller
// Walking, sprinting with stamina, crouching,
// grounded movement, subtle camera bob and jump toggle.
// Based on the original BasicFPCC by Alucard Jay Kay.
// ------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CharacterController))]
public class BasicFPCC : MonoBehaviour
{
    [Header("Layer Mask")]
    [Tooltip("Layer Mask used by the ground and ceiling checks. Ignore the player's own layer.")]
    public LayerMask castingMask;

    [Header("Main Camera")]
    [Tooltip("Drag the FPC Camera here.")]
    public Transform cameraTx;

    [Header("Optional Player Graphic")]
    [Tooltip("Optional capsule to visualize the player in the Scene view.")]
    public Transform playerGFX;

    [Header("Inputs")]
    [Tooltip("Disable if you send inputs from another script.")]
    public bool useLocalInputs = true;

    public string axisLookHorzizontal = "Mouse X";
    public string axisLookVertical = "Mouse Y";
    public string axisMoveHorzizontal = "Horizontal";
    public string axisMoveVertical = "Vertical";

    public KeyCode keyRun = KeyCode.LeftShift;
    public KeyCode keyCrouch = KeyCode.LeftControl;
    public KeyCode keyJump = KeyCode.Space;
    public KeyCode keyToggleCursor = KeyCode.BackQuote;

    // Input Variables that can also be assigned externally.
    [HideInInspector] public float inputLookX = 0f;
    [HideInInspector] public float inputLookY = 0f;
    [HideInInspector] public float inputMoveX = 0f;
    [HideInInspector] public float inputMoveY = 0f;
    [HideInInspector] public bool inputKeyRun = false;
    [HideInInspector] public bool inputKeyCrouch = false;
    [HideInInspector] public bool inputKeyDownJump = false;
    [HideInInspector] public bool inputKeyDownCursor = false;

    [Header("Look Settings")]
    public float mouseSensitivityX = 1.35f;
    public float mouseSensitivityY = 1.35f;
    [Tooltip("Higher values make the camera more responsive.")]
    public float mouseSnappiness = 18f;
    public bool invertLookY = false;
    public float clampLookY = 85f;

    [Header("Horror Movement Settings")]
    [Tooltip("Slow, grounded movement intended for realistic horror games.")]
    public float crouchSpeed = 1.8f;
    public float walkSpeed = 3.2f;
    public float runSpeed = 5.2f;

    [Tooltip("How quickly the player reaches the target speed.")]
    public float acceleration = 9f;
    [Tooltip("How quickly the player stops on the ground.")]
    public float deceleration = 12f;
    [Tooltip("Air control. Lower values feel heavier.")]
    [Range(0f, 1f)] public float airControl = 0.35f;

    [Header("Stamina")]
    public bool useStamina = true;
    public float maxStamina = 5f;
    public float staminaDrain = 1f;
    public float staminaRegen = 1.35f;
    [Tooltip("Seconds before stamina starts recovering after sprinting.")]
    public float staminaRecoveryDelay = 0.65f;

    [Header("Jump")]
    [Tooltip("Disabled by default for a grounded horror-game feel.")]
    public bool allowJump = false;
    public float jumpHeight = 1.25f;
    public float gravity = -18f;

    [Header("Crouch")]
    [Tooltip("Percentage of the normal controller height used while crouching.")]
    [Range(0.35f, 0.8f)] public float crouchHeightPercent = 0.5f;
    public float crouchTransitionSpeed = 6f;

    [Header("Grounded Settings")]
    public float groundCheckY = 0.33f;
    public float ceilingCheckY = 1.83f;
    public float sphereCastRadius = 0.25f;
    public float sphereCastDistance = 0.75f;
    public float raycastLength = 0.75f;
    public Vector3 rayOriginOffset1 = new Vector3(-0.2f, 0f, 0.16f);
    public Vector3 rayOriginOffset2 = new Vector3(0.2f, 0f, -0.16f);

    [Header("Camera Bob")]
    public bool enableHeadBob = true;
    [Tooltip("Very subtle camera motion helps the character feel less robotic.")]
    public float walkBobAmount = 0.025f;
    public float runBobAmount = 0.04f;
    public float crouchBobAmount = 0.015f;
    public float walkBobFrequency = 7f;
    public float runBobFrequency = 10.5f;
    public float crouchBobFrequency = 5f;
    public float cameraBobSmoothness = 10f;

    [Header("Debug Gizmos")]
    public bool showGizmos = false;

    [Header("Runtime State")]
    public float xRotation = 0f;
    public bool isGrounded = false;
    public bool isCeiling = false;
    public bool isCrouching = false;
    public bool isSprinting = false;
    public float stamina = 0f;
    public float currentSpeed = 0f;

    // Private references.
    private CharacterController controller;
    private Transform playerTx;

    private float defaultHeight = 0f;
    private float cameraStartY = 0f;
    private Vector3 cameraStartLocalPosition;
    private float groundOffsetY = 0f;
    private float ceilingOffsetY = 0f;

    private Vector3 verticalVelocity = Vector3.zero;
    private float targetSpeed = 0f;
    private float lastSpeed = 0f;

    private float accMouseX = 0f;
    private float accMouseY = 0f;

    private float bobTimer = 0f;
    private float staminaRecoveryTimer = 0f;

    public bool cursorActive = false;

    void Start()
    {
        Initialize();
    }

    void Update()
    {
        ProcessInputs();
        ProcessLook();
        ProcessMovement();
        ProcessCameraBob();
    }

    void Initialize()
    {
        if (!cameraTx)
        {
            Debug.LogError("* " + gameObject.name + ": BasicFPCC has NO CAMERA ASSIGNED in the Inspector *");
        }

        controller = GetComponent<CharacterController>();
        playerTx = transform;

        defaultHeight = controller.height;
        lastSpeed = 0f;
        stamina = maxStamina;

        cameraStartY = cameraTx ? cameraTx.localPosition.y : 2.0f;
        cameraStartLocalPosition = cameraTx
            ? cameraTx.localPosition
            : new Vector3(0f, cameraStartY, 0f);

        groundOffsetY = groundCheckY;
        ceilingOffsetY = ceilingCheckY;

        RefreshCursor();
    }

    void ProcessInputs()
    {
        if (useLocalInputs)
        {
            inputLookX = Input.GetAxis(axisLookHorzizontal);
            inputLookY = Input.GetAxis(axisLookVertical);

            inputMoveX = Input.GetAxis(axisMoveHorzizontal);
            inputMoveY = Input.GetAxis(axisMoveVertical);

            inputKeyRun = Input.GetKey(keyRun);
            inputKeyCrouch = Input.GetKey(keyCrouch);

            inputKeyDownJump = Input.GetKeyDown(keyJump);
            inputKeyDownCursor = Input.GetKeyDown(keyToggleCursor);
        }

        if (inputKeyDownCursor)
        {
            ToggleLockCursor();
        }
    }

    void ProcessLook()
    {
        accMouseX = Mathf.Lerp(accMouseX, inputLookX, mouseSnappiness * Time.deltaTime);
        accMouseY = Mathf.Lerp(accMouseY, inputLookY, mouseSnappiness * Time.deltaTime);

        float mouseX = accMouseX * mouseSensitivityX * 100f * Time.deltaTime;
        float mouseY = accMouseY * mouseSensitivityY * 100f * Time.deltaTime;

        xRotation += (invertLookY ? mouseY : -mouseY);
        xRotation = Mathf.Clamp(xRotation, -85f, 30f);  // -85 up, 30 down max

        if (cameraTx)
        {
            Quaternion currentBobRotation = cameraTx.localRotation;
            cameraTx.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        playerTx.Rotate(Vector3.up * mouseX);
    }

    void ProcessMovement()
    {
        GroundCheck();
        CeilingCheck();

        Vector3 inputMove = (playerTx.right * inputMoveX) + (playerTx.forward * inputMoveY);

        // Prevent faster diagonal movement.
        if (inputMove.magnitude > 1f)
        {
            inputMove.Normalize();
        }

        bool hasMovementInput = inputMove.sqrMagnitude > 0.01f;
        bool wantsToSprint = inputKeyRun && !inputKeyCrouch && hasMovementInput && isGrounded;
        bool canSprint = !useStamina || stamina > 0.05f;

        isSprinting = wantsToSprint && canSprint;

        // Stamina.
        if (useStamina)
        {
            if (isSprinting)
            {
                stamina -= staminaDrain * Time.deltaTime;
                stamina = Mathf.Max(stamina, 0f);
                staminaRecoveryTimer = staminaRecoveryDelay;
            }
            else
            {
                staminaRecoveryTimer -= Time.deltaTime;

                if (staminaRecoveryTimer <= 0f)
                {
                    stamina += staminaRegen * Time.deltaTime;
                    stamina = Mathf.Min(stamina, maxStamina);
                }
            }

            if (stamina <= 0.01f)
            {
                isSprinting = false;
            }
        }
        else
        {
            stamina = maxStamina;
        }

        // Choose speed.
        targetSpeed = walkSpeed;

        if (inputKeyCrouch)
        {
            targetSpeed = crouchSpeed;
        }
        else if (isSprinting)
        {
            targetSpeed = runSpeed;
        }

        // Crouching / standing.
        float targetHeight = inputKeyCrouch ? defaultHeight * crouchHeightPercent : defaultHeight;
        isCrouching = inputKeyCrouch || isCeiling;

        if (inputKeyCrouch || !isCeiling)
        {
            float previousHeight = controller.height;
            controller.height = Mathf.Lerp(
                controller.height,
                targetHeight,
                crouchTransitionSpeed * Time.deltaTime
            );

            float heightDelta = controller.height - previousHeight;

            Vector3 playerPosition = playerTx.position;
            playerPosition.y += heightDelta * 0.5f;
            playerTx.position = playerPosition;

            if (cameraTx)
            {
                float cameraHeight =
                    (controller.height / defaultHeight) +
                    cameraStartY -
                    (defaultHeight * 0.5f);

                Vector3 cameraPosition = cameraTx.localPosition;
                cameraPosition.y = cameraHeight;
                cameraTx.localPosition = cameraPosition;
            }

            float heightFactor = (defaultHeight - controller.height) * 0.5f;
            groundOffsetY = heightFactor + groundCheckY;
            ceilingOffsetY =
                heightFactor +
                controller.height -
                (defaultHeight - ceilingCheckY);

            if (playerGFX)
            {
                Vector3 scale = playerGFX.localScale;
                scale.y = Mathf.Lerp(
                    scale.y,
                    inputKeyCrouch ? crouchHeightPercent : 1f,
                    crouchTransitionSpeed * Time.deltaTime
                );
                playerGFX.localScale = scale;
            }
        }

        // Movement speed smoothing.
        float speedDifference = targetSpeed - lastSpeed;
        float rate = Mathf.Abs(speedDifference) > 0.05f
            ? (Mathf.Abs(targetSpeed) > Mathf.Abs(lastSpeed) ? acceleration : deceleration)
            : deceleration;

        float control = isGrounded ? 1f : airControl;

        currentSpeed = Mathf.MoveTowards(
            lastSpeed,
            targetSpeed,
            rate * control * Time.deltaTime
        );

        if (!hasMovementInput)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0f,
                deceleration * control * Time.deltaTime
            );
        }

        lastSpeed = currentSpeed;

        // Gravity.
        if (isGrounded)
        {
            if (verticalVelocity.y < 0f)
            {
                verticalVelocity.y = -2f;
            }

            if (allowJump && !isCrouching && inputKeyDownJump && !isCeiling)
            {
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }

        if (isCeiling && verticalVelocity.y > 0f)
        {
            verticalVelocity.y = 0f;
        }

        Vector3 horizontalMovement = inputMove * currentSpeed;
        Vector3 finalMovement = horizontalMovement + verticalVelocity;

        controller.Move(finalMovement * Time.deltaTime);

        // Refresh grounded state after the move as well.
        lastSpeed = currentSpeed;
    }

    void ProcessCameraBob()
    {
        if (!cameraTx)
        {
            return;
        }

        Vector3 targetPosition = cameraStartLocalPosition;

        bool moving = isGrounded && currentSpeed > 0.1f;

        if (enableHeadBob && moving)
        {
            float bobAmount = walkBobAmount;
            float frequency = walkBobFrequency;

            if (isCrouching)
            {
                bobAmount = crouchBobAmount;
                frequency = crouchBobFrequency;
            }
            else if (isSprinting)
            {
                bobAmount = runBobAmount;
                frequency = runBobFrequency;
            }

            bobTimer += Time.deltaTime * frequency;

            float verticalBob = Mathf.Sin(bobTimer * 2f) * bobAmount;
            float horizontalBob = Mathf.Cos(bobTimer) * bobAmount * 0.55f;

            targetPosition.y += verticalBob;
            targetPosition.x += horizontalBob;
        }
        else
        {
            bobTimer = Mathf.Lerp(bobTimer, 0f, cameraBobSmoothness * Time.deltaTime);
        }

        cameraTx.localPosition = Vector3.Lerp(
            cameraTx.localPosition,
            targetPosition,
            cameraBobSmoothness * Time.deltaTime
        );
    }

    // Lock/hide or show/unlock cursor.
    public void SetLockCursor(bool doLock)
    {
        cursorActive = doLock;
        RefreshCursor();
    }

    void ToggleLockCursor()
    {
        cursorActive = !cursorActive;
        RefreshCursor();
    }

    void RefreshCursor()
    {
        if (!cursorActive && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (cursorActive && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        Cursor.visible = cursorActive;
    }

    // Check the area above the player to prevent standing under a ceiling.
    void CeilingCheck()
    {
        Vector3 origin = new Vector3(
            playerTx.position.x,
            playerTx.position.y + ceilingOffsetY,
            playerTx.position.z
        );

        isCeiling = Physics.CheckSphere(
            origin,
            sphereCastRadius,
            castingMask
        );
    }

    // Ground detection.
    void GroundCheck()
    {
        Vector3 origin = new Vector3(
            playerTx.position.x,
            playerTx.position.y + groundOffsetY,
            playerTx.position.z
        );

        RaycastHit hit;

        isGrounded = Physics.SphereCast(
            origin,
            sphereCastRadius,
            Vector3.down,
            out hit,
            sphereCastDistance,
            castingMask
        );
    }

    // Push rigidbodies that the character touches.
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        if (body == null || body.isKinematic)
        {
            return;
        }

        // Do not push objects below the player.
        if (hit.moveDirection.y < -0.3f)
        {
            return;
        }

        body.linearVelocity = hit.moveDirection * lastSpeed;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!showGizmos)
        {
            return;
        }

        float debugGroundOffset = Application.isPlaying ? groundOffsetY : groundCheckY;
        float debugCeilingOffset = Application.isPlaying ? ceilingOffsetY : ceilingCheckY;

        Vector3 startPoint = new Vector3(
            transform.position.x,
            transform.position.y + debugGroundOffset,
            transform.position.z
        );

        Vector3 endPoint = startPoint + new Vector3(
            0f,
            -sphereCastDistance,
            0f
        );

        Vector3 ceilingPoint = new Vector3(
            transform.position.x,
            transform.position.y + debugCeilingOffset,
            transform.position.z
        );

        Gizmos.color = isGrounded ? Color.green : Color.white;
        Gizmos.DrawWireSphere(startPoint, sphereCastRadius);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(endPoint, sphereCastRadius);
        Gizmos.DrawLine(startPoint, endPoint);

        Gizmos.color = isCeiling ? Color.red : Color.white;
        Gizmos.DrawWireSphere(ceilingPoint, sphereCastRadius);
    }

    // Adds the controller to Unity's GameObject > 3D Object menu.
    [MenuItem("GameObject/3D Object/BasicFPCC", false, 0)]
    public static void CreateBasicFPCC()
    {
        const int playerLayer = 2;

        GameObject go = new GameObject("Player");

        CharacterController controller = go.AddComponent<CharacterController>();
        controller.center = new Vector3(0f, 1f, 0f);

        BasicFPCC basicFPCC = go.AddComponent<BasicFPCC>();

        go.layer = playerLayer;
        basicFPCC.castingMask = ~(1 << playerLayer);

        GameObject mainCamObject = GameObject.Find("Main Camera");

        if (mainCamObject)
        {
            mainCamObject.transform.parent = go.transform;
            mainCamObject.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            mainCamObject.transform.localRotation = Quaternion.identity;
            basicFPCC.cameraTx = mainCamObject.transform;
        }
        else
        {
            Debug.LogError(
                "** Main Camera NOT FOUND **\n" +
                "A new Camera has been created and assigned. " +
                "Replace it with your Main Camera and AudioListener if needed."
            );

            GameObject camGo = new GameObject("BasicFPCC Camera");
            camGo.AddComponent<Camera>();

            camGo.transform.parent = go.transform;
            camGo.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            camGo.transform.localRotation = Quaternion.identity;

            basicFPCC.cameraTx = camGo.transform;
        }

        GameObject gfx = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        Collider capsuleCollider = gfx.GetComponent<Collider>();

        if (capsuleCollider)
        {
            Object.DestroyImmediate(capsuleCollider);
        }

        gfx.transform.parent = go.transform;
        gfx.transform.localPosition = new Vector3(0f, 1f, 0f);
        gfx.name = "GFX";
        gfx.layer = playerLayer;

        basicFPCC.playerGFX = gfx.transform;
    }
#endif
}
