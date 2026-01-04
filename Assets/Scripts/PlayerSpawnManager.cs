using System.Collections.Generic;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawnManager : MonoBehaviour
{
    // ✅ Dynamic list of all active players
    public static PlayerSpawnManager Instance { get; private set; }
    public List<GameObject> AllPlayers { get; private set; } = new List<GameObject>();
    //The reason that "public static PlayerSpawnManager Instance" is neccessary is that enables other files to access AllPlayers by doing PlayerSpawnManager.Instance.AllPlayers, rather than having to do [SerializeField] private PlayerSpawnManager manager;

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameManager gManager;
    [SerializeField] private Transform[] spawnPoints;

    private bool keyboardJoined = false;
    private bool controllerJoined = false;

    private PlayerHealth pHealth;
    private InventoryUI invUI;

    //private int nextDisplay = 0; // 0 = Display 1, 1 = Display 2, etc.

    private void Start()
    {

        // Enable all connected displays
        for (int i = 0; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }
    }


    void Awake()    //This generates Instance.AllPlayers before anything happens, which is neccesary so it always exists.
    {
        if (Instance != null && Instance != this)   //Is there already a different manager named instance active?
        {
            Destroy(gameObject);    //If there’s already another manager, we destroy the entire GameObject this script is attached to (the duplicate)
            return;
        }
        Instance = this; //If there wasn’t an existing different instance, assign the static Instance to this instance. Now other classes can use PlayerSpawnManager.Instance to access it!
    }
    //todo Player 1 spawns facing the wrong direction
    // Update is called once per frame
    void Update()   //todo This runs every frame? Maybe that's too much
    {
        if (Keyboard.current == null) return;
        
        if (!keyboardJoined && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            var player = PlayerInput.Instantiate(playerPrefab,  //Creates the player prefab, assigns it to keyboard
                controlScheme: "Keyboard&Mouse",
                pairWithDevice: Keyboard.current);

            if (spawnPoints.Length > 0) //Just so it doesn't crash
            {
                player.transform.position = spawnPoints[0].position;         //finds a spawn point, sets a player position equal to a spawn point
            }
            keyboardJoined = true;  //Marks that the keyboard player already spawned

            var cam = player.GetComponentInChildren<Camera>();
            cam.targetDisplay = 0; // Display 1

            player.name = "Keyboard Player";    //Names it
            player.tag = "Player"; //tags it
            player.GetComponent<PlayerHealth>().PlayerIndex = 0;
            AllPlayers.Add(player.gameObject); //Stores it in the global list
            Debug.Log("Player has joined the game");

              // Save reference to PlayerHealth
            pHealth = player.GetComponent<PlayerHealth>();
            if (pHealth == null)
            Debug.LogError("PlayerHealth component missing on prefab!");

            AssignPlayerLayers(player.gameObject, 0);
            ConfigureCamera(cam, 0);

            if(pHealth.keyboardSound){
                if (cam.GetComponent<AudioListener>() == null){cam.gameObject.AddComponent<AudioListener>();}
            }
        }

        if (!controllerJoined && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            var player = PlayerInput.Instantiate(playerPrefab,      //Creates the player prefab, assigns it to controller
                controlScheme: "Gamepad",
                pairWithDevice: Gamepad.current);


            if (spawnPoints.Length > 0) //Just so it doesn't crash
            {
                player.transform.position = spawnPoints[1].position;         //finds a spawn point, sets a player position equal to a spawn point
            }
            controllerJoined = true;


            var cam = player.GetComponentInChildren<Camera>();
            cam.targetDisplay = 1; // Display 2


            player.name = "Controller Player";
            player.tag = "Player";
            player.GetComponent<PlayerHealth>().PlayerIndex = 1;
            AllPlayers.Add(player.gameObject);

            AssignPlayerLayers(player.gameObject, 1);
            ConfigureCamera(cam, 1);

               // Save reference to PlayerHealth
            pHealth = player.GetComponent<PlayerHealth>();
            if (pHealth == null)
            Debug.LogError("PlayerHealth component missing on prefab!");

            invUI = player.GetComponent<InventoryUI>();
            invUI.OnControllerUI(controllerJoined);

            if(!pHealth.keyboardSound){
                if (cam.GetComponent<AudioListener>() == null){cam.gameObject.AddComponent<AudioListener>();}
            }
    }
        
        //By pressing L, counts number of players and what player number they are
        if (Keyboard.current.lKey.wasPressedThisFrame)
            {
                Debug.Log("=== Current Players in AllPlayers List ===");

                for (int i = 0; i < AllPlayers.Count; i++)
                {
                    GameObject player = AllPlayers[i];
                    Debug.Log($"Player {i}: {player.name}");
                }

                Debug.Log($"Total players: {AllPlayers.Count}");
            }

        //Kill yourself button
        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            Debug.Log("DEATH");
            pHealth.UDied();
            //Respawn();
        }
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            Debug.Log("RESTART");
            gManager.RestartGame();
            //Respawn();
        }
    }

void AssignPlayerLayers(GameObject player, int index)
{
    Transform fp = player.transform.Find("FirstPerson");
    Transform tp = player.transform.Find("ThirdPerson");

    int fpLayer = LayerMask.NameToLayer(index == 0 ? "P1_FirstPerson" : "P2_FirstPerson");//If index == 0:Assign layer "P1_FirstPerson". Else, Assign layer "P2_FirstPerson"
    int tpLayer = LayerMask.NameToLayer(index == 0 ? "P1_ThirdPerson" : "P2_ThirdPerson");
    //Result:
        //Player 1 arms → P1_FirstPerson
        //Player 2 arms → P2_FirstPerson

    SetLayerRecursively(fp.gameObject, fpLayer);    //Takes the FirstPerson root GameObject, Sets its layer and all children’s layers to fpLayer
    SetLayerRecursively(tp.gameObject, tpLayer);
}

void SetLayerRecursively(GameObject obj, int layer)
{
    obj.layer = layer;
    foreach (Transform child in obj.transform)
        SetLayerRecursively(child.gameObject, layer);
}



    void ConfigureCamera(Camera cam, int playerIndex)
    {
        int mask = 0;

        int layerPlayer1UI = LayerMask.NameToLayer("UI_Player1");
        int layerPlayer2UI = LayerMask.NameToLayer("UI_Player2");
        
        mask |= 1 << LayerMask.NameToLayer("Default");
        mask |= 1 << LayerMask.NameToLayer("Ground");
        mask |= 1 << LayerMask.NameToLayer("UI");
        mask |= 1 << LayerMask.NameToLayer("Grenade");
        mask |= 1 << LayerMask.NameToLayer("SpiralNade");
    mask |= 1 << layerPlayer1UI;
    mask |= 1 << layerPlayer2UI;

        if (playerIndex == 0)
        {
            mask |= 1 << LayerMask.NameToLayer("P1_FirstPerson");
            mask |= 1 << LayerMask.NameToLayer("P2_ThirdPerson");
            mask &= ~(1 << layerPlayer2UI);
            
        }
        else if (playerIndex == 1)
        {
            mask |= 1 << LayerMask.NameToLayer("P2_FirstPerson");
            mask |= 1 << LayerMask.NameToLayer("P1_ThirdPerson");
             mask &= ~(1 << layerPlayer1UI);
        }

        cam.cullingMask = mask;
    }

    public void RespawnPlayer(GameObject player)
    {
        // Example: respawn all players at spawnPoints[0]
        player.transform.position = spawnPoints[0].position;
    }

}
