using System.Collections;
using UnityEngine;

public class UpgradesScript : MonoBehaviour
{
    public FirstPersonController fpc;
    public InventoryUI invUI;
    [SerializeField] public GameObject stickyBombPrefab;
    
    public int runFastUpgrade = 0;
    public int jumpHighUpgrade = 0;
    public int speedScalerUpgrade = 0;
        public int numObelisksWon = 0;
    public int sizeUpUpgrade = 0;
    public int sizeDownUpgrade = 0;
    public int doubleJumpUpgrade = 0;
        [HideInInspector] public bool doubleJumpRecharged = true;
        [HideInInspector] public float[] doubleJumpRechargeTimeLeft = new float[1];
        public GrenadeStats doubleJumpStats;

    public int throwStrengthUpgrade = 0;
    public int explRadiusUpgrade = 0;
    public int farExplRadiusUpgrade = 0;
    public int noGravUpgrade = 0;
    public int powerOfWillUpgrade = 0;
    
    public int theSniperUpgrade = 0;
    public int explStrengthUpgrade = 0;
    public int crazedBomberUpgrade = 0;
    public int huggerUpgrade = 0;
    public int fastBallJuiceUpgrade = 0;
    public int comboUpgrade = 0;
    public int KzarosRingUpgrade = 0;
    public int fiftyFiftyUpgrade = 0;

    public int cooldownUpgrade = 0;
    public int slot1CooldownUpgrade = 0;
    public int slot4TsarBoombaUpgrade = 0;
    public int masochismUpgrade = 0;
    public int indecisiveUpgrade = 0;

    public int angryBuildingsUpgrade = 0;
    public int engineeringDegreeUpgrade = 0;

    public int stickyBombUpgrade = 0;
    public int spiralNadesUpgrade = 0;

    public int buildingLordUpgrade = 0;
    public int midAirUpgrade = 0;
    public int luckUpgrade = 0;
    //public int momsEyeUpgrade = 0;
    public int stormUpgrade = 0;
        private int stormStacks = 0;   // Current stacks
        private Coroutine stormRoutine;

        
public void GrantUpgrade(UpgradeType type)
{
    switch (type)       //!When adding nade, you must: update it's Data scriptable object, put it in the obelisk prefab, and do this!
    {
        case UpgradeType.RunFast:
            runFastUpgrade += 1;
            break;
        case UpgradeType.JumpHigh:
            jumpHighUpgrade += 1;
            break;
        case UpgradeType.SpeedScaler:
            speedScalerUpgrade += 1;
            break;
        case UpgradeType.SizeDown:
            sizeUpUpgrade += 1;
            break;
        case UpgradeType.SizeUp:
            sizeDownUpgrade += 1;
            break;

        case UpgradeType.ThrowStrength:
            throwStrengthUpgrade += 1;
            break;
        case UpgradeType.ExplRadius:
            explRadiusUpgrade += 1;
            break;
        case UpgradeType.FarExplRadius:
            farExplRadiusUpgrade += 1;
            break;
        case UpgradeType.noGrav:
            noGravUpgrade += 1;
            break;

        case UpgradeType.theSniper:
            theSniperUpgrade += 1;
            break;
        case UpgradeType.explStrength:
            explStrengthUpgrade += 1;
            break;
        case UpgradeType.crazedBomber:
            crazedBomberUpgrade += 1;
            break;
        case UpgradeType.hugger:
            huggerUpgrade += 1;
            break;
        case UpgradeType.fastBallJuice:
            fastBallJuiceUpgrade += 1;
            break;
        case UpgradeType.combo:
            comboUpgrade += 1;
            break;
        case UpgradeType.KzarosRing:
            KzarosRingUpgrade += 1;
            break;
        case UpgradeType.FiftyFifty:
            fiftyFiftyUpgrade += 1;
            break;

        case UpgradeType.cooldown:
            cooldownUpgrade += 1;
            break;
        case UpgradeType.slot1Cooldown:
            slot1CooldownUpgrade += 1;
            break;
        case UpgradeType.slot4TsarBoomba:
            slot4TsarBoombaUpgrade += 1;
            break;
        case UpgradeType.MasochismUp:
            masochismUpgrade += 1;
            break;
        case UpgradeType.indecisive:
            indecisiveUpgrade += 1;
            break;

        case UpgradeType.StickyBomb:
            stickyBombUpgrade += 1;
            break;
        case UpgradeType.SpiralNades:
            spiralNadesUpgrade += 1;
            break;
            
        case UpgradeType.angryBuildings:
            angryBuildingsUpgrade += 1;
            break;
        case UpgradeType.engineeringDegree:
            engineeringDegreeUpgrade += 1;
            break;

        case UpgradeType.buildingLord:
            buildingLordUpgrade += 1;
            break;
        case UpgradeType.midAir:
            midAirUpgrade += 1;
            break;
        case UpgradeType.Storm:
            stormUpgrade += 1;
            break;
        case UpgradeType.Luck:
            luckUpgrade += 1;
            break;
        case UpgradeType.PowerOfWill:
            powerOfWillUpgrade += 1;
            break;
        case UpgradeType.DoubleJump:
            doubleJumpUpgrade += 1;
            invUI.ShowJetpack(doubleJumpStats);
            break;

        // add all cases
        default:
            Debug.LogWarning("Unknown upgrade type: " + type);
            break;
    }
}

