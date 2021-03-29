using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapAttack : Bolt.EntityBehaviour<IWeapon>
{
    Transform player;
    [SerializeField] GameObject TrapToSpawn;
    [SerializeField] float GroundCheckDistance = 5f;
    [SerializeField] LayerMask GroundLayer;
    [SerializeField] int NumberOfUses = 3;
    private MeshRenderer Model;
    private GameObject m_CurrentTile;
    public GameObject CurrentTile
    {
        get
        {
            return m_CurrentTile;
        }
        set
        {
            if(value != m_CurrentTile)      // do this for different tiles
            {
                ToggleAreaEntity(false);
                m_CurrentTile = value;
                ToggleAreaEntity(true);     //show hollogram for new tile;
            }
        }
    }

    private WeaponManager ItemUI;
    private void ToggleAreaEntity(bool entitystate)         // Turns on and off the hologram so the player knows where they will place a trap.
    {
        if(CurrentTile != null)
        {
            var effector = CurrentTile.GetComponent<AreaEffector>();
            if (effector)
            {
                effector.ToggleAreaEntity(entitystate);
            }
        }
    }
    public void InitializeTrapSystem()
    {
        player = GetComponentInParent<Bolt_PlayerController>().GetGroundCheckTransform();
    }

    public override void Attached()
    {
        Model = GetComponent<MeshRenderer>();
        
        state.OnToggleWeapon = Toggleweapon;
        if (entity.IsOwner)
        {
            state.InUse = true;
        }
    }

    public void InitializeUI(WeaponManager wepManager)
    {
        ItemUI = wepManager;
        ItemUI.UpdateItemCount(NumberOfUses.ToString());
    }
    public override void SimulateOwner()
    {
        if (!player) { return; }

        RaycastHit GroundTile;
        if (Physics.Raycast(player.position, Vector3.down, out GroundTile, GroundCheckDistance,GroundLayer))       // use a raycast instead of a spherecast
        {
            Debug.DrawRay(player.position, Vector3.down * GroundCheckDistance, Color.green);
            if (GroundTile.collider.gameObject != null)      // if the ground tile is not equal to null, and the player clicks the left mouse button, they will place down a  trap.
            {
                CurrentTile = GroundTile.collider.gameObject;
                if (Input.GetMouseButtonDown(0))
                {
                    var areaEffector = CurrentTile.GetComponent<AreaEffector>();
                    if (areaEffector)
                    {
                        var successful = areaEffector.PlaceDownTrap(BoltPrefabs.ElectricTrapPlacement);
                        if (!successful) { return; }
                        // subtract one from the count here.
                        NumberOfUses -= 1;
                        ItemUI.UpdateItemCount(NumberOfUses.ToString());
                        if (NumberOfUses <= 0) {
                            GetComponentInParent<WeaponManager>().ResetWeapon();
                            BoltNetwork.Destroy(gameObject);
                        }      // if the item has been used more than x amount of times, destroy it.
                    }
                }
            }
        }
    }

    private void Toggleweapon()
    {
        bool newVal = !this.enabled;
        Model.enabled = newVal;
        ToggleAreaEntity(newVal);
        this.enabled = newVal;
        if (entity.IsOwner)
        {
            state.InUse = newVal;
        }
    }
    private void OnDisable()
    {
        CurrentTile = null;     // turn off all holograms when the trap is deactivated.
    }

}
