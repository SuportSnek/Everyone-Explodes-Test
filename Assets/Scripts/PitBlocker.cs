using UnityEngine;

public class PitBlocker : MonoBehaviour
{
    //Todo bug that I'm saving for future me:
    //Todo you can get caught in the wall. The wall is still Ghost (see debug log), but the player is still stuck for some reason
    //Todo, for some stupid ass reason, it crashes if you throw an explosive while stuck.

    //!Only blocker cube 2 is "done"
    //Todo Even just running into the wall can get you stuck!
    //Todo I suspect this has something to do with my non-existant wall collision code
    public string ground = "Ground";
    public string ghostBlocker = "ghostBlocker";

    [Header("Assign the Solid Collider here")]
    public Collider solidCollider;

    private bool playerInside = false;
    private FirstPersonController fpc;
    private HoverController hover;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) {
        FirstPersonController fpc = other.GetComponent<FirstPersonController>();
        HoverController hover = other.GetComponent<HoverController>();

        playerInside = true;

        TryUpdateWallState(fpc, hover);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!playerInside) return;
        FirstPersonController fpc = other.GetComponent<FirstPersonController>();
        HoverController hover = other.GetComponent<HoverController>();

        // Continuously check state as long as the player is inside
        TryUpdateWallState(fpc, hover);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return; 

        playerInside = false;

        // When the player leaves, NOW attempt to turn the wall solid again
        SetWallSolid(true);
    }

    private void TryUpdateWallState(FirstPersonController fpc, HoverController hover)
    {       
        if (fpc.beingExploded == true || hover.isGrounded == false)     //!Line that crashes
        {
            // Player should phase through the wall
            SetWallSolid(false);
        }
        else if (hover.isGrounded && !fpc.beingExploded && !playerInside)       
        {
            // Only set solid if player is NOT intersecting the wall
            SetWallSolid(true);
        }
        else{Debug.Log("Still in Wall?");}
    }

    public void SetWallSolid(bool solid)
    {
        if (solid==true){
            solidCollider.gameObject.layer = LayerMask.NameToLayer(ground);
            Debug.Log("SOLID");
            }
         if (solid==false){
            solidCollider.gameObject.layer = LayerMask.NameToLayer(ghostBlocker);
            Debug.Log("GHOST");
            }
    }
}
