using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [HideInInspector] public AudioSource source;

    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool playOnAwake;
    public bool loop;
}

public class AudioManager : MonoBehaviour
{
    #region Variables

    public static AudioManager Instance { get; private set; }

    [SerializeField] private Sound[] sounds;

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    void Awake()
    {
        //If Instance does not exist yet, this instance should be the Instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("More than one AudioManager in the scene " + transform + " - " + Instance);
        }

        //
        foreach(Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.playOnAwake = s.playOnAwake;
            s.source.loop = s.loop;
        }
    }//end Awake

    // Start is called before the first frame update
    void Start()
    {
        PersistantManager.Instance.OnLoadingStart += PersistantManager_OnLoadingStart;
        PersistantManager.Instance.OnLoadingComplete += PersistantManager_OnLoadingComplete;

        PlaySound("Theme");
    }//end Start

    // Update is called once per frame
    void Update()
    {
        
    }//end Update

    #endregion //end Unity Control Methods

    #region

    public void PlaySound(string soundName)
    {
        Sound s = Array.Find(sounds, sound => sound.name.Equals(soundName));

        if(s != null)
        {
            s.source.Play();
        }
        else
        {
            Debug.LogWarning("No sound with name " + soundName + " was found!");
        }
    }


    private void PersistantManager_OnLoadingStart(object sender, EventArgs e)
    {

    }


    private void PersistantManager_OnLoadingComplete(object sender, EventArgs e)
    {

    }

    #endregion
}
