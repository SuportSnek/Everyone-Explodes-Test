using UnityEngine;
using System.Collections;


public class C4 : GrenadeBase
{
    public enum C4State
    {
        Unarmed,
        Armed,
        Detonated
    }

    public AudioClip c4ArmedClip;

    public C4State State { get; private set; } = C4State.Unarmed;

    [SerializeField] private float armDelay = 1f;

    protected override void Start()
    {
        StartCoroutine(ArmAfterDelay());
        if (Owner != null){
            ownerFPC = Owner.GetComponent<FirstPersonController>();
            ownerPController = Owner.GetComponent<PlayerController>();}
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        
    }

    private IEnumerator ArmAfterDelay()
    {
        yield return new WaitForSeconds(armDelay);
        State = C4State.Armed;
        audioSource.PlayOneShot(c4ArmedClip, 0.5f);
    }

    public bool CanDetonate()
    {
        return State == C4State.Armed;
    }

    public override void Explode()
    {
        if (State != C4State.Armed)
            return;

        State = C4State.Detonated;
        base.Explode();
    }
}
