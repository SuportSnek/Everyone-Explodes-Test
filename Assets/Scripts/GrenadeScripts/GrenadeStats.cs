using UnityEngine;

[CreateAssetMenu(menuName = "Grenades/GrenadeStats")]
public class GrenadeStats : ScriptableObject
{
    public string GrenadeName;
    public GameObject nadePrefab;
    public GameObject weaponInvUI;
    public GameObject weaponMenuUIPrefab;
    public GameObject visualPrefab;
    public AudioClip explosionClip;
    public AudioClip throwClip;
    public AudioClip specialMusic;

    public bool isSpecial;  
    public float dropWeight = 1f;

    [Header("Stats (CD=9, S=12.5, R=10, tF=10)")]
    public float Cooldown;
    public float Strength;
    public float explosionRadius;
    public float visualRadiusMultiplier;
    public float throwForce;
    public float duration;   //Time building last, time inverse curse lasts, time for frag explode
    public bool freezeRotationOnThrow = false;  
    public bool remoteDetonation = false;  
    
    [Header("SpawnStuff (For/Back=1.65, Up/Down=0.4)")]
    public float spawnPosForwardBackMultiplier = 1.65f;
    public float spawnPosUpDownMultiplier = 0.4f;
    public float upThrowAngle;
    public Quaternion rotationOffset = Quaternion.identity;

    [Header("Building stuff")]
    public bool isBuilding;
    public float fireRate;
    public float armTime;   //Time it takes for buildings to first activate
}

