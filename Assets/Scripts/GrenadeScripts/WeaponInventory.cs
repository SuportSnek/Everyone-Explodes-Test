using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using System;

public class WeaponInventory : MonoBehaviour
{
    [Header("Weapon slots (0–3)")]
    public GrenadeStats[] slots = new GrenadeStats[4]; 
    public float[] cooldownTimers = new float[4];
    public float[] specialWeaponDurations = new float[4];
    public float[] indecisiveWeapons = new float[4];

    private NadeThrower nthrower;
    //public InventoryUI invUI;  // assign in inspector
    [SerializeField] public FirstPersonController fpc;
    [SerializeField] public UpgradesScript up;    
    [SerializeField] public PlayerController pController;    
    [SerializeField] private UITimeSlider cookFragSlider;

    public float WeaponSpamLockout = 0.5f;
    public bool secondC4Lockout = false;
    bool canThrow = true;
    public float indecisiveTime = 30f;

    public AudioClip tickingFragClip;
    private AudioSource audioSource;
    private AudioSource fragTickAudioSource;

//Todo fix bug where player can't grab weapon sometimes?
private void Awake()
{
    nthrower = GetComponent<NadeThrower>();

    audioSource = GetComponent<AudioSource>();
    if(audioSource == null){
        audioSource = gameObject.AddComponent<AudioSource>();}

    audioSource.loop = true;
    audioSource.playOnAwake = false;
    audioSource.spatialBlend = 1;

    // Second audio source for frag ticking
    fragTickAudioSource = gameObject.AddComponent<AudioSource>();
    fragTickAudioSource.loop = true;
    fragTickAudioSource.playOnAwake = false;
    fragTickAudioSource.spatialBlend = 1;
}

    private void Update()
    {
        if (!fpc.inUpgradeMenu){//No cooldowns while in menus!
            // reduce cooldown timers
            for (int i = 0; i < cooldownTimers.Length; i++) //Starting at the weapon cooldown timer length,
            {
                if (cooldownTimers[i] > 0)  //as long as timer >0
                    cooldownTimers[i] -= Time.deltaTime;    //decrease by 1 each second, unless made faster
                if (cooldownTimers[i] <= 0)  //as long as timer <0 (fixes funny bug)
                    cooldownTimers[i] = 0;
            }
                    // reduce special weapon timer. When 0, remove it from struct.

        // Special weapon countdown, and play music
            for (int i = 0; i < specialWeaponDurations.Length; i++) //was slots.Length
            {
                var weapon = slots[i];
                if (weapon == null) continue;

                if (weapon.isSpecial)
                {
                    if (specialWeaponDurations[i] >= 0f){
                        specialWeaponDurations[i] -= Time.deltaTime;

                        if (!audioSource.isPlaying){
                            audioSource.clip = weapon.specialMusic;
                            audioSource.loop = true;
                            audioSource.Play(); //Todo need to change this so that it doesn't overlap with music
                            Debug.Log("test");
                        }
                    }

                    // Remove special weapon
                    //Todo also need code for when someone replaces special weapon
                    if (specialWeaponDurations[i] <= 0f && specialWeaponDurations[i] > -1f) //Basically, as long as timer = 0
                    {
                        slots[i] = null;    //remove from inventory
                        specialWeaponDurations[i] = -1; //-1 indicates the weapon is gone, used in PlayerHealth
                        Debug.Log($"Removed special weapon from slot {i}");
                        var ui = GetComponentInChildren<InventoryUI>();
                        ui.RemoveSlot(i);   //deletes image
                        if (audioSource.isPlaying){
                            audioSource.Stop();
                            audioSource.clip = null;
                        }
                        if (weapon.GrenadeName == "Jetpack"){fpc.hasJetpack=false;} //if it was a jetpack, remove improved air mobility
                    }
                }
            }
        }
    }

    public void SetWeapon(int slotIndex, GrenadeStats weapon)
    {
        if (slotIndex < 0 || slotIndex > 3) return;
        //Todo Bug for future me to fix: if you throw a c4, then replace it, the c4 on the ground will remain forever

        slots[slotIndex] = weapon;
        cooldownTimers[slotIndex] = 0f;

        if (weapon.isSpecial){
            specialWeaponDurations[slotIndex] = weapon.duration; 
            if (weapon.GrenadeName == "Jetpack"){
                fpc.hasJetpack=true;} //extra air mobility for having jetpack
        }
        SetIndecisive(slotIndex);
    }

    public void TryThrowSlot(int slotIndex)
    {
        var weapon = slots[slotIndex];
        if (weapon == null) return;
        if(fpc.inUpgradeMenu || fpc.inWeaponMenu || fpc.frozenCurse || fpc.wasJustInMenu) return;  //prevents shooting while in menu, and just after you exit it
        if (slotIndex < 0 || slotIndex > 3) return;
        if (!canThrow && !weapon.isSpecial) return;  //global lockout after throwing nade

    //For c4:
    //If weapon can remote detonate, nThrower remembers a c4 existing, AND it's cooldown armed
        if (weapon.remoteDetonation && nthrower.activeC4Instance != null &&nthrower.activeC4Instance.CanDetonate()){
            nthrower.activeC4Instance.Explode();
            StartCoroutine(NoInstantlyThrowSecondC4(0.2f));
            nthrower.activeC4Instance = null;
            return;
        }

        else{
            if (cooldownTimers[slotIndex] > 0) return;  //if grenade still on cooldown return
            if (secondC4Lockout == true) return;    //if it's been a split second since you threw a c4
            
            ApplyCooldown(slotIndex, weapon.Cooldown);   //apply weapon cooldown

            if (weapon.GrenadeName == "Frag"){
                StartCoroutine(CookFrag(weapon, slotIndex));
                return;} // stops TryThrowSlot for now
            
            Throw(weapon, slotIndex, 0f);      //needs to be another fraction cause of the frag
            }
    }

public void Throw(GrenadeStats weapon, int slotIndex, float tCooked)
    {
        nthrower.SpawnAndThrow(weapon, slotIndex, tCooked);      //THROW BABY

        if(weapon.GrenadeName=="Jetpack") return;
        StartCoroutine(NadeSpamPreventer(WeaponSpamLockout));
    }

