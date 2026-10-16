using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationHandler : MonoBehaviour
{
    [Header("The Character Animator")]
    [SerializeField]
    private Animator animator;

    [Header("Setup")]
    [SerializeField]
    private float smoothing = 0.03f;
    [SerializeField]
    private float runningSmoothing = 0.03f;

    private float targetX = 0f;
    private float targetY = 0f;
    private float currentX = 0f;
    private float currentY = 0f;

    void Update()
    {
        // Check if Shift is pressed (new Input System)
        bool isRunning = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;

        float currentSmoothing = isRunning ? runningSmoothing : smoothing;

        targetX = 0f;
        targetY = 0f;

        // WASD input handling
        if (Keyboard.current.wKey.isPressed)
        {
            targetY = isRunning ? 1f : 0.5f;
        }
        else if (Keyboard.current.sKey.isPressed)
        {
            targetY = isRunning ? -1f : -0.5f;
        }

        if (Keyboard.current.aKey.isPressed)
        {
            targetX = isRunning ? -1f : -0.5f;
        }
        else if (Keyboard.current.dKey.isPressed)
        {
            targetX = isRunning ? 1f : 0.5f;
        }

        // Smoothly interpolate
        currentX = Mathf.Lerp(currentX, targetX, currentSmoothing);
        currentY = Mathf.Lerp(currentY, targetY, currentSmoothing);

        animator.SetFloat("x", currentX);
        animator.SetFloat("y", currentY);
    }
}