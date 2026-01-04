using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonScript : MonoBehaviour
{
    [SerializeField] private PlayerController pController;
    [SerializeField] private FirstPersonController fpc;
    public Camera cam;
    public float maxDistance = 2.5f;
    //Note: There is a SEPERATE distance check in ObeliskScript that you must also change, in the inspector?. Search for if (Physics.Raycast(ray, out


    void Update()
    {
        Click();    
    }


    public void Click()
        {
            if (pController.InteractPressedThisFrame && !fpc.inUpgradeMenu && !fpc.inWeaponMenu) // X (controller) or E (keyboard)
                {
                    Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

                    if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
                    {
                        Debug.Log("Clicked: " + hit.collider.name);

                        // try to get a custom component
                        IClickable clickable = hit.collider.GetComponent<IClickable>();
                        if (clickable != null)
                        {
                            //Debug.Log($"test"+pController.gameObject);
                            clickable.OnInteract(pController.gameObject); // pass the player (this GameObject)
                        }
                        if (clickable == null)
                        {
                            Debug.Log("Can't interact with that");
                        }
                    }
                }  
        }
    
    
    public interface IClickable
    {
        void OnInteract(GameObject playerWhoClicked);    //This passes the player who clicked the box to the box
    }
}
 



