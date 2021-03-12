using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Bolt.EntityBehaviour<IMasterPlayerState>
{
    // CONTAINS THE HEALTH AND COLLISION MANAGER
    public float PlayerHealth = 3;


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
            state.Health += Delta;              // take away health with Delta < 0

            print("LOSING HEALTH!!");
        }
    }

    private void HealthCallback()
    {
        if (state.LostGame) { return; }
        if (state.Health <= 0)     // run this code if the health is less than 0 and the player has not lost the game yet.
            LoseGame();
        if (entity.IsOwner)
        {
            if(GameUI.UserInterface != null)
                GameUI.UserInterface.UpdateHealth_UI(state.Health);
        }
    }

    public override void SimulateOwner()
    {
        if (Input.GetKeyDown(KeyCode.K))
            ChangeHealth(-1);
    }

    private void OnTriggerEnter(Collider other)
    {
        // handle any damage here!
        if (!entity.IsOwner) { return; }
        if(other.tag == Tags.ATTACK_TAG)
        {
            if (!other.transform.IsChildOf(transform))
            {
                ChangeHealth(-1);
                print("GOT HIT!!");
            }
        }

        else if(other.tag == Tags.WEAPON_TAG)
        {
            print("Got ITEM!!");
            var new_Item = ItemPickedUpEvent.Create();
            new_Item.PlayerEntity = GetComponent<BoltEntity>();
            new_Item.ItemEntity = other.GetComponent<BoltEntity>();
            new_Item.Send();
        }
        else if(other.tag == Tags.DEATHZONE_TAG)
        {
            LoseGame();
        }

    }

    public void LoseGame()
    {
        if (!entity.IsOwner) { return; }
        print("LOST THE GAME!");
        state.LostGame = true;
        // disable the UI here.
        transform.position = SpawnPositionManager.instance.LobbySpawnPosition.position; 
        GameUI.UserInterface.HealthUI.transform.parent.gameObject.SetActive(false);                 // disable the health bar if you lost!
    }
}
