using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Item_Type
{
    Hammer, Chicken, Bat, Shield, Electric_Trap
}

public class Hover : Bolt.EntityBehaviour<IWeapon>
{
    #region Variables
    public Item_Type TypeOfItem;
    [Header("Hover Properties")]
    public float HoverAmplitude = 0.9f;
    public float HoverSpeed = 3f;
    public float torqueSpeed = 100f;
    float TimeSinceStartup;
    Vector3 StartPosition;
    public float WeaponLifeTime = 20f;
    #endregion

    public override void Attached()
    {
        //transform.localScale = Vector3.one * 10;
        state.SetTransforms(state.WeaponPos, transform);

        if (entity.IsOwner)
        {
            StartPosition = transform.position;
            Invoke("GetRidOfItem", WeaponLifeTime);         // delete the item after 20s if nobody picks it up.
        }
    }
    private void GetRidOfItem()
    {
        BoltNetwork.Destroy(entity);
    }
    public override void SimulateOwner()
    {
        float Vec_Y = Mathf.Sin(BoltNetwork.Time * HoverSpeed) * HoverAmplitude;
        transform.position = StartPosition + new Vector3(0, Vec_Y, 0);
        HandleTorque();
    }

    #region Custom Methods

    void HandleTorque()
    {
        transform.Rotate(torqueSpeed * BoltNetwork.FrameDeltaTime * Vector3.up,Space.World);
    }
    #endregion
}
