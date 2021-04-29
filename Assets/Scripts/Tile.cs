using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] int LayerID = 0;       // 0 for center, 1 for layer 1, etc.
    public MeshRenderer TheTile;
    [SerializeField] Material Danger_Tile_Material;          // tile turns red.
    [SerializeField] Material Safe_Tile_Material;            // normal material of the tile
    [SerializeField] LayerMask layerMask;           // change the layer to avoid having the platform collide with other platforms.
    Rigidbody PlatformMotion;
    [SerializeField] float Drag = 0.3f;
    bool Is_Falling;
    float TimeBeforeItFallsFullSpeed = 2f;
    [SerializeField] ParticleSystem DangerVFX;
    [SerializeField] Transform DangerLoc;

    [SerializeField] SphereCollider RangedAttackCollider;           // collider to damage players
    [SerializeField] bool EnableDamage = true;                      // debug variable. It gets annoying walking around and getting damaged.

    public void DiscardTile()
    {
        TheTile.material = Danger_Tile_Material;
        TheTile.gameObject.layer = 10;
        TheTile.gameObject.AddComponent<Rigidbody>();
        PlatformMotion = TheTile.GetComponent<Rigidbody>();
        PlatformMotion.drag = Drag;   // tile will fall
        PlatformMotion.isKinematic = true;
        Is_Falling = true;
        GetComponentInChildren<AreaEffector>().RemoveEntityOnFall();
    }

    private void Update()
    {
        if (Is_Falling)
        {
            transform.position = transform.position + Vector3.down * 1 * BoltNetwork.FrameDeltaTime;
            TimeBeforeItFallsFullSpeed -= BoltNetwork.FrameDeltaTime;
        }

        if(TimeBeforeItFallsFullSpeed < 0 && Is_Falling)
        {
            Is_Falling = false;
            PlatformMotion.isKinematic = false;
            Vector3 RandomTorque = new Vector3(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
            PlatformMotion.AddTorque(RandomTorque);
        }
    }

    public void SetDanger()
    {
        TheTile.material = Danger_Tile_Material;
    }

    public void SpawnDanger()
    {
        if(TheTile.material != Danger_Tile_Material)
            SetDanger();
        if (EnableDamage)
            RangedAttackCollider.enabled = true;
        DangerVFX.Play();
    }

    public void SetSafe()           // add this to the delegate in the tile_manager.
    {
        RangedAttackCollider.enabled = false;
        TheTile.material = Safe_Tile_Material;
    }

}
