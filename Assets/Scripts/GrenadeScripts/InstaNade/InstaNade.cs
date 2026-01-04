using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class InstaNade : GrenadeBase
{
    protected override void OnCollisionEnter(Collision collision)
    {

    }

    public override bool InstaNadeCooldownLogic(GameObject Owner)
    {
        WeaponInventory inv = Owner.GetComponent<WeaponInventory>();
        inv.ApplyCooldownAfterThrow(slotIndex, true);     //InstaNade shorter cooldown    
        return false;
    }
}

