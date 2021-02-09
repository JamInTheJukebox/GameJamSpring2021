using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAttack : Bolt.EntityBehaviour<IMasterPlayerState>
{
    [SerializeField] GameObject hitJoint;  // the location where the collider is expected to spawn.
    [SerializeField] Animator WeaponAnimator;
    bool ReadyToAttackAgain = true;

    public override void Attached()
    {
        state.OnAttack = AttackPlayer;
    }
    public override void SimulateOwner()
    {
        if (Input.GetMouseButtonDown(1) && ReadyToAttackAgain && entity.IsOwner)
        {
            state.Attack();
            print("Is Attacking");
        }
    }

    public void ResetAttackState()
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
        WeaponAnimator.SetTrigger(AnimationTags.ATTACK);   
        //mess with animator states here.
        // stop the player from moving if they attack.
    }
    
}