    public void ApplyCooldown(int slotIndex, float baseCooldown)
    {
        float finalCooldown = baseCooldown;

        // Basic cooldown upgrades
        finalCooldown = finalCooldown / (up != null ? up.CooldownUp() : 1f); 
        finalCooldown = finalCooldown / (up != null ? up.SpeedScalerUp() : 1f); 

        if(indecisiveWeapons[slotIndex]>-1 && up.indecisiveUpgrade>0){       //if you have the indecisive upgrade
            finalCooldown = finalCooldown / (up != null ? up.IndecisiveUp() : 1f);      
            }

        if(slotIndex == 0 && up.slot1CooldownUpgrade>0){       //if you have the slot 1 cooldown upgrade
            finalCooldown = finalCooldown / (up != null ? up.Slot1CooldownUp() : 1f);       //Slot 1 cooldown reduction
            }

        if(slotIndex == 3 && up.slot4TsarBoombaUpgrade>0){       //if you have the slot 4 cooldown upgrade
            finalCooldown = finalCooldown * (up != null ? up.Slot4TsarBoombaUp(true) : 1f);       //Slot 4 cooldown up
            }

        cooldownTimers[slotIndex] = finalCooldown;
    }

    public void ApplyCooldownAfterThrow(int slotIndex, bool instanadeHitOpponent) //for masochism and insta-nade
        { 
            float cooldownMult = 1;
            cooldownMult = cooldownMult * (up != null ? up.MasochismUp() : 1f);
            if(instanadeHitOpponent){cooldownMult*=3;}
            cooldownTimers[slotIndex] = cooldownTimers[slotIndex] / cooldownMult;
        }

    public void SetIndecisive(int slotIndex)
    {
        if (up.indecisiveUpgrade>0){
            StartCoroutine(IndecisiveTimer(indecisiveTime, slotIndex));}
    }
    public IEnumerator IndecisiveTimer(float time, int slotIndex)
        {
            InventoryUI invUI = gameObject.GetComponent<InventoryUI>();

            indecisiveWeapons[slotIndex] = 1;          
            invUI.IndecisiveBorder(slotIndex, true);
            yield return new WaitForSeconds(time);
            indecisiveWeapons[slotIndex] = 0;
            invUI.IndecisiveBorder(slotIndex, false);
        }

    public IEnumerator NadeSpamPreventer(float cooldown)    //A 0.5 second delay on throwing any grenade after a grenade is thrown.
        {
            canThrow = false;
            yield return new WaitForSeconds(cooldown);
            canThrow = true;
        }

    public IEnumerator NoInstantlyThrowSecondC4(float cooldown)    //A tiny delay on throwing a c4 after a c4 is detonated, so you don't do both at same time
        {
            secondC4Lockout = true;
            yield return new WaitForSeconds(cooldown);
            secondC4Lockout = false;
        }



    //This method checks one specific slot in the inventory:
    public bool IsSlotEmpty(int index)
        {
            return slots[index] == null; //returns true if slot is empty, false otherwise
        }

    //This method checks all 4 slots to see if any of them is empty:
    public bool HasAnyEmptySlot()   
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null)
                    return true;
                    //If this slot is empty → it immediately returns true
                    //The search stops early because we already know “yes, at least one slot is empty”
            }
            return false;   //otherwise, return false
        }


public IEnumerator CookFrag(GrenadeStats weapon, int slotIndex)
{
    canThrow = false;  
    fpc.cookingFrag = true;
    float timeCooked = 0f; // seconds frag has been held
    float maxCookTime = weapon.duration;    //time before it explodes

    cookFragSlider.Show();
    cookFragSlider.SetNormalized(0f); // full fuse at start
        

    // Determine which throw button corresponds to this slot
    Func<bool> throwButtonHeld = slotIndex switch
    {
        0 => () => pController.Throw1Triggered,
        1 => () => pController.Throw2Triggered,
        2 => () => pController.Throw3Triggered,
        3 => () => pController.Throw4Triggered,
        _ => () => false
    };

    // While player holds the button, or 0.1 seconds before it would explode
    while (throwButtonHeld() && timeCooked<maxCookTime-0.05f && !fpc.inUpgradeMenu)
    {
        timeCooked += Time.deltaTime;
        float normalized = timeCooked / maxCookTime;
        cookFragSlider.SetNormalized(normalized);
        if (!fragTickAudioSource.isPlaying){
            fragTickAudioSource.clip = tickingFragClip;
            fragTickAudioSource.loop = true;
            fragTickAudioSource.Play(); //Todo need to change this so that it doesn't overlap with music
        }
        yield return null;
    }

    cookFragSlider.Hide();
    canThrow = true;
    fpc.cookingFrag = false;

    if (fragTickAudioSource.isPlaying){
        fragTickAudioSource.Stop();
        fragTickAudioSource.clip = null;}

    Throw(weapon, slotIndex, timeCooked);
}

}


