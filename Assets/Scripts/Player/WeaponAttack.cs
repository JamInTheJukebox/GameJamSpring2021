using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAttack : Bolt.EntityBehaviour<IWeapon>
{
    [SerializeField] GameObject hitJoint;  // the location where the collider is expected to spawn.
    [SerializeField] Animator WeaponAnimator;
    bool ReadyToAttackAgain = true;
    bool Collected;

    public override void Attached()
    {
        transform.localScale = Vector3.one * 10;
        /*WeaponAnimator = GetComponent<Animator>();
        WeaponAnimator.enabled = true;
        hitJoint = transform.GetChild(0).gameObject;
        */
        state.OnAttack = AttackPlayer;
        state.SetAnimator(WeaponAnimator);
        //state.SetTransforms(state.WeaponPos, transform);
        //         state.SetTransforms(state.PlayerTransform, transform);
    }
    public override void SimulateOwner()
    {
        if (GameUI.UserInterface != null && GameUI.UserInterface.Paused) { return; }                            // do not attack if paused.
        if (Input.GetMouseButtonDown(1) && ReadyToAttackAgain && entity.IsOwner)
        {
            state.Attack();
            print("Is Attacking");
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
        state.Animator.SetTrigger(AnimationTags.ATTACK);   
        //mess with animator states here.
        // stop the player from moving if they attack.
    }
    
}