    private void Update()
    {
        DoubleJumpCooldown();
    }

    public float JumpHighUp()
    {
        return 1f + (1.00f * jumpHighUpgrade);  //2x
    }

    public float RunFastUp()
    {
        return 1f + (0.15f * runFastUpgrade);
    }

    public float SpeedScalerUp()
    {
        float statsGained = numObelisksWon * 0.05f;
        return 1f + (statsGained * speedScalerUpgrade);
    }


    public void ApplyDoubleJumpCooldown()
    {
        doubleJumpRecharged = false;

        float doubleJumpCooldown = 15 / doubleJumpUpgrade;
        doubleJumpRechargeTimeLeft[0] = doubleJumpCooldown;
    }
    public void DoubleJumpCooldown()
    {
        if (!fpc.inUpgradeMenu && doubleJumpUpgrade>0){//No cooldowns while in upgrade menus!
        // reduce cooldown timer
            if (doubleJumpRechargeTimeLeft[0] > 0){  //as long as timer >0
                doubleJumpRechargeTimeLeft[0] -= Time.deltaTime;    //decrease by 1 each second, unless made faster
            }
            if (doubleJumpRechargeTimeLeft[0] <= 0){  //as long as timer <0 (fixes funny bug)
                doubleJumpRechargeTimeLeft[0] = 0;
                doubleJumpRecharged = true;}
        }
    }
/*
    //Todo DON'T WORK
    public float SizeUpUp()
    {
        return 1f + (0.33f * sizeUpUpgrade);
    }
    public float SizeDownUp()
    {
        return 1f + (0.1f * sizeDownUpgrade);
    }
    public void SizeUpgrades()
    {
        float sizeDownNum = 1 - (0.25f * sizeUpUpgrade);
        float sizeUpNum = 1 + (0.5f * sizeUpUpgrade);
        float scaleStep = 1f * sizeDownNum * sizeUpNum;   //normal case, size == 1
        Debug.Log("scaleStep"+scaleStep);
        float minScale = 0.5f;
        float maxScale = 2f;
        
        Vector3 newScale = transform.localScale + Vector3.one * scaleStep;

        // Clamp to avoid breaking physics/camera/etc.
        newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
        newScale.y = Mathf.Clamp(newScale.y, minScale, maxScale);
        newScale.z = Mathf.Clamp(newScale.z, minScale, maxScale);

        transform.localScale = newScale;
    }*/



    public float ThrowStrengtUp()
    {
        return 1f + (0.50f * throwStrengthUpgrade);
    }

    public float ExplRadiusUp()
    {
        return 1f + (0.20f * explRadiusUpgrade);
    }
    public float FarExplRadiusUp(float distanceFromThrower)
    {
        float minStrength = 0f;   // minimum multiplier
        float maxStrength = 0.30f;   // maximum multiplier
        float maxDistance = 30f;  // the distance at which the effect hits maxStrength
        float distanceMultiplier = Mathf.Clamp(distanceFromThrower / maxDistance, 0f, 1f);  //ensures the multiplier stays between 0 and 1.
        float radGiven = Mathf.Lerp(minStrength, maxStrength, distanceMultiplier);
        return 1f + (radGiven * farExplRadiusUpgrade);
    }
    public float noGravUp()
    {
        return 1f + (0.25f * noGravUpgrade);
    }
    public float PowerOfWillUp()
    {
        return 1f + (6f * powerOfWillUpgrade);    //gives 5, then 9 then 14, which is weird but whatever
    }



    public float ExplStrengtUp()
    {
            return 1f + (0.25f * explStrengthUpgrade);    //1.15^2 = 1.3225 
    }

    public float CrazedBomberUp()
    {
        return 1f + (0.5f * crazedBomberUpgrade);  //1.3^2 = 1.69
    }

