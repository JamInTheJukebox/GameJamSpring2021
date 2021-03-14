using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : Bolt.EntityBehaviour<IMasterPlayerState>
{
    [SerializeField] Transform Hammer_Transform;
    /*
     * 0 :  Fist
     * 1.1: Hammer                1s are weapons
     * 1.2: Bat
     * 2.1: Trap 1
     */
    public override void Attached()
    {
        if(entity.IsOwner)
        {
            state.WeaponIndex = "0";            // spawn with nothing but fists!
        }
        state.AddCallback("WeaponIndex", SetWeapon);              // spawn the weapon when 
    }

    public void SpawnWeapon() // Equip The Weapon
    {
        if (entity.IsOwner)
        {
            var Entity = BoltNetwork.Instantiate(BoltPrefabs.hammer, Hammer_Transform.position, Hammer_Transform.rotation);
            state.WeaponIndex = "1.1";
            state.Weapon = Entity;      // reference for the weapon.
            Entity.transform.SetParent(transform);
            Entity.transform.localPosition = Hammer_Transform.localPosition;
            /*
            var evnt = GetWeaponEvent.Create();
            evnt.Weapon = Entity;
            evnt.PlayerEntity = GetComponentInParent<BoltEntity>();
            evnt.Send();
            */
        }

    }

    public void SetWeapon()
    {
        if(state.Weapon == null | state.WeaponIndex == "0")
        { return; }
        state.Weapon.transform.SetParent(transform);
    }
}
