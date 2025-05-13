using UnityEngine;

public class AudioSourcePool : GenericComponentPool<AudioSource>
{
    public int prewarmAmount = 15;
    private GameObject audioSourcePoolParentObject;
    
    /// <summary>
    /// Constructor for an audio source pool, calls prewarm by default
    /// </summary>
    /// <param name="prefab"></param>
    public AudioSourcePool(AudioSource prefab) : base(prefab)
    {
        Prewarm();
    }

    /// <summary>
    /// Calls GetPooledAudioSource() to instantiate a specified number of sources on initial construction.
    /// It will also move the object to the supplied parent gameobject 
    /// </summary>
    private void Prewarm()
    {
        audioSourcePoolParentObject = AudioManager.Instance.audioPoolParent;
        
        for (int j = 0; j < prewarmAmount; j++)
        {
            GetPooledAudioSource(audioSourcePoolParentObject);
        }
    }

    /// <summary>
    /// Checks whether or not the audiosource is playing.
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    protected override bool IsActive(AudioSource component)
    {
        return component.isPlaying;
    }

    /// <summary>
    /// Calls base method Get() to spawn or reuse an available audiosource.
    /// </summary>
    /// <returns></returns>
    public AudioSource GetPooledAudioSource(GameObject parent)
    {
        var src = Get(parent);
        
        if(src.isPlaying)
            src.Stop();
        
        src.clip = null;
        src.loop = false;

        return src;
    }
    
    /// <summary>
    /// Method to Stop pooled sources from playing on scene exit.
    /// </summary>
    /// <returns></returns>
    private void StopAudioSources()
    {
        foreach (var pSource in pool)
        {
            if(pSource.isPlaying)
                pSource.Stop();
        }
    }
}
