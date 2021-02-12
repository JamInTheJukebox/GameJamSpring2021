using UnityEngine;

public class EventManager : Bolt.GlobalEventListener
{
    private void Awake()
    {
        if (BoltNetwork.IsServer)
        {
            var entity = BoltNetwork.Instantiate(BoltPrefabs.Hammer_ItemBlock, new Vector3(0, 0.2f, 0), Quaternion.Euler(5.293f, -92.402f, 65.55f));
        }
    }
    public override void OnEvent(ItemPickedUpEvent evnt)
    {
        if (BoltNetwork.IsServer)
        {
            BoltNetwork.Destroy(evnt.ItemEntity);
        }

        if (evnt.FromSelf)
        {
            evnt.PlayerEntity.GetComponentInChildren<WeaponManager>().SpawnWeapon();
            //BoltNetwork.Instantiate(BoltPrefabs.hammer_low, new Vector3(0,0.2f,0))
        }
    }

   
    public override void OnEvent(PlayerJoinedEvent evnt)
    {
        Debug.LogWarning(evnt.Message);
    }

    public override void OnEvent(SetPlayerPersonalizationEvent evnt)
    {
        
    }

    public override void OnEvent(GetWeaponEvent evnt)
    {
        print(evnt.PlayerEntity.gameObject.name);
        evnt.Weapon.transform.SetParent(evnt.PlayerEntity.transform.GetChild(1).Find("WeaponSlot"));
    }
}
