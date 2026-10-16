using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleKeyInputJump : MonoBehaviour
{
    public Animator characterAnimator;

    void Start()
    {
        characterAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        // Hold W to walk continuously
        if (Input.GetKey(KeyCode.W))
        {
            characterAnimator.SetFloat("Speed", 1f);  // Walking
        }
        else
        {
            characterAnimator.SetFloat("Speed", 0f);  // Idle
        }

        // Handle jump input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            characterAnimator.SetTrigger("JumpTrigger");
        }
    }
}