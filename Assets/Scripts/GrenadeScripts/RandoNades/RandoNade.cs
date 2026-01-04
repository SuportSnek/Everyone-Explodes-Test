using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandoNade : GrenadeBase
{
    private int whichExplosion;
    [SerializeField] public GameObject terrorTurtlePrefab;   
    public AudioClip fartClip;
    public AudioClip suckClip;
    public AudioClip glassClip;
    public AudioClip freezeClip;
    public AudioClip bigExplosionClip;
    
public override void Explode()
    {
        List<int> randomCards = cardsDrawn.cards;    //uses list made by nadethrower. But it first goes through GrenadeBase.

        float nadeDistanceFromThrower = Vector3.Distance(Owner.transform.position, gameObject.transform.position);  //distance of thrower vs opponent, used for upgrades

        float initialStrength = stats.Strength;
        float strengthMultiplier = 1;
        float currentRadius = RadiusUpgrades(stats.explosionRadius, nadeDistanceFromThrower, gameObject);
            if (randomCards.Contains(14)){currentRadius/=2;}   //Half radius for Ace 
            if (randomCards.Contains(2)){currentRadius*=99;}   //Infinite radius for 2

        bool spawnTurtle = randomCards.Contains(6);

        whichExplosion = DetermineExplosionType(randomCards);

        // apply effect to players within radius
        if (PlayerSpawnManager.Instance != null)
        {
            foreach (GameObject player in PlayerSpawnManager.Instance.AllPlayers)
            {
                if (player == null) continue;
                float distance = Vector3.Distance(transform.position, player.transform.position);
                    
                if (distance <= currentRadius) 
                {
                    if (!HasLineOfSightToPlayer(player)){continue;}

                    Rigidbody playerRb = player.GetComponent<Rigidbody>();
                    FirstPersonController controller = player.GetComponent<FirstPersonController>();
                    if (playerRb != null && controller != null)
                    {
                        Vector3 direction;
                        direction = FindDirection(player.transform.position, gameObject.transform.position);

                        strengthMultiplier = StrengthUpgrades(player, nadeDistanceFromThrower, strengthMultiplier);
                        float finalStrength = initialStrength * strengthMultiplier;

                        if (Owner != player && Up.stickyBombUpgrade > 0){SpawnStickyBomb(player, Owner);}
                        
                        if (randomCards.Contains(2))    //card 2: Fart sound
                        {
                            Vector3 push = direction * finalStrength/3;
                            HowMuchKnockback(push, Owner, player, finalStrength);
                            //TODO play fart noise
                        }
                        if (randomCards.Contains(3))    //card 3: Normal Bounce-Nade
                        {
                            Vector3 push = direction * finalStrength;
                            HowMuchKnockback(push, Owner, player, finalStrength);
                        }
                        if (randomCards.Contains(4))    //card 4: Suck Star
                        {
                            Vector3 target = transform.position + Vector3.up * 7.5f;//Finds target in space above suck build
                            if (player.transform.position.y <= target.y){    //if player's Y coordinate is less than the target
                                    direction = (target - player.transform.position).normalized;//player pulled towards the target in this direction
                                }
                            else{ //Just pull towards nade's center
                                    Vector3 newTarget = transform.position + Vector3.up * 1.5f;//Target in space above suck build, but only 1.5 above
                                    direction = (newTarget - player.transform.position).normalized;//player pulled towards it in this direction
                                } 
                            Vector3 push = direction * finalStrength;
                            HowMuchKnockback(push, Owner, player, finalStrength);
                        }
                        if (randomCards.Contains(5))    //card 5: Inverse-Nade
                        {
                            controller.ApplyInverseCurse(finalStrength, stats.duration); 
                        }
                        if (randomCards.Contains(6))    //card 6: Terror Turtle
                        {
                            Vector3 push = direction * finalStrength;
                            HowMuchKnockback(push, Owner, player, finalStrength);
                        }
                        if (randomCards.Contains(7))    //card 8: Sticky-Bomb
                        {
                            Vector3 push = direction * finalStrength;
                            HowMuchKnockback(push, Owner, player, finalStrength);
                            if(Owner != player){SpawnStickyBomb(player, Owner);}
                        }
                        if (randomCards.Contains(8))   //card 8: Hor push
                        {
                            direction.x *= 2.5f;
                            direction.z *= 2.5f;
                            direction.y /= 2.5f;
                            Vector3 push = direction * finalStrength;
                            HowMuchKnockback(push, Owner, player, finalStrength);
                        }
                        if (randomCards.Contains(9))   //card 9: Vert push
                        {
                            direction.x /= 2.5f;
                            direction.z /= 2.5f;
                            direction.y *= 2.5f;
                            Vector3 push = direction * finalStrength;
                            HowMuchKnockback(push, Owner, player, finalStrength);
                        }
                        if (randomCards.Contains(10))    //card 10: Freeze!
                        {
                            //If I want freeze time to scale (Which I think is a bad idea?)
                            //float frozenTime = 3f * (0.05f*finalStrength + 1) - 1.875f; //The 1.875 is just the nade's basic strength, which I subtract so that the nade starts at 3 seconds of freeze.
                            float frozenTime = 3f;
                            controller.ApplyFrozenCurse(frozenTime);
                        }
                        if (randomCards.Contains(14))    //card 11/A: PERISH
                        {
                            finalStrength *= 3;   //Triple knockback
                            Vector3 push = direction * finalStrength;
                            HowMuchKnockback(push, Owner, player, finalStrength);
                        }
                    }
                }
            }
        }
        Destroy(gameObject);
        VisualExplosion(currentRadius, whichExplosion);
        SpawnTurtle(spawnTurtle);
        //thrower.ResetCardsDrawn();
    }


private int DetermineExplosionType(List<int> randomCards)
{
    if (randomCards.Contains(2))
        {
        AudioSource.PlayClipAtPoint(fartClip, transform.position, 1f);
        return 0;}    // fart
    if (randomCards.Contains(4))
        {
            AudioSource.PlayClipAtPoint(suckClip, transform.position, 1f);
            return 4;   // suck star
        } 
        if (randomCards.Contains(5))
        {
            AudioSource.PlayClipAtPoint(glassClip, transform.position, 1f);
            return 3;   // inverse
        }    
    if (randomCards.Contains(10))
        {
            AudioSource.PlayClipAtPoint(freezeClip, transform.position, 1f);
            return 5;  // freeze
        } 
    if (randomCards.Contains(14))
        {
            AudioSource.PlayClipAtPoint(bigExplosionClip, transform.position, 1f);
            return 2;  // PERISH
        } 
     
    if(stats.explosionClip != null){
            AudioSource.PlayClipAtPoint(stats.explosionClip, transform.position, 1f);}
    return 1; // default explosion
}


public void SpawnTurtle(bool spawnTurtle)
    {
        if (spawnTurtle)
        {
            GameObject instance = Instantiate(terrorTurtlePrefab, transform.position, Quaternion.identity);

            GrenadeBase gBase = instance.GetComponent<GrenadeBase>();
            if (gBase != null)
                gBase.Owner = Owner; //owner of turtle = owner of rando-nade
        }
    }

    //Note: When testing, you must have both a face card and non-face card within list, or it explodes
public List<int> DrawCards()
{
    List<int> drawnCards = new List<int>();
    HashSet<int> faceCardsDrawn = new HashSet<int>();

    while (drawnCards.Count < 4)
    {
        int card = Random.Range(2, 15); //2-15

        if (Up != null && Up.luckUpgrade > 0)
            card = LuckMultipier(card);

        // Face cards: 11, 12, 13
        if (card >= 11 && card <= 13)
        {
            // Prevent duplicate face cards
            if (faceCardsDrawn.Contains(card))
                continue;

            faceCardsDrawn.Add(card);
            drawnCards.Add(card);

            // Keep drawing after face cards
            continue;
        }

        // Non-face card → final card, stop drawing
        drawnCards.Add(card);
        break;
    }
    Debug.Log("Cards drawn: " + string.Join(", ", drawnCards));
    return drawnCards;
}

//How this function works:
//Anything below a 7 is bad
//For each luck upgrade you have, it runs a statement that rerolls your number if it's less than 7.
//this means that for each luck up you have, it becomes less likely (but not impos)
public int LuckMultipier(int randomCard)
    {
        int newRandomCard = randomCard;
        for (int i = 0; i < Up.luckUpgrade; i++)
            {
                if (newRandomCard <= 6)
                {
                    newRandomCard = Random.Range(2, 15);
                }
            }
        return newRandomCard;
    }
           
}