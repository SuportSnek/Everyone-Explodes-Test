using UnityEngine;

public class SpiralMotion : MonoBehaviour
{
    public Rigidbody coreRb;
    public Rigidbody[] orbitRbs;    //Array holding the two real grenade Rigidbodies that orbit the core.

    public float orbitRadius = 1f;    //Controls how far away the orbiting grenades sit from the core. Bigger = wider spiral
    public float orbitSpeed = 9f;      // ontrols how fast the orbit rotates around the core. Higher = faster spin. radians/sec
    public float followStrength = 20f; // Controls how aggressively the orbiting grenades correct toward their target orbit position.

    void Awake()
    {
        if (coreRb == null)
            coreRb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
            orbitRbs = System.Array.FindAll(orbitRbs, r => r != null); //Removes any entries whose grenade has been destroyed

    if (orbitRbs.Length == 0)
    {
        Destroy(gameObject); // kill the invisible core if no orbit nades remain
        return;
    }

        if (coreRb.linearVelocity.sqrMagnitude < 0.01f)
            return;

        Vector3 forward = coreRb.linearVelocity.normalized; //Gets the current direction of travel of the core grenade.

        // Build a perpendicular basis
        Vector3 right = Vector3.Cross(forward, Vector3.up); //Computes a vector perpendicular to: The forward direction, world up. gives sideways direction.
        if (right.sqrMagnitude < 0.01f)
            right = Vector3.Cross(forward, Vector3.right);//edge case
        right.Normalize();  //Ensures right has unit length.

        Vector3 up = Vector3.Cross(right, forward); //Creates a second perpendicular vector so we now have forward/up

        float angleBase = Time.fixedTime * orbitSpeed;  //Computes the base rotation angle for the orbit.

        for (int i = 0; i < orbitRbs.Length; i++)
        {
            Rigidbody orb = orbitRbs[i];
            if (orb == null) continue;

            float angle = angleBase + i * Mathf.PI; // keeps them 180° apart

            Vector3 orbitOffset =
                right * Mathf.Cos(angle) * orbitRadius +    //cos(angle) moves along the right axis
                up    * Mathf.Sin(angle) * orbitRadius;     //sin(angle) moves along the up axis
                //Combined, they trace a circle around forward

            Vector3 targetPos = coreRb.position + orbitOffset; //Computes the desired world-space position of this orbiting grenade.

            // Velocity-based Steering physics
            Vector3 toTarget = targetPos - orb.position; //Direction vector from the orbit grenade’s current position to its desired orbit position.
            Vector3 desiredVel = toTarget * followStrength; 

            orb.AddForce(desiredVel - orb.linearVelocity, ForceMode.Acceleration);  //Applies acceleration to correct the grenade’s velocity:
        }
    }
}

/*
Each physics tick:

Use the invisible grenade as a moving reference

Compute a rotating orbit around its velocity direction

Calculate where each real grenade should be

Push them toward that position using forces

Let physics handle collisions and explosions*/