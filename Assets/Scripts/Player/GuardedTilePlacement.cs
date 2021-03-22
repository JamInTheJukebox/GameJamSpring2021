using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardedTilePlacement : Bolt.EntityBehaviour<IWeapon>
{
    // player should walk to guarded tile to take control of it.
    // will copy the material of the player.
    // deals small damage to players overtime if they are on it.

    public GameObject DestroyVFX;
    public float Damage = 0.3f;
    public float CooldownTime;
    private SphereCollider AreaOfAttack;

    public override void Attached()
    {
        state.AddCallback("EntityOwner", InitializeGuardedTile);
        AreaOfAttack = GetComponent<SphereCollider>();
    }


    private void InitializeGuardedTile()        // copy the owner's material.
    {
        Material GuardedMaterial = state.EntityOwner.GetComponent<PlayerPersonalization>().PlayerGraphics.material;
        GetComponent<Renderer>().material = GuardedMaterial;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == Tags.PLAYER_TAG)
        {
            if(state.EntityOwner == null)       // if nobody owns the tile, assign ownership
            {
                print("Claiming Tile!");
                if (BoltNetwork.IsServer)            // only server can assign ownership
                    state.EntityOwner = other.GetComponent<BoltEntity>();
                
            }
            else
            {
                BoltEntity player = other.GetComponent<BoltEntity>();
                if (!player.IsOwner) { return; }
                if(state.EntityOwner == player)     // owned guard tile
                {
                    print("This is your guarded tile. Enjoy!!");        // possibly heal player here in random event?
                }
                else// tile that is not owned
                {
                    AreaOfAttack.enabled = false;
                    player.GetComponent<Health>().DamagedByAreaEffector(Damage);
                    Invoke("ResetCollider", CooldownTime);     // needs to be about the same as "Weak stun time"
                }
            }

        }
    }

    private void ResetCollider()
    {
        AreaOfAttack.enabled = true;            // re-enable collider to strike players again
    }

    public void DestroyGuardedTile()
    {
        //Instantiate(DestroyVFX, transform.position, Quaternion.identity);
        if (BoltNetwork.IsServer)
        {
            BoltNetwork.Destroy(gameObject);
        }
    }
}
