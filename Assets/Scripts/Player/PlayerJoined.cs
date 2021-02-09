using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// only servers should call events.
public class PlayerJoined : Bolt.EntityBehaviour<IMasterPlayerState>
{
    public Camera EntityCamera;
    public GameObject CinematicCamera;
    public override void Attached()
    {
        if (BoltNetwork.IsServer)
        {
            var evnt = PlayerJoinedEvent.Create();
            evnt.Message = "Hello There";
            evnt.Send();
        }

        if (entity.IsOwner)             // only activate the camera for the player.
        {
            EntityCamera.gameObject.SetActive(true);
            CinematicCamera.SetActive(true);
        }
    }
}
