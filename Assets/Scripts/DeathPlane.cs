using UnityEngine;

public class DeathPlane : MonoBehaviour
{
    //private PlayerHealth pHealth;
    public ObeliskSpawner oSpawner;

    private void Start()
    {
    //How do I reference pHealth?
    }

    
    private void OnTriggerEnter(Collider other)
    {
        
        //ObeliskSpawner oSpawner = GetComponent<ObeliskSpawner>();

        // Check if the player touched the plane
        if (other.CompareTag("Player"))
        {
            PlayerHealth pHealth = other.GetComponent<PlayerHealth>();
            if (pHealth != null)
            {
                Debug.Log("Player died!");
                pHealth.UDied();
            }
        }
        if (other.CompareTag("C4")){//
            Destroy(other.gameObject);
        }
        
        if (other.CompareTag("Obelisk"))    //respawn obelisk
        {
            Debug.Log("Obelisk/weaponbox has respawned!");
            Destroy(other.gameObject);
            oSpawner.SpawnObelisk();
        }
            
        if (other.CompareTag("WeaponBox"))    //respawn drop
        {
            Debug.Log("Obelisk/weaponbox has respawned!");
            Destroy(other.gameObject);
            oSpawner.SpawnDrop();
        }
        /*
        if (other.CompareTag("Obelisk") || other.CompareTag("WeaponBox"))    //respawn obelisk
        {
            Debug.Log("Obelisk/weaponbox has respawned!");
            Destroy(other.gameObject);
            oSpawner.RespawnObject(other.gameObject);
        }*/
    }
}