
using UnityEngine;

public class DestinationTransition : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "scene 02";

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        F2F_CarController car =
            other.GetComponentInParent<F2F_CarController>();

        if (car == null || !car.isDriving)
            return;

        if (ScreenFader.Instance == null)
        {
            Debug.LogError("ScreenFader is missing from the scene!");
            return;
        }

        triggered = true;
        ScreenFader.Instance.LoadNextScene(nextSceneName);
    }
}
