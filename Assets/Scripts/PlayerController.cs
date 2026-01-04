using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//!This script basically converts input actions → readable variables.

public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;
    //Exposes a private field in the Inspector so you can drag in an .inputactions asset. InputActionAsset is the container for "Action Maps" (like Gameplay)

//Private variables that will store the InputAction objects returned from FindAction. These are used to subscribe to input events and read values.
    private InputAction movementAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction interactAction;
    private InputAction throw1Action;
    private InputAction throw2Action;
    private InputAction throw3Action;
    private InputAction throw4Action;

    //holds inputs that other scripts can read from
    public Vector2 MovementInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool JumpTriggered { get; private set; }
    public bool InteractTriggered { get; private set; }
    public bool Throw1Triggered { get; private set; }
    public bool Throw2Triggered { get; private set; }
    public bool Throw3Triggered { get; private set; }
    public bool Throw4Triggered { get; private set; }

    //These are PRESS inputs, good for booleans (happen once, then it's done)
    public bool JumpPressedThisFrame => jumpAction != null && jumpAction.triggered;
    public bool InteractPressedThisFrame => interactAction != null && interactAction.triggered;  //Somehow (not sure how) this lets you press Interact to Interact only a single time, not multiple times per frame
    public bool Throw1PressedThisFrame => throw1Action != null && throw1Action.triggered;
    public bool Throw2PressedThisFrame => throw2Action != null && throw2Action.triggered;
    public bool Throw3PressedThisFrame => throw3Action != null && throw3Action.triggered;
    public bool Throw4PressedThisFrame => throw4Action != null && throw4Action.triggered;

    private void Awake()    //happens before Start
    {
        playerInput = GetComponent<PlayerInput>();

        var gameplayMap = playerInput.actions.FindActionMap("Player");

        movementAction = gameplayMap.FindAction("Movement");
        lookAction = gameplayMap.FindAction("Look");
        jumpAction = gameplayMap.FindAction("Jump");
        interactAction = gameplayMap.FindAction("Interact");
        throw1Action = gameplayMap.FindAction("Throw1");
        throw2Action = gameplayMap.FindAction("Throw2");
        throw3Action = gameplayMap.FindAction("Throw3");
        throw4Action = gameplayMap.FindAction("Throw4");

        SubscribeActionValuesToInputEvents();
    }

    private void Update()
    {
        //InteractPressedThisFrame = false;
    }




    private void SubscribeActionValuesToInputEvents()
    {
        //movementAction.performed: called every time our movement action changes.
        //inputInfo: "+= subscribe. It's our context" 
        //Our movement input is then turned into the value that's passed in with our context.
        //In other words: .performed: called when the action operation is considered performed (for value-type actions this happens when a value is read). The lambda reads the Vector2 value from the callback context and stores it in MovementInput.
        movementAction.performed += inputInfo => MovementInput = inputInfo.ReadValue<Vector2>();    //For example, W would pass in 0, 1. S would be 0, -1.
        movementAction.canceled += inputInfo => MovementInput = Vector2.zero;    //Set back to 0 when no input detected.
        //"Whenever movement input changes (stick moves, WASD pressed), set MovementInput = new vector."

        lookAction.performed += inputInfo => LookInput = inputInfo.ReadValue<Vector2>();
        lookAction.canceled += inputInfo => LookInput = Vector2.zero;

        jumpAction.performed += inputInfo => JumpTriggered = true;
        jumpAction.canceled += inputInfo => JumpTriggered = false;

        throw1Action.performed += inputInfo => Throw1Triggered = true;
        throw1Action.canceled += inputInfo => Throw1Triggered = false;

        throw2Action.performed += inputInfo => Throw2Triggered = true;
        throw2Action.canceled += inputInfo => Throw2Triggered = false;

        throw3Action.performed += inputInfo => Throw3Triggered = true;
        throw3Action.canceled += inputInfo => Throw3Triggered = false;

        throw4Action.performed += inputInfo => Throw4Triggered = true;
        throw4Action.canceled += inputInfo => Throw4Triggered = false;

        interactAction.performed += inputInfo => InteractTriggered = true;
        interactAction.canceled += inputInfo => InteractTriggered = false;

        //interactAction.performed += inputInfo => InteractHeld = true;
        //interactAction.canceled += inputInfo => InteractHeld = false;
    }



    /*
    public void OnPlayerJoined()
    {
        Debug.Log($"Player joined: {playerInput.playerIndex}");

        var cam = playerInput.GetComponentInChildren<Camera>();
        if (cam != null)
        {
            int displayIndex = Mathf.Clamp(playerInput.playerIndex, 0, Display.displays.Length - 1);
            cam.targetDisplay = displayIndex;
            Debug.Log($"🎥 Assigned Player {playerInput.playerIndex + 1} to Display {displayIndex + 1}");
        }
        else
        {
            Debug.LogWarning($"⚠️ No camera found for player {playerInput.playerIndex}");
        }
    }*/


/*
    private void OnEnable()
    {
        playerControls.FindActionMap(actionMapName).Enable();
    }
    
    private void OnDisable()
    {
        playerControls.FindActionMap(actionMapName).Disable();
    }*/
}
