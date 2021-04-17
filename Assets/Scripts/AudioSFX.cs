using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "AudioObj", menuName = "ScriptableObjects/AudioObject", order = 1)]
public class AudioSFX : ScriptableObject            // used to manage each type of SFX sound.
{
    [Range(0,1)] public float Source_volume = 1;

    public List<AudioClip> SFX_Sounds = new List<AudioClip>();

    public float GetSourceVolume()          // each sfx is for some reason created at a different volume
    {
        return Source_volume;
    }

    public AudioClip GetRandomSFXsound()
    {
        var sfx = SFX_Sounds[Random.Range(0, SFX_Sounds.Count)];
        return sfx;
    }

    public void PlayRandomSFX()
    {
        AudioManager.Instance.PlaySFX(GetRandomSFXsound(), GetSourceVolume());
    }
}
