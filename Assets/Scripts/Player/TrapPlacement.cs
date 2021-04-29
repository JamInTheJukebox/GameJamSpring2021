using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapPlacement : Bolt.EntityBehaviour<IWeapon>      // in charge of storing damage value, and initializing destruction sequence when player walks on trap or the tile falls.
{
    public float Damage = 2;
    public GameObject DestructionVFX;
    public float WaitTimeToInitializeTrap = 1f;             // time the player has to get away from the trap.
    public bool ReadyToAttack;
    [Header("Rendering")]
    public Color bombUnlitColor;
    public Color bombLitColor;
    public Renderer BombTip;
    private float LitIntensity = 4f;
    private Color currentColor;
    private float Intensity;
    [Header("Audio")]
    public AudioClip ExplosionSound;
    public AudioClip TrapSet;
    public override void Attached()
    {
        state.SetTransforms(state.WeaponPos, transform);
        Invoke("InitializeTrap", WaitTimeToInitializeTrap);
    }
    private void InitializeTrap()       // to avoid damaging the player who sets down the trap. Give them time to move away.
    {
        currentColor = bombLitColor;
        Intensity = LitIntensity;
        BombTip.material.color = currentColor;
        BombTip.material.SetColor("_EmissionColor", currentColor * Intensity); // change this multiplier.
        Invoke("LoopMaterial", 0.4f);
        // set color here.
        GetComponent<SphereCollider>().enabled = false;
        ReadyToAttack = true;
        GetComponent<SphereCollider>().enabled = true;  // to avoid having someone inside the trap. If they are inside the trap before readytoAttack is set to true, then the collision will not be counted and they will be standing inside of a trap that refuses to be set off!
    }

    private void LoopMaterial()
    {
        currentColor = (currentColor == bombUnlitColor) ? bombLitColor : bombUnlitColor;            // if the color is unlit, move to the lit color.
        Intensity = (Intensity <= 0.1f) ? 3.5f : 0;
        BombTip.material.color = currentColor;
        BombTip.material.SetColor("_EmissionColor", currentColor * Intensity); // change this multiplier.
        Invoke("LoopMaterial", 0.4f);
    }

    public void DestroyTrap()
    {
        // instantiate destruction vfx        
        if (entity.IsOwner)
        {
            BoltNetwork.Destroy(gameObject);    // only owner can destroy this entity.
        }
    }


    private void OnTriggerEnter(Collider other)         // test with other.
    {
        if (!ReadyToAttack) { return; }     // if not ready to attack, go back
        if (!other.GetComponentInParent<Health>()) { return; }
        Instantiate(DestructionVFX, transform.position + new Vector3(0,2.5f,0), Quaternion.identity);
        GetComponent<SphereCollider>().radius = 2;
        print("In blast Range. Taking Damage!");
        other.GetComponentInParent<Health>().StunnedByTrap(Damage);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(ExplosionSound);              // play explosion sfx
        DestroyTrap();
    }
}