    public float HugUp(float distanceFromThrower)
    {
        float minStrength = 0f;   // minimum multiplier
        float maxStrength = 0.40f;   // maximum multiplier      //1.2^2 = 1.44
        float maxDistance = 15f;  // the distance at which the effect hits minStrength
        float distanceMultiplier = Mathf.Clamp01(1f - (distanceFromThrower / maxDistance));  //ensures the multiplier stays between 0 and 1.
        float strengthGiven = Mathf.Lerp(minStrength, maxStrength, distanceMultiplier);
        return 1f + (strengthGiven * huggerUpgrade);
        
    } 
    public float FastBallJuiceUp(float distanceFromThrower)
    {
        float minStrength = 0f;   // minimum multiplier
        float maxStrength = 0.60f;   // maximum multiplier      //1.3^2 = 1.69
        float maxDistance = 25f;  // the distance at which the effect hits maxStrength
        float minDistance = 5f;  // the distance at which the effect hits maxStrength
        float distanceFromThrowerMinus10 = distanceFromThrower - minDistance;
        float distanceMultiplier = Mathf.Clamp(distanceFromThrowerMinus10 / maxDistance, 0f, 1f);  //ensures the multiplier stays between 0 and 1.
        float strengthGiven = Mathf.Lerp(minStrength, maxStrength, distanceMultiplier);
        return 1f + (strengthGiven * fastBallJuiceUpgrade);
    }
    //If <10: 0 strengthGiven
    //Maximum distance: 30f (because it's 5 + 25 = 30)
    public float SniperUpgrade(bool radiusCheck)
    {
        if (radiusCheck){
            return 1f + (0.3f * theSniperUpgrade);}
        else{
            return 1f + (0.60f * theSniperUpgrade);}   //1.3^2 = 1.69x
    }
    public float ComboUp()  //CUT
    {
        return 1f + (1.85f * comboUpgrade);         //1.4^2 = 1.96x
    }
    public float KzarosRingUp()
    {
        return 1f + (0.50f * KzarosRingUpgrade);    //1.33^2=1.7689
    }
    public float Slot4TsarBoombaUp(bool isFromWeaponInv)      
    {
        if (isFromWeaponInv)
        {
            return 1f + (0.5f * slot4TsarBoombaUpgrade);     //MAKES COOLDOWN 50% longer
        }
        else
        {
            return 1f + (1f * slot4TsarBoombaUpgrade);     //1.5^2 = 2.25
        }
    }
    public float MidAirUp(bool isFromNadeThrower)
    {
        if (fpc.beingExploded)  //if in midair from explosion
        {
            if (!isFromNadeThrower){    //Grenadebase
                return 1f + (0.75f * midAirUpgrade);}       //1.5^2 = 2.25
            else{           //nadethrower
                return 1f + (0.5f * midAirUpgrade);} 
        }
        else{
            return 1f;}
    }
    public float FiftyFiftyUp()
    {
        float luckMultiplier = LuckUp();
        float chance = 0.5f * luckMultiplier;
        chance = Mathf.Clamp01(chance);
        if (Random.value < chance){
            return 1f + (0.5f * fiftyFiftyUpgrade);}
        else{
            return 1f;}
    }
    //extra copies only scale knockback, not chance


    public float CooldownUp()
    {
        return 1f + (0.1f * cooldownUpgrade);
    }

    public float Slot1CooldownUp()      
    {
        return 1f + (0.40f * slot1CooldownUpgrade);
    }
    public float MasochismUp()      
    {
        return 1f + (0.25f * masochismUpgrade);
    }
    public float IndecisiveUp()      
    {
        return 1f + (0.30f * indecisiveUpgrade);    //TO CHANGE TIMER FOR THIS, WEAPON INVENTORY IN THE INSPECTOR!
    }



    public float StickyBombUp()      
    {
        float luckMultiplier = LuckUp();
        float luck = 0.1f * stickyBombUpgrade * luckMultiplier;
        return luck;
    }
    public float SpiralNadesUp()      
    {
        float luckMultiplier = LuckUp();
        float luck = 0.1f * spiralNadesUpgrade * luckMultiplier;
        return luck;
    }

    public void AngryBuildingsUp()      
    {
        //All done within grenadebase
    }
    public float EngineeringDegreeUp(bool isFromNadeThrower)      
    {
        if (isFromNadeThrower){  //Throw Force
        return 1f + (2f * engineeringDegreeUpgrade);}
        else{  //fire rate
        return 1f + (0.25f * engineeringDegreeUpgrade);}
    }

    public float LuckUp()      
    {
        float luckMult = 1; //initially 1
        if (luckUpgrade > 0)    //if you have luck upgrade:
        {
            luckMult = 2 * luckUpgrade;
        }
        return luckMult;
    }

    public float BuildingLordUp()      
    {
        float luckMultiplier = LuckUp();
        float luck = 0.5f * buildingLordUpgrade * luckMultiplier;
        return luck;
        //All done within nadeThrower
    }
    public float StormUp(bool isFromNadeThrower)
    {
        if (stormUpgrade==0)
            return 1f;

        if (isFromNadeThrower)  //Only adds stacks when THROWN, not when it expldoes!
        {
            // Add 1 stack
            stormStacks++;

            // Restart the timer
            if (stormRoutine != null)
                StopCoroutine(stormRoutine);

            stormRoutine = StartCoroutine(StormTimer());
        }

        // Each stack = +25%
        return 1f + ((stormStacks - 1) * 0.33f * stormUpgrade);     //1.25^2=1.5625
    }

    private IEnumerator StormTimer()
    {
        yield return new WaitForSeconds(2f);

        // Reset stacks after inactivity
        stormStacks = 0;
        stormRoutine = null;
    }
}
