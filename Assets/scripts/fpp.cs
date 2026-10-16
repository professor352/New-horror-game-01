using UnityEngine;

public class FPPBodyAnimator : MonoBehaviour
{
    public Transform player;            // drag the Player object here
    public float walkSpeed = 3.2f;      // same as BasicFPCC walk speed
    public string headBoneName = "B-head";
    public float damp = 0.1f;

    private Animator animator;
    private Transform head;
    private Vector3 lastPos;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (!player) player = transform.parent;
        lastPos = player.position;
        head = FindDeep(transform, headBoneName);
        if (!head) Debug.LogWarning("Head bone '" + headBoneName + "' not found. Check the name under Rig.");
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        Vector3 vel = (player.position - lastPos) / dt;
        lastPos = player.position;
        Vector3 local = player.InverseTransformDirection(vel);

        animator.SetFloat("MoveX", local.x / walkSpeed, damp, dt);
        animator.SetFloat("MoveZ", local.z / walkSpeed, damp, dt);
    }

    void LateUpdate()
    {
        if (head) head.localScale = Vector3.zero;   // hides the head from the camera
    }

    Transform FindDeep(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }
}