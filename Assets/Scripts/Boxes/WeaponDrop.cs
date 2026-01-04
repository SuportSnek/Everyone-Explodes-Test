using UnityEngine;
using System.Collections.Generic;

public class WeaponDrop : MonoBehaviour

{
    public List<GrenadeStats> possibleWeapons; // assign via inspector

    private GrenadeStats dropChoice;
    private bool initialized = false;
    private bool sessionOpen = false;
    private bool claimed = false;

    private readonly HashSet<GameObject> playersInRange = new HashSet<GameObject>(); //Stores which player is within the trigger
    private GameObject owningPlayer = null; //This is the player allowed to interact once claimed.

    [SerializeField] private Transform modelHolder; // assign empty child object in prefab. This is where where the grenade model will appear.
    private GameObject currentModelInstance;
    private float floatAmplitude = 0.25f;
    private float floatFrequency = 2f;
    private Vector3 basePosition;
    public ParticleSystem lightParticle;
    private ParticleSystem lightParticleInstance;

    void Start()
    {
        InitializeDrop();
        basePosition = modelHolder.position;
    }

    private void Update()
    {
        // If already claimed or session open, do nothing
        if (owningPlayer != null || sessionOpen || claimed)
            return;

        if (currentModelInstance != null)
        {
            currentModelInstance.transform.Rotate(Vector3.up, 50f * Time.deltaTime, Space.World);   //rotate 3d model
            float newY = basePosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            currentModelInstance.transform.position = new Vector3(basePosition.x, newY, basePosition.z);
        }

        foreach (var player in playersInRange)
        {
            var pc = player.GetComponent<PlayerController>();
            var fpc = player.GetComponent<FirstPersonController>();
            if (pc == null) continue;
            if (fpc != null && fpc.InteractionLocked) continue;

            if(!fpc.inUpgradeMenu && !fpc.inWeaponMenu && !fpc.wasJustInMenu){
                //Open via menu:
                if (pc.InteractPressedThisFrame)    
                {
                    owningPlayer = player;
                    sessionOpen = true;
                    OnInteract(player);
                    break;
                }
                // Instant pickup:
                if (pc.Throw1Triggered) { claimed = true; AssignInstantly(player, 0); break; }
                if (pc.Throw2Triggered) { claimed = true; AssignInstantly(player, 1); break; }
                if (pc.Throw3Triggered) { claimed = true; AssignInstantly(player, 2); break; }
                if (pc.Throw4Triggered) { claimed = true; AssignInstantly(player, 3); break; }
            }
        }
                // rotate the visual mesh for animation
        if (currentModelInstance != null)
        {
            currentModelInstance.transform.Rotate(Vector3.up, 50f * Time.deltaTime, Space.World);
        }
    }


    private void InitializeDrop()
    {
        if (initialized) return;

        dropChoice = GetWeightedRandomChoice(possibleWeapons);
        initialized = true;

            // Spawn visual mesh
        if (dropChoice.visualPrefab != null && modelHolder != null)
        {
            if (currentModelInstance != null)
                Destroy(currentModelInstance);

            currentModelInstance = Instantiate(dropChoice.visualPrefab, modelHolder);
            currentModelInstance.transform.localPosition = Vector3.zero;
            currentModelInstance.transform.localRotation = Quaternion.identity;
            currentModelInstance.transform.localScale = dropChoice.visualPrefab.transform.localScale;

            lightParticleInstance = Instantiate(lightParticle, transform.position, lightParticle.transform.rotation, transform); // parent it to the drop so it auto-destroys if needed



        }
    }



    public GrenadeStats GetWeightedRandomChoice(List<GrenadeStats> pool)
    {
        float totalWeight = 0f;

        foreach (var weapon in pool)
            totalWeight += weapon.dropWeight;

        float roll = Random.Range(0f, totalWeight);

        foreach (var weapon in pool)
        {
            roll -= weapon.dropWeight;
            if (roll <= 0f)
                return weapon;
        }

        // Fallback (should never hit)
        return pool[pool.Count - 1];
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playersInRange.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playersInRange.Remove(other.gameObject);
    }

