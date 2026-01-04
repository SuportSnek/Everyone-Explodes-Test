using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 40f;
    [SerializeField] float airAcceleration = 2.5f;
    [SerializeField] float explodeAcceleration = 1f;
    [SerializeField] private float jetpackAcceleration = 10f;
    [SerializeField] private float jumpForce = 6.5f;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] public float controllerSensitivityMultiplier = 11f;
    [SerializeField] private float groundDrag = 5f; //Higher Values slows the Rigidbody faster.
    [SerializeField] private float airDrag = 0.5f;
    [HideInInspector] private float acceleration = 1f;
    [HideInInspector] public float gravityMultiplier = 1f;

    [Header("References")]  //Test if these can be changed to not be serializedfields
    [SerializeField] public Camera mainCamera;
    [SerializeField] public PlayerController pController;
    [SerializeField] public PlayerInput playerInput;    
    [SerializeField] public UpgradesScript up;    
    private Rigidbody rb;
    private HoverController hover;

    [Header("In Menu")]
    [HideInInspector] public bool wasJustInMenu = false;  //Hear so that, after player clicks on menu option, they don't instantly throw a grenade
    public bool inUpgradeMenu = false;
    public bool inWeaponMenu = false;
    public bool cappingObelisk = false;
    public bool InteractionLocked { get; private set; }

    [Header("Explosion stuff")]
    public bool beingExploded = false;
    [HideInInspector] public bool checkIfBeingExploded = false;
    public GameObject lastExplosionCauser;  //used for Suck Pits

    [Header("Debuffs")]
    public bool inverseCurse = false;
    public float invCurseStrength = 1;
    public bool frozenCurse = false;

    [HideInInspector] public bool hasJetpack = false;
    [HideInInspector] public bool cookingFrag = false;

    private float verticalRotation;
    public bool fpcGrounded = false;    //hover.isGrounded, but for scripts that use FirstPersonController

    public float didYouJustJump = 0f;   //Exclusively for Double Jump Upgrade
    public bool showJumpAnimation = false;

    public GrenadeStats startingWeapon1; // assign via inspector
    //public GrenadeStats startingWeapon2; // These 3 are for testing
    //public GrenadeStats startingWeapon3; // 
    //public GrenadeStats startingWeapon4; // 

    private void Awake()
    {
    
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var inv = GetComponent<WeaponInventory>();
        rb = GetComponent<Rigidbody>();
        hover = GetComponent<HoverController>();    //todo I think this is a replacement for using [SerializeField]? When can I use one and not other?

        inv.SetWeapon(0, startingWeapon1);
        //inv.SetWeapon(1, startingWeapon2);
        //inv.SetWeapon(2, startingWeapon3);
        //inv.SetWeapon(3, startingWeapon4);
        var ui = GetComponentInChildren<InventoryUI>();
        ui.SetSlot(startingWeapon1, 0);
        //ui.SetSlot(startingWeapon2, 1);
        //ui.SetSlot(startingWeapon3, 2);
        //ui.SetSlot(startingWeapon4, 3);

        rb.freezeRotation = true; // prevent physics tilt
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();   
        Throw1();
        Throw2();
        Throw3();
        Throw4();
        WhenLastJumped();
    }

    private void FixedUpdate() // Because this is consistent across frame rates, use this for phsycis!
    {
        HandleMovement();
        ApplyGravityMultiplier();
        CheckGroundedExplodedStatus();
        HandleJumping();
        rb.linearDamping = hover.isGrounded ? groundDrag : airDrag; //This gives a natural stop when you release movement. Drag doesn’t affect vertical motion. 

        //if (!hover.isGrounded)
        //{
            //up.SizeUpgrades();
        //}
    }


    private void HandleMovement()
    {
        if (inUpgradeMenu || inWeaponMenu || cappingObelisk || frozenCurse){
            return;} 
        Vector3 inputDirection = new Vector3(pController.MovementInput.x, 0f, pController.MovementInput.y); //Where you want to go
        Vector3 currentHorizontalVelocity = CurrentHorizantalVelocity();
        Vector3 desiredVelocity = DesiredVelocity();
        Vector3 velocityChange = desiredVelocity - currentHorizontalVelocity;

        if (beingExploded && !hasJetpack)      //If being exploded, you have 0.03x air control
        {
            Vector3 explodeMove = transform.TransformDirection(inputDirection.normalized);
            rb.AddForce(explodeMove * explodeAcceleration, ForceMode.Acceleration);
        }
        else if (hasJetpack && !hover.isGrounded)  //Have jetpack
        {
            Vector3 jetpackMove = transform.TransformDirection(inputDirection.normalized);
            rb.AddForce(jetpackMove * jetpackAcceleration, ForceMode.Acceleration);
            //Todo velocityChange = Vector3.ClampMagnitude(velocityChange, maxAirAccel);
            //Todo Maybe add above code, cause it prevents bug: currentHorizontalVelocity increases, but your desiredVelocity stays the same, velocityChange increases
        }
        else if (hover.isGrounded && inputDirection.magnitude > 0.1f)  //Normal movement. If grounded & trying to move (If no input detected, no physics applied)
        {
            rb.AddForce(velocityChange * acceleration, ForceMode.Acceleration);
            //Lets say you're moving at 5, and desire to move at 10. 10 - 5 = 5 movement added to your speed
        }
        else if (!hover.isGrounded) //Normal air movement, jumping/falling
        {
            Vector3 airMove = transform.TransformDirection(inputDirection.normalized);
            rb.AddForce(airMove * airAcceleration, ForceMode.Acceleration); 
        }
        Debug.Log("weird movement thing happened, fix");
        //Debug.Log(velocityChange, gameObject);
    }

    public Vector3 CurrentHorizantalVelocity()
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        return horizontalVelocity;
    }


    public Vector3 DesiredVelocity()
    {
        float finalWalkStrength = walkSpeed;
        finalWalkStrength = finalWalkStrength * (up != null ? up.RunFastUp() : 1f);
        finalWalkStrength = finalWalkStrength * (up != null ? up.SpeedScalerUp() : 1f);
        //finalWalkStrength = finalWalkStrength * (up != null ? up.SizeDownUp() : 1f);

        Vector3 inputDirection = new Vector3(pController.MovementInput.x, 0f, pController.MovementInput.y); //Where you want to go
        Vector3 moveDirection = transform.TransformDirection(inputDirection.normalized);    //converts local direction (Relative to player) into world space.

        if (hover.isGrounded && inputDirection.magnitude > 0.1f){
            moveDirection = Vector3.ProjectOnPlane(moveDirection, hover.GroundNormal).normalized;
        }

        if (!hover.isGrounded && hasJetpack==false)  //If AIRBORNE, you can't move forward/backwards, only left/right
        {
            inputDirection = new Vector3(pController.MovementInput.x, 0f, 0f); 
            moveDirection = transform.TransformDirection(inputDirection.normalized);    
            Vector3 desiredVelocityAirborne = moveDirection * finalWalkStrength;
            return desiredVelocityAirborne;
        }
        Vector3 desiredVelocity = moveDirection * finalWalkStrength;
        //Debug.Log("finalWalkStrength");
        return desiredVelocity;
    }


    public void ApplyPush(Vector3 pushVector, GameObject causer)
    {
        if (inUpgradeMenu){
            return;} 

        Vector3 finalPushVector = pushVector;
//Debug.Log("pushVectorBefore"+pushVector);
        lastExplosionCauser = causer; 
        beingExploded = true;
        StartCoroutine(checkIfStillBeingExploded(0.3f));

        if (inverseCurse && causer == gameObject){  //Inverse knockback, and apply extra knockback
            finalPushVector.x *=-invCurseStrength;
            finalPushVector.z *=-invCurseStrength;}

//Todo the next 3 if statements are my attempt at making a minimum knockback mechanic. Prevents cases where players explode but go basically nowhere due to friction/weird angle. Buggy as fuck though.
        //if Y is extremely negative, and X/Z are really small, increase X/Z. This is for cases like Fastball hitting head, shooting mostly downward
        if (hover.isGrounded && finalPushVector.y<-10 && causer != gameObject){
           if(finalPushVector.x<3 && finalPushVector.x>-3 && finalPushVector.z<3 && finalPushVector.z>-3){  //if both X and Z are between -3 and 3
                {finalPushVector.x*=3f;}
                {finalPushVector.z*=3f;}
                Debug.Log("test2");
            }
        }

        //if Y is extremely positive, and X/Z are really small, increase X/Z. This is for cases like Fastball hitting feet, shooting mostly upwards
        if (hover.isGrounded && finalPushVector.y>10 && causer != gameObject){
           if(finalPushVector.x<3 && finalPushVector.x>-3 && finalPushVector.z<3 && finalPushVector.z>-3){  //if both X and Z are between -3 and 3
                {finalPushVector.x*=3f;}
                {finalPushVector.z*=3f;}
                Debug.Log("test1");
            }
        }

        if (hover.isGrounded && finalPushVector.y<5f && causer != gameObject){   
            Debug.Log("test3");
            finalPushVector.y = 5f;}

        rb.AddForce(finalPushVector, ForceMode.Impulse); //FLYYY
        Debug.Log("Explosion Push Vector"+finalPushVector);
    }

    private IEnumerator checkIfStillBeingExploded(float time)   //Todo does this do anything?
    {
        checkIfBeingExploded = false;
        yield return new WaitForSeconds(time);
        checkIfBeingExploded = true;
    }

    public void ApplyInverseCurse(float strength, float duration)
    {
        StartCoroutine(InverseCurseTimer(duration));
        invCurseStrength = strength;
    }

    private IEnumerator InverseCurseTimer(float time)
    {
        if (!inUpgradeMenu)
        {
            inverseCurse = true;
            PlayerHealth pHealth = gameObject.GetComponent<PlayerHealth>();
            pHealth.ApplyInverseCurseGraphic(true);
            yield return new WaitForSeconds(time);
            inverseCurse = false;
            invCurseStrength = 1;
            pHealth.ApplyInverseCurseGraphic(false);
        }
    }

    public void ApplyFrozenCurse(float frozenTime)
    {
        StartCoroutine(FrozenCurseTimer(frozenTime));
    }

    private IEnumerator FrozenCurseTimer(float time)
    {
        if (!inUpgradeMenu)
        {
            frozenCurse = true;
            PlayerHealth pHealth = gameObject.GetComponent<PlayerHealth>();
            pHealth.ApplyFrozenCurseGraphic(true);
            yield return new WaitForSeconds(time);
            frozenCurse = false;
            pHealth.ApplyFrozenCurseGraphic(false);
        }
    }

    public void TriggerWasJustInMenu()  //Stupid function needed by WeaponDrop (explaination there, don't delete this future me)
        {
            StartCoroutine(WasJustInMenu());
            return;
        }
    public IEnumerator WasJustInMenu()
    {
        wasJustInMenu = true;
        yield return new WaitForSeconds(0.3f);
        wasJustInMenu = false;
    }

    public void LockInteraction(){  //used by ObeliskScript, so that capping obelisk overrights opening weapon menu
        InteractionLocked = true;}

    public void UnlockInteraction(){
        InteractionLocked = false;}

    private void WhenLastJumped()   //Exclusively for Double Jump Upgrade
    {
        if(pController.JumpPressedThisFrame){
            didYouJustJump += Time.deltaTime;}
    }
    //How this stupid piece of code above works:
    //When you press jump, it makes didYouJustJump>0 until you jump
    //This is required, for some reason. Otherwise, pressing jump will actually press jump not on a real frame, cancelling the input

    private void HandleJumping()
    {
        if (inUpgradeMenu || inWeaponMenu || frozenCurse){
            return;} 

        float finalJumpForce = jumpForce * (up != null ? up.JumpHighUp() : 1f);

        if (didYouJustJump>0 && up.doubleJumpUpgrade>0 && up.doubleJumpRecharged==true && !hover.isGrounded)  //Double Jump Upgrade
        {
            var nThrower = GetComponent<NadeThrower>();
            up.ApplyDoubleJumpCooldown();
            nThrower.SpawnAndThrow(up.doubleJumpStats, -1, 0);
            didYouJustJump = 0f;
        }
        
        if (pController.JumpTriggered && hover.isGrounded)  //Normal Jump
        {
            lastExplosionCauser = null; //This is really dumb, but it clears lastExplosionCauser when you jump. Used for the Combo Upgrade.
            //Basically, because lastExplosionCauser is cleared when you jump, and the Combo Upgrade works while airborne, it all works.

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // reset Y
            rb.AddForce(Vector3.up * finalJumpForce, ForceMode.Impulse);
            didYouJustJump = 0f;
            //GetComponentInChildren<PAnimationController>()?.TriggerJump();  //Todo no longer used
            showJumpAnimation = true;
       }

       if (pController.JumpTriggered && hasJetpack) //Jetpack
        {
            var inv = GetComponent<WeaponInventory>();
            for (int i = 0; i < inv.slots.Length; i++)
            {
                var weapon = inv.slots[i];
                if (weapon == null) continue; //without this, code hits a null and explodes
                if (weapon.GrenadeName == "Jetpack"){
                    inv.TryThrowSlot(i);
                }
            }
        }
    }

    private void CheckGroundedExplodedStatus()
    {
        fpcGrounded = hover.isGrounded; //for other scripts that already reference FPC

        //Todo figure out which of these are needed, and how to improve them
        if (!hover.isGrounded){
            showJumpAnimation = false;}
        if (hover.isGrounded && !checkIfBeingExploded){ //I THINK this means that you're grounded for at least 2 FixedUpdates?
            showJumpAnimation = false;}
        
        if (hover.isGrounded && checkIfBeingExploded) //I THINK this means that you're grounded for at least 2 FixedUpdates?
        {
            beingExploded = false;
        }
    }

    private void ApplyGravityMultiplier()
    {
        //float finalGravMultiplier = gravityMultiplier * (up != null ? up.SizeUpUp() : 1f);

        if (!hover.isGrounded)
        {
            //rb.AddForce(Physics.gravity * finalGravMultiplier, ForceMode.Acceleration);
            rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);//Normal Gravity
        }
    }

    private void HandleRotation()
    {
        if (inUpgradeMenu|| inWeaponMenu || frozenCurse){
            return;} 
        
        float sensitivity = (playerInput.currentControlScheme == "Gamepad") ? mouseSensitivity * controllerSensitivityMultiplier : mouseSensitivity;
        //“If Gamepad is the current controlscheme, use mouseSensitivity * controllerSensitivityMultiplier. Otherwise, just use mouseSensivity"

        float XRotation = pController.LookInput.x * sensitivity;
        float YRotation = pController.LookInput.y * sensitivity;

        transform.Rotate(0, XRotation, 0);

        verticalRotation = Mathf.Clamp(verticalRotation - YRotation, -89f, 89f);    //Max Distance you're allowed to look up or down
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void Throw1()
    { 
        if (pController.Throw1Triggered){
            GetComponent<WeaponInventory>().TryThrowSlot(0);
        }
    }
    private void Throw2()
    { 
        if (pController.Throw2Triggered){
            GetComponent<WeaponInventory>().TryThrowSlot(1);
        }
    }
    private void Throw3()
    { 
        if (pController.Throw3Triggered){
            GetComponent<WeaponInventory>().TryThrowSlot(2);
        }
    }
    private void Throw4()
    { 
        if (pController.Throw4Triggered){
            GetComponent<WeaponInventory>().TryThrowSlot(3);
        }
    }
}
