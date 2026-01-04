using UnityEngine;

public class FragGrenade : GrenadeBase
{
    // Frag doesn't explode on impact
    protected override void OnCollisionEnter(Collision collision)
    {
        
    }
    //Note: Logic for frag is mostly in WeaponInventory and NadeThrower.
}
