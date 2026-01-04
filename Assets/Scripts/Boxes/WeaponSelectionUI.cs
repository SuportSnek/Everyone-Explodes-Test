using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using TMPro;
using System;

//Every player prefab has its own copy of this UI.
//This script handles:
    //Displaying choices
    //Handling navigation (left/right)
    //Showing Red Xs
    //Sending confirmation back to the WeaponBox
    //Being locked to a particular player


public class WeaponSelectionUI : MonoBehaviour
{
    private PlayerController pController;
    private FirstPersonController fpc;

    public GameObject WhiteBackGround;
    public GameObject RedBackGround1;

    [SerializeField] TextMeshProUGUI chooseYourNade;
    [SerializeField] TextMeshProUGUI howToAssign;
    [SerializeField] TextMeshProUGUI SelectAnEmptySlot;
    
    private GameObject player;  

    private bool isOpen = false;

    private GrenadeStats choice;

    // We'll store the callback the WeaponBox gave us. It should return true when the box accepted the selection.
    // signature: (playerGameObject, choiceIndex, invSlot) => bool success
    private Func<GameObject, int, bool> confirmCallback;


    void Awake()
    {
        WhiteBackGround.SetActive(false);
        RedBackGround1.SetActive(false);
        chooseYourNade.enabled = false;
        howToAssign.enabled = false;
        SelectAnEmptySlot.enabled = false;
    }


    void Update()
    {
        if (pController == null) return;

        if (pController.Throw1PressedThisFrame) ConfirmSelection(0);
        if (pController.Throw2PressedThisFrame) ConfirmSelection(1);
        if (pController.Throw3PressedThisFrame) ConfirmSelection(2);
        if (pController.Throw4PressedThisFrame) ConfirmSelection(3);
    }




    public void OpenUI(GameObject player, GrenadeStats choice, Func<GameObject, int, bool> onConfirm)
    {
        this.player = player;
        this.choice = choice;
        this.confirmCallback = onConfirm;

        pController = player.GetComponent<PlayerController>();
        fpc = player.GetComponent<FirstPersonController>();

        fpc.inWeaponMenu = true;

        WhiteBackGround.SetActive(true);
        chooseYourNade.enabled = true;
        howToAssign.enabled = true;

        // Clean + spawn single icon
        foreach (Transform child in WhiteBackGround.transform)
            Destroy(child.gameObject);

        if (choice.weaponMenuUIPrefab != null)
            Instantiate(choice.weaponMenuUIPrefab, WhiteBackGround.transform);

        isOpen = true;
    }




private void ConfirmSelection(int invSlot)
{
    if (confirmCallback == null)
        return;

    bool accepted = confirmCallback.Invoke(player, invSlot);

    if (accepted)
        Debug.Log("Weapon assigned");
}



    public void NotifyMustPickEmptySlot()
    {
        SelectAnEmptySlot.enabled = true;
    }

    public void ForceClose()    //!remove?!
    {
        CloseForPlayer();
    }

    private void CloseForPlayer()
    {
        confirmCallback = null;
        player = null;

        fpc.inWeaponMenu = false;
        StartCoroutine(fpc.WasJustInMenu());
        
        WhiteBackGround.SetActive(false);
        isOpen = false;
        RedBackGround1.SetActive(false);
        chooseYourNade.enabled = false;
        howToAssign.enabled = false;
        SelectAnEmptySlot.enabled = false;
    }


    public bool IsOpenForPlayer(GameObject forPlayer)
    {
        return isOpen && player == forPlayer;   //“Is the UI open, and is it open FOR THIS player?”
    }
    //player == forPlayer
    //Each UI stores which player it belongs to, like this:
    //private GameObject player;
    //When you open the UI, you set:
    //this.player = thePlayerWhoOpenedIt;
    //So, player → the player this UI instance is assigned to
    //forPlayer → the player being checked by WeaponBox in the loop
    
}
