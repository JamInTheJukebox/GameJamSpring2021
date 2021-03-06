﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaEffector : MonoBehaviour
{
    // manages spaces with guarded and trapped tiles.
    // Trap tiles cannot be on the same tile as a guarded tile
    // Start is called before the first frame update

    public GameObject AreaEntity;       // the guarded or trap entity itself.
    public GameObject TrapHollogram;
    public GameObject GuardedHologram;
    public LayerMask AreaEffectorLayer;

    public void ToggleAreaEntity(bool state)        // tool for player to know where they lay down a trap;
    {
        TrapHollogram.SetActive(state);
    }

    public bool PlaceDownTrap(Bolt.PrefabId EntityToSpawn)
    {
        if(AreaEntity == null && !CheckForPlacements())                                                          // if there is nothing currently occupying this area, place down trap.
        {
            ToggleAreaEntity(false);        // successfully placed down trap;
            AreaEntity = BoltNetwork.Instantiate(EntityToSpawn, TrapHollogram.transform.position, Quaternion.identity).gameObject;
            return true;
        }
        return false;
    }
    public void PlaceDownGuardedTile(Bolt.PrefabId EntityToSpawn)            
    {
        if (AreaEntity == null && !CheckForPlacements())                                                          // if there is nothing currently occupying this area, place down trap.
        {
            ToggleAreaEntity(false);        // successfully placed down trap;
            Debug.LogWarning(transform.parent.name);
            AreaEntity = BoltNetwork.Instantiate(EntityToSpawn, GuardedHologram.transform.position, new Quaternion(0,0,0,0)).gameObject;
        }
    }

    public void RemoveEntityOnFall()
    {
        if(AreaEntity != null)          // if it is not null, the tile is either guarded or is trapped.
        {
            if(AreaEntity.GetComponent<TrapPlacement>() != null)
            {
                AreaEntity.GetComponent<TrapPlacement>().DestroyTrap();
            }
            else if(AreaEntity.GetComponent<GuardedTilePlacement>() != null)
            {
                AreaEntity.GetComponent<GuardedTilePlacement>().DestroyGuardedTile();
            }
        }
    }

    public bool CheckForPlacements()       // check if there is something on the tile
    {
        return Physics.CheckSphere(TrapHollogram.transform.position, 3, AreaEffectorLayer);
    }
}
