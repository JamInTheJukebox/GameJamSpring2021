using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardedTilePlacement : Bolt.EntityBehaviour<IWeapon>
{
    // player should walk to guarded tile to take control of it.
    // will copy the material of the player.
    // deals small damage to players overtime if they are on it.
    #region Claimed
    public GameObject DestroyVFX;
    public float Damage = 0.3f;
    public float CooldownTime = 0.1f;
    private SphereCollider AreaOfAttack;

    public override void Attached()
    {
        state.AddCallback("EntityOwner", InitializeGuardedTile);
        AreaOfAttack = GetComponent<SphereCollider>();
    }


    private void InitializeGuardedTile()        // copy the owner's material.
    {
        if(PlayerDetector != null)
        {
            Destroy(PlayerDetector.gameObject);
        }

        Color GuardedMaterialColor = state.EntityOwner.GetComponent<PlayerPersonalization>().PlayerGraphics.material.color;
        ButtonRender.material.color = GuardedMaterialColor*3;
        print("I am now claimed!");
        ButtonAnim.Play(AnimHashID);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == Tags.PLAYER_TAG)
        {
            if(state.EntityOwner == null)       // if nobody owns the tile, assign ownership
            {
                return;     // do nothing if there is no owner. 
            }
            else
            {
                BoltEntity player = other.GetComponent<BoltEntity>();   // fix this.
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
    #endregion
    #region claimed
    // initial part of script where the tile has no owner.
    private Animator ButtonAnim;
    private int AnimHashID;
    public Renderer ButtonRender;
    public MeshCollider PlayerDetector;
    public Material OwnedMaterial;      // material to assign when Player owns the material.
    private void Awake()
    {
        ButtonAnim = GetComponent<Animator>();
        AnimHashID = Animator.StringToHash("Pushed");
    }
    public void ClaimTile(BoltEntity PlayerEntity)
    {
        print("Claiming Tile!");
        if (BoltNetwork.IsServer)            // only server can assign ownership
            state.EntityOwner = PlayerEntity;
    }
    #endregion
}
