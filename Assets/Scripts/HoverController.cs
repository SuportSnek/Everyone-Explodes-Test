using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoverController : MonoBehaviour
{

    //Script:
    //1. SphereCasts downward to see how far the ground is.
    //2. If you're close enough, 
        //Computes how far off your desired hover height you are
        //Calculates a spring force to push you up or down
    //3. Applies force at the hit location to create smooth hovering and natural tilting.
    //4. While in midair, code is disabled
    // References
    private Rigidbody rb;

    // Hover tuning
    [Header("Hover Settings")]
    [Tooltip("Target height above ground")]
    public float RideHeight = 0.5f;
    [Tooltip("How quickly the spring pushes back (bigger = stiffer)")]
    public float RideSpringStrength = 400f;
    [Tooltip("Velocity damping for the spring (bigger = less bounce)")]
    public float RideSpringDamper = 60f;
    [Tooltip("Max force applied by the spring per FixedUpdate (prevent explosion)")]
    public float MaxSpringForce = 2000f;

    // Ground detection
    [Header("Ground Check")]
    public float groundCheckDistance = 1.6f;   // how far down we check
    public float footRadius = 0.45f;           // spherecast radius (match player collider)
    public float footOffset = 0.9f;            // starting point offset from transform.position
    public LayerMask groundMask;

    // state
    public bool isGrounded;
    public RaycastHit _rayHit;

    // some smoothing (optional)
    private float lastSpringForce;

    public Vector3 GroundNormal => _rayHit.normal;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // recommended Rigidbody settings:
        // rb.interpolation = RigidbodyInterpolation.Interpolate;
        // rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void FixedUpdate()
    {
        CheckGround();
        ApplyHoverSpring();
    }

    // Simple ground boolean check (can be used by other systems)
    private void CheckGround()
    {
       Vector3 castOrigin = transform.position + Vector3.down * footOffset;
       //Starts the ray slightly below the player (footOffset)
       //Shoots a sphere downward
       //If the sphere touches ground → you are grounded

        // spherecast downwards
        bool hit = Physics.SphereCast(castOrigin, footRadius, Vector3.down, out _rayHit, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);

        if (!hit)
        {
            isGrounded = false;
            return;
        }

        float actualHeight = _rayHit.distance;

        // Only grounded if you're within hover range
        isGrounded = actualHeight <= RideHeight + 0.05f;
    }

    // Core spring/damper logic
    private void ApplyHoverSpring()
    {
                // Calculate how far you are from the ground:
        float hitDistance = _rayHit.distance;
        // The cast origin was transform.position - up * footOffset, so adjust measured distance to be from the desired ride reference:
        // We want x = (desired height - actual height)
        float actualHeight = hitDistance;
        float compression = RideHeight - actualHeight; // positive => we are below desired height and need upward force

        if (!isGrounded)
        {
            //rb.AddForce(Physics.gravity, ForceMode.Acceleration);
            // if player is in midair, or they're above the height at which they get springed, exit the function.
            lastSpringForce = 0f;
            return;
        }



        // get direction (normal from hit)
        Vector3 hitNormal = _rayHit.normal;

        // compute relative velocity along the normal between player and the hit surface at the hit point
        Vector3 myPointVelocity = rb.GetPointVelocity(_rayHit.point);
        Vector3 otherVel = Vector3.zero;
        Rigidbody hitBody = _rayHit.rigidbody;
        if (hitBody != null)
        {
            otherVel = hitBody.GetPointVelocity(_rayHit.point);
        }
        float relativeVelAlongNormal = Vector3.Dot(myPointVelocity - otherVel, hitNormal);      //This calculates how fast you're moving toward or away from the ground. This prevents bouncing.

        // spring + damper
        float springForce = (compression * RideSpringStrength) - (relativeVelAlongNormal * RideSpringDamper);
                //If you're too low, a strong upward force is applied
                //If you're moving downward too fast, extra force is applied
                //If you're moving upward too fast, force is reduced

        // clamp the force to avoid excessive upward force
        //springForce = Mathf.Clamp(springForce, -MaxSpringForce, MaxSpringForce);

        // apply force at the hit point so we also rotate/tilt correctly
        // choose ForceMode.Acceleration if you want mass-independent behavior, Force otherwise.
        //! Don't want to rotate
        rb.AddForceAtPosition(hitNormal * springForce, _rayHit.point, ForceMode.Force);

        // apply equal and opposite to dynamic ground if it has a rigidbody (aka, makes code work on moving platforms)
         
        if (hitBody != null)
        {
            hitBody.AddForceAtPosition(-hitNormal * springForce, _rayHit.point, ForceMode.Force);
        }

        lastSpringForce = springForce;
    }

    // useful debug draw
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        Gizmos.color = Color.cyan;
        Vector3 castOrigin = transform.position + Vector3.down * (-footOffset);
        Gizmos.DrawWireSphere(castOrigin, footRadius);
        if (isGrounded)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(castOrigin, castOrigin + Vector3.down * _rayHit.distance);
            Gizmos.DrawSphere(_rayHit.point, 0.05f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_rayHit.point, _rayHit.point + _rayHit.normal * 0.5f);
        }
    }
}