    public void AssignInstantly(GameObject player, int invSlot)
    {
        InitializeDrop();

        var inv = player.GetComponent<WeaponInventory>();
        var invUi = player.GetComponentInChildren<InventoryUI>();
        var fpc = player.GetComponent<FirstPersonController>();
        
            if (inv.HasAnyEmptySlot())  //IF you have an empty slot
            {
                if (inv.IsSlotEmpty(invSlot))   //IF that specific slot is empty
                {
                    inv.SetWeapon(invSlot, dropChoice); //Set weapon
                    invUi.SetSlot(dropChoice, invSlot);

                    Debug.Log($"Player {player.name} took {dropChoice.GrenadeName}");

                    if (currentModelInstance != null){
                        Destroy(currentModelInstance);}
                        
                    if (lightParticleInstance != null){
                        Destroy(lightParticleInstance.gameObject);}
                    Destroy(gameObject);  
                    fpc.TriggerWasJustInMenu();
                }
            }
            else if (!inv.HasAnyEmptySlot())        //If all weapon slots are filled
                {
                    inv.SetWeapon(invSlot, dropChoice); //Set weapon
                    invUi.SetSlot(dropChoice, invSlot);

                    Debug.Log($"Player {player.name} took {dropChoice.GrenadeName}");

                    if (currentModelInstance != null){
                        Destroy(currentModelInstance);}
                    if (lightParticleInstance != null){
                        Destroy(lightParticleInstance.gameObject);}
                    fpc.TriggerWasJustInMenu();
                }
    }
    //What above code does:
    //If you have an empty slot, you must assign nade to empty slot
    //Otherwise, you can just assign it
    //Todo Reason for TriggerWasJustInMenu() is that if you directly call WasJustInMenu, Destroy(gameObject) would destroy the timer before it finishes.
    //Todo And so because the timer was triggered by FPC, it's not deleted with Destroy(gameObject), I guess.

    public void OnInteract(GameObject playerWhoClicked)
    {
        if (owningPlayer != playerWhoClicked)
            return;

        InitializeDrop();

        var ui = playerWhoClicked.GetComponentInChildren<WeaponSelectionUI>();
        if (ui == null) return;

        ui.OpenUI(playerWhoClicked, dropChoice,
            (GameObject playerArg, int invSlot) => {return OnPlayerConfirm(playerArg, invSlot);}
        );
    }

    private bool OnPlayerConfirm(GameObject player, int invSlot)
    {
        if (player != owningPlayer)
            return false;

        var ui = player.GetComponentInChildren<WeaponSelectionUI>();
        var inv = player.GetComponent<WeaponInventory>();
        var invUi = player.GetComponentInChildren<InventoryUI>();
        var fpc = player.GetComponent<FirstPersonController>();

        if (inv.HasAnyEmptySlot())
            {
                // Player must choose an empty slot
                if (!inv.IsSlotEmpty(invSlot))
                {
                    Debug.Log("You must place this weapon in an empty slot.");
                    ui.NotifyMustPickEmptySlot();
                    return false;
                }
            }

        inv.SetWeapon(invSlot, dropChoice);
        invUi.SetSlot(dropChoice, invSlot);

        Debug.Log($"Player {player.name} took {dropChoice.GrenadeName}");

        CloseAllOpenUIs();

        if (currentModelInstance != null){
            Destroy(currentModelInstance);}
        if (lightParticleInstance != null){
            Destroy(lightParticleInstance.gameObject);}
        Destroy(gameObject);
        fpc.TriggerWasJustInMenu();

        return true;
    }


    private void CloseAllOpenUIs()
    {
        Debug.Log("CloseAllOpenUIs()");
        var psm = PlayerSpawnManager.Instance;
        IEnumerable<GameObject> players = psm != null ? psm.AllPlayers : GameObject.FindGameObjectsWithTag("Player");
        foreach (var pl in players)
        {
            var ui = pl.GetComponentInChildren<WeaponSelectionUI>();
            if (ui != null && ui.IsOpenForPlayer(pl))
            {
                ui.ForceClose();
            }
        }

        // reset session state
        sessionOpen = false;
    }
}

