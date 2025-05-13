using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu( fileName = "Audio Bank.asset", menuName = "Audio/Audio Bank")]
public class AudioBank : ScriptableObject
{
    [SerializeField]
    public List<AudioData> soundData = new List<AudioData>();
    
    /// <summary>
    /// Helper function used to retrieve a specific set of audio data from the associated bank.
    /// </summary>
    /// <param name="soundNameToFind"></param>
    /// <returns></returns>
    public AudioData GetSoundByName(string soundNameToFind)
    {
        for (int i = 0; i < soundData.Count; i++)
        { 
            if (soundData[i].soundName != soundNameToFind) 
                continue;
            
            return soundData[i];
        }

        return null;
    }

    /// <summary>
    /// Calls LoadAudioData() on all clips stored in this AudioBank
    /// </summary>
    public void Preload()
    {
        for(int i = 0; i < soundData.Count; i++)
        {
            var clips = soundData[i].sourceVariables.clips;
            for(int j = 0; j < clips.Count; j++)
            {
                clips[j].LoadAudioData();
            }
        }
    }

    /// <summary>
    /// Calls UnloadAudioData() on all clips stored in this AudioBank
    /// </summary>
    public void Unload()
    {
        for(int i = 0; i < soundData.Count; i++)
        {
            var clips = soundData[i].sourceVariables.clips;
            for(int j = 0; j < clips.Count; j++)
            {
                clips[j].UnloadAudioData();
            }
        }
    }
}
