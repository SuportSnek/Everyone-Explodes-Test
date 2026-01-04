using System.Collections.Generic;
using UnityEngine;

public class ObeliskScript : MonoBehaviour, ButtonScript.IClickable
{
    public List<UpgradeData> possibleUpgrades; // fill via inspector

    private UpgradeData[] boxChoices = new UpgradeData[3];
    private bool[] taken = new bool[3];
    private bool initialized = false;
    private bool sessionOpen = false;

    [Header("Hold Interaction Settings")]
    public float holdDuration = 10f; // seconds REQUIRED to hold
    public float minObeliskTimer = 3f; // minimum seconds to hold
    private Dictionary<GameObject, float> holdTimers = new Dictionary<GameObject, float>(); // player → time held
    private Dictionary<GameObject, float> obeliskCapProgress = new Dictionary<GameObject, float>();//This is for when a player only makes partial progress on capping obelisk. It's time is taken off next attempt.
    public float holdDurationTimeDown = 5f;

    // session tracking
    private GameObject firstTaker = null;   // who first interacted with the box (gets priority)
    private int firstTakenIndex = -1;       // which index the first taker chose (-1 = not chosen yet)

    private AudioSource audioSource;
    public AudioClip cappingSound;
    private GameObject cappingPlayer = null;    //Technically, this only resets when another person starts capping, but that's actually fine

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null){
            audioSource = gameObject.AddComponent<AudioSource>();}

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;   // 2D sound
    }

    void Start()
    {
        InitializeBox();
        //InvokeRepeating(nameof(ReduceHoldDuration), 5f, 5f);
        //ReduceHoldDuration = Name of method to call repeatedly
        //Wait 5 seconds before first reduction
        //Call again every 5 seconds
        
    }

    void Update()
    {
        ReduceHoldDuration();
        //Debug.Log("obeliskCapProgressMade"+obeliskCapProgressMade);

        // If the obelisk session is already open (UI shown), don't track holds.
        if (sessionOpen) return;

        // get player list safely
        List<GameObject> players = PlayerSpawnManager.Instance != null
            ? PlayerSpawnManager.Instance.AllPlayers
            : new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));

        foreach (var player in players)
        {
            if (player == null) continue;

            var pHealth = player.GetComponentInChildren<PlayerHealth>();
            var pController = player.GetComponent<PlayerController>();
            var fpc = player.GetComponent<FirstPersonController>();
            if (pController == null || fpc == null) continue;
            if (fpc.inWeaponMenu || fpc.inUpgradeMenu) continue;   //If already in menu, no capping!
            if (fpc.cookingFrag) continue;   //If cooking frag, no capping!

            bool isHoldingInteract = pController.InteractTriggered; //Is holding interact
            bool isLooking = IsPlayerLookingAt(player); //is looking at the obelisk

            if (!holdTimers.ContainsKey(player)) {holdTimers[player] = 0f;}
            if (!obeliskCapProgress.ContainsKey(player)) {obeliskCapProgress[player] = 0f;}

            if (isLooking)
            {
                if(isHoldingInteract)
                {
                    holdTimers[player] += Time.deltaTime;   //player's time held increases with time
                    float heldFor = holdTimers[player];
                    obeliskCapProgress[player] += Time.deltaTime;
                    pHealth.DisplayObeliskTimer(heldFor, holdDuration, true);

                    fpc.LockInteraction();  //this prevents you from capping the obelisk and opening weapon menu at the same time. Obelisk has priority.
                    fpc.cappingObelisk = true;

                    if (cappingPlayer == null || cappingPlayer != player){cappingPlayer = player;}

                    if (cappingPlayer == player )    //Todo once multiple players can hear sound, test out what happens to sound when multiple people cap
                        {
                            if (!audioSource.isPlaying){
                                audioSource.clip = cappingSound;
                                audioSource.loop = true;
                                audioSource.Play(); //Todo need to change this so that it doesn't overlap with music
                            }
                        }

                    if (holdTimers[player] >= holdDuration)
                        {
                            // Interaction complete: open UI for everyone
                            CompleteInteraction(player);

                            // reset this player's timer (defensive)
                            holdTimers[player] = 0f;
                            
                            if (audioSource.isPlaying && cappingPlayer == player){
                                audioSource.Stop();
                                audioSource.clip = null;}
                        }
                    
                    if (!isHoldingInteract) //if looking at obelisk, but not pressing button
                    {
                        if (holdTimers.ContainsKey(player)) holdTimers[player] = 0f;
                        fpc.cappingObelisk = false;
                        fpc.UnlockInteraction();
                        if(obeliskCapProgress[player]!=0){
                             Debug.Log("test1");
                            holdDuration-=obeliskCapProgress[player]/holdDurationTimeDown;
                            obeliskCapProgress[player]=0;
                            pHealth.DisplayObeliskTimer(heldFor, holdDuration, false); //Counter-intuitively, this DISABLES the timer, because PlayerHealth needs to be run for one frame for the logic in DisplayObeliskTimer to turn it off.
                            }  //records what progress player made, lets say 1 seconds. That's 0.2s off timer


                        if (audioSource.isPlaying && cappingPlayer == player){
                            audioSource.Stop();
                            audioSource.clip = null;}
                    }

                    continue;
                }

                holdTimers[player] = 0f; // LOOKING BUT NOT HOLDING
            }

            else
            {
                holdTimers[player] = 0f;
            }

            fpc.cappingObelisk = false;
            fpc.UnlockInteraction();
            if(obeliskCapProgress.ContainsKey(player) && obeliskCapProgress[player] != 0){
                Debug.Log("test2");
                holdDuration -= obeliskCapProgress[player]/holdDurationTimeDown;
                obeliskCapProgress[player]=0;
                pHealth.DisplayObeliskTimer(0, holdDuration, false);}  //records what progress player made, lets say 1 seconds. That's 0.2s off timer
            

            if (audioSource.isPlaying && cappingPlayer == player)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }
        }
    }
        
    void ReduceHoldDuration()
    {
            // Check if ANY player is currently capping
    bool anyoneCapping = false;
    foreach (var progress in obeliskCapProgress.Values)
    {
        if (progress > 0)
        {
            anyoneCapping = true;
            break;
        }
    }
        if(holdDuration > minObeliskTimer && !anyoneCapping) {holdDuration -= Time.deltaTime/holdDurationTimeDown;}    //if greater than min and nobody is capping, timer goes down
        if (holdDuration <= minObeliskTimer) {holdDuration = minObeliskTimer;} //if hold timer is <=3, make it 3.
    }


    private bool IsPlayerLookingAt(GameObject player)
    {
        Camera cam = player.GetComponentInChildren<Camera>();
        if (cam == null) return false;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 2.5f))
        {
            // Use collider/gameObject equality; depending on your setup you might prefer hit.collider == thisCollider
            return hit.collider != null && hit.collider.gameObject == gameObject;
        }

        return false;
    }

    private void CompleteInteraction(GameObject playerWhoClicked)
    {
        // Ensure choices exist
        if (!initialized) InitializeBox();

        // If UI session already open, do nothing
        if (sessionOpen) return;

        //For the Speed Stacker upgrade
        UpgradesScript up = null;
        up = playerWhoClicked.GetComponent<UpgradesScript>();
        if (up.speedScalerUpgrade > 0){
            up.numObelisksWon++;}


        sessionOpen = true;
        firstTaker = playerWhoClicked;
        firstTakenIndex = -1;

        // Open UI for all players
        var players = PlayerSpawnManager.Instance != null ? PlayerSpawnManager.Instance.AllPlayers : new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        foreach (var pl in players)
        {
            if (pl == null) continue;
            var ui = pl.GetComponentInChildren<UpgradeSelectionUI>();
            if (ui == null) continue;

            bool[] takenClone = (bool[])taken.Clone();
            ui.OpenUI(pl, (UpgradeData[])boxChoices.Clone(), takenClone, (playerArg, choiceIndex) =>
            {
                return OnPlayerConfirm(playerArg, choiceIndex);
            });
        }
    }

    private void InitializeBox()
    {
        if (initialized) return;
        boxChoices = GetRandomChoices(possibleUpgrades);
        for (int i = 0; i < taken.Length; i++) taken[i] = false;
        initialized = true;
    }

    private UpgradeData[] GetRandomChoices(List<UpgradeData> pool)
    {
        var result = new UpgradeData[3];
        var copy = new List<UpgradeData>(pool);
        for (int i = 0; i < 3; i++)
        {
            if (copy.Count == 0) break;
            int r = Random.Range(0, copy.Count);
            result[i] = copy[r];
            copy.RemoveAt(r);
        }
        return result;
    }

    // ButtonScript will still call this on the initial press; we don't need it for the hold logic,
    // but keep it if you want visual feedback or to play a sound when the player presses interact.
    public void OnInteract(GameObject playerWhoClicked)
    {
        // optional: play initial "start holding" sound/animation
        // We intentionally DO NOT set any timer here because ButtonScript only calls OnInteract on the press frame.
    }

    private bool OnPlayerConfirm(GameObject player, int choiceIndex)
    {
        if (choiceIndex < 0 || choiceIndex >= boxChoices.Length) return false;
        if (taken[choiceIndex]) return false;

    var ui = player.GetComponentInChildren<UpgradeSelectionUI>();

     // If already taken, reject
    if (taken[choiceIndex])
    {
        if (ui != null) ui.NotifySelectionFailed(choiceIndex);
        Debug.Log("Choice already taken.");
        return false;
    }

// If someone other than the first taker is trying to pick before the first taker has chosen -> reject
    if (player != firstTaker && firstTakenIndex == -1)
    {
        // Tell them to wait
        if (ui != null) ui.NotifyWaitForFirstTaker();
        Debug.Log("Second player attempted to pick before first taker chose.");
        return false;
    }


    //If you're first taker, and have already taken a weapon
    if (player == firstTaker && firstTakenIndex != -1)
    {
        // Tell them to wait
        if (ui != null) ui.NotifyWaitForFirstTaker();
        Debug.Log("No taking two upgrades, ya greedy bastard!");
        return false;
    }


        taken[choiceIndex] = true;

        // If this was the first taker, record which index they chose
        if (player == firstTaker)
        {
            firstTakenIndex = choiceIndex;
        }

        UpgradeData upgrade = boxChoices[choiceIndex];
        var up = player.GetComponent<UpgradesScript>();
        if (up != null) up.GrantUpgrade(upgrade.type);

        UpdateAllOpenUIs();

        if (AllTaken()) {
            CloseAllOpenUIs();
            Destroy(gameObject);}
        else
        {
            // If first taker has chosen and second player now has to choose, do nothing special here.
            // The second player will be able to choose the remaining slot.
        }

        return true;
    }

    private void UpdateAllOpenUIs()
    {
        var psm = PlayerSpawnManager.Instance;
        IEnumerable<GameObject> players = psm != null ? psm.AllPlayers : GameObject.FindGameObjectsWithTag("Player");
        foreach (var pl in players)
        {
            var ui = pl.GetComponentInChildren<UpgradeSelectionUI>();
            if (ui != null && ui.IsOpenForPlayer(pl))
            {
                ui.UpdateTakenOptions(taken);
            }
        }
    }

    private void CloseAllOpenUIs()
    {
        var psm = PlayerSpawnManager.Instance;
        IEnumerable<GameObject> players = psm != null ? psm.AllPlayers : GameObject.FindGameObjectsWithTag("Player");
        foreach (var pl in players)
        {
            //ApplySizeUpDownUpgrades(pl);

            var pHealth = pl.GetComponentInChildren<PlayerHealth>();
            float heldFor = 0f;
pHealth.DisplayObeliskTimer(heldFor, holdDuration, false);


            var ui = pl.GetComponentInChildren<UpgradeSelectionUI>();
            
            if (ui != null && ui.IsOpenForPlayer(pl))
            {
                ui.ForceClose();
            }
        }

        // reset session state
        sessionOpen = false;
        firstTaker = null;
        firstTakenIndex = -1;
        holdTimers.Clear();  // reset hold timers so next obelisk use starts fresh
    }

/*
    public void ApplySizeUpDownUpgrades(GameObject pl)
    {
        UpgradesScript up = null;
        up = pl.GetComponent<UpgradesScript>();

        up.SizeUpgrades();
    }
*/
    private bool AllTaken()
    {
        int count = 0;

        foreach (bool t in taken)
        {
            if (t) count++;
            if (count >= 2) return true; // close after 2 upgrades
        }

        return false;
    }
    //Why this works
    //It loops through the taken array (bool[] taken = new bool[3])
    //Each time a player picks an upgrade, the corresponding index becomes true
    //As soon as the counter reaches 2, the obelisk considers itself “finished”
    //It returns true early instead of waiting for all 3 to be taken
}
