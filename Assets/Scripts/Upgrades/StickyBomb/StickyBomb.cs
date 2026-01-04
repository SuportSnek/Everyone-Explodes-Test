using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class StickyBomb : MonoBehaviour
{
    [SerializeField] private float delay = 1f;
    public float strength = 20f;
    public ParticleSystem explosionParticle;

public void AttachToPlayer(GameObject playerHit, GameObject Owner, Vector3 explosionOrigin, bool hitGround)
{
    Vector3 dir = (playerHit.transform.position - explosionOrigin).normalized;
    transform.position = playerHit.transform.position - dir * 0.4f;

        //Sets y position:
        Vector3 explPos = transform.position;
        if (hitGround){   //if nade hit ground
            explPos.y = explosionOrigin.y + 0.75f;  //have it spawn a little bit upwards, looks way better
            }
        else{                           //otherwise,
            explPos.y = explosionOrigin.y;  //just spawn it at it's normal Y position
            } 
        transform.position = explPos;   //updates the y pos, everything else stays same
        
    transform.rotation = Quaternion.LookRotation(dir);

    transform.SetParent(playerHit.transform, true);

    PlayerHealth pHealth = playerHit.GetComponent<PlayerHealth>();
    pHealth.StuckStickyGrenade(true);
    StartCoroutine(DelayedExplosion(playerHit, Owner, delay));
}


    private IEnumerator DelayedExplosion(GameObject playerHit, GameObject Owner, float time)
    {
        yield return new WaitForSeconds(time);

        FirstPersonController fpc = playerHit.GetComponent<FirstPersonController>();
        PlayerHealth pHealth = playerHit.GetComponent<PlayerHealth>();

        Vector3 direction = (playerHit.transform.position - gameObject.transform.position).normalized;
        Vector3 push = direction * strength;
        fpc.ApplyPush(push, Owner); 
        Instantiate(explosionParticle, transform.position, explosionParticle.transform.rotation);
        pHealth.StuckStickyGrenade(false);   //disables sound
        Destroy(gameObject);
    }

}
//Todo if on slope, then the Y position formula can break. Can be fixed by making slopes not count or something, but I don't care, it was such a fucking challenge to make it work as is. Something about using the player's Y position just breaks everything.