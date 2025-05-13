using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Sound : MonoBehaviour
{
    public string soundName;
    
    [HideInInspector]
    public AudioSource audioSource;

    [HideInInspector] 
    public AudioData audioData;

    public UnityEvent OnStartedPlaying = new UnityEvent();
    public UnityEvent OnFinishedPlaying = new UnityEvent();
    
    [HideInInspector]
    public bool isPlaying = false;

    [HideInInspector]
    public bool isPaused = false;
    
    [HideInInspector]
    public bool shouldExitBeforePlaying;

    //privates
    private bool queuedPlay = false;
    private bool initialised = false;
    private bool initialiseStarted = false;
    private Coroutine startRoutine;
    private AudiosourceVariator attachedVariator;


    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="soundName"></param>
    /// <param name="source"></param>
    public Sound(string soundName, AudioSource source)
    {
        this.soundName = soundName;
        this.audioSource = source;
        Initialize();
    }

    public void Awake()
    {
        CheckForAudioSource();
    }

    /// <summary>
    /// Assigns audiosource component reference if needed.
    /// </summary>
    private void CheckForAudioSource()
    {
        if(audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public virtual void Start()
    {
        startRoutine = StartCoroutine(Start_Co());
    }

    /// <summary>
    /// Grabs a reference to the attached audiosource and then calls to the audiomanager class to initialize it.
    /// </summary>
    protected virtual IEnumerator Start_Co()
    {
        initialiseStarted = true;
        
        while (AudioManager.Instance == null)
        {
            yield return null;
        }
        
        while (AudioManager.Instance.audioLoader.isLoading)
        {
            yield return null;
        }

        CollectVariators();
        
        if (!initialised)
            Initialize();
    }

    private void OnDestroy()
    {
        if (startRoutine != null)
        {
            StopCoroutine(startRoutine);
            startRoutine = null;
        }
    }

    private void OnEnable()
    {
        if (initialiseStarted && !initialised)
        {
            Debug.LogWarning("redoing initialise on Sound " + gameObject.name);
            StartCoroutine(Start_Co());
        }
    }
    private void OnDisable()
    {
        if(initialiseStarted && !initialised)
            Debug.LogWarning("initialise interrupted on Sound " + gameObject.name);
    }

    /// <summary>
    /// Collects any attached variators in list form
    /// </summary>
    private void CollectVariators()
    {
        attachedVariator = GetComponent<AudiosourceVariator>();
    }

    /// <summary>
    /// Function to apply any data from attached variatior
    /// </summary>
    private void ProcessVariator()
    {
        if(attachedVariator != null)
            attachedVariator.ProcessVariant(audioSource, this);
    }

    /// <summary>
    /// Initializes the sound
    /// </summary>
    private void Initialize(string overrideValue = null, bool randomClip = false)
    {
        string finalStringValue;
        if (!string.IsNullOrEmpty(overrideValue))
        {
            finalStringValue = overrideValue;
        }
        else
        {
            finalStringValue = soundName;
        }

        AudioManager.Instance.InitializeSound(finalStringValue, audioSource, out audioData, randomClip);

        if (audioData == null || audioData.sourceVariables == null)
        {
            Debug.LogError($"Null audio data or sourceVariables for sound {finalStringValue} on object {gameObject.name}");
            return;
        }
        
        if (audioData.sourceVariables.playOnAwake)
        {
            StartCoroutine(IsPlayingMonitor());
        }

        initialised = true;
        if (queuedPlay)
        {
            queuedPlay = false;
            Debug.Log($"Queued play happening on {gameObject}");
            Play();
        }
    }

    public virtual void Play()
    {
        Play();
    }

    public void PlayNamed(string newSound, bool randomClip = false)
    {
        if(newSound != soundName || randomClip)
        {            
            soundName = newSound;
            
            if(!enabled)
            {
                //OnEnable can call initialize if this is true
                initialiseStarted = false;
                enabled = true;
            }
            
            Initialize(randomClip: randomClip);
        }

        Play();
    }

    /// <summary>
    /// Standard Play method, SkipSetup is used by sequencers solely.
    /// </summary>
    /// <param name="handSide"></param>
    /// <param name="skipSetup"></param>
    public virtual void Play(bool skipSetup = false, GameObject emitting = null)
    {
        if (!initialised)
        {
            queuedPlay = true;
            return;
        }
        
        if (shouldExitBeforePlaying)
        {
            Debug.Log($" sound.cs {soundName} should exit before playing is true - returning.");
            return;
        }

        CheckForNulls();
        ProcessVariator();
        
        if(AudioManager.Instance)
        {
            if (audioData == null)
            {
                Debug.LogError("Failed to play audio on object: " + name + " because the data is null");
                return;
            }

            AudioManager.Instance.PlayAndSetupSource(audioData, audioSource, skipSetup, emitting);
            OnStartedPlaying?.Invoke();
        }
        else
        {
            Debug.Log($" sound.cs {soundName} AudioManager.Instance is null - returning.");
            return;
        }

        StartCoroutine(IsPlayingMonitor());
    }

    /// <summary>
    /// Method to check for nulls, log the issue, and exit if it finds any.
    /// </summary>
    /// <returns></returns>
    private void CheckForNulls()
    {
        if (AudioManager.Instance == null)
        {
            Debug.Log($"Sound.cs - CheckForNulls - {soundName} AudioManager.Instance == null");
        }
        
        if (audioData == null)
        {
            Debug.Log($"Sound.cs - CheckForNulls - {soundName} audioData == null - initializing");
            Initialize();
        }
        
        if (audioSource == null)
        {
            Debug.Log($"Sound.cs - CheckForNulls - {soundName} audioSource == null");
        }
    }

    /// <summary>
    /// Stops the attached audiosource if it hasn't been stopped already
    /// </summary>
    public virtual void Stop()
    {
        if(AudioManager.Instance)
            AudioManager.Instance.StopAudio(audioSource);
    }
    
    /// <summary>
    /// Monitors the isPlaying flag of the selected audiosource, and fires an onfinished event if applicable.
    /// </summary>
    /// <returns></returns>
    private IEnumerator IsPlayingMonitor()
    {
        isPlaying = true;

        while (audioSource.isPlaying)
        {
            yield return null;
        }
        
        isPlaying = false;
        OnFinishedPlaying?.Invoke();
        
        yield return null;
    }
}
