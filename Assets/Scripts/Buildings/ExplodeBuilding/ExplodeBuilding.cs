using System.Collections;
using UnityEngine;
using System.Collections.Generic;

//Example building:
public class ExplodeBuilding : GrenadeBase
{
    // Update is called once per frame
    void Update()
    {
        if (readyToFire)
        {
            Explode();
            StartCoroutine(FireRate(stats.fireRate));
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        // Do nothing. Building should NOT explode on collision. Overrides function in Grenadebase
    }


}
