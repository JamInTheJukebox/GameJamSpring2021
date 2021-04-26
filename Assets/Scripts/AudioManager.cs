using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    #region Static Instance
    private static AudioManager m_instance;
    public static AudioManager Instance
    {
        get
        {
            return m_instance;
        }
        private set
        {
            m_instance = value;
        }
    }
    #endregion

    #region Fields
    private AudioSource musicsource;                                // song 1 in case we want to crossfade from one song to another
    private AudioSource musicsource2;                               // song 2 to crossfade from song 1 to 2.
    private AudioSource sfxsource;                                  // sound Effect Source.

    private bool firstMusicSourceIsPlaying = true;
    //unity fix
    public AudioClip musicClip;
    //public static AudioMixer audioMixer = null;
    #endregion
    private float GlobalVolume;
    private void Awake()
    {
        //Makes sure the instance doesn't get destroyed


        //Creates and Saves audio sources as references
        Instance = this;
        musicsource = this.gameObject.AddComponent<AudioSource>();
        musicsource2 = this.gameObject.AddComponent<AudioSource>();
        sfxsource = this.gameObject.AddComponent<AudioSource>();
        //This loops the music tracks
        EnableLooping();
        PlayMusic(musicClip);       // plays the level song

    }

    public void PlayMusic(AudioClip musicClip, int StartPosition = 0)           // plays song
    {
        //Figures out which clip is playing
        AudioSource activeSource = (firstMusicSourceIsPlaying) ? musicsource : musicsource2;        // (boolean variable) Assign this if true; else assign this if false

        activeSource.clip = musicClip;
        activeSource.volume = 0.25f;
        activeSource.Play();
        //activeSource.time = StartPosition;
        if (StartPosition != 0)
            activeSource.timeSamples = StartPosition;
    }

    public void PlayMusicwithFade(AudioClip newClip, float transitionTime = 1.5f)       // plays song with fade, then to new song
    {
        //Figures out which clip is playing
        AudioSource activeSource = (firstMusicSourceIsPlaying) ? musicsource : musicsource2;

        StartCoroutine(UpdateMusicWithFade(activeSource, newClip, transitionTime));
    }

    public void PlayMusicwithCrossFade(AudioClip newClip, float transitionTime = 1.0f)   // crossfades to new song
    {
        //Figures out which clip is playing
        AudioSource activeSource = (firstMusicSourceIsPlaying) ? musicsource : musicsource2;
        AudioSource newSource = (firstMusicSourceIsPlaying) ? musicsource2 : musicsource;

        //Swap the music playing
        firstMusicSourceIsPlaying = !firstMusicSourceIsPlaying;

        //set the fields of the audio sources, and start the corutine for crossfading
        newSource.clip = newClip;   // um why didnt this man change this
        newSource.Play();

        StartCoroutine(UpdateMusicWithCrossFade(activeSource, newSource, transitionTime));
    }


    private IEnumerator UpdateMusicWithFade(AudioSource activesource, AudioClip newClip, float transitionTime)
    {
        //Makes sure the source is active and playing
        if (!activesource.isPlaying)
            activesource.Play();

        float setTransitiontime = 0.0f;

        //fades out the music
        for (setTransitiontime = 0; setTransitiontime < transitionTime; setTransitiontime += Time.deltaTime)
        {
            activesource.volume = (1 - (setTransitiontime / transitionTime));
            yield return null;
        }

        activesource.Stop();
        activesource.clip = newClip;
        activesource.Play();

        //fades in the music
        for (setTransitiontime = 0; setTransitiontime < transitionTime; setTransitiontime += Time.deltaTime)
        {
            activesource.volume = (setTransitiontime / transitionTime);
            yield return null;
        }
    }

    private IEnumerator UpdateMusicWithCrossFade(AudioSource original, AudioSource newSource, float transitionTime)
    {
        float transt = 0.0f;
        for (transt = 0.0f; transt <= transitionTime; transt += Time.deltaTime)
        {
            original.volume = (1 - (transt / transitionTime));
            newSource.volume = (transt / transitionTime);
            yield return null;
        }
        original.Stop();
        SetMusicVolume(0.6f);
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxsource.PlayOneShot(clip);
    }
    public void PlaySFX(AudioClip clip, float volume)
    {
        sfxsource.PlayOneShot(clip, volume);
    }

    public void StopMusic()
    {
        AudioSource activeSource = (firstMusicSourceIsPlaying) ? musicsource : musicsource2;
        activeSource.Stop();
        activeSource.time = 0;
    }

    //Sets the volume for the music and sfx in the game(like through an options menu)
    public void SetMusicVolume(float Volume)
    {
        musicsource.volume = Volume;
        musicsource2.volume = Volume;
    }

    public void SetSFXVolume(float Volume)
    {
        sfxsource.volume = Volume;
    }

    public void EnableLooping()
    {
        musicsource.loop = true;
        musicsource2.loop = true;
    }
    public void DisableLooping()
    {
        musicsource.loop = false;
        musicsource2.loop = false;
    }
}