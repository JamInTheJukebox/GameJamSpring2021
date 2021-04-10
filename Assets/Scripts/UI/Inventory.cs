using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour
{
    // inventory UI shows the player what weapon they are holding.

    public Color DefaultColor = Color.white;
    public Color PlayerColor;

    public float TimeToTransition = 2f;
    private float CurrentChangeTime;
    [Header("Default Slot")]
    public GameObject Slot1;        // default slot when initialized.
    public Image Slot1ColorObj;
    [Header("Item Slot")]
    public GameObject Slot2;        // item slot when initialized
    public Image Slot2ColorObj;
    public Image ItemSlotPNG;
    public TextMeshProUGUI SlotCounter;     // uses left for item.
    private Animator Inventory_Animator;        // when the animation is toggled, lerp the new item to the player color.
    private int InventoryToggleHash;
    private bool initialized;

    [Header("Image Portraits")]
    public Sprite DefPNG;
    public Sprite WeaponPNG;
    public Sprite TrapPNG;

    // Start is called before the first frame update

    private void Awake()
    {
        Inventory_Animator = GetComponent<Animator>();
        InventoryToggleHash = Animator.StringToHash("SwitchWep");
        ItemSlotPNG.sprite = null;
    }

    public void InitializeInventory(Color playerColor)      // get the player color.
    {   
        PlayerColor = playerColor;
        Slot1ColorObj.color = PlayerColor;
        Slot2ColorObj.color = DefaultColor;
    }

    public void InitializeInventory(string typeofItem)
    {
        Sprite item_png = null;
        if(typeofItem == "1.1")
        {
            item_png = WeaponPNG;
        }
        else if(typeofItem == "2.1")
        {
            item_png = TrapPNG;
        }
        ItemSlotPNG.sprite = item_png;
    }
   
    public void DeInitializeInventory()
    {
        ItemSlotPNG.sprite = null;
    }
    public void UpdateCounter(string newCount)
    {
        SlotCounter.text = "x" + newCount;
    }
    // Update is called once per frame
    void Update()
    {

        if(CurrentChangeTime >= 0)
        {
            var Color1 = Slot1ColorObj.color;
            
            CurrentChangeTime -= BoltNetwork.FrameDeltaTime;
            var ratio1 = CurrentChangeTime / TimeToTransition;
            Slot1ColorObj.color = Color.Lerp(DefaultColor, Slot1ColorObj.color,ratio1);
            Slot2ColorObj.color = Color.Lerp(PlayerColor, Slot2ColorObj.color, ratio1);
        }
    }

    public void ChangeItem()        // UI only.
    {
        if (initialized)
        {
            ChangeItemPos();
        }
        Slot1ColorObj.color = PlayerColor;
        Slot2ColorObj.color = DefaultColor;
        initialized = true;
        Inventory_Animator.Play(InventoryToggleHash, -1, 0f);
        CurrentChangeTime = TimeToTransition;
        
    }

    public void ChangeItemPos()         // change gameobject Hierarchy in order to only use one animation.
    {
        print("Changing Pos");
        var temp = Slot1.transform.parent;
        Slot1.transform.SetParent(Slot2.transform.parent);
        Slot1.transform.localPosition = Vector3.zero;
        Slot1.transform.SetAsFirstSibling();
        Slot2.transform.SetParent(temp);
        Slot2.transform.localPosition = Vector3.zero;
        Slot2.transform.SetAsFirstSibling();
    }
}
