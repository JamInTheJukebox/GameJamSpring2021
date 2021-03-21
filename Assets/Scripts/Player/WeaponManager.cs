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
        if (entity.IsOwner)
        {
            state.WeaponIndex = "0";            // spawn with nothing but fists!
        }
        state.AddCallback("Weapon", SetWeapon);              // spawn the weapon when 
    }

    public void InitializeItem(string ItemID)                       // initialize only weapons and traps
    {
        if (entity.IsOwner)
        {
            Bolt.PrefabId ItemPrefab = c_Item_Types.GetItem(ItemID);
            var Entity = BoltNetwork.Instantiate(ItemPrefab, Hammer_Transform.position, Hammer_Transform.rotation);
            state.Weapon = Entity;      // reference for the weapon.
            state.WeaponIndex = ItemID;
            Entity.transform.SetParent(transform);
            Entity.transform.localPosition = Hammer_Transform.localPosition;        // varies from item to item.
            if(ItemID == "2.1")
            {
                Entity.GetComponent<TrapAttack>().InitializeTrapSystem();
            }
            /*
            var evnt = GetWeaponEvent.Create();
            evnt.Weapon = Entity;
            evnt.PlayerEntity = GetComponentInParent<BoltEntity>();
            evnt.Send();
            */
        }

    }

    private BoltEntity GetItem(string Weapon_Index)          // get Item from weaponIndex
    {
        switch (Weapon_Index)
        {

            default:
                return null;
        }
    }
    public void SetWeapon()
    {
        if(state.Weapon == null | state.WeaponIndex == "0")
        { return; }
        state.Weapon.transform.SetParent(transform);
    }
}

public class c_Item_Types
{
    public const string Default = "0";
    // weapons
    public const string Hammer = "1.1";
    public const string Bat = "1.2";
    // 
    public const string ElectricTrap = "2.1";

    public static Dictionary<Item_Type, string> Items = new Dictionary<Item_Type, string>
    {
        [Item_Type.Bat] = Bat,
        [Item_Type.Hammer] = Hammer,
        [Item_Type.Electric_Trap] = ElectricTrap
    };

    public static Bolt.PrefabId GetItem(string Item)
    {
        switch (Item)
        {
            case "1.1":
                return BoltPrefabs.hammer;
            case "1.2":
            //return BoltPrefabs.bat;
            case "2.1":
                return BoltPrefabs.ElectricTrap;
            default:
                return BoltPrefabs.hammer;
        }
    }
}