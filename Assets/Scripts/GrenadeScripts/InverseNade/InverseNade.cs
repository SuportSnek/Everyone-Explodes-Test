using UnityEngine;
using System.Collections;

public class InverseNade : GrenadeBase
{

/* //Todo fix spawnpos later
    public override Vector3 GetSpawnOffset(Transform thrower)
    {
        // Example: spawn lower, or no up offset
        return thrower.forward * 1.15f + thrower.up * 0.1f;
    }*/

public override void HowMuchKnockback(Vector3 push, GameObject Owner, GameObject player, float currentStrength)
    {
        FirstPersonController controller = player.GetComponent<FirstPersonController>();
        controller.ApplyInverseCurse(currentStrength, stats.duration); 
    }

}

