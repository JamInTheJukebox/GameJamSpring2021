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
                        areaEffector.PlaceDownTrap(BoltPrefabs.ElectricTrapPlacement);
                        NumberOfUses -= 1;
                        if(NumberOfUses <= 0) { BoltNetwork.Destroy(gameObject); }      // if the item has been used more than x amount of times, destroy it.
                    }
                }
            }
        }
    }

    private void OnDisable()
    {
        CurrentTile = null;     // turn off all holograms when the trap is deactivated.
    }
}
