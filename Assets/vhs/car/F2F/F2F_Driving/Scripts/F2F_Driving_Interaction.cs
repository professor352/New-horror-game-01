using UnityEngine;

public class F2F_Driving_Interaction : MonoBehaviour
{

    public float InteractionDistance = 3f; // how far we can interact 

    private GameObject currentRadio; // Stores the Current radio we wanna target
    private GameObject CurrentRadioOutline; // Stores the current radio Outline we wanna target 

    // Update is called once per frame
    void Update()
    {
        CheckForInteractable(); // interaction system
        HandleInput(); // checks if we pressed mouse to active radio
    }

    void CheckForInteractable()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, InteractionDistance))
        {
            if (hit.collider.CompareTag("Radio")) // Checks if the object has a tag of Radio
            {
                if(currentRadio != hit.collider.gameObject) // prevent doing the same stuff for the same Radio
                {
                    
                    
                    ClearCurrent(); // Cleans up previously used radio and radio outline 

                    currentRadio = hit.collider.gameObject; // Put our radio in the container

                    if(currentRadio.transform.childCount > 0) // If we don't have a child (outline), this won't run
                    {
                        CurrentRadioOutline = currentRadio.transform.GetChild(0).gameObject; // get the first child as an outline
                        CurrentRadioOutline.SetActive(true);
                    }

                }
                return;
            }
            else
            {
                ClearCurrent(); // Cleans up previously used radio and radio outline 
            }
        }

        ClearCurrent(); // Cleans up previously used radio and radio outline 


    }


    void HandleInput()
    {
        if(currentRadio != null && Input.GetMouseButtonDown(0))
        {
            OnRadioInteracted();
        }
    }

    void ClearCurrent()
    {
        if(CurrentRadioOutline != null) // Check if we have an outline object 
        {
            CurrentRadioOutline.SetActive(false);
            CurrentRadioOutline = null;
        }
        currentRadio = null;
    }

    void OnRadioInteracted()
    {
        
        // Play music from music player 

        F2F_Driving_Radio radioManager = currentRadio.GetComponent<F2F_Driving_Radio>(); // get the script from the interacted Radio
        if(radioManager != null)
        {
            radioManager.ToggleRadio(); // change the radio mute = !mute when we interact with it 
        }

    }


}
