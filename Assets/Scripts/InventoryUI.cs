using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.VisualScripting.FullSerializer;

public class InventoryUI : MonoBehaviour
{
    //public GameObject panel;
    public GameObject[] activeSlots = new GameObject[4];
    //This stores the currently spawned UI for each slot. 0 = slot 1, 1 = slot 2, etc.
    public GameObject[] upgradeUI = new GameObject[2];
    //0 = double jump

    [SerializeField] TextMeshProUGUI cooldown1;
    [SerializeField] TextMeshProUGUI cooldown2;
    [SerializeField] TextMeshProUGUI cooldown3;
    [SerializeField] TextMeshProUGUI cooldown4;
    [SerializeField] TextMeshProUGUI doubleJumpCooldown;
    private Vector2 cooldown1DefaultPos;
    private Vector2 cooldown2DefaultPos;
    private Vector2 cooldown3DefaultPos;
    private Vector2 cooldown4DefaultPos;
    private Vector2 doubleJumpDefaultPos;
    [SerializeField] private RectTransform cooldown1Rect;
    [SerializeField] private RectTransform cooldown2Rect;
    [SerializeField] private RectTransform cooldown3Rect;
    [SerializeField] private RectTransform cooldown4Rect;
    [SerializeField] private RectTransform doubleJumpRect;
    public Image indecisiveBorder1;
    public Image indecisiveBorder2;
    public Image indecisiveBorder3;
    public Image indecisiveBorder4;

    public Image ControllerUI1;
    public Image ControllerUI2;
    public Image ControllerUI3;
    public Image ControllerUI4;

    [SerializeField] TextMeshProUGUI UpObSpawned;

    [Header("References")]
    [SerializeField] private WeaponInventory weaponInv;
    [SerializeField] public UpgradesScript up;

    void Awake()
    {
         Debug.Assert(ControllerUI1 != null, "ControllerUI1 not assigned");
    Debug.Assert(cooldown1Rect != null, "cooldown1Rect not assigned");

        ControllerUI1.gameObject.SetActive(false);
        ControllerUI2.gameObject.SetActive(false);
        ControllerUI3.gameObject.SetActive(false);
        ControllerUI4.gameObject.SetActive(false);
        indecisiveBorder1.gameObject.SetActive(false);
        indecisiveBorder2.gameObject.SetActive(false);
        indecisiveBorder3.gameObject.SetActive(false);
        indecisiveBorder4.gameObject.SetActive(false);

        //For cooldowns >9
        cooldown1DefaultPos = cooldown1Rect.anchoredPosition;
        cooldown2DefaultPos = cooldown2Rect.anchoredPosition;
        cooldown3DefaultPos = cooldown3Rect.anchoredPosition;
        cooldown4DefaultPos = cooldown4Rect.anchoredPosition;
        doubleJumpDefaultPos = doubleJumpRect.anchoredPosition;

        UpObSpawned.gameObject.SetActive(false);
    }

