using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class FloatingTimerUI : MonoBehaviour
{
    public GameObject Owner { get; set; }   //The Owner is the person this script is referring to, which might be DIFFERENT than grenadeOwner
    public GameObject grenadeOwner;   // Who threw the nade


    [Header("Ownership")]
    public Transform target;           // Object the UI floats above
    public float height = 3f;          // World space height
    public Camera assignedCamera;      // THIS PLAYER'S camera
    public TextMeshProUGUI timerText;


    private void Start()
    {
        // If the user forgets, auto-detect a target
        if (target == null)
            target = transform.parent;
    }
    private void Update()
    {
        
    }

    private void LateUpdate()
    {
        if (!target || Owner == null || assignedCamera == null) return;

        // --- 1. Position above object ---
        transform.position = target.position + Vector3.up * height;

        // --- 2. Billboard toward THIS PLAYER ONLY ---
        Vector3 dir = transform.position - assignedCamera.transform.position;
        transform.rotation = Quaternion.LookRotation(dir);

        GrenadeBase gb = target.GetComponent<GrenadeBase>();
        float scale = gb != null ? gb.floatingTimerScale : 1f;

        if(gb.stats.GrenadeName == "Slurpinator"){
            Vector3 newScale = new Vector3(1, 5, 1);
            transform.localScale = newScale * scale;}
        else{
            transform.localScale = Vector3.one * scale;}
        

        
        if (grenadeOwner == Owner)
        {// Viewer is the grenade owner → BLUE
            timerText.color = Color.blue;}
        else
        {// Viewer is NOT the grenade owner → RED
            timerText.color = Color.red;}

    }



    public void AssignOwner(GameObject viewer, GameObject nadeOwner)//this stupid function is required because Start isn't fast enough at assigning owner
    {
        Owner = viewer;                   // viewer of this UI
        grenadeOwner = nadeOwner;         // actual grenade thrower
        assignedCamera = viewer.GetComponentInChildren<Camera>();
    }



    public void SetTime(float seconds)
    {
        timerText.text = seconds.ToString("F1");
    }
}


//Note to self: a Transform stores an object's Vector3 (position), it's Quanternion (rotation), it's Vector3 (scale). The three in the top-right of the inspector!
//Every single GameObject always has a Transform.