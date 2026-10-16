
using System.Collections;
using UnityEngine;

public class Scene2CarStart : MonoBehaviour
{
    [SerializeField] private F2F_CarController car;

    private IEnumerator Start()
    {
        // Wait until Scene 2 has initialized.
        yield return null;

        if (car == null)
            car = GetComponent<F2F_CarController>();

        if (car == null)
        {
            Debug.LogError(
                "Scene2CarStart: F2F_CarController is missing!"
            );
            yield break;
        }

        if (car.PlayerObject == null || car.carCamera == null)
        {
            Debug.LogError(
                "Assign PlayerObject and carCamera on the car controller."
            );
            yield break;
        }

        // Start the player already inside the car.
        car.EnterCar();
    }
}
