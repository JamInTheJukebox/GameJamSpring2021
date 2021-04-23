using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// only servers should call events.
public class PlayerJoined : Bolt.EntityBehaviour<IMasterPlayerState>
{
    public Camera EntityCamera;
    public GameObject CinematicCamera;
    [HideInInspector] public PlayerPersonalization Personalization;

    public override void Attached()
    {
        Personalization = GetComponent<PlayerPersonalization>();

        if (BoltNetwork.IsServer)
        {
            var evnt = PlayerJoinedEvent.Create();
            evnt.Message = "Hello There";
            evnt.Send();
        }

        if (entity.IsOwner)             // only activate the camera for the player. Nobody else's camera!
        {
            Personalization.SetName();
            Personalization.SetHat_Eye();
            EntityCamera.gameObject.SetActive(true);
            CinematicCamera.SetActive(true);
            var evntPly = GetPlayerPersonalizationEvent.Create();
            evntPly.PlayerEntity = entity;
            evntPly.Send();
        }
    }
}
