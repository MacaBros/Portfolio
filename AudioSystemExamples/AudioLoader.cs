using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class AudioLoaderData
{
    [SerializeField] 
    public string associatedLevelName;
    
    [SerializeField]
    public List<string> bankNames = new List<string>();
}

public class AudioLoader : MonoBehaviour
{
    [Header("Look Up")]
    public List<AudioLoaderData> levelBanks = new List<AudioLoaderData>();
    
    [Header("Common Banks")] 
    [SerializeField]
    public List<AudioLoaderData> commonBanks = new List<AudioLoaderData>();

    [Header("Loaded Banks")] 
    [SerializeField]
    public List<AudioBank> loadedBanks = new List<AudioBank>();

    [NonSerialized]
    public bool isLoading = true;
    
#if UNITY_EDITOR
    private IEnumerator Start()
    {
            yield return StartCoroutine(Process(SceneManager.GetActiveScene().name, true));
    }
#endif

    private List<string> RetrieveBanksForLoadedLevel(string levelName)
    {
        for (int i = 0; i < levelBanks.Count; i++)
        {
            if (levelName == levelBanks[i].associatedLevelName)
            {
                return levelBanks[i].bankNames;
            }
        }
        return null;
    }

    public IEnumerator Process(string levelName, bool isLoad)
    {
        var retrievedList = RetrieveBanksForLoadedLevel(levelName);
        
        if(retrievedList == null)
            yield break;
        
        if (isLoad)
        {
            isLoading = true;
            yield return StartCoroutine(LoadCommonBanks());
            if (retrievedList != null && retrievedList.Any())
            {
                yield return StartCoroutine(Load(retrievedList));
            }

            isLoading = false;
        }

        else
        {
            yield return StartCoroutine(Unload());
        }

        yield return null;
    }

    private IEnumerator LoadCommonBanks()
    {
        if(commonBanks == null || !commonBanks.Any())
            yield break;
        
        yield return Load(commonBanks[0].bankNames);
        yield return null;
    }

    private IEnumerator Load(List<string> bankNames)
    {
        for (int i = 0; i < bankNames.Count; i++)
        {
            var loaded = Resources.Load<AudioBank>("AudioBanks/"+bankNames[i]);
            if (loaded == null)
                Debug.LogError("Failed to load AudioBanks/" + bankNames[i]);
            else
            {
                loaded.Preload();
                if (!loadedBanks.Contains(loaded))
                    loadedBanks.Add(loaded);
            }
        }
        yield return null;
    }
    
    private IEnumerator Unload()
    {
        for (int i = 0; i < loadedBanks.Count; i++)
        {
            AudioBank bank = loadedBanks[i];
            loadedBanks.Remove(bank);
            bank.Unload();
            Resources.UnloadAsset(bank);
        }

        yield return null;
    }
}
