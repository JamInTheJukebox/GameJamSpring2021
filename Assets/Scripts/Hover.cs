using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover : Bolt.EntityBehaviour<IWeapon>
{
    #region Variables
    [Header("Hover Properties")]
    public float HoverAmplitude = 3f;
    public float HoverSpeed = 1f;
    public float torqueSpeed = 4f;
    float TimeSinceStartup;
    Vector3 StartPosition;
    #endregion

    public override void Attached()
    {
        transform.localScale = Vector3.one * 10;
        state.SetTransforms(state.WeaponPos, transform);

        if (entity.IsOwner)
            StartPosition = transform.position;
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
