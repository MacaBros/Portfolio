using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(Sequencer))] 
public class SequencerEditor : Editor
{
    private Sequencer myScript;
    private void AssignScriptReference()
    {
        myScript = target as Sequencer;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        AssignScriptReference();
        if (GUILayout.Button("Fire Sequence"))
        {
            myScript.RunSequence();
        }
        
    }
}
#endif


/// <summary>
/// This script should handle;
/// 1.
/// plays from start, to completion, no interval.
/// plays from start, to completion, with randomised intervals,
/// plays from start. to completion, with a constant interval,
/// plays from start, single step
///
/// 2.
/// plays from trigger, to completion, no interval.
/// plays from trigger, to completion, with randomised intervals,
/// plays from trigger. to completion, with a constant interval,
/// plays from trigger, single step
///
/// 3.
/// plays from start, to completion, and loops, no interval.
/// plays from start, to completion, and loops, with randomised intervals,
/// plays from start. to completion, and loops, with a constant interval,
/// plays from trigger, to completion, and loops, no interval.
/// plays from trigger, to completion, and loops, with randomised intervals,
/// plays from trigger. to completion, and loops, with a constant interval,
/// </summary>

public class Sequencer : SequenceBase
{
    [HideInInspector] public bool isRunning = false;

    public override void Start()
    {
        base.Start();
        StartCoroutine(CheckForRunOnStart());
    }
    
    /// <summary>
    /// Assesses whether or not to run the sequencer on Start()
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckForRunOnStart()
    {
        if (shouldPlayOnAwake)
        {
            if (useIntervalOnStart)
            {
                yield return DoWait(initialInterval);
            }
            RunSequence();
        }
        yield return null;
    }

    /// <summary>
    /// Method to call coroutine RunSequence_Co
    /// </summary>
    public void RunSequence()
    {
        StartCoroutine(RunSequence_Co());
    }

    /// <summary>
    /// Main coroutine for sequencer behaviour, will run until told not to.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RunSequence_Co()
    {
        ToggleRoutineFlag(true);
        while (isRunning)
        {
            yield return SelectClipAndPlay();
            yield return null;
        }
        yield return null;
    }

    /// <summary>
    /// Selects a valid clip dependant on sequencer type, stores previously played info for stepped sequences
    /// Calls to play the audio and handles any waiting before resuming.
    /// </summary>
    private IEnumerator SelectClipAndPlay()
    {
        if(previouslyPlayedList == null || audioData == null)
            yield break;
        
        //Check List
        if (previouslyPlayedList.Count >= audioData.sourceVariables.clips.Count)
        {
            previouslyPlayedList.Clear();
            if (stopUponListCompletion)
            {
                ToggleRoutineFlag(false);
                yield break;
            }

            if (useRepetition)
            {
                if (!UseRepetitionLogic())
                {
                    yield break;
                }
            }
        }

        //Get Valid Selection
        AudioClip clipToPlay = null;
        bool hasValidSelection = false;

        while (!hasValidSelection)
        {
            clipToPlay = SelectClip();
            hasValidSelection = !previouslyPlayedList.Contains(clipToPlay);
            yield return null;
        }

        //Add our selection to the previously played list.
        previouslyPlayedList.Add(clipToPlay);

        //Play
        Play(true);
        internalLoopCount++;
        
        if (CheckForStepBreak())
        {
            ToggleRoutineFlag(false);
            yield break;
        }
        
        //Wait
        yield return SelectInterval(clipToPlay.length);
    }

    internal bool UseRepetitionLogic()
    {
        if (useRepetition)
        {
            if (internalLoopCount > 0)
            {
                if (internalLoopCount <= repetition)
                {
                    return true;
                }
                else
                {
                    ToggleRoutineFlag(false);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Sets a flag that controls the while loop in RunSequence_Co
    /// </summary>
    private void ToggleRoutineFlag(bool value)
    {
        isRunning = value;
    }

    public override void Stop()
    {
        base.Stop();
        ToggleRoutineFlag(false);
    }

    public Sequencer(string soundName, AudioSource source) : base(soundName, source)
    {
    }
}
