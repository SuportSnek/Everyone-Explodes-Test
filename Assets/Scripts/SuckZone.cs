using UnityEngine;

public class GravityZone : MonoBehaviour
{
    public float pullForce = 2.5f;      // how strongly it pulls

    private void OnTriggerStay(Collider other)
    {
        FirstPersonController fpc = other.GetComponentInParent<FirstPersonController>();
        if (fpc == null) return;  // nothing to do

        // Only suck if being exploded by someone ELSE
       if (fpc.beingExploded && fpc.lastExplosionCauser != fpc.gameObject)
        {
            Rigidbody rb = other.attachedRigidbody;

            // Direction toward the center (sphere or cube, doesn't matter)
            Vector3 direction = (transform.position - other.transform.position).normalized;
            rb.AddForce(direction * pullForce);
            }
    }
}
