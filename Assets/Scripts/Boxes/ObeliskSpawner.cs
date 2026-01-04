using System.Collections;
using UnityEngine;

//Todo: Change this so that it only works while >0 players are spawned in
public class ObeliskSpawner : MonoBehaviour
{
    public LayerMask allowedLayer;
    public float spawnHeight = 50f; // height the object falls from
    public float spawnFreqObelisk = 60f; // spawnFreq
    public float spawnFreqDrop = 25f; // spawnFreq
    public Vector2 MapMinMaxX; 
    public Vector2 MapMinMaxZ;
    public Vector2 MiddleMinMaxX; 
    public Vector2 MiddleMinMaxZ;
    public Vector2 GreenHillX; 
    public Vector2 GreenHillZ;
    public Vector2 BlueHillX; 
    public Vector2 BlueHillZ;
    public Vector2 BlackHillX; 
    public Vector2 BlackHillZ;
    public Vector2 TealHillX; 
    public Vector2 TealHillZ;
    [SerializeField] private GameObject ObeliskPrefab;
    [SerializeField] private GameObject weaponDropPrefab;

        private AudioSource audioSource;
    public AudioClip spawningSound;
    //[HideInInspector] public int beenHereTwice; //stops it from spawning it in the middle thrice in a row*


    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null){
            audioSource = gameObject.AddComponent<AudioSource>();}
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpawnObeliskRoutine());
        StartCoroutine(SpawnDropRoutine());
    }

    // Update is called once per frame
    void Update()
    {
       
    }

        // 1/3 chance of spawning in the middle
        // 1/3 chance it spawns on one of the outer hills
        // 1/3 chance it spawns somewhere completely random
    public Vector3 GetSafeSpawnPosition(bool isObelisk)
    {
        
        for (int i = 0; i < 20; i++)
        {
            float x = 0;
            float z = 0;
            float finalSpawnHeight = spawnHeight;

            //int random = (beenHereTwice >= 2) ? Random.Range(9, 17) //If haven't been to middle twice, use 1-16. If you have, 9-16.
                                           // : Random.Range(1, 17);
            int random = Random.Range(1, 25);
            if (random <= 8) // Middle
            {
                x = Random.Range(MiddleMinMaxX.x, MiddleMinMaxX.y);
                z = Random.Range(MiddleMinMaxZ.x, MiddleMinMaxZ.y);
                finalSpawnHeight = 10.7f;
            }
               else if (random > 8 && random <=16) // 9-12: Random spot on map
                {
                    x = Random.Range(MapMinMaxX.x, MapMinMaxX.y);
                    z = Random.Range(MapMinMaxZ.x, MapMinMaxZ.y);
                    finalSpawnHeight = 0f;
                }
                else if (random == 17 || random == 18) // Green hill
                {
                    x = Random.Range(GreenHillX.x, GreenHillX.y);
                    z = Random.Range(GreenHillZ.x, GreenHillZ.y);
                    finalSpawnHeight = 8.5f;
                }
               else if (random == 19 || random == 20)  // Blue hill
                {
                    x = Random.Range(BlueHillX.x, BlueHillX.y);
                    z = Random.Range(BlueHillZ.x, BlueHillZ.y);
                    finalSpawnHeight = 8.2f;
                }
                else if (random == 21 || random == 22)  // Black hill
                {
                    x = Random.Range(BlackHillX.x, BlackHillX.y);
                    z = Random.Range(BlackHillZ.x, BlackHillZ.y);
                    finalSpawnHeight = 8.5f;
                }
                else if (random == 23 || random == 24)  // Teal hill
                {
                    x = Random.Range(TealHillX.x, TealHillX.y);
                    z = Random.Range(TealHillZ.x, TealHillZ.y);
                    finalSpawnHeight = 8.2f;
                }
            if(isObelisk){finalSpawnHeight = 50f;}
            Vector3 pos = new Vector3(x, finalSpawnHeight, z);

            // Check ground
            if (Physics.Raycast(pos, Vector3.down, 1000f, allowedLayer))
            {
                return pos;
            }
        }

        // fallback
        Debug.LogWarning("Could not find safe spawn");
        return Vector3.zero;
    }

//How spawn height code works:
//If it's a weapon drop, the spawn location determines the final spawn height
//Otherwise, it just uses 50f for obelisks


    IEnumerator SpawnObeliskRoutine()
    {
        float timer = spawnFreqObelisk;
        while (true)
        {
            // If any player is in menu: pause timer
            if (AnyPlayerIsInMenu())
            {
                yield return null; 
                continue;
            }

            // Count down normally
            timer -= Time.deltaTime;

            //When timer hits 0
            if (timer <= 0f)
            {
                SpawnObelisk();
                timer = spawnFreqObelisk;
            }

            yield return null;
        }
    }


    IEnumerator SpawnDropRoutine()
    {
        float timer = spawnFreqDrop;

        while (true)
        {
            if (AnyPlayerIsInMenu())
            {
                yield return null;
                continue;
            }

            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                SpawnDrop();
                timer = spawnFreqDrop;
            }

            yield return null;
        }
    }

    public void SpawnObelisk()
    {
        Vector3 spawnPos = GetSafeSpawnPosition(true);
        Instantiate(ObeliskPrefab, spawnPos, Quaternion.identity);
        if(spawningSound != null){
            audioSource.PlayOneShot(spawningSound, 0.75f);}

        var psm = PlayerSpawnManager.Instance;
        foreach (var player in psm.AllPlayers)
        {
            var InvUI = player.GetComponent<InventoryUI>();
            StartCoroutine(ObeliskSpawnedText(InvUI));
        }

        Debug.Log("Upgrade Obelisk Spawned!");
    }
    public void SpawnDrop()
    {
        Vector3 spawnPos = GetSafeSpawnPosition(false);
        Instantiate(weaponDropPrefab, spawnPos, Quaternion.identity);
        Debug.Log("Weapon Drop Spawned!");
    }
    /*
    public void RespawnObject(GameObject thingToRespawn)  
    {
        Vector3 spawnPos = GetSafeSpawnPosition();
        Instantiate(thingToRespawn, spawnPos, Quaternion.identity);
        Debug.Log("thing Respawned!");
    }*/

    public IEnumerator ObeliskSpawnedText(InventoryUI InvUI)
    {
        InvUI.ShowObeliskSpawnedText(true);
        yield return new WaitForSeconds(3f);
        InvUI.ShowObeliskSpawnedText(false);
    } 


    //This gives you a single clean call that answers the question: “Are players in the upgrade obelisk menu?”
    bool AnyPlayerIsInMenu()
    {
        var psm = PlayerSpawnManager.Instance;
        if (psm == null || psm.AllPlayers == null)
            return false;

        foreach (var player in psm.AllPlayers)
        {
            var fpc = player.GetComponent<FirstPersonController>();
            if (fpc != null && fpc.inUpgradeMenu)
                return true;
        }

        return false;
    }



}
//Todo to add later: Add logic from other code (idk where it is) that spawns them at a cool angle. 
//Todo Then, when they hit the ground, they explode and freeze position.
