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


public class UpgradeSelectionUI : MonoBehaviour
{
    private PlayerController pController;
    private FirstPersonController fpc;

    public GameObject panel;

    public GameObject UpgradeTitle;
    public GameObject WhiteBackGround;
    public GameObject RedBackGround1;
    public GameObject RedBackGround2;
    public GameObject RedBackGround3;
    public GameObject RedX1;
    public GameObject RedX2;
    public GameObject RedX3;
    public GameObject youDidntCapture;
    
    private int selectedIndex = 0; // 0 = left, 1 = middle, 2 = right
    private float navCooldown = 0f;

    private GameObject player;  

    //public GameObject[] optionButtons = new GameObject[2];
    public GameObject[] optionButtons; // assign 3 option containers in inspector
    private UpgradeData[] choices;


    private bool isOpen = false;

    private bool[] takenOptions;

    // We'll store the callback the WeaponBox gave us. It should return true when the box accepted the selection.
    // signature: (playerGameObject, choiceIndex, invSlot) => bool success
    private System.Func<GameObject,int,bool> confirmCallback;


    void Awake()
    {
        panel.SetActive(false);
        UpgradeTitle.SetActive(false);
        WhiteBackGround.SetActive(false);
        RedBackGround1.SetActive(false);
        RedBackGround2.SetActive(false);
        RedBackGround3.SetActive(false);
        RedX1.SetActive(false);
        RedX2.SetActive(false);
        RedX3.SetActive(false);
        youDidntCapture.SetActive(false);
    }


    void Update()
    {
        if (!panel.activeSelf || pController == null) return;
        navCooldown -= Time.deltaTime;

        Vector2 moveInput = pController.MovementInput; // now works because we got it from the player


                if (navCooldown <= 0f)  
        {
            if (moveInput.x > 0.5f) 
            {
                selectedIndex = Mathf.Min(selectedIndex + 1, 2);
                UpdateSelectionVisual();
                navCooldown = 0.2f; //Time between inputs allowed
                //Debug.Log(selectedIndex);
            }
            else if (moveInput.x < -0.5f) 
            {
                selectedIndex = Mathf.Max(selectedIndex - 1, 0);
                UpdateSelectionVisual();
                navCooldown = 0.2f;
                //Debug.Log(selectedIndex);
            }
        }

        if ((pController.InteractPressedThisFrame || pController.Throw1PressedThisFrame) && navCooldown <= -0.01f)  //Nav cooldown must be any number BELOW ZERO for some reason
        {
            OnChooseIndex(selectedIndex);
        }
    }



    public void OpenUI(GameObject player, UpgradeData[] choices, bool[] takenOptions, System.Func<GameObject,int,bool> onConfirm)
    {
        this.player = player;
        this.choices = choices;
        this.confirmCallback = onConfirm;
        
        pController = player.GetComponent<PlayerController>();  // <-- get PlayerController dynamically
        fpc = player.GetComponent<FirstPersonController>(); // <-- get FPC dynamically

        fpc.inUpgradeMenu = true;

        panel.SetActive(true);
        WhiteBackGround.SetActive(true);
        UpgradeTitle.SetActive(true);
        //RedX1.SetActive(false);
        //RedX2.SetActive(false);

        for (int i = 0; i < optionButtons.Length && i < choices.Length; i++)
        {
            // clear previous
            foreach (Transform t in optionButtons[i].transform) Destroy(t.gameObject);

            // place icon or prefab
            if (choices[i].UpgradeMenuUIPrefab != null)
            {
                Instantiate(choices[i].UpgradeMenuUIPrefab, optionButtons[i].transform);
            }
            else if (choices[i].icon != null)
            {
                // simple icon
                var img = optionButtons[i].GetComponentInChildren<UnityEngine.UI.Image>();
                if (img != null) img.sprite = choices[i].icon;
            }
        }

        selectedIndex = 0;
        UpdateSelectionVisual();
        isOpen = true;
    }


    private void UpdateSelectionVisual()        //!Note: CAn be changed if I want to use this same function for Obelisk.
    {   
        // Reset all buttons
        RedBackGround1.SetActive(false);
        RedBackGround2.SetActive(false);
        RedBackGround3.SetActive(false);

        // Highlight selected button
        switch (selectedIndex)
        {
            case 0: RedBackGround1.SetActive(true); break;

            case 1:RedBackGround2.SetActive(true); break;

            case 2: RedBackGround3.SetActive(true); break;
        }
    }


    public void UpdateTakenOptions(bool[] taken)
    {
        this.takenOptions = taken;
        for (int i = 0; i < optionButtons.Length && i < taken.Length; i++)
        {
            var btn = optionButtons[i].GetComponent<UnityEngine.UI.Button>();
            if (btn != null) btn.interactable = !taken[i];
            // optionally show a red X child object depending on taken[i]
        }
        ShowFuckingRedX();
    }

    // Call this from buttons wired up in inspector (Button OnClick)
    public void OnChooseIndex(int index)    {
        if (confirmCallback == null) { Debug.LogError("No confirm callback"); return; }
        bool accepted = confirmCallback.Invoke(player, index);
        if (accepted)
        {
             Debug.Log("ConfirmSelection() accepted");
        } 
        else NotifySelectionFailed(index);}


    public void ShowFuckingRedX()
    {
        // Turn them all off first
        RedX1.SetActive(false);
        RedX2.SetActive(false);
        RedX3.SetActive(false);


        // Enable based on taken[]
        if (takenOptions.Length > 0 && takenOptions[0] == true){
            RedX1.SetActive(true);
            }
        
        if (takenOptions.Length > 1 && takenOptions[1] == true)
        {
            RedX2.SetActive(true);
        }
    
        if (takenOptions.Length > 2 && takenOptions[2] == true)
            RedX3.SetActive(true);

    }


    public void NotifySelectionFailed(int choiceIndex)
    {
        // Visual/sound feedback that the pick failed because it was already taken.
        Debug.Log("Selection failed — option already taken: " + choiceIndex);
        // TODO: play sound
    }

    public void NotifyWaitForFirstTaker()
    {
        // Replace with UI feedback you want (sound, temporary message, etc.)
        youDidntCapture.SetActive(true);
    }

    public void ForceClose()    //!remove?!
    {
        CloseForPlayer();
    }

    private void CloseForPlayer()
    {
        if (fpc != null){fpc.inUpgradeMenu = false;}
        fpc.cappingObelisk = false; //unneccesary?
        StartCoroutine(fpc.WasJustInMenu());
        panel.SetActive(false);
        WhiteBackGround.SetActive(false);
        UpgradeTitle.SetActive(false);
        isOpen = false;
        RedBackGround1.SetActive(false);
        RedBackGround2.SetActive(false);
        RedBackGround3.SetActive(false);
        RedX1.SetActive(false);
        RedX2.SetActive(false);
        RedX3.SetActive(false);
        youDidntCapture.SetActive(false);
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
