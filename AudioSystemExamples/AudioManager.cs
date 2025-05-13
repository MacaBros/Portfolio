using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public GameObject audioPoolParent;

    [Header("AudioSource Prefab")] 
    public AudioSource audioSourceGenericPrefab;

    [Header("Audio Bank Manager")]
    public AudioBankManager audioBankManager;

    [Header("Audio Loader")] 
    public AudioLoader audioLoader;
   
    [Header("Logging")] 
    public bool doLogs = false;

    //private
    private AudioSourcePool audioSourcePool;
    private Coroutine curveEvaluation;

    private void Start()
    {
        if(Instance != null)
            return;
        
        DontDestroyOnLoad(gameObject);
        SetStaticInstanceReference();
        SetupAudioSourcePool();
    }

    /// <summary>
    /// Creates a static reference to the class
    /// </summary>
    private void SetStaticInstanceReference()
    {
        if (Instance == null)
            Instance = this;
    }

    /// <summary>
    /// Initialize and prewarm the audio source pool
    /// </summary>
    private void SetupAudioSourcePool()
    {
        audioSourcePool = new AudioSourcePool(audioSourceGenericPrefab);
    }

    private void CheckForInitialDelay(AudioData data, AudioSource source)
    {
        if (data.sourceVariables.playDelayed)
        {
            source.PlayDelayed(data.sourceVariables.initialPlayDelay);
        }
        else
        {
            source.Play();
        }
    }

    /// <summary>
    /// Called from Sound.cs component, 
    /// </summary>
    /// <param name="soundName"></param>
    /// <param name="audioSource"></param>
    /// <param name="data"></param>
    public void InitializeSound(string soundName, AudioSource audioSource, out AudioData data, bool randomClip = false)
    {
        data = GetAudioData(soundName);

        if (data != null && data.sourceVariables != null)
        {
            if(audioSource == null)
                audioSource = GetAudioSourceFromPool();
            
            SetupAudioSource(data, audioSource, randomClip: randomClip);
            if (data.sourceVariables.playOnAwake)
            {
                CheckForInitialDelay(data, audioSource);
            }
        }
        else
        {
            Debug.LogWarning($"Could not retrieve data for sound: {soundName}");
        }
    }
    
    /// <summary>
    /// Standard but delayed playback
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="sourceVariablesInitialPlayDelay"></param>
    private void PlayDelayed(AudioSource audioSource, float sourceVariablesInitialPlayDelay)
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.PlayDelayed(sourceVariablesInitialPlayDelay);
        }
    }

    /// <summary>
    /// Helper function to be used by the Radio to call play. 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="source"></param>
    /// <param name="isRadioOn"></param>
    /// <param name="objectEmittingSound"></param>
    public SoundBase PlayAndSetupSource(AudioData data, AudioSource source, bool isRadioOn = false, bool isCurrentlySelectedStation = false,  GameObject objectEmittingSound = null)
    {
        var soundBase = new SoundBase(data, data.soundName, source, false);
        
        if (isRadioOn && isCurrentlySelectedStation)
        {
            source.mute = false;
        }
        else 
        {
            source.mute = true;
        }
        
        soundBase.Play(true, objectEmittingSound);

        return soundBase;
    }

    /// <summary>
    /// Entry point for calling play from sound.cs or SoundBase
    /// </summary>
    /// <param name="audioSource"></param>
    public void PlayAndSetupSource(AudioData data, AudioSource audioSource, bool skipSetup = false, GameObject emitting = null)
    {
        InternalLoggingHandler("IMN - PlayAndSetupSource - hit");

        if (data == null || data.sourceVariables == null)
        {
            Debug.LogError("Null audio data or sourceVariables on PlayAndSetupSource in audiomanager");
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetAudioSourceFromPool();
            
            if (emitting != null)
                audioSource.transform.position = emitting.transform.position;
        }

        if (audioSource != null)
        {
            if (data.sourceVariables.isOneShot)
            {
                audioSource.PlayOneShot(audioSource.clip);
            }
            else
            {
                if(!audioSource.isPlaying)
                    CheckForInitialDelay(data, audioSource);
            }
        }
    }

    private void InternalLoggingHandler(string message)
    {
        if (doLogs)
        {
            Debug.Log(message);
        }
    }

    public SoundBase PlayGrabbable(string soundToPlayName, GameObject objectEmittingSound = null)
    {
        return Play(soundToPlayName, objectEmittingSound);
    }

    /// <summary>
    /// Plays audio using a pooled audiosource and repositions it if there's a gameobject argument
    /// </summary>
    /// <param name="soundToPlayName"></param>
    /// <param name="objectEmittingSound"></param>
    public SoundBase Play(string soundToPlayName, GameObject objectEmittingSound = null)
    {
        if (string.IsNullOrEmpty(soundToPlayName) || soundToPlayName == "<None>" || soundToPlayName == "<none>")
        {
            //Debug.LogWarning($"Audio manager - caught a n empty or none sfx on objectEmittingSound {objectEmittingSound}");
            return null;
        }

        var data = GetAudioData(soundToPlayName);

        if (data == null)
        {
            //LogExtension.Log("IMN", $"soundToPlayName is {soundToPlayName} but data is null, sound is most likely not setup correctly");
            if(objectEmittingSound != null)
                Debug.LogWarning($"Audio manager - play and setupsource data is null - soundToPlayName {soundToPlayName}, objectEmittingSound is {objectEmittingSound}");
            return null;
        }
        else
        {
            var source = GetAudioSourceFromPool();

            if (objectEmittingSound != null)
            {
                source.transform.position = objectEmittingSound.transform.position;
            }
                
            var setupSound = new SoundBase(data, soundToPlayName, source);

            //play the audio
            if(data.sourceVariables.playDelayed)
                PlayDelayed(source, data.sourceVariables.initialPlayDelay);
            else
            {
                setupSound.Play(false);
            }

            return setupSound;
        }
    }

    /// <summary>
    /// Stops the selected audiosource if it's playing
    /// </summary>
    /// <param name="audioSource"></param>
    public void StopAudio(AudioSource audioSource)
    {
        if (audioSource != null && audioSource.isPlaying)
             audioSource.Stop();
    }
    
    
    /// <summary>
    /// Retrieves the audioData by searching through all banks, also returns the data if successful
    /// </summary>
    /// <param name="soundName"></param>
    /// <param name="audioSource"></param>
    /// <param name="objectEmittingSound"></param>
    /// <returns></returns>
    private AudioData GetAudioData(string soundName)
    {
        return audioLoader.loadedBanks.SelectMany(b => b.soundData.Where(ad => ad.soundName == soundName)).FirstOrDefault();
    }

    /// <summary>
    /// Positions audiosource if required, using the data it, populates variables & wires in the mixer channel.
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="audioData"></param>
    /// <param name="objectEmittingSound"></param>
    private void SetupAudioSource(AudioData audioData, AudioSource audioSource, GameObject objectEmittingSound = null, bool force2d = false, bool randomClip = false)
    { 
        //Would parenting be better?
        if(objectEmittingSound != null)
            audioSource.transform.position = objectEmittingSound.transform.position;
        
        audioSource = audioData.sourceVariables.ApplyDataToSource(audioSource, randomClip);

        if (force2d)
        {
            audioSource.spatialize = false;
            audioSource.spatialBlend = 0f;
        }
    }

    /// <summary>
    /// Helper function to return an audiosource from the pool
    /// </summary>
    private AudioSource GetAudioSourceFromPool()
    {
        return audioSourcePool.GetPooledAudioSource(audioPoolParent);
    }

    /// <summary>
    /// Helper to get a clips length
    /// </summary>
    /// <param name="value"></param>
    public float GetClipLength(string value)
    {
        var data = GetAudioData(value);
        if (data != null)
        {
            return data.sourceVariables.clips[0].length;
        }
        else
        {
            return 0;
        }
    }
    
    /// <summary>
    /// Generic setting of an exposed audio mixer variable value
    /// </summary>
    /// <param name="mixer"></param>
    /// <param name="exposedParam"></param>
    /// <param name="value"></param>
    public static void SetAudioMixerParameter(AudioMixer mixer, string exposedParam, float value)
    {
        mixer.SetFloat(exposedParam, value);
    }
}
