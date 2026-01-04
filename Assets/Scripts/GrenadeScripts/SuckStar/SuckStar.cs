using UnityEngine;
using System.Collections;

public class SuckStar : GrenadeBase
{
    public override Vector3 FindDirection(Vector3 playerPos, Vector3 nadePos)
    {
        Vector3 direction;
        Vector3 target = transform.position + Vector3.up * 7.5f;//Finds target in space above suck build
            if (playerPos.y <= target.y){    //if player's Y coordinate is less than the target
                    direction = (target - playerPos).normalized;//player pulled towards the target in this direction
                }
            else{ //Just pull towards nade's center
                    Vector3 newTarget = transform.position + Vector3.up * 1.5f;//Target in space above suck build, but only 1.5 above
                    direction = (newTarget - playerPos).normalized;//player pulled towards it in this direction
                }   
            return direction;
    }
}