using System;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(AudiosourceVariator))]
public class AudiosourceVariatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Accepted ranges:\nVolume [Range(0,1)]\nPitch [Range(-3,3)]" +
                                "\nLPF [Range(0,0)]\nHPF [Range(0,0)]\nPlay Chance [Range(N/A,100)]\n", MessageType.Info);
        DrawDefaultInspector();
    }
}
#endif

public enum VariatorType
{
    Volume,
    Pitch,
    HPF,
    HPFLocal,
    LPF,
    LPFLocal,
    PlayChance
}

/// <summary>
/// A class to house variator data
/// </summary>
[Serializable]
public class AudiosourceVariatorData
{
    public VariatorType variatorType;
    public float minRange;
    public float maxRange;
}

/// <summary>
/// AudiosourceVariator is an optional component class,
/// if present next to a 'sound.cs' or inheritor it will apply a random value within the specified range
/// </summary>
public class AudiosourceVariator : MonoBehaviour
{
    [Header("Variator Data")]
    public AudiosourceVariatorData[] variatorDatas;
    
    [Header("Exposed Mixer Variables")]
    public string lpfExposedVariable;
    public string hpfExposedVariable;
    
    private AudioLowPassFilter localLpf;
    private AudioHighPassFilter localHpf;

    /// <summary>
    /// Entry function to delegate based on variator type
    /// </summary>
    /// <param name="source"></param>
    public void ProcessVariant(AudioSource source, Sound sound)
    {
        foreach (var variatorData in variatorDatas)
        {
            var vdType = variatorData.variatorType;
            switch (vdType)
            {
                case VariatorType.Volume:
                    ProcessVolumeVariant(source, variatorData.minRange, variatorData.maxRange);
                    break;
                case VariatorType.Pitch:
                    ProcessPitchVariant(source, variatorData.minRange, variatorData.maxRange);
                    break;
                case VariatorType.HPF:
                    break;
                case VariatorType.LPF:
                    break;
                case VariatorType.PlayChance:
                    ProcessPlayChance(sound, variatorData.maxRange);
                    break;
                case VariatorType.LPFLocal:
                    ProcessLPFLocalVariant(variatorData.minRange, variatorData.maxRange);
                    break;
                case VariatorType.HPFLocal:
                    ProcessHPFLocalVariant(variatorData.minRange, variatorData.maxRange);
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="variatorDataMinRange"></param>
    /// <param name="variatorDataMaxRange"></param>
    private void ProcessHPFLocalVariant(float variatorDataMinRange, float variatorDataMaxRange)
    {
        if (localLpf == null)
            localLpf = transform.gameObject.AddComponent<AudioLowPassFilter>();

        localLpf.cutoffFrequency = Random.Range(variatorDataMinRange, variatorDataMaxRange);
    }

    /// <summary>
    /// Method to provide a lpf component if needed and to randomize it's cutoff frequency value.
    /// </summary>
    /// <param name="variatorDataMinRange"></param>
    /// <param name="variatorDataMaxRange"></param>
    private void ProcessLPFLocalVariant(float variatorDataMinRange, float variatorDataMaxRange)
    {
        if (localHpf == null)
            localHpf = transform.gameObject.AddComponent<AudioHighPassFilter>();
        
        localHpf.cutoffFrequency = Random.Range(variatorDataMinRange, variatorDataMaxRange);

    }

    /// <summary>
    /// Function to process playing chance.
    /// </summary>
    /// <param name="sound"></param>
    /// <param name="variatorDataMAXRange"></param>
    private void ProcessPlayChance(Sound sound, float variatorDataMAXRange)
    {
        if(sound == null)
            Debug.LogWarning("AudiosourceVariator - Process Play Chance sound ref is null returning.");
        
        bool shouldPlay = false;
        float randomPercent = Random.Range(0, 100);

        if (randomPercent >= variatorDataMAXRange)
            shouldPlay = true;

        sound.shouldExitBeforePlaying = shouldPlay;
    }

    /// <summary>
    /// applies a random value to the volume of the audiosource.
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="variatorDataMINRange"></param>
    /// <param name="variatorDataMAXRange"></param>
    ///
    private void ProcessVolumeVariant(AudioSource audioSource, float variatorDataMINRange, float variatorDataMAXRange)
    {
        audioSource.volume = Random.Range(variatorDataMINRange, variatorDataMAXRange);
    }
    
    /// <summary>
    /// applies a random value to the pitch of the audiosource.
    /// </summary>
    /// <param name="audioSource"></param>
    private void ProcessPitchVariant(AudioSource audioSource, float variatorDataMINRange, float variatorDataMAXRange)
    {
        audioSource.pitch = Random.Range(variatorDataMINRange, variatorDataMAXRange);
    }
}
