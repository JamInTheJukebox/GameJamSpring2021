using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Bolt.EntityBehaviour<IMasterPlayerState>
{
    // CONTAINS THE HEALTH AND COLLISION MANAGER
    public float PlayerHealth = 3;
    public float MaxPlayerShield = 2;      // 

    private float CurrentShield;
    public float StunTime = 1.5f;
    public float WeakStunTime = 0.3f;       // damage you take when you are in fields of damage.
    [HideInInspector] public bool Hit = false;        // Stunned to provide i frames for x seconds. i frames will only work against actual weapons and the push. No protection against traps or guard tile at this time(But it is easy to add).
    private bool AreaEffectorHit = false;
    Bolt_PlayerController PlayerController;


    public override void Attached()
    {
        PlayerController = GetComponent<Bolt_PlayerController>();
        if (entity.IsOwner)
        {
            state.Health = PlayerHealth;
            state.LostGame = false;         // player did not lose the game yet.
        }
        state.AddCallback("Health", HealthCallback);            // we changed a state. When the state is changed, the server will call the callback on everyone's computer.
    }

    public void InitializeHealthUI()        // called when players enter the game room
    {
        GameUI.UserInterface.InitializeHealth(state.Health);            // REFERENCE ERROR!!!
    }

    public void ChangeHealth(float Delta)
    {
        if(GameManager.instance.Game_Started && entity.IsOwner)       // if the game has started and you are the owner, change the health
        {
            if(CurrentShield > 0)               // shield
            {
                CurrentShield += Delta;
                GameUI.UserInterface.UpdateShield(CurrentShield);
            }
            else
            {
                state.Health += Delta;              // take away health with Delta < 0
                if (state.Health <= 0)     // run this code if the health is less than 0 and the player has not lost the game yet.
                    LoseGame();
            }

            print("LOSING HEALTH!!");
        }
    }

    private void HealthCallback()
    {
        if (state.LostGame) { return; }                     // do not update anything if the player has already lost the game
        if (entity.IsOwner)
        {
            if(GameUI.UserInterface != null)
                GameUI.UserInterface.UpdateHealth_UI(state.Health);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // handle any damage here!
        if (!entity.IsOwner) { return; }
        if (other.tag == Tags.ATTACK_TAG)
        {
            if (!other.transform.IsChildOf(transform) && !Hit)          // line to not hurt yourself. Also, do not run this code if you have already been hurt.
            {
                PlayerController.MajorDamage();
                var evnt = SuccessfulAttackEvent.Create();      // successfully attacked!
                evnt.WeaponDamage = other.transform.parent.parent.parent.GetComponent<WeaponAttack>().Damage;
                evnt.WeaponEntity = other.transform.parent.parent.parent.GetComponent<BoltEntity>();
                evnt.Send();
            }
        }
        else if (other.tag == Tags.PUSH_TAG)
        {
            if (!other.transform.IsChildOf(transform) && !Hit)          // line to not hurt yourself. Also, do not run this code if you have already been hurt.
            {
                Hit = true;
                Invoke("ResetHit", StunTime);
                PlayerController.MinorDamage();
            }
        }
        else if(other.tag == Tags.SHIELD_TAG)
        {
            print("Got ITEM!!");
            var new_Item = ItemPickedUpEvent.Create();
            new_Item.PlayerEntity = GetComponent<BoltEntity>();     
            new_Item.ItemEntity = other.GetComponent<BoltEntity>();
            new_Item.Send();
            CurrentShield = MaxPlayerShield;
            GameUI.UserInterface.InitializeShield(CurrentShield);
            // add callback here.
            // run shield here.
        }

        else if(other.tag == Tags.WEAPON_TAG && state.Weapon == null)               // pick up an item if you have no weapon or trap.
        {
            print("Got ITEM!!");
            var new_Item = ItemPickedUpEvent.Create();
            new_Item.PlayerEntity = GetComponent<BoltEntity>();                                     // get the player who picked up the  entity.
            new_Item.ItemEntity = other.GetComponent<BoltEntity>();                                 // get Itembox for the host to destroy
            new_Item.ItemType = c_Item_Types.Items[other.GetComponent<Hover>().TypeOfItem];         // get Item ID
            new_Item.Send();
        }
        else if(other.tag == Tags.DEATHZONE_TAG)
        {
            LoseGame();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!entity.IsAttached) { return; }
        if (!entity.IsOwner) { return; }
        if (other.tag == Tags.AREA_EFF_TAG && !AreaEffectorHit)
        {
            print("Getting Hurt by tiles");
            AreaEffectorHit = true;
            ChangeHealth(-0.1f);
            Invoke("ResetEffectorHit", WeakStunTime);       // reset hit variable to get attacked again
        }
    }

    private void ResetEffectorHit()
    {
        AreaEffectorHit = false;
    }

    public void StunnedByTrap(float damage) // group of players can get stunned by a trap.
    {
        if (!entity.IsOwner) { return; }    // getting hit by a trap will guarantee damage to you. However, it gives you some i-frames for some other attacks such as hamemrs. YOu can't just get hit by something and try to set off all the traps!.
        Hit = true;
        ChangeHealth(-damage);
        PlayerController.MajorDamage();

        Invoke("ResetHit", StunTime);       // check movement script if still stunned.
        print("GOT HIT!!");
    }

    public void DamagedByWeapon(float damage)
    {
        Hit = true;
        ChangeHealth(-1);
        Invoke("ResetHit", StunTime);
        print("GOT HIT!!");
    }

    private void ResetHit()     // reset invisibility-frames.
    {
        Hit = false;
    }

    public void LoseGame()
    {
        if (!entity.IsOwner) { return; }
        print("LOST THE GAME!");
        state.LostGame = true;
        // disable the UI here.
        GameUI.UserInterface.HealthUI.transform.parent.gameObject.SetActive(false);                 // disable the health bar if you lost!

        var evnt = LoseGameEvent.Create();
        evnt.Player = GetComponent<BoltEntity>();
        evnt.Send();
    }

    public IEnumerator AreaEffectorDeltaHealth(float delta, float restTime)         // can be used to heal or damage players. Depends on what the guarded tile placement says! Fixed when you leave the collider.
    {
        while (true)
        {
            ChangeHealth(delta);
            yield return new WaitForSeconds(restTime);
        }
    }
}
