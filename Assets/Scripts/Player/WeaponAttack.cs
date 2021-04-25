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
            //FindObjectOfType<Camera>().GetComponentInParent<BoltEntity>().GetState<IMasterPlayerState>().OnAttack = Turn_On_Attack_Joint;
        }
    }

    public void InitializeUI(WeaponManager wepManager)
    {
        ItemUI = wepManager;
        ItemUI.UpdateItemCount(NumberOfHitsLeft.ToString());
    }

    public void UseAttacK()     // toggle only when you successfully hit someone. called by the player you hit.
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
            FindObjectOfType<Camera>().transform.parent.GetComponentInChildren<WeaponManager>().ResetWeapon();          // reset UI weapon.
            BoltNetwork.Destroy(gameObject);
        }
    }

    public void Turn_On_Attack_Joint()
    {
        print("Turning On Attack Joint");
        hitJoint.SetActive(true);
        Invoke("Turn_Off_Attack_Joint", 0.13f);
    }

    public void Turn_Off_Attack_Joint()
    {
        print("Stop Attacking");
        hitJoint.SetActive(false);
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