    void Update()
    {
        WeaponCooldowns1();
        WeaponCooldowns2();
        WeaponCooldowns3();
        WeaponCooldowns4();
        DoubleJumpCooldown();
    }


public void WeaponCooldowns1(){
    if (weaponInv.cooldownTimers[0] > 0){   //if a cooldown timer isn't zero
        cooldown1.gameObject.SetActive(true);   //show cooldown timer
        DisplayTime1(weaponInv.cooldownTimers[0]);
        
        if (weaponInv.cooldownTimers[0] > 9)   //if cooldown timer > 9
            cooldown1Rect.anchoredPosition = cooldown1DefaultPos + new Vector2(-50f, 0);    //Show shifted cooldown timer
        else
            cooldown1Rect.anchoredPosition = cooldown1DefaultPos;   //then once it hits <=9 again, reset back to original position
    }
    if (weaponInv.cooldownTimers[0]==0){    //if nade isn't on cooldown, show nothing
        cooldown1.gameObject.SetActive(false);
        cooldown1Rect.anchoredPosition = cooldown1DefaultPos;
    }
    if (weaponInv.specialWeaponDurations[0] > 0){  //if Special weapon is active
            cooldown1.gameObject.SetActive(true);   
            DisplayTime1(weaponInv.specialWeaponDurations[0]); //show it's duration
            
            if (weaponInv.specialWeaponDurations[0] > 9)   //if special weapon cooldown timer > 9
                cooldown1Rect.anchoredPosition = cooldown1DefaultPos + new Vector2(-50f, 0); //Show shifted cooldown timer
            else
                cooldown1Rect.anchoredPosition = cooldown1DefaultPos;   //once it hits <=9 again, reset back to original position
    } 
    if (weaponInv.specialWeaponDurations[0]==-1 && weaponInv.slots[0]==null){    //special weapon expires
        cooldown1.gameObject.SetActive(false);  //remove it's cooldown from screen
        cooldown1Rect.anchoredPosition = cooldown1DefaultPos;}
    }
public void WeaponCooldowns2(){
    if (weaponInv.cooldownTimers[1] > 0){   //if a cooldown timer isn't zero
        cooldown2.gameObject.SetActive(true);   //show cooldown timer
        DisplayTime2(weaponInv.cooldownTimers[1]);
        
        if (weaponInv.cooldownTimers[1] > 9)   //if cooldown timer > 9
            cooldown2Rect.anchoredPosition = cooldown2DefaultPos + new Vector2(-50f, 0);
        else
            cooldown2Rect.anchoredPosition = cooldown2DefaultPos;   //once it hits <=9 again, reset back to original position
    }
    if (weaponInv.cooldownTimers[1]==0){    //otherwise, show nothing
        cooldown2.gameObject.SetActive(false);
        cooldown2Rect.anchoredPosition = cooldown2DefaultPos;
    }
    if (weaponInv.specialWeaponDurations[1] > 0){  //Special weapon is active
            cooldown2.gameObject.SetActive(true);
            DisplayTime2(weaponInv.specialWeaponDurations[1]);
            
            if (weaponInv.specialWeaponDurations[1] > 9)   //if special weapon cooldown timer > 9
                cooldown2Rect.anchoredPosition = cooldown2DefaultPos + new Vector2(-50f, 0);
            else
                cooldown2Rect.anchoredPosition = cooldown2DefaultPos;   //once it hits <=9 again, reset back to original position
    } 
    if (weaponInv.specialWeaponDurations[1]==-1 && weaponInv.slots[1]==null){    //special weapon expires
        cooldown2.gameObject.SetActive(false);
        cooldown2Rect.anchoredPosition = cooldown2DefaultPos;}
    }
public void WeaponCooldowns3(){
    if (weaponInv.cooldownTimers[2] > 0){   //if a cooldown timer isn't zero
        cooldown3.gameObject.SetActive(true);   //show cooldown timer
        DisplayTime3(weaponInv.cooldownTimers[2]);
        
        if (weaponInv.cooldownTimers[2] > 9)   //if cooldown timer > 9
            cooldown3Rect.anchoredPosition = cooldown3DefaultPos + new Vector2(-50f, 0);
        else
            cooldown3Rect.anchoredPosition = cooldown3DefaultPos;   //once it hits <=9 again, reset back to original position
    }
    if (weaponInv.cooldownTimers[2]==0){    //otherwise, show nothing
        cooldown3.gameObject.SetActive(false);
        cooldown3Rect.anchoredPosition = cooldown3DefaultPos;
    }
    if (weaponInv.specialWeaponDurations[2] > 0){  //Special weapon is active
            cooldown3.gameObject.SetActive(true);
            DisplayTime3(weaponInv.specialWeaponDurations[2]);
            
            if (weaponInv.specialWeaponDurations[2] > 9)   //if special weapon cooldown timer > 9
                cooldown3Rect.anchoredPosition = cooldown3DefaultPos + new Vector2(-50f, 0);
            else
                cooldown3Rect.anchoredPosition = cooldown3DefaultPos;   //once it hits <=9 again, reset back to original position
    } 
    if (weaponInv.specialWeaponDurations[2]==-1 && weaponInv.slots[2]==null){    //special weapon expires
        cooldown3.gameObject.SetActive(false);
        cooldown3Rect.anchoredPosition = cooldown3DefaultPos;}
    }
public void WeaponCooldowns4(){
    if (weaponInv.cooldownTimers[3] > 0){   //if a cooldown timer isn't zero
        cooldown4.gameObject.SetActive(true);   //show cooldown timer
        DisplayTime4(weaponInv.cooldownTimers[3]);
        
        if (weaponInv.cooldownTimers[3] > 9)   //if cooldown timer > 9
            cooldown4Rect.anchoredPosition = cooldown4DefaultPos + new Vector2(-50f, 0);
        else
            cooldown4Rect.anchoredPosition = cooldown4DefaultPos;   //once it hits <=9 again, reset back to original position
    }
    if (weaponInv.cooldownTimers[3]==0){    //otherwise, show nothing
        cooldown4.gameObject.SetActive(false);
        cooldown4Rect.anchoredPosition = cooldown4DefaultPos;
    }
    if (weaponInv.specialWeaponDurations[3] > 0){  //Special weapon is active
            cooldown4.gameObject.SetActive(true);
            DisplayTime4(weaponInv.specialWeaponDurations[3]);
            
            if (weaponInv.specialWeaponDurations[3] > 9)   //if special weapon cooldown timer > 9
                cooldown4Rect.anchoredPosition = cooldown4DefaultPos + new Vector2(-50f, 0);
            else
                cooldown4Rect.anchoredPosition = cooldown4DefaultPos;   //once it hits <=9 again, reset back to original position
    } 
    if (weaponInv.specialWeaponDurations[3]==-1 && weaponInv.slots[3]==null){    //special weapon expires
        cooldown4.gameObject.SetActive(false);
        cooldown4Rect.anchoredPosition = cooldown4DefaultPos;}
    }

