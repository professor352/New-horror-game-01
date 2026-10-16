using System;
using System.Collections;
using UnityEngine;


public class DoorInteraction : MonoBehaviour
{
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public bool isOpen = false;
    public float interactionDistance = 10f; // How far the player can interact from

    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Coroutine _currentCoroutine;
    private Camera _playerCamera;

    void Start()
    {
        _closedRotation = transform.rotation;
        _openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));

        // Get the main camera (usually on the player)
        _playerCamera = Camera.main;

        if (_playerCamera == null)
        {
            Debug.LogError("Camera.main not found! Make sure your Camera is tagged as 'MainCamera'");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Check if player is looking at THIS door
            if (IsPlayerLookingAtDoor())
            {
                if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
                _currentCoroutine = StartCoroutine(ToggleDoor());
            }
        }
    }

    private bool IsPlayerLookingAtDoor()
    {
        if (_playerCamera == null) return false;

        Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
        RaycastHit hit;

        // Cast a ray forward from the camera
        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // Check if the raycast hit THIS door or any of its children
            Transform hitTransform = hit.transform;

            // Check if hit object is this door or a child of this door
            while (hitTransform != null)
            {
                if (hitTransform == transform)
                {
                    return true;
                }
                hitTransform = hitTransform.parent;
            }
        }

        return false;
    }

    private IEnumerator ToggleDoor()
    {
        Quaternion targetRotation = isOpen ? _closedRotation : _openRotation;
        isOpen = !isOpen;

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }

        // Snap to exact rotation at end
        transform.rotation = targetRotation;
    }
}