using UnityEngine;
using System.Collections;

public class FastBall : GrenadeBase
{
    public AudioClip maxDistanceCheer;
    private AudioSource fastballAudioSource;

    public override void Awake()
    {
        rb = GetComponent<Rigidbody>();

        fastballAudioSource = GetComponent<AudioSource>();
        if(fastballAudioSource == null){
            fastballAudioSource = gameObject.AddComponent<AudioSource>();}
        fastballAudioSource.spatialBlend = 0; // 2D sound
    }

    public override float NadeModifyingExplosionStrength(float currentStrength, float nadeDistanceFromThrower)
    {
        float minStrength = 1f;   // minimum multiplier
        float maxStrength = 2f;   // maximum multiplier
        float maxDistance = 30f;  // the distance at which the effect hits maxStrength

        float distanceMultiplier = Mathf.Clamp(nadeDistanceFromThrower / maxDistance, 0f, 1f);  //ensures the multiplier stays between 0 and 1.
        if (distanceMultiplier >= 0.99f){
            AudioSource.PlayClipAtPoint(maxDistanceCheer,transform.position,1f);}
        currentStrength = currentStrength * Mathf.Lerp(minStrength, maxStrength, distanceMultiplier);
        
        return currentStrength;
    }
}
