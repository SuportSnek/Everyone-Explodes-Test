using UnityEngine;

public class PAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [HideInInspector] private FirstPersonController fpc;
    //[SerializeField] private HoverController hover;

    void Awake()
    {
        animator = GetComponent<Animator>();

        // Search UP the hierarchy
        fpc = GetComponentInParent<FirstPersonController>();
    }

void Update()
{
    Vector3 localVelocity = fpc.transform.InverseTransformDirection(fpc.CurrentHorizantalVelocity());
    // Normalize so diagonals aren't faster
    localVelocity /= Mathf.Max(localVelocity.magnitude, 1f);

    animator.SetFloat("MoveX", localVelocity.x); // right(+) + left(-)
    animator.SetFloat("MoveY", localVelocity.z); // forward(+) / backward(-)

    animator.SetBool("IsGrounded", fpc.fpcGrounded);
    animator.SetBool("beingExploded", fpc.beingExploded);
    animator.SetBool("showJumpAnimation", fpc.showJumpAnimation);
}

/*
    public void TriggerJump()
    {
        animator.Play("Jump");  //unused
    }*/

    
}
