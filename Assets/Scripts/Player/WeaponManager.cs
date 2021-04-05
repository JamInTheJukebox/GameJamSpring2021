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
    // if you do not have a weapon, default to a push.
    // fists
    public BoxCollider Fist;   // make this a generic collider for the animations to "enable" when attacking.
    public bool CanPushAgain = true;

    private TrapAttack trap_attack;
    private WeaponAttack wep_attack;

    private Inventory PlayerInventory;

    // switching weapons
    private bool CanSwitchWeaponsAgain = true;
    public override void Attached()
    {
        if (entity.IsOwner)
        {
            state.WeaponIndex = "0";            // spawn with nothing but fists!
            PlayerInventory = FindObjectOfType<Inventory>();
        }
        state.AddCallback("Weapon", SetWeapon);              // spawn the weapon when 
        state.OnAttack = PushPlayer;
        state.OnChangeWeapon = ToggleWeapon;
        
    }

    public override void SimulateOwner()
    {
        if (Input.GetMouseButtonDown(0) && CanPushAgain && state.OnAttack != null)     // do not try to punch if the player has a weapon.
        {
            state.Attack();
        }
        

        if (state.Weapon != null && Input.GetAxis("Mouse ScrollWheel") != 0 && CanSwitchWeaponsAgain)       // change weapons if you have one and you can switch weapons(Avoid spam).
        {
            PlayerInventory.ChangeItem();       // change item in UI;
            print("SwitchingWeapon");
            CanSwitchWeaponsAgain = false;
            Invoke("EnableSwitchingWeapons", 0.3f);            /// make this dependent on a collider.
            state.ChangeWeapon();   // change weapons.
        }
        // if scrolling and have another weapon, change weapon.
    }

    private void PushPlayer()
    {
        print("Pushing That guy");
        Fist.enabled = true;
        CanPushAgain = false;
        Invoke("DisablePushCollider", 0.1f);            /// make this dependent on a collider.
    }

    private void DisablePushCollider()
    {
        Fist.enabled = false;
        CanPushAgain = true;
    }

    private void EnableSwitchingWeapons()
    {
        CanSwitchWeaponsAgain = true;
    }
    public void InitializeItem(string ItemID)                       // initialize only weapons and traps
    {
        if (entity.IsOwner)
        {
            if(PlayerInventory == null)
            {
                PlayerInventory = FindObjectOfType<Inventory>();
            }
            PlayerInventory.ChangeItem();
            PlayerInventory.InitializeInventory(ItemID);
            Bolt.PrefabId ItemPrefab = c_Item_Types.GetItem(ItemID);
            var Entity = BoltNetwork.Instantiate(ItemPrefab, Hammer_Transform.position, Hammer_Transform.rotation);
            state.Weapon = Entity;      // reference for the weapon.
            state.WeaponIndex = ItemID;
            Entity.transform.SetParent(transform);
            Entity.transform.localPosition = Hammer_Transform.localPosition;        // varies from item to item.
            if(ItemID == "2.1")
            {
                var trap = Entity.GetComponent<TrapAttack>();
                trap.InitializeTrapSystem();
                trap.InitializeUI(this);
            }
            else if (ItemID == "1.1")
            {
                var wep = Entity.GetComponent<WeaponAttack>();
                wep.InitializeUI(this);
            }
            /*
            var evnt = GetWeaponEvent.Create();
            evnt.Weapon = Entity;
            evnt.PlayerEntity = GetComponentInParent<BoltEntity>();
            evnt.Send();
            */
        }
    }

    public void UpdateItemCount(string Count)       // tell the user how many hits they have left.
    {
        PlayerInventory.UpdateCounter(Count);
    }
    public void SetWeapon()
    {
        print("Got a new Item");
        if(state.Weapon == null | state.WeaponIndex == "0")
        {
            if (entity.IsOwner)
            {
                PlayerInventory.ChangeItem();
                PlayerInventory.DeInitializeInventory();
            }

            state.WeaponIndex = "0";
            state.OnAttack = PushPlayer;        // go back to default attack.
            return; }

        state.Weapon.transform.SetParent(transform);
        // add state.onattack.
        state.OnAttack = null;
    }

    public void ToggleWeapon()     // incase the user wants to change weapons.
    {
        if(state.Weapon == null)
        {
            // do nothing
            state.OnAttack = PushPlayer;
        }
        else
        {
            if(entity.IsOwner)
                state.Weapon.GetState<IWeapon>().ToggleWeapon();
            
            if (!state.Weapon.GetState<IWeapon>().InUse)
            {
                Debug.LogWarning("Attacking with item");
                state.OnAttack = null;
            }
            else
            {
                Debug.LogWarning("Attacking with fists!");
                state.OnAttack = PushPlayer;
            }
        }
    }

    public void ResetWeapon()
    {
        state.Weapon = null;
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