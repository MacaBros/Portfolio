using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public static class AudioFadeManager
{
    private const float MixerChannelVolumeMax  = 20f;
    private const float MixerChannelVolumeMin  = -80f;

    /// <summary>
    /// Helper Coroutine to provide a linear fade for any given float value.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="duration"></param>
    /// <param name="targetValue"></param>
    /// <returns></returns>
    public static IEnumerator StartLinearVolumeFade(AudioSource source, float duration, float targetValue)
    {
        var t = 0f;
        float currentVolume = source.volume; 
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            source.volume = Mathf.Lerp(currentVolume, targetValue, t);
            yield return null;
        }
    }
    
    /// <summary>
    /// Fade through the spatial blend from original to target using Lerp
    /// </summary>
    /// <returns></returns>
    public static IEnumerator StartLinearSpatialBlendFade(AudioSource source, float duration, float targetValue)
    {
        var t = 0f;
        float currentValue = source.spatialBlend;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            source.spatialBlend = Mathf.Lerp(currentValue, targetValue, t);
            yield return null;
        }
    }

    /// <summary>
    /// A coroutine to handle equal power fading on a non normalized range on an audiomixer
    /// </summary>
    /// <param name="exposedParam"></param>
    /// <param name="duration"></param>
    /// <param name="targetValue"></param>
    /// <param name="mixer"></param>
    /// <param name="callback"></param>
    /// <param name="includePersistentMixer"></param>
    /// <returns></returns>
    public static IEnumerator StartMixerFade(AudioMixer mixer, string exposedParam, float duration, float targetValue, Action callback = null, bool includePersistentMixer = false)
    {
        var mixerDone = false;
        var persistantMixerDone = false;
        
        mixer.GetFloat(exposedParam, out var currentValue); 
        var distance = targetValue - currentValue;
        AudioManager.Instance.StartCoroutine(LerpMixerValue(distance, duration, targetValue, currentValue, mixer,
            exposedParam, ()=>mixerDone = true));

        while (!mixerDone && !persistantMixerDone)
            yield return null;
           
        callback?.Invoke();
    }

    private static IEnumerator LerpMixerValue(float distance, float duration, float targetValue,
        float currentValue, AudioMixer mixer, string exposedParam, Action onLerped)
    {
        if (distance != 0)
        {
            float t;
            float equalPowerRate;
    
            if (distance > 0)
            {
                t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / duration;
                    equalPowerRate = Mathf.Cos((1 - t) * 0.5f * Mathf.PI);
                    ApplyCalculation(currentValue, targetValue, equalPowerRate, mixer, exposedParam);
                    yield return null;
                }
            }
            else
            {
                t = 1f;
                while (t > 0f)
                {
                    t -= Time.deltaTime / duration;
                    equalPowerRate = Mathf.Cos((t) * 0.5f * Mathf.PI);
                    ApplyCalculation(currentValue, targetValue, equalPowerRate, mixer, exposedParam);
                    yield return null;
                }
            }
        }
        
        onLerped?.Invoke();
    }

    /// <summary>
    /// Apply appropriate calculation to the started audio mixer fade
    /// </summary>
    /// <param name="currentValue"></param>
    /// <param name="targetValue"></param>
    /// <param name="equalPowerRate"></param>
    /// <param name="mixer"></param>
    /// <param name="exposedParam"></param>
    /// <param name="includePersistentMixer"></param>
    private static void ApplyCalculation(float currentValue, float targetValue, float equalPowerRate, AudioMixer mixer, string exposedParam, bool includePersistentMixer = false)
    {
        float final = Mathf.Lerp(currentValue, targetValue, equalPowerRate);
        AudioManager.SetAudioMixerParameter(mixer, exposedParam, Mathf.Clamp((float) System.Math.Round(final), MixerChannelVolumeMin, MixerChannelVolumeMax));
    }

    /// <summary>
    /// Crossfades a set of audiosources, A is always faded in and B always out.
    /// </summary>
    /// <param name="sourceA"></param>
    /// <param name="sourceB"></param>
    /// <param name="duration"></param>
    /// <param name="dataA"></param>
    /// <param name="dataB"></param>
    public static IEnumerator AudiosourceCrossfade(AudioSource sourceA, AudioSource sourceB, float duration, AudioData dataA, AudioData dataB)
    {
        var tOut = 0f;
        var tIn = 1f;

        bool inSequenceComplete = false;
        bool outSequenceComplete = false;

        while (!outSequenceComplete && !inSequenceComplete)
        {
            if (!outSequenceComplete)
            {
                tOut += Time.deltaTime / duration;
                var equalPowerRateOut = Mathf.Cos((1 - tOut) * 0.5f * Mathf.PI);
                var calculatedOut = Mathf.Lerp(dataA.sourceVariables.volume, 0.0001f, equalPowerRateOut);
                var clampedRoundedOut = Mathf.Clamp((float) System.Math.Round(calculatedOut, 2), 0, dataA.sourceVariables.volume);
                sourceA.volume = clampedRoundedOut;
                //Debug.Log($"sourceA.volume is {sourceA.volume}, tOut is {tOut}, equalPowerRateOut is {equalPowerRateOut}, calculatedOut is {calculatedOut}, clampedRoundedOut is {clampedRoundedOut}");
            }

            if (!inSequenceComplete)
            {
                tIn -= Time.deltaTime / duration;
                var equalPowerRateIn = Mathf.Cos((tIn) * 0.5f * Mathf.PI);
                var calculatedIn = Mathf.Lerp(0.0001f, dataB.sourceVariables.volume, equalPowerRateIn);
                var clampedRoundedIn = Mathf.Clamp((float) System.Math.Round(calculatedIn, 2), 0, dataB.sourceVariables.volume);
                sourceB.volume = clampedRoundedIn;
                //Debug.Log($"sourceB.volume is {sourceB.volume} tIn is {tIn} equalPowerRateIn is {equalPowerRateIn}, calculatedIn is {calculatedIn}, clampedRoundedIn is {clampedRoundedIn}");
            }

            if (Mathf.Approximately(sourceA.volume, 0))
            {
                outSequenceComplete = true;
            }

            if (Mathf.Approximately(sourceB.volume, dataB.sourceVariables.volume))
            {
                inSequenceComplete = true;
            }

            yield return null;
        }

        yield return null;
    }
}
