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
    [HideInInspector] public bool Hit = false;        // Stunned to provide i frames
 

    public override void Attached()
    {
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
                Hit = true;
                ChangeHealth(-1);
                Invoke("ResetHit",StunTime);
                print("GOT HIT!!");
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

    private void GiveShield()
    {

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
}
