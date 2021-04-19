using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : Bolt.EntityBehaviour<IMasterPlayerState>
{
    /*
     * 0 :  Fist
     * 1.1: Hammer                1s are weapons
     * 1.2: Bat
     * 2.1: Trap 1
     */
    // if you do not have a weapon, default to a push.
    // fists
    public BoxCollider Fist;   // make this a generic collider for the animations to "enable" when attacking.
    public bool IsAttacking;         // used for preventing walking/jumping when attacking.
    private TrapAttack trap_attack;
    private WeaponAttack wep_attack;

    private Inventory PlayerInventory;
    public PlayerAnimation playerAnim;

    public Transform Hammer;
    public Transform RightArm;
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
        //state.OnChangeWeapon = ToggleWeapon;
        
    }

    public override void SimulateOwner()
    {
        if (GameUI.UserInterface != null && GameUI.UserInterface.Paused) { return; }                            // do not attack if paused.

        if (Input.GetMouseButtonDown(0) && !IsAttacking)     // do not try to punch if the player has a weapon.
        {
            AttackThePlayer();         // change this. No callback needed anymore.
        }
        

        if (state.Weapon != null && Input.GetAxis("Mouse ScrollWheel") != 0 && CanSwitchWeaponsAgain)       // change weapons if you have one and you can switch weapons(Avoid spam).
        {
            PlayerInventory.ChangeItem();       // change item in UI;
            print("SwitchingWeapon");
            
            CanSwitchWeaponsAgain = false;
            Invoke("EnableSwitchingWeapons", 0.5f);            /// make this dependent on a collider.
            ToggleWeapon();
            //state.ChangeWeapon();   // change weapons.
        }
        // if scrolling and have another weapon, change weapon.
    }

    private void AttackThePlayer()          // if statements for trap, weapon, and push
    {
        if(state.Weapon == null)      // if u have no weapon, or you are NOT using your weapon, throw a punch instead.
        {
            PushPlayer();
        }
        else if(!state.Weapon.GetState<IWeapon>().InUse)    // if the weapon is not null, check whether we are using it. If we are not using it, push the player.
        {
            PushPlayer();
        }
        else
        {
            switch (state.WeaponIndex)          // if u continue to here, this means u either have a trap or a weapon.
            {
                case "1.1":
                    SwingHammer();
                    break;
                default:
                    PushPlayer();
                    break;
            }
        }
    }

    private void PushPlayer()       // push player
    {
        print("Pushing That guy");
        playerAnim.ChangeAnimation(AnimationTags.PUNCH);
        Fist.enabled = true;
        IsAttacking = true;
    }

    private void SwingHammer()      // swing your hammer
    {
        print("Swinging Hammer");
        playerAnim.ChangeAnimation(AnimationTags.SWING);
        IsAttacking = true;
    }



    public void DisablePushCollider()
    {
        Fist.enabled = false;
        IsAttacking = false;    
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
            PlayerInventory.InitializeInventory(ItemID);
            Bolt.PrefabId ItemPrefab = c_Item_Types.GetItem(ItemID);
            var Entity = BoltNetwork.Instantiate(ItemPrefab, Hammer.position, Hammer.rotation);
            state.Weapon = Entity;      // reference for the weapon.
            state.WeaponIndex = ItemID;
            Entity.transform.SetParent(RightArm);
            Entity.transform.localPosition = Hammer.localPosition;        // varies from item to item.
            //Entity.transform.localScale = Hammer.localScale;        // ask about size
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

        if(entity.IsOwner)
            PlayerInventory.ChangeItem();
        print("Got a new Item");
        if(state.Weapon == null | state.WeaponIndex == "0")
        {
            if (entity.IsOwner)
            {
                PlayerInventory.DeInitializeInventory();
            }

            state.WeaponIndex = "0";
            return; }

        state.Weapon.transform.SetParent(RightArm);
    }

    public IEnumerator DelayNextAttack()        // run when attacked or switching weapons.
    {
        IsAttacking = true;     // just to avoid attacking again.
        yield return new WaitForSeconds(0.2f);
        IsAttacking = false;
    }
    public void ToggleWeapon()     // incase the user wants to change weapons.
    {

        if(state.Weapon == null)        // if no weapon, just change attack to default attack.
        {
            // do nothing
            //state.OnAttack = PushPlayer;
        }
        else
        {
            StartCoroutine(DelayNextAttack());
            var wep = state.Weapon.GetState<IWeapon>();
            if (entity.IsOwner)
                wep.InUse = !wep.InUse;
            
            if (wep.InUse)
            {
                Debug.LogWarning("Attacking with item");
               // state.OnAttack = null;
            }
            else
            {
                Debug.LogWarning("Attacking with fists!");
              //  state.OnAttack = PushPlayer;
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
                return BoltPrefabs.Hammer_Final;
            case "1.2":
            //return BoltPrefabs.bat;
            case "2.1":
                return BoltPrefabs.ElectricTrap;
            default:
                return BoltPrefabs.Hammer_Final;
        }
    }
}