using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
//Todo Scripts with their own Explode code:
//Bounce Pad
//Instanade

public class GrenadeBase : MonoBehaviour
{
    //the player (or GameObject) who threw/owns this grenade.
    public GameObject Owner { get; set; }

    [Header("Data")]
    public GrenadeStats stats; // assign either asset or runtime-clone
    
    public int slotIndex { get; set; } //records SlotIndex for stuff like Tsar Boomba

    [Header("Explosions")]
    public ParticleSystem explosionParticle;
    public ParticleSystem poisonParticle;
    public ParticleSystem iceParticle;
    public ParticleSystem suckParticle;
    public ParticleSystem largeParticle;

    protected Rigidbody rb;
    public bool hasExploded = false;
    [HideInInspector] public bool freezeRotationOnThrow = false;  //Only for Bounce Pad
    protected bool bonusExplosionComplete = false;

    [Header("Flight Control")]
    [HideInInspector] public FirstPersonController ownerFPC;
    [HideInInspector] public PlayerController ownerPController;

    [Header("Building Stuff")]
    public bool readyToFire;
    public FloatingTimerUI floatingUIPrefab;
    private FloatingTimerUI uiPlayer1;
    private FloatingTimerUI uiPlayer2;
    public SpinObject spinningPart;
    public float floatingTimerScale = 1f;

    [Header("Explosion Blocking")]
    [SerializeField] private LayerMask explosionBlockers;

    protected UpgradesScript upScript;
    public bool hitGround = false;  //for C4 and stuff

        //public AudioClip explosionClip;
    protected AudioSource audioSource;

    public NadeThrower.RandoSnapshot cardsDrawn;    //Creates empty struct

    public void Initialize(NadeThrower.RandoSnapshot snap)
    {
        cardsDrawn = snap;  //This copies everything in the struct into a new Struct called cardsDrawn

        if (cardsDrawn.cards == null)   //Detects an invalid snapshot
        {
            Debug.LogWarning("RandoSnapshot had null cards list — fixing.");
            cardsDrawn.cards = new List<int>();
        }
    }
    //Protected = Only this script, or SCRIPTS THAT INHERIT FROM IT, are allowed to touch this variable/function
    //Virtual = The defauly version of this function, but if a child class wants, it can replace it with an override.

    public virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (spinningPart == null){
            spinningPart = GetComponentInChildren<SpinObject>();}

        audioSource = GetComponent<AudioSource>();
        if(audioSource == null){
            audioSource = gameObject.AddComponent<AudioSource>();}
        audioSource.spatialBlend = 1; // 3D sound
    }

    protected virtual void Start()
    {
        if (stats.isBuilding){StartCoroutine(SelfDestruct(stats.duration));}
        if (Owner != null){
            ownerFPC = Owner.GetComponent<FirstPersonController>();
            ownerPController = Owner.GetComponent<PlayerController>();
            if(stats.isBuilding){AssignUI();}
            }

        if(stats.fireRate>0){
            float fireRate = stats.fireRate;
            fireRate = fireRate / (Up != null ? Up.EngineeringDegreeUp(false) : 1f);
            StartCoroutine(FireRate(fireRate));
            }
    }

    protected UpgradesScript Up   //called a "lazy getter", Tries to fetch UpgradesScript only when needed, Works no matter when Owner is assigned
    {
        get
        {
            if (upScript == null && Owner != null)
            {
                upScript = Owner.GetComponent<UpgradesScript>();
            }
            return upScript;
        }
    }

   
    
    private void FixedUpdate()
    {
        if (Up != null && Up.powerOfWillUpgrade > 0){
                ControlMidFlight(Up.PowerOfWillUp());}
    }

    public void AssignUI()
    {
        var players = PlayerSpawnManager.Instance.AllPlayers;

            // Spawn Player 1 UI
        if (players.Count >= 1)
        {
            uiPlayer1 = Instantiate(floatingUIPrefab, transform);
            uiPlayer1.target = this.transform;
            uiPlayer1.AssignOwner(players[0], Owner);
            SetLayerRecursively(uiPlayer1.gameObject, LayerMask.NameToLayer("UI_Player1"));
        }

        // Spawn Player 2 UI
        if (players.Count >= 2)
        {
            uiPlayer2 = Instantiate(floatingUIPrefab, transform);
            uiPlayer2.target = this.transform;
            uiPlayer2.AssignOwner(players[1], Owner);
            SetLayerRecursively(uiPlayer2.gameObject, LayerMask.NameToLayer("UI_Player2"));
        } 
    }

        // Helper function
    public void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    public void ControlMidFlight(float controlStrength)
    {
        if (rb == null || Owner == null) return;
        if (rb.IsSleeping()) return; // Only control if grenade is airborne

        Vector3 input = new Vector3(ownerPController.LookInput.x, 0, ownerPController.LookInput.y);

        if (input.sqrMagnitude < 0.01f) return; // ignore tiny input
        
        Vector3 moveDirection = ownerFPC.mainCamera.transform.TransformDirection(input); // Convert input to world space (relative to camera)
        moveDirection.y = 0; // only horizontal influence
        moveDirection.x = math.clamp(moveDirection.x, -1.5f, 1.5f); // max -1.5 to 1.5 allowed
        moveDirection.z = math.clamp(moveDirection.z, -1.5f, 1.5f); // max -1.5 to 1.5 allowed
        
        rb.AddForce(moveDirection.normalized * controlStrength, ForceMode.Acceleration);
        //You can optionally clamp the velocity so the player can’t accelerate it infinitely.
        //Todo I feel as though this drastically reduces their total distance, probably the pythagorian theorem. Find way to equalize.
    }

    public IEnumerator FireRate(float cooldown) //rate at which buildings activate
    {
        readyToFire = false;
        float timeLeft = cooldown;
        if(stats.isBuilding && Up.angryBuildingsUpgrade>0 && !bonusExplosionComplete){  //Only happens once
            timeLeft=1f;
            bonusExplosionComplete = true;}

        while (timeLeft >= 0f)
            {
                //For Slurpinator's spinning blase:
                float normalized = cooldown > 0f ? 1f - (timeLeft / cooldown) : 1f; // normalized goes 0 → 1 as the timer runs out
                // ^^ Above code says //float normalized = 1f - (timeLeft / cooldown);   
                float spinMultiplier = Mathf.Lerp(0.1f, 3f, normalized) * Mathf.Lerp(0.1f, 3f, normalized);  // spin gets 3x^2 faster near the end
                if (spinningPart != null){spinningPart.SetSpinMultiplier(spinMultiplier);}

                if (uiPlayer1 != null)
                    uiPlayer1.SetTime(timeLeft);    

                if (uiPlayer2 != null)
                    uiPlayer2.SetTime(timeLeft);

                if (!ownerFPC.inUpgradeMenu)   //timer doesn't go down while in menu
                    {
                        timeLeft -= Time.deltaTime; //Building timers tick down
                    }
                
                yield return null;
            }

        readyToFire = true;
    }

    public IEnumerator SelfDestruct(float time)
    {
        yield return new WaitForSeconds(time);
        Explode();  //Buildings always explode one last time before death

        var thrower = Owner.GetComponent<NadeThrower>();
        if (thrower != null){
            thrower.activeBuildings.Remove(this);}  //Remove from active building list

        Destroy(gameObject);
    }

    public void ConvertToSpiralCore()
    {
        // Disables visual for center nade
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        // Disable all colliders (no collision at all)
        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        // Mark as already exploded so grenade logic ignores it
        hasExploded = true;
        Destroy(gameObject, 10f);   //center nade eventually destroys itself
    }



    public void ForceActivate()
    {
        Explode();
    }

    public virtual float RadiusUpgrades(float radius, float nadeDistanceFromThrower, GameObject bPad)
    {
        //Note:  GameObject bPad is only used by the Bounce Pad script.
        float finalRadius = radius;
        finalRadius = finalRadius * (Up != null ? Up.ExplRadiusUp() : 1f);
        finalRadius = finalRadius / (Up != null ? Up.SniperUpgrade(true) : 1f);     //76.92 of the original radius
        finalRadius = finalRadius * (Up != null ? Up.FarExplRadiusUp(nadeDistanceFromThrower) : 1f);

        if(cardsDrawn.king){finalRadius*=1.5f;}

        //Debug.Log("finalRadius"+ finalRadius);
        return finalRadius;
    }

    public virtual float StrengthUpgrades(GameObject player, float distanceFromThrower, float strengthMultiplier)
    {
        float finalStrength = strengthMultiplier;

        FirstPersonController fpc = player.GetComponent<FirstPersonController>();
        WeaponInventory inv = Owner.GetComponent<WeaponInventory>();

        finalStrength = finalStrength * (Up != null ? Up.ExplStrengtUp() : 1f);
        finalStrength = finalStrength * (Up != null ? Up.SniperUpgrade(false) : 1f);
        finalStrength = finalStrength * (Up != null ? Up.HugUp(distanceFromThrower) : 1f); 
        finalStrength = finalStrength * (Up != null ? Up.FastBallJuiceUp(distanceFromThrower) : 1f);
        finalStrength = finalStrength * (Up != null ? Up.MidAirUp(false) : 1f);
        finalStrength = finalStrength * (Up != null ? Up.FiftyFiftyUp() : 1f);
        if (!stats.isSpecial) {finalStrength = finalStrength * (Up != null ? Up.StormUp(false) : 1f);}
        if (slotIndex == 3)
        {
            finalStrength = finalStrength * (Up != null ? Up.Slot4TsarBoombaUp(false)  : 1f);
        }    
        if (Owner == player)      //If the owner is also hit by the explosion, 
        {
            finalStrength = finalStrength * (Up != null ? Up.CrazedBomberUp() : 1f);
            if (Up.masochismUpgrade > 0 && slotIndex>-1){    //SlotIndex>0 is so that double jump doesn't break
                inv.ApplyCooldownAfterThrow(slotIndex, false);}
        }

            //CUT combo upgrade:
            if (fpc.lastExplosionCauser != null && fpc.fpcGrounded == false) //if you hit opponent, and they're in midair,
                {finalStrength = finalStrength * (Up != null ? Up.ComboUp() : 1f);}     

        float finalTotalStrength = finalStrength * fpc.invCurseStrength;    //used for Kzaro's Ring, to see if knockback>double base
        if (finalTotalStrength>2)   //This one MUST ALWAYS BE LAST
        {
            /*Instantiate(up.kzaroExplosionParticle,transform.position,Quaternion.identity);}*/
            finalStrength = finalStrength * (Up != null ? Up.KzarosRingUp() : 1f);
        }

        //This reduces the scaling that Inverse-Nades recieves by 3. They still recieve some scaling from strength ups, just not much
        if(stats.GrenadeName=="InverseNade"){finalStrength = finalStrength/3;}

        //Debug.Log("finalStrength"+finalStrength);
        float finalStrengthSquared = Mathf.Sqrt(finalStrength);
            return finalStrengthSquared;      
    }
    //Todo: Be aware that bonus strength is squared at the end, because strength is added to horizontal AND vertical. So if you just increase strength by 1.3x, then
    //Todo: Both horiziontal and vertical would increased by 1.3x, which is 1.69x. This fixes that issue.
    //Todo: Also, there *might* be a minor bug, where having 2 of the same strength upgrade results in slightly less of a boost

    public virtual void VisualExplosion(float radius, int style)
    {
        Vector3 circularExplosion = Vector3.one;

        if (explosionParticle != null) 
        {
            ParticleSystem explosion = null;
            if (style==0){return;}
            if (style==1){explosion = Instantiate(explosionParticle, transform.position, explosionParticle.transform.rotation);}
            if (style==2){explosion = Instantiate(largeParticle, transform.position, explosionParticle.transform.rotation);}
            if (style==3){explosion = Instantiate(poisonParticle, transform.position, explosionParticle.transform.rotation);}
            if (style==4){explosion = Instantiate(suckParticle, transform.position, explosionParticle.transform.rotation);}
            if (style==5){explosion = Instantiate(iceParticle, transform.position, explosionParticle.transform.rotation);}
            
            // compute scale multiplier
            float scaleMultiplier = stats.visualRadiusMultiplier*radius / stats.explosionRadius;    

            // apply scaling
            explosion.transform.localScale = circularExplosion * scaleMultiplier;
        }
    }

    public virtual Vector3 FindDirection(Vector3 playerPos, Vector3 nadePos)
    {
        Vector3 direction;

        direction = (playerPos - nadePos).normalized;   //Normal case: This is the direction for all other grenades 
        return direction;
    }

    
    public void SpawnStickyBomb(GameObject playerHit, GameObject Owner)    //owner only for fpc
    {
        float chance = Up.StickyBombUp();
        chance = Mathf.Clamp01(chance);
        if (UnityEngine.Random.value < chance)
        {
            GameObject sticky = Instantiate(Up.stickyBombPrefab, playerHit.transform.position, Quaternion.identity);

            StickyBomb stickyScript = sticky.GetComponent<StickyBomb>();
            if (stickyScript == null) return;

            stickyScript.AttachToPlayer(playerHit, Owner, transform.position, hitGround);   // transform.position = explosion origin
        }
    }

    public virtual float NadeModifyingExplosionStrength(float currentStrength, float distance)  //Used for Fastball
    {
        return currentStrength;
    }   

    public virtual bool InstaNadeCooldownLogic(GameObject Owner) //Used for InstaNade
    {
        return false;    //Always returns true for everything except for Instanade, which overrides this function
    }  

    public virtual void ReducedBuildingKnockback(float currentStrengthMult, float nadeDistanceFromThrower, GameObject player, Vector3 direction)
    {
        float thisPlayerStrength = currentStrengthMult / 2;     
        float finalStrength = stats.Strength * thisPlayerStrength;                      
        Vector3 push = direction * finalStrength;
        HowMuchKnockback(push, Owner, player, finalStrength);
    }  

    public bool HasLineOfSightToPlayer(GameObject player)
    {
        Vector3 origin = transform.position;
        Vector3 target = player.transform.position;

        Vector3 direction = target - origin;
        float distance = direction.magnitude;

        // Optional: raise ray slightly so ground doesn’t block everything
        origin.y += 0.2f;
        target.y += 0.2f;
        direction = target - origin;

        if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, distance, explosionBlockers))
        {
            // Something blocked the ray before reaching the player
            return false;
        }

        return true;
    }

    protected virtual void OnCollisionEnter(Collision collision)    //For nades which explode upon hitting anything (ex. bouncenade hitting floor)
    {
        if (hasExploded) return;
            GameObject other = collision.gameObject;
            hitGround = other.layer == LayerMask.NameToLayer("Ground");
        hasExploded = true;
        Explode();
    }

    private void OnTriggerEnter(Collider other) //For nades which explode upon hitting PLAYERS
{
    if (hasExploded) return;

    if (other.gameObject.layer == LayerMask.NameToLayer("PlayerHitbox"))
    {
        Explode();
    }
}

    // Kaboom!
    public virtual void Explode()
    {
        float nadeDistanceFromThrower = Vector3.Distance(Owner.transform.position, gameObject.transform.position);  //distance of thrower vs opponent, used for upgrades

        float initialStrength = stats.Strength;
        float strengthMultiplier = 1;
        float currentRadius = RadiusUpgrades(stats.explosionRadius, nadeDistanceFromThrower, gameObject);

        bool instanadeHasntHitOpponent = true;
 
                Collider[] hits = Physics.OverlapSphere(transform.position, currentRadius, LayerMask.GetMask("PlayerHitbox"));

                HashSet<GameObject> affectedPlayers = new HashSet<GameObject>();

                foreach (Collider hit in hits)
                {
                    GameObject player = hit.GetComponentInParent<FirstPersonController>()?.gameObject;

                    if (player == null) continue;
                    // Prevent double hits from multiple colliders
                    if (!affectedPlayers.Add(player)) continue;
                    if (!HasLineOfSightToPlayer(player)) continue;

                    FirstPersonController controller = player.GetComponent<FirstPersonController>();
                    if (controller == null) continue;
                        Vector3 direction;
                        direction = FindDirection(player.transform.position, gameObject.transform.position);    //Suck Star/Slurp Modifying code
                        strengthMultiplier = NadeModifyingExplosionStrength(strengthMultiplier, nadeDistanceFromThrower);   //Mostly for Fastball

                        if (Owner != player && Up.stickyBombUpgrade > 0){SpawnStickyBomb(player, Owner);}
                        if (Owner != player && instanadeHasntHitOpponent == true){    //Insta-Nade Logic
                            instanadeHasntHitOpponent = InstaNadeCooldownLogic(Owner);}

                        //Applies Strength upgrades:
                        strengthMultiplier = StrengthUpgrades(player, nadeDistanceFromThrower, strengthMultiplier);

                        //This is so that your own buildings deal 25% knockback to self
                        if (Owner == player && stats.isBuilding){
                            ReducedBuildingKnockback(strengthMultiplier, nadeDistanceFromThrower, player, direction); }

                        else{   //Explode! Apply knockback
                            float finalStrength = initialStrength * strengthMultiplier;
                            Vector3 push = direction * finalStrength;
                            Debug.Log("finalStrength"+finalStrength);
                            HowMuchKnockback(push, Owner, player, finalStrength);}
                    
                
            }
        VisualExplosion(currentRadius, 1);

        if(stats.explosionClip != null){
            AudioSource.PlayClipAtPoint(stats.explosionClip, transform.position, 1f);}

        if (!stats.isBuilding){Destroy(gameObject);} 
    }

    public virtual void HowMuchKnockback(Vector3 push, GameObject Owner, GameObject player, float currentStrength)
    {
        //currentStrength here isn't used for most nades, currently only used by InverseNade.
        FirstPersonController controller = player.GetComponent<FirstPersonController>();
        controller.ApplyPush(push, Owner); 
    }


}


