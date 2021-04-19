using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAttack : Bolt.EntityBehaviour<IWeapon>
{
    [SerializeField] GameObject hitJoint;  // the location where the collider is expected to spawn.
    bool ReadyToAttackAgain = true;
    // in case you hit more than 1 person with one attack.
    int NumberOfHitsLeft = 2;
    bool ToggleHitsLeft = false;
    float HitCoolDown = 0.1f;
    public float Damage = 2;
    public GameObject GFX;
    private WeaponManager ItemUI;
    
    public override void Attached()
    {
        string weaponName = gameObject.name.Substring(0, gameObject.name.IndexOf('_'));
        gameObject.name = weaponName;       // for the animator. THIS STEP IS CRITICAL!
        //transform.localScale = Vector3.one * 10;
        /*WeaponAnimator = GetComponent<Animator>();
        WeaponAnimator.enabled = true;
        hitJoint = transform.GetChild(0).gameObject;
        */


        //state.OnToggleWeapon = Toggleweapon;
        state.AddCallback("InUse", Toggleweapon);
        if (entity.IsOwner)
        {
            state.InUse = true; 
        }
    }

    public void InitializeUI(WeaponManager wepManager)
    {
        ItemUI = wepManager;
        ItemUI.UpdateItemCount(NumberOfHitsLeft.ToString());
    }

    public void UseAttacK()     // toggle only when you successfully hit someone.
    {
        if (ToggleHitsLeft) { return; }
        ToggleHitsLeft = true;
        NumberOfHitsLeft -= 1;
        ItemUI.UpdateItemCount(NumberOfHitsLeft.ToString());
        Invoke("ResetHitsLeft", HitCoolDown);
    }

    private void ResetHitsLeft()
    {
        ToggleHitsLeft = false;
        if (NumberOfHitsLeft <= 0)
        {
            BoltNetwork.Destroy(gameObject);
        }
    }

    public void ResetAttackState()                                                                              // able to attack again. Set to false when attacked or when attacking someone.
    {
        ReadyToAttackAgain = true;
    }
    
    public void Turn_On_Attack_Joint()
    {
        hitJoint.SetActive(true);
    }

    public void Turn_Off_Attack_Joint()
    {
        print("Stop Attacking");
        hitJoint.SetActive(false);
    }
    private void AttackPlayer()
    {
        ReadyToAttackAgain = false;
        //mess with animator states here.
        // stop the player from moving if they attack.
    }

    private void Toggleweapon()     // function for setting player attack visuals.
    {
        bool newVal = state.InUse;
        GFX.SetActive(newVal);
        this.enabled = state.InUse;
        /*
        if (entity.IsOwner)
        {
        // set this outside    state.InUse = newVal;
        }*/
    }
}
