using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Audio data scriptable object for variable value storage
/// </summary>
[Serializable]
public class AudioData
{
    [SerializeField]
    public string soundName;

    [SerializeField] public AudioSourceVariables sourceVariables;
}

/// <summary>
/// Everything we need to author the audio source
/// </summary>
[Serializable]
public class AudioSourceVariables
{
    [Header("Clip")]
    [SerializeField]
    public List<AudioClip> clips = new List<AudioClip>();

    [Header("Flags")] 
    [SerializeField] 
    public bool playDelayed = false;
    [SerializeField] 
    public float initialPlayDelay = 0f;
    [SerializeField] 
    public bool isOneShot = false;
    [SerializeField]
    public bool mute = false;
    [SerializeField]
    public bool spatialize = false;
    [SerializeField]
    public bool bypassEffects = false;
    [SerializeField]
    public bool bypassListenerEffects = false;
    [SerializeField]
    public bool bypassReverbZones = false;
    [SerializeField]
    public bool playOnAwake = true;
    [SerializeField]
    public bool loop = false;
    
    [Header("Ranges")]
    [Range(0,256)]
    [SerializeField]
    public int priority = 128;
    [Range(0,1)]
    [SerializeField]
    public float volume = 1;
    [Range(-3,3)]
    [SerializeField]
    public float pitch = 1;
    [Range(-1,1)]
    [SerializeField]
    public float pan = 0;
    [Range(0,1)]
    [SerializeField]
    public float spatialBlend = 0;
    [Range(0,1.1f)]
    [SerializeField]
    public float reverbZoneMix = 1;
    
    [Header("3D Sound Settings")] 
    [Range(0, 5)]
    [SerializeField]
    public float dopplerLevel = 1;
    [Range(0, 360)] 
    [SerializeField]
    public int spread;
    [SerializeField]
    public AudioRolloffMode rolloffMode;
    [Min(0)]
    [SerializeField]
    public int minDistance = 1;
    [Min(0)]
    [SerializeField]
    public int maxDistance = 500;

    /// <summary>
    /// Helper function to apply saved authered data to a given audiosource
    /// </summary>
    /// <param name="sourceToAuthor"></param>
    /// <returns></returns>
    public AudioSource ApplyDataToSource(AudioSource sourceToAuthor, bool randomClip = false)
    {
        if(randomClip)
            SetSourceClip(sourceToAuthor, clips[Random.Range(0, clips.Count)]);
        else
            SetSourceClip(sourceToAuthor, clips[0]);
        
        sourceToAuthor.mute = mute;
        sourceToAuthor.spatialize = spatialize;
        sourceToAuthor.bypassEffects = bypassEffects;
        sourceToAuthor.bypassListenerEffects = bypassListenerEffects;
        sourceToAuthor.bypassReverbZones = bypassReverbZones;
        sourceToAuthor.playOnAwake = playOnAwake;
        sourceToAuthor.loop = loop;
        sourceToAuthor.volume = volume;
        sourceToAuthor.priority = priority;
        sourceToAuthor.pitch = pitch;
        sourceToAuthor.panStereo = pan;
        sourceToAuthor.spatialBlend = spatialBlend;
        sourceToAuthor.reverbZoneMix = reverbZoneMix;
        sourceToAuthor.rolloffMode = rolloffMode;
        sourceToAuthor.dopplerLevel = dopplerLevel;
        sourceToAuthor.spread = spread;
        sourceToAuthor.minDistance = minDistance;
        sourceToAuthor.maxDistance = maxDistance;
        
        return sourceToAuthor;
    }
    
    /// <summary>
    /// Helper function to set the audiosource clip
    /// </summary>
    /// <param name="sourceToAuthor"></param>
    /// <param name="clip"></param>
    /// <returns></returns>
    public AudioSource SetSourceClip(AudioSource sourceToAuthor, AudioClip clip)
    {
        sourceToAuthor.clip = clip;
        return sourceToAuthor;
    }
}
