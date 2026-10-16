using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private float moveSpeed = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        moveSpeed = new Vector2(horizontal, vertical).magnitude;

        animator.SetFloat("Speed", moveSpeed);
        animator.SetFloat("Direction", horizontal);
        animator.SetBool("IsSprinting", isSprinting);

        // Move character
        float speed = isSprinting ? 7f : 5f; // Faster when sprinting
        Vector3 movement = (transform.forward * vertical + transform.right * horizontal) * speed * Time.deltaTime;
        transform.Translate(movement);
    }
}