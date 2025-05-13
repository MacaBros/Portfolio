using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
    [CustomEditor(typeof(AudioBankManager))] 
    public class AudioBankManagerEditor : Editor
    {
        private AudioBankManager myScript;
        private void AssignScriptReference()
        {
            myScript = target as AudioBankManager;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            AssignScriptReference();
            if (GUILayout.Button("Create Audio Bank"))
            {
                myScript.CreateAudioBank();
            }
        }
    }
#endif

[CreateAssetMenu(fileName = "AudioBankManager", menuName = "Audio/CreateAudioBankManager", order = 50)]
public class AudioBankManager : ScriptableObject
{
    [Header("Name of bank to be created")] 
    public string bankName;

#if UNITY_EDITOR
    /// <summary>
    /// Handles the creation of an audio bank of sounds. Currently adds them to a master list and a specified one.
    /// </summary>
    public void CreateAudioBank()
    {
        if (CheckEarlyExits())
            return;

        var newBank = CreateInstance<AudioBank>();
        AssetDatabase.CreateAsset(newBank, "Assets/Audio/Final/Banks/");

        Reset();
    }

#endif

    /// <summary>
    /// Helper function to check for early exist at the start of the creation process.
    /// </summary>
    /// <returns></returns>
    private bool CheckEarlyExits()
    {
        if (string.IsNullOrEmpty(bankName))
        {
            Debug.LogError("You must provide a name for the audio bank to be created/removed successfully");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Post creation cleanup helper function.
    /// </summary>
    private void Reset()
    {
        bankName = "";
    }
}
