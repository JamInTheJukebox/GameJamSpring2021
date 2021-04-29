using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionBasedSound : Bolt.EntityBehaviour<IMasterPlayerState>
{
    public AudioClip BlackHoleSFX;

    private void OnTriggerEnter(Collider other)
    {
        if (!entity.IsOwner) { return; }

        if(other.tag == Tags.SOUND_TAG)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(BlackHoleSFX);
        }
    }
}
