using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SoundBase
{
    private string soundName;
    public AudioSource audioSource;
    public AudioData audioData;
    public UnityEvent OnFinishedPlaying = new UnityEvent();
    public bool isPlaying = false;
    [HideInInspector]
    public bool isPaused = false;
    
    private Coroutine playingMonitor;
    
    
    public SoundBase(AudioData data, string soundName, AudioSource source, bool skipInitialization = false)
    {
        this.audioData = data;
        this.soundName = soundName;
        this.audioSource = source;

        if(!skipInitialization)
            Initialize();
    }

    private void Initialize()
    {
        if(AudioManager.Instance)
            AudioManager.Instance.InitializeSound(soundName, audioSource, out audioData);
    }

    public void Play(bool skipSetup, GameObject emitting = null)
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlayAndSetupSource(audioData, audioSource, skipSetup, emitting);
            playingMonitor = AudioManager.Instance.StartCoroutine(IsPlayingMonitor());
        }
    }

    public void Stop()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.StopAudio(audioSource);
            ResetRoutine();
        }
    }

    /// <summary>
    /// Monitors the isPlaying flag of the selected audiosource, and fires an onfinished event if applicable.
    /// </summary>
    /// <returns></returns>
    private IEnumerator IsPlayingMonitor()
    {
        isPlaying = true;
        
        while (audioSource != null && audioSource.isPlaying)
        {
            yield return null;
        }
        
        isPlaying = false;
        OnFinishedPlaying?.Invoke();

        yield return null;
    }

    private void ResetRoutine()
    {
        if (playingMonitor != null)
        {
            AudioManager.Instance.StopCoroutine(playingMonitor);
            playingMonitor = null;
        }
    }
}
