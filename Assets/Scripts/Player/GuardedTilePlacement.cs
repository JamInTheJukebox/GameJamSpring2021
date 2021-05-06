using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GuardedTilePlacement : Bolt.EntityBehaviour<IWeapon>
{
    // player should walk to guarded tile to take control of it.
    // will copy the material of the player.
    // deals small damage to players overtime if they are on it.
    #region Claimed
    public float Damage = 0.3f;
    public float CooldownTime = 0.1f;
    private SphereCollider AreaOfAttack;
    public TextMeshProUGUI OwnerText;
    [Header("ParticleEffects")]
    public GameObject DestroyVFX;
    public ParticleSystem BoltVFX;
    public ParticleSystem BoltVFX2;
    public ParticleSystem HealVFX;

    // add destroy method for when it has been on the board for too long.
    List<GameObject> players = new List<GameObject>();
    private IEnumerator coroutineGuardTile;
    private float colorIntensity;

    [Header("Healing")]
    public float HealRate = 0.1f;
    public float LifeTime = 60f;
    private float HealingTime = 30;     // time it takes for the tile to initialize its "heal" phase.
    public float HealingDuration = 10f;  // the time the tile will be healing for.

    [Header("Audio")]
    [SerializeField] AudioClip GuardTileClaimed;
    [SerializeField] AudioClip LightningSFX;
    public override void Attached()
    {
        state.AddCallback("EntityOwner", InitializeGuardedTile);
        state.AddCallback("Healing", HealingCallback);
        AreaOfAttack = GetComponent<SphereCollider>();
        BoltVFX.Stop();
        BoltVFX2.Stop();
        HealVFX.Stop();
        

        if (entity.IsOwner)     // only entity owner can turn healing on. effects are observed on clients.
        {
            Invoke("EndGuardLifetime", LifeTime);
            HealingTime = LifeTime / 2 + Random.Range(-5f, 10f);
            Invoke("InitializeHeal", HealingDuration);
        }
    }

    private void InitializeHeal()
    {
        if (entity.IsOwner)
        {
            state.Healing = true;
        }
        Invoke("StopHeal", HealingTime);
    }

    private void StopHeal()
    {
        if (entity.IsOwner)
        {
            state.Healing = false;
        }
    }
    private void EndGuardLifetime()
    {
        if (entity.IsOwner)
        {
            BoltNetwork.Destroy(entity);
        }
    }
    private void InitializeGuardedTile()        // run when an owner is assigned to this guarded tile.
    {
        if(PlayerDetector != null)
        {
            Destroy(PlayerDetector.gameObject);
        }
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(GuardTileClaimed);
        AreaOfAttack.enabled = true;
        PlayerDetector.enabled = false;
        ButtonRender.material = OwnedMaterial;
        PlayerPersonalization ownerPersonalization = state.EntityOwner.GetComponent<PlayerPersonalization>();
        colorIntensity = Player_Colors.GetColorIntensity(ownerPersonalization.GetColor());
        OwnerText.text = ownerPersonalization.GetName() + "'s tile";

        Color GuardedMaterialColor = state.EntityOwner.GetComponent<PlayerPersonalization>().PlayerGraphics.material.color;
        ButtonRender.material.color = GuardedMaterialColor;
        ButtonRender.material.SetColor("_EmissionColor", GuardedMaterialColor * colorIntensity*0.6f); // change this multiplier.
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
            if (players.Contains(other.gameObject))
            {
                return;
            }
            else
            {
                BoltVFX.Play();
                BoltVFX2.Play();
                players.Add(other.gameObject);
                BoltEntity player = other.GetComponent<BoltEntity>();  
                if (!player.IsOwner) { return; }            // only continue for local machine here. not a server based event.
                // apply damage recursion function here.
                if(state.EntityOwner == player)     // owned guard tile
                {
                    if (!state.Healing) { return; }
                    if (coroutineGuardTile == null)
                        coroutineGuardTile = player.GetComponent<Health>().AreaEffectorDeltaHealth(HealRate, CooldownTime);
                    StartCoroutine(coroutineGuardTile);
                    print("This is your guarded tile. Enjoy!!");        // possibly heal player here in random event?
                }
                else// tile that is not owned
                {
                    if (coroutineGuardTile == null)
                    {
                        coroutineGuardTile = player.GetComponent<Health>().AreaEffectorDeltaHealth(-Damage, CooldownTime);
                    }
                    Invoke("PlayBoltSFX", 4f);
                    StartCoroutine(coroutineGuardTile);
                    
                }
            }
            // cancel the invoke with on-trigger-exit.
        }
    }
    private void PlayBoltSFX()
    {
        if(BoltVFX.isPlaying)     // if it is still playing, play the sfx again and loop
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(LightningSFX,1.2f);
            Invoke("PlayBoltSFX", 4f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == Tags.PLAYER_TAG && state.EntityOwner != null)
        {
            if (players.Contains(other.gameObject))       // if nobody owns the tile, assign ownership
            {
                // stop when the list is 0, or when
                players.Remove(other.gameObject);
                if(players.Count == 0)
                {
                    BoltVFX.Stop();
                    BoltVFX2.Stop();
                }
                BoltEntity player = other.GetComponent<BoltEntity>();
                if (!player.IsOwner) { return; }
                // cancel damage here.
                if(coroutineGuardTile != null)
                    StopCoroutine(coroutineGuardTile);
            }
        }
    }

    private void HealingCallback()
    {
        // use state.healing here.
        if (state.Healing)
        {
            HealVFX.Play();
        }
        else
        {
            HealVFX.Stop();
        }
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
    #region BeforeClaimed
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
    public void ClaimTile(BoltEntity PlayerEntity, string color,string playerName)
    {
        if (state.EntityOwner != null) { return; }       // do not reassign ownership if someone has claimed this tile.
        print("Claiming Tile!");
        if (BoltNetwork.IsServer)            // only server can assign ownership        // handle this in the guarded tile!
            state.EntityOwner = PlayerEntity;

    }
    #endregion
}