    void DisplayTime1(float timeToDisplay)
    {
        // Ensure time disappears after 1 second
        if (timeToDisplay == 0){
            cooldown1.gameObject.SetActive(false);
            }

        timeToDisplay +=1;  //Lets say the cooldown is 5 seconds. Previously, it would show 4 seconds -> 0. This fixes that, making it 5->1 (it looks way better, like overwatch)

        //float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        cooldown1.text = seconds.ToString();
    }
    void DisplayTime2(float timeToDisplay)
    {
        if (timeToDisplay < 0){
            cooldown2.gameObject.SetActive(false);
            }

        timeToDisplay +=1;
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        cooldown2.text = seconds.ToString();
    }
    void DisplayTime3(float timeToDisplay)
    {
        if (timeToDisplay < 0){
            cooldown3.gameObject.SetActive(false);
            }

        timeToDisplay +=1;
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        cooldown3.text = seconds.ToString();
    }
    void DisplayTime4(float timeToDisplay)
    {
        if (timeToDisplay < 0){
            cooldown4.gameObject.SetActive(false);
            }

        timeToDisplay +=1;
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        cooldown4.text = seconds.ToString();
    }
    void DisplayTimeDoubleJump(float timeToDisplay)
    {
        if (timeToDisplay < 0){
            doubleJumpCooldown.gameObject.SetActive(false);
            }

        timeToDisplay +=1;
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        doubleJumpCooldown.text = seconds.ToString();
    }

    public void OnControllerUI(bool onController)
    {
        if (onController)
        {
            ControllerUI1.gameObject.SetActive(true);
            ControllerUI2.gameObject.SetActive(true);
            ControllerUI3.gameObject.SetActive(true);
            ControllerUI4.gameObject.SetActive(true);
            Debug.Log("controller");
        }
        else
        {
            Debug.Log("keyboard");
        }
    }

    public void IndecisiveBorder(int slotIndex, bool turnOn)
    {
        if (slotIndex==0 && turnOn==true) indecisiveBorder1.gameObject.SetActive(true);
        if (slotIndex==1 && turnOn==true) indecisiveBorder2.gameObject.SetActive(true);
        if (slotIndex==2 && turnOn==true) indecisiveBorder3.gameObject.SetActive(true);
        if (slotIndex==3 && turnOn==true) indecisiveBorder4.gameObject.SetActive(true);

        if (slotIndex==0 && turnOn==false) indecisiveBorder1.gameObject.SetActive(false);
        if (slotIndex==1 && turnOn==false) indecisiveBorder2.gameObject.SetActive(false);
        if (slotIndex==2 && turnOn==false) indecisiveBorder3.gameObject.SetActive(false);
        if (slotIndex==3 && turnOn==false) indecisiveBorder4.gameObject.SetActive(false);
        return;
    }


    public void SetSlot(GrenadeStats weapon, int slotNum)
    {
        if (activeSlots[slotNum].transform.childCount > 0)  //If one of 4 slots is already filled
        {
            Destroy(activeSlots[slotNum].transform.GetChild(0).gameObject); //destroy previous image
        }

            if (weapon == null)
            {
                Debug.Log("Weapon Null");
                return;} // if weapon doesn't exist, do not spawn UI

        Instantiate(weapon.weaponInvUI, activeSlots[slotNum].transform);    //show new UI
    }
    public void RemoveSlot(int slotNum)    //used when a special weapon runs out
    {
        if (activeSlots[slotNum].transform.childCount > 0)  
        {
            Destroy(activeSlots[slotNum].transform.GetChild(0).gameObject); 
        }
    }

    public void ShowObeliskSpawnedText(bool showText)
    {
        if(showText){UpObSpawned.gameObject.SetActive(true);
            Debug.Log("SHOW TEXT");}

        if(!showText){UpObSpawned.gameObject.SetActive(false);
            Debug.Log("HIDE TEXT");}
    } 


    public void ShowJetpack(GrenadeStats weapon)
    {
        if (up.doubleJumpUpgrade>0)
        {
            Debug.Log("test");
            GameObject jetpackIcon = Instantiate(weapon.weaponInvUI, upgradeUI[0].transform);
        jetpackIcon.SetActive(true);   // make sure it’s visible
        }
    }
    public void DoubleJumpCooldown()
    {
        if (up.doubleJumpRecharged == false){   //if a cooldown timer isn't zero
        doubleJumpCooldown.gameObject.SetActive(true);   //show cooldown timer
        DisplayTimeDoubleJump(up.doubleJumpRechargeTimeLeft[0]);
        }

        if (up.doubleJumpRechargeTimeLeft[0] > 9){   //if cooldown timer > 9
            doubleJumpRect.anchoredPosition = doubleJumpDefaultPos + new Vector2(-50f, 0);}    //Show shifted cooldown timer
        else{
            doubleJumpRect.anchoredPosition = doubleJumpDefaultPos;}   //then once it hits <=9 again, reset back to original position
        if (up.doubleJumpRecharged == true){    //if nade isn't on cooldown, show nothing
            doubleJumpCooldown.gameObject.SetActive(false);
            doubleJumpRect.anchoredPosition = doubleJumpDefaultPos;
        }
    }
}