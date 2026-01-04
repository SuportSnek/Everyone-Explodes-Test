using System.Collections;
using UnityEngine;

public class BouncePad : GrenadeBase       //! This inherits from GrenadeBase!
{
   //private Rigidbody bouncePadRb;
    //private GameObject player;
    private bool bouncePadArmed = false;
    float currentRadius = 1;
    float nadeDistanceFromThrower;

    protected override void Start()
    {
        base.Start();

        if (Owner == null || stats == null){
            Debug.LogError("BouncePad missing Owner or stats!");
            return;}

        nadeDistanceFromThrower = Vector3.Distance(Owner.transform.position, gameObject.transform.position);
        currentRadius = RadiusUpgrades(stats.explosionRadius, nadeDistanceFromThrower, gameObject);
        ScaleBouncePad(currentRadius);
    }

    void ScaleBouncePad(float radius)
    {
        float scaleMultiplier = radius / stats.explosionRadius;

        Vector3 scale = transform.localScale;
        scale.x *= scaleMultiplier;
        scale.z *= scaleMultiplier;
        transform.localScale = scale;
    }

    private void OnEnable() //Todo note for future me: OnEnable() should only do things that do not depend on runtime wiring! ex. Don't reference "owner"
    {
        StartCoroutine(BouncePadArmTime(stats.armTime));
    }



    private void OnTriggerEnter(Collider other)
    {
        if (!bouncePadArmed) return;
        if (!other.CompareTag("Player")) return;   // Pad only activates when touched by a player

float nadeDistanceFromThrower = Vector3.Distance(Owner.transform.position, gameObject.transform.position);  //distance of thrower vs opponent, used for upgrades

        float currentStrength = stats.Strength;
        float initialStrength = stats.Strength;
        float strengthMultiplier = 1;
        //float currentRadius = RadiusUpgrades(stats.explosionRadius, nadeDistanceFromThrower, gameObject);
        //Todo I *THINK* cause the thing naturally gets bigger, it doesn't need this?

                // apply effect to players within radius
        if (PlayerSpawnManager.Instance != null)
        {
            foreach (GameObject player in PlayerSpawnManager.Instance.AllPlayers)
            {
                if (player == null) continue;
                Vector3 bounceDirection = transform.up;
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= currentRadius)
                {
                    //Rigidbody playerRb = player.GetComponent<Rigidbody>();
                    FirstPersonController controller = player.GetComponent<FirstPersonController>();
                    if (controller != null)
                    {
                        
                      strengthMultiplier = StrengthUpgrades(player, nadeDistanceFromThrower, strengthMultiplier);
                      if (Owner != player && Up.stickyBombUpgrade > 0){SpawnStickyBomb(player, Owner);}

                    float finalStrength = initialStrength * strengthMultiplier;
                       Vector3 bounceVector = bounceDirection * currentStrength;  //Multiply by bounce pad strength
                        controller.ApplyPush(bounceVector, Owner); //Tell FirstPersonController script to launch player
                        
                    }
                }
            }
            Destroy(gameObject);
            if(stats.explosionClip != null){
                AudioSource.PlayClipAtPoint(stats.explosionClip, transform.position, 1f);}
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        // Do nothing. Building should NOT explode on collision. Overrides function in Grenadebase
    }


    public IEnumerator BouncePadArmTime(float cooldown)    //Time it takes for a thrown bounce pad to arm.
    {
        bouncePadArmed = false;
        yield return new WaitForSeconds(cooldown);
        bouncePadArmed = true;
    }
}
