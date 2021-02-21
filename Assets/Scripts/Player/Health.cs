using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Bolt.EntityBehaviour<IMasterPlayerState>
{
    // CONTAINS THE HEALTH AND COLLISION MANAGER
    private float m_PlayerHealth = 3;
    public float PlayerHealth
    {
        get { return m_PlayerHealth; }
        set {
            if (m_PlayerHealth != value)
            { m_PlayerHealth = value;
                if (m_PlayerHealth <= 0)
                    BoltNetwork.Destroy(gameObject);

            }
        }
    }

    public override void Attached()
    {
        if (entity.IsOwner)
        {
            state.Health = PlayerHealth;
        }
        state.AddCallback("Health", HealthCallback);            // we changed a state. When the state is changed, the server will call the callback on everyone's computer.

    }

    private void HealthCallback()
    {
        PlayerHealth = state.Health;
    }

    public override void SimulateOwner()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        // handle any damage here!
        if (!entity.IsOwner) { return; }
        if(other.tag == Tags.ATTACK_TAG)
        {
            if (!other.transform.IsChildOf(transform))
            {
                state.Health -= 1;
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

    }
}
