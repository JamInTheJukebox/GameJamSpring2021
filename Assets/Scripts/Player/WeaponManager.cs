using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : Bolt.EntityBehaviour<IMasterPlayerState>
{
    [SerializeField] Transform Hammer_Transform;

    public void SpawnWeapon() // Equip The Weapon
    {
        if (entity.IsOwner)
        {
            var Entity = BoltNetwork.Instantiate(BoltPrefabs.hammer, Hammer_Transform.position, Hammer_Transform.rotation);
            Entity.transform.SetParent(transform);
            Entity.transform.localPosition = Hammer_Transform.localPosition;
            var evnt = GetWeaponEvent.Create();
            evnt.Weapon = Entity;
            evnt.PlayerEntity = GetComponentInParent<BoltEntity>();
            evnt.Send();
        }

    }
}
