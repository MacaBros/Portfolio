using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum SequenceType
{
    Linear,
    LinearStep,
    Random,
    RandomStep
}

public enum IntervalType
{
    None,
    Constant,
    Random
}

public class SequenceBase : Sound
{
    protected List<AudioClip> previouslyPlayedList;
    
    [Header("Config")]
    public SequenceType sequenceType;
    public bool shouldPlayOnAwake = false;
    public bool useIntervalOnStart = false;
    public bool stopUponListCompletion = false;
    
    [Header("Interval Settings")]
    public IntervalType intervalType;
    [Min(0)] public float initialInterval = 10f;
    [Min(0)] public float minIntervalTime = 5f;
    [Min(0)] public float maxIntervalTime = 30f;
    [Min(0)] public float constantIntervalTime = 30f;

    [Header("Repetition Settings")]
    public bool useRepetition;
    [Min(0)] public int repetition;

    private int linearSequenceCurrentIndex = -1;
    internal int internalLoopCount;

    public override void Start()
    {
        if (previouslyPlayedList == null)
            previouslyPlayedList = new List<AudioClip>();
        base.Start();
    }

    /// <summary>
    /// Selects and returns a clip dependant on sequence type, retrieved from the relevant audio data.
    /// </summary>
    /// <returns></returns>
    protected AudioClip SelectClip()
    {
        AudioClip retrievedClip = null;
        switch (sequenceType)
        {
            case SequenceType.Linear:
            case SequenceType.LinearStep:
                ProcessLinearSequence();
                retrievedClip = audioData.sourceVariables.clips[linearSequenceCurrentIndex];
                audioData.sourceVariables.SetSourceClip(audioSource, retrievedClip);
                break;
            case SequenceType.Random:
            case SequenceType.RandomStep:
                var rnd = Random.Range(0, audioData.sourceVariables.clips.Count);
                //Debug.Log($"SelectClip - random step random val is {rnd} amount of clips in data is {audioData.sourceVariables.clips.Count}, sound is {audioData.soundName}");
                retrievedClip = audioData.sourceVariables.clips[rnd];
                //Debug.Log($"SelectClip - randomly selected clip is named {retrievedClip.name}");
                audioData.sourceVariables.SetSourceClip(audioSource, retrievedClip);
                break;
            default:
                break;
        }

        return retrievedClip;

    }

    /// <summary>
    /// Manages the current index for a linear sequence
    /// </summary>
    private void ProcessLinearSequence()
    {
        linearSequenceCurrentIndex++;
        if (linearSequenceCurrentIndex >= audioData.sourceVariables.clips.Count)
        {
            linearSequenceCurrentIndex = 0;
        }
    }

    /// <summary>
    /// Select the appropriate interval and wait.
    /// </summary>
    protected IEnumerator SelectInterval(float currentClipDuration)
    {
        float selectedIntervalTime = 0f;
        switch (intervalType)
        {
            case IntervalType.None:
                break;
            case IntervalType.Constant:
                selectedIntervalTime = constantIntervalTime;
                break;
            case IntervalType.Random:
                selectedIntervalTime = Random.Range(minIntervalTime, maxIntervalTime);
                break;
            default:
                break;
        }
        
        yield return DoWait(currentClipDuration + selectedIntervalTime);
    }

    /// <summary>
    /// Helper function for waiting in a coroutine.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected IEnumerator DoWait(float value)
    {
        yield return new WaitForSeconds(value);
        yield return null;
    }
    
    /// <summary>
    /// Checks for a step sequence and breaks or stops the sequencer if needed.
    /// </summary>
    /// <returns></returns>
    protected bool CheckForStepBreak()
    {
        return sequenceType == SequenceType.LinearStep || sequenceType == SequenceType.RandomStep;
    }

    public SequenceBase(string soundName, AudioSource source) : base(soundName, source)
    {
    }
}
