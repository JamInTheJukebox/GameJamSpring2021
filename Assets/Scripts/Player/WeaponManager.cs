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
    private bool m_IsAttacking;         // used for preventing walking/jumping when attacking.
    public bool IsAttacking
    {
        get { return m_IsAttacking; }
        set
        {
            if (value != m_IsAttacking)
            {
                m_IsAttacking = value;
                if(!m_IsAttacking)
                {
                    DisableAttack();
                }
            }
        }
    }

    private bool CanAttackAgain = true;

    private void DisableAttack()
    {
        if(CanAttackAgain == false) { return; }
        CanAttackAgain = false;
        Invoke("EnableAttack", 0.2f);
    }

    private void EnableAttack()
    {
        CanAttackAgain = true;
    }

    // Dependencies
    //private TrapAttack trap_attack;
    private WeaponAttack wep_attack;
    private Inventory PlayerInventory;
    public PlayerAnimation playerAnim;
    private Bolt_PlayerController PlayerController;

    public Transform Hammer;
    public Transform Chicken;
    public Transform BombTrap;
    public Transform RightArm;
    public float WeaponSpeedMultiplier;     // when wielding a hammer or chicken, you do not move when attacking. When punching, you have a speed of 0.7f
    // switching weapons
    private bool CanSwitchWeaponsAgain = true;
    public override void Attached()
    {
        if (entity.IsOwner)
        {
            state.WeaponIndex = "0";            // spawn with nothing but fists!
            PlayerInventory = FindObjectOfType<Inventory>();
        }
        PlayerController = GetComponentInParent<Bolt_PlayerController>();
        state.AddCallback("Weapon", SetWeapon);              // spawn the weapon when 
        state.OnAttack = null;        // 
        //state.OnChangeWeapon = ToggleWeapon;
        
    }

    public override void SimulateOwner()
    {
        if (GameUI.UserInterface != null && GameUI.UserInterface.Paused) { return; }                            // do not attack if paused.
        bool SpecialCondition = !PlayerController.CheckGrounded() | PlayerController.CheckStunned();            // do not attack if you are NOT grounded or if you are stunned.
        if (SpecialCondition) { return; }
        if (Input.GetMouseButtonDown(0) && CanAttackAgain)     // do not try to punch if the player has a weapon.
        {
            IsAttacking = true;
            AttackThePlayer();         // change this. No callback needed anymore.
        }
        

        if (state.Weapon != null && Input.GetAxis("Mouse ScrollWheel") != 0 && CanSwitchWeaponsAgain && CanAttackAgain && !IsAttacking)       // change weapons if you have one and you can switch weapons(Avoid spam).
        {
            PlayerInventory.ChangeItem();       // change item in UI;
            CanAttackAgain = false;
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
                case "1.3":
                    SwingChicken();
                    break;
                case "2.1":
                    SetTrapAnimation();
                    break;
                default:
                    PushPlayer();
                    break;
            }
        }
    }
    #region Fist
    private void PushPlayer()       // push player
    {
        print("Pushing That guy");
        WeaponSpeedMultiplier = 0.7f;
        playerAnim.ChangeAnimation(AnimationTags.PUNCH);
    }

    public void EnablePushCollider()
    {
        Fist.enabled = true;
        print("enable Push");
        Invoke("DisablePushCollider", 0.13f);
    }

    public void DisablePushCollider()
    {
        print("disable Push");
        Fist.enabled = false;
    }
    #endregion

    #region otherWeapons
    private void SwingHammer()      // swing your hammer.       // enable collider here.
    {
        print("Swinging Hammer");
        WeaponSpeedMultiplier = 0f;
        playerAnim.ChangeAnimation(AnimationTags.SWING);
    }

    private void SwingChicken()
    {
        print("Swinging Chicken");
        WeaponSpeedMultiplier = 0f;
        playerAnim.ChangeAnimation(AnimationTags.CHICKENSLAP);
    }

    public void EnableGenericCollider()         // enable the weapon's collider
    {
        print("ATTACK PLAYER!!!");
        if(wep_attack == null) { wep_attack = state.Weapon.GetComponent<WeaponAttack>(); }
        if(wep_attack == null) { Debug.LogError("WeaponManager.cs: There is no weapon attack found on the weapon. Please attach this behavior."); return; }
        wep_attack.Turn_On_Attack_Joint();
    }
    #endregion

    private void SetTrapAnimation()
    {
        print("Setting Trap");
        WeaponSpeedMultiplier = 0.7f;
        playerAnim.ChangeAnimation(AnimationTags.TRAPSET);

    }
    private void EnableSwitchingWeapons()
    {
        CanSwitchWeaponsAgain = true;
        CanAttackAgain = true;
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
            Entity.transform.SetParent(SetWeaponParent());                         // spawn under the hammer to copy its animation.
            Entity.transform.localPosition = Vector3.zero;                         // varies from item to item.
            Entity.transform.localRotation = new Quaternion(0,0,0,0);

            if (ItemID == "2.1")
            {
                var trap = Entity.GetComponent<TrapAttack>();
                trap.InitializeTrapSystem(GetComponentInParent<Bolt_PlayerController>().GetGroundCheckTransform(), this);
                trap.InitializeUI(this);
            }
            else if (ItemID == "1.1" | ItemID == "1.3")
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

    public void SetWeapon()         // used to set the weapon on the people who are not the owner of this weapon.
    {
        if(state.WeaponIndex == "1.1" | state.WeaponIndex == "1.3")
        {
            if(state.Weapon != null)
                wep_attack = state.Weapon.GetComponent<WeaponAttack>();     // call the turn on attack joint function from this.
        }

        if (entity.IsOwner)
            PlayerInventory.ChangeItem();
        if(state.Weapon == null | state.WeaponIndex == "0")
        {
            if (entity.IsOwner)
            {
                PlayerInventory.DeInitializeInventory();
                state.WeaponIndex = "0";
            }

            return; }
        state.Weapon.transform.SetParent(SetWeaponParent());
        if (state.WeaponIndex == "1.3" | state.WeaponIndex == "2.1")         // reset chicken to the size of the chicken parent.
        {
            state.Weapon.transform.localScale = Vector3.one;
        }

    }

    private Transform SetWeaponParent()     // add for chicken, and trap.
    {
        switch (state.WeaponIndex)
        {
            case "1.1":
                return Hammer;
            case "1.3":
                return Chicken;
            case "2.1":
                return BombTrap;
            default:
                return RightArm;
        }
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
            DisableAttack();
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

    public void DelayNextAttack()        // run when attacked or switching weapons.
    {
        IsAttacking = false;            // go to setter function
    }
}

public class c_Item_Types
{
    public const string Default = "0";
    // weapons
    public const string Hammer = "1.1";
    public const string Bat = "1.2";
    public const string Chicken = "1.3";
    // 
    public const string ElectricTrap = "2.1";

    public static Dictionary<Item_Type, string> Items = new Dictionary<Item_Type, string>
    {
        [Item_Type.Bat] = Bat,
        [Item_Type.Hammer] = Hammer,
        [Item_Type.Electric_Trap] = ElectricTrap,
        [Item_Type.Chicken] = Chicken
    };

    public static Bolt.PrefabId GetItem(string Item)
    {
        switch (Item)
        {
            case "1.1":
                return BoltPrefabs.Hammer_Final;
            case "1.2":
            //return BoltPrefabs.bat;
            case "1.3":
                return BoltPrefabs.Chicken;
            case "2.1":
                return BoltPrefabs.BombTrap;
            default:
                return BoltPrefabs.Hammer_Final;
        }
    }
}