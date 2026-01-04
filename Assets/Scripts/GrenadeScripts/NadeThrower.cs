using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NadeThrower : MonoBehaviour
{

    [SerializeField] public Camera mainCamera;

    [HideInInspector] public C4 activeC4Instance; // keep a reference to the last spawned C4 instance
    [HideInInspector] public BouncePad activeBPadInstance;
    [HideInInspector] public InstaNade activeInstaNadeInstance;
    [HideInInspector] public FragGrenade activeFragInstance;
    [HideInInspector] private float timeCooked = 0f;    //seconds held    
    //[HideInInspector] private float timeInstaNadeHasExisted = 0f;    //seconds held   

    //List of all active buildings (For building lord upgrade)
    public List<GrenadeBase> activeBuildings = new List<GrenadeBase>();
//!This is a list of grenades (Grenadebase), but it's owned by nadethrower, which is weird but works apparently

    public struct RandoSnapshot
    {
        public List<int> cards;
        public bool jack;
        public bool queen;
        public bool king;
    }

    private struct ThrowParameters
    {
        public Vector3 nadeDir;
        public float throwForce;
    }

    private AudioSource audioSource;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null){
            audioSource = gameObject.AddComponent<AudioSource>();}
        audioSource.spatialBlend = 1; // 3D sound
    }

    private void Update()
    {
        if (activeFragInstance != null){
            timeCooked += Time.deltaTime;}
        //if (activeInstaNadeInstance != null){
            //timeInstaNadeHasExisted += Time.deltaTime;}
    }
    
    private Vector3 NadeSpawnPos(float spawnPosFoBaMult, float spawnPosUpDownMult, GrenadeStats stats)
    {
        //Upgrades!
        UpgradesScript up = null;
        up = GetComponent<UpgradesScript>();

        if(stats.GrenadeName == "Jetpack" || stats.GrenadeName == "DoubleJump"){
            return transform.position + transform.forward * spawnPosFoBaMult + transform.up * spawnPosUpDownMult;}
            //Swapping to transform.forward made it so that I can look straight down with the jetpack, without hitting my face.
            //Needs to be before up.NoGravUpgrade, otherwise jetpack rockets come out of your chest
            //Todo maybe jetpack should shoot left rocket launcher, then right? But then trajectory could be messed up. Maybe just a visual effect, actually shoots straight invisible rockets? Or have both shoot at same time

        if(up.noGravUpgrade>0){
            return transform.position + transform.forward * spawnPosFoBaMult;}  //Has no Up/Down because it spawns in center of player

        else{
        return transform.position + mainCamera.transform.forward * spawnPosFoBaMult + transform.up * spawnPosUpDownMult;} 
            //Notes: mainCamera.transform.forward is how far away from face nade spawns, transform.up is how far up/down

            //mainCamera.transform.forward is an alternative to transform.forward. It takes distance from camera instead of distance from center of you.
            //mainCamera.transform.forward is generally superior, because it accounts for things like "looking straight down"

            //At base: Forward/back is 1.65f (can't be any lower), Up/Down is 0.4f
    }

    private GameObject SpawnGrenade(GameObject prefab, Vector3 spawnPos, Quaternion spawnRotation, int slotIndex)
    {
        GameObject instance = Instantiate(prefab, spawnPos, spawnRotation);

        // Set owner
        GrenadeBase gBase = instance.GetComponent<GrenadeBase>();
        gBase.slotIndex = slotIndex; //Sets slot index for Tsar Boomba
        if (gBase != null)
            gBase.Owner = this.gameObject;  //Sets the owner as yourself, sent to grenadeBase

        // Track C4
        C4 c4 = instance.GetComponent<C4>();
        if (c4 != null)
            activeC4Instance = c4;

        // Track Frag
        FragGrenade frag = instance.GetComponent<FragGrenade>();
        if (frag != null){
            activeFragInstance = frag;
        }

        // Track Bounce Pad
        BouncePad bpad = instance.GetComponent<BouncePad>();
        if (bpad != null){
            activeBPadInstance = bpad;}

        // Track InstaNade
        InstaNade instaNade = instance.GetComponent<InstaNade>();
        if (instaNade != null)
            activeInstaNadeInstance = instaNade;

        if (!activeBPadInstance){   //AWFUL way to say "if not bouncepad"
            StartCoroutine(IgnoreOwnerCollisionTemporarily(instance, gameObject, 1f));} //So that players don't collide with their own grenades after being thrown

        // Handle freeze rotation (bounce pad)
        Rigidbody rb = instance.GetComponent<Rigidbody>();
        if (gBase.freezeRotationOnThrow){
            rb.constraints = RigidbodyConstraints.FreezeRotation;}

        // Track buildings
        if (gBase.stats.isBuilding){
            activeBuildings.Add(gBase);}

        return instance;
    }


    public void SpawnAndThrow(GrenadeStats stats, int slotIndex, float timeAlreadyCooked)
    {
        GameObject prefab = stats.nadePrefab;
        UpgradesScript up = GetComponent<UpgradesScript>();

        RandoSnapshot drawnCardsSnapshot = default; //initializes all fields: cards, jack, queen and king
        if (stats.GrenadeName == "RandoNade")
        {
            RandoNade rando = prefab.GetComponent<RandoNade>();
            drawnCardsSnapshot.cards = rando.DrawCards();   //Calls DrawCards() exactly once, and stores it in the Struct
            drawnCardsSnapshot.jack  = drawnCardsSnapshot.cards.Contains(11);
            drawnCardsSnapshot.queen = drawnCardsSnapshot.cards.Contains(12);
            drawnCardsSnapshot.king  = drawnCardsSnapshot.cards.Contains(13);
        }

        Quaternion rotOffset = stats.rotationOffset;
        float spawnPosFoBaMult = stats.spawnPosForwardBackMultiplier;
        float spawnPosUpDownMult = stats.spawnPosUpDownMultiplier;
        float throwForce = stats.throwForce;
        float upThrowAngle = 0.2f;  //upThrowAngle is 0.2f unless otherwise specified (ex. jetpack)
            if (stats.GrenadeName == "Jetpack" || stats.GrenadeName == "DoubleJump"){upThrowAngle = stats.upThrowAngle;}  

        // Base throw angle
        Vector3 upAngle = Vector3.up * upThrowAngle;    //Almost always 0.2f
        Vector3 gravUpAngle = Vector3.up * 0.01f;
        Vector3 nadeDir = (mainCamera.transform.forward + upAngle).normalized;

        if (stats.GrenadeName == "Jetpack" || stats.GrenadeName == "DoubleJump") {
            nadeDir = upAngle;}

        //Apply most upgrades related to throwing
        ThrowParameters throwParams = ApplyUpgradesToThrow(stats, up, nadeDir, throwForce, gravUpAngle);
        nadeDir = throwParams.nadeDir;
        float currentThrowForce = throwParams.throwForce;


        Vector3 spawnPos = NadeSpawnPos(spawnPosFoBaMult, spawnPosUpDownMult, stats);
        Quaternion spawnRotation = Quaternion.LookRotation(nadeDir) * rotOffset;
        if(stats.isBuilding){spawnRotation = Quaternion.Euler(0f, 0f, 0f);}

        GameObject spawnedNade = SpawnGrenade(prefab, spawnPos, spawnRotation, slotIndex);  //Spawn the nade

        //for rando-nades
        RandoNade spawnedRando = spawnedNade.GetComponent<RandoNade>();
        if (spawnedRando != null){
            spawnedRando.Initialize(drawnCardsSnapshot);}   //Passes the snapshot into the grenade

        if(drawnCardsSnapshot.jack){currentThrowForce*=1.5f;}

        //Spiral Nades
        float spChance =  up.SpiralNadesUp();
        spChance = Mathf.Clamp01(spChance);
        if ((Random.value < spChance && up.spiralNadesUpgrade > 0) || drawnCardsSnapshot.queen){

                // 1. Spawn the normal grenade (this becomes the core)
                Rigidbody coreRb = spawnedNade.GetComponent<Rigidbody>();

                if (up.noGravUpgrade > 0)
                    coreRb.useGravity = false;

                // Convert it into a trajectory carrier
                GrenadeBase coreBase = spawnedNade.GetComponent<GrenadeBase>();
                coreBase.ConvertToSpiralCore();
            
                coreRb.linearVelocity = nadeDir * currentThrowForce;    //THROW

                // 2. Spawn the two real orbiting grenades
                Rigidbody[] orbitRbs = new Rigidbody[2];
                spawnPos.z+=0.8f;

                for (int i = 0; i < 2; i++)
                {
                    spawnPos.y += i+0.5f;
                    GameObject g = SpawnGrenade(prefab, spawnPos, spawnRotation, slotIndex);
                    Rigidbody rb = g.GetComponent<Rigidbody>();

                    coreBase.SetLayerRecursively(g, LayerMask.NameToLayer("SpiralNade"));   //prevents nades from colliding with each other

                    rb.linearVelocity = coreRb.linearVelocity;
                    orbitRbs[i] = rb;

                    RandoNade orbitRando = g.GetComponent<RandoNade>(); //This is here so that the spawned nades have the same cards drawn as original (passes it into grenade)
                        if (orbitRando != null){orbitRando.Initialize(drawnCardsSnapshot);}
                }

               
                // 3. Attach spiral controller to the core
                SpiralMotion spiral = spawnedNade.AddComponent<SpiralMotion>();
                spiral.coreRb = coreRb;
                spiral.orbitRbs = orbitRbs;
        }

        else    //Normal throwing
        {
            Rigidbody rb1 = spawnedNade.GetComponent<Rigidbody>();

            if (up.noGravUpgrade>0){
                rb1.useGravity = false;}

            InstaNade insta = spawnedNade.GetComponent<InstaNade>();
            if (insta != null)
            {
                insta.Explode();
                return;   
            }

            rb1.linearVelocity = nadeDir * currentThrowForce; //THROW
        }

            if(stats.throwClip != null){
                AudioSource.PlayClipAtPoint(stats.throwClip, transform.position, 0.05f);}

            if (stats.GrenadeName == "Frag" || stats.GrenadeName == "InstaNade"){       //todo might be broken with insta-nade, i dunno
                timeCooked = timeAlreadyCooked;
                StartCoroutine(TimeToExplodeInMidair(stats));}
    }

    public IEnumerator TimeToExplodeInMidair(GrenadeStats stats)
    {
        float cookTimer = stats.duration;
        yield return new WaitUntil(() => timeCooked>=cookTimer);
            Debug.Log($"Grenade cooked for {timeCooked} seconds");
            if(activeFragInstance!=null){activeFragInstance.Explode();}
            timeCooked = 0f;
    }

    private ThrowParameters ApplyUpgradesToThrow(GrenadeStats stats, UpgradesScript up, Vector3 baseNadeDir, float baseThrowForce, Vector3 gravUpAngle)
    {
        ThrowParameters throwParams = new ThrowParameters
        {
            nadeDir = baseNadeDir,
            throwForce = baseThrowForce
        };

        // Low grav upgrade
        if (up.noGravUpgrade > 0 && stats.GrenadeName != "Jetpack" && stats.GrenadeName != "DoubleJump"){   //jetpack doesn't work with 0 grav
            throwParams.nadeDir = (mainCamera.transform.forward + gravUpAngle).normalized;}

        // Upgrades affecting force
        throwParams.throwForce = throwParams.throwForce * (up != null ? up.ThrowStrengtUp() : 1f);
        throwParams.throwForce = throwParams.throwForce * (up != null ? up.MidAirUp(true) : 1f);
        throwParams.throwForce = throwParams.throwForce * (up != null ? up.noGravUp() : 1f);
        if (stats.isBuilding){throwParams.throwForce = throwParams.throwForce * (up != null ? up.EngineeringDegreeUp(true) : 1f);}

        // Increase Storm Count
        if (up.stormUpgrade > 0 && !stats.isSpecial){
            up.StormUp(true);}

        // Crossed Wires Upgrade. 50% with 1 of them, 100% with 2.
        if (up.buildingLordUpgrade > 0)
        {
            float chance = up.BuildingLordUp();
            chance = Mathf.Clamp01(chance);
            if (Random.value < chance)
            {
                foreach (var b in activeBuildings)
                {
                    if (b != null)
                    {
                        b.ForceActivate();
                    }
                }
            }
        }

        return throwParams;
    }


//This function prevents grenades from instantly hitting their owner
    IEnumerator IgnoreOwnerCollisionTemporarily(GameObject grenade, GameObject owner, float delay)
    {
        Collider[] grenadeColliders = grenade.GetComponentsInChildren<Collider>();
        Collider[] ownerColliders = owner.GetComponentsInChildren<Collider>();

        // Disable collision
        foreach (var gCol in grenadeColliders)
        {
            foreach (var oCol in ownerColliders)
            {
                if (oCol.gameObject.layer != LayerMask.NameToLayer("PlayerHitbox")) continue;   //stops the loop for everything EXCEPT player hitbox
                Physics.IgnoreCollision(gCol, oCol, true);
            }
        }

        yield return new WaitForSeconds(delay);

        // Re-enable collision
        foreach (var gCol in grenadeColliders)
        {
            foreach (var oCol in ownerColliders)
            {
                if (gCol != null && oCol != null)
                    Physics.IgnoreCollision(gCol, oCol, false);
            }
        }
    }
}



