using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class MenuSongManager : MonoBehaviour
{
    public static MenuSongManager songInstance;
    public string menuSoundtrack = "event:/Menu";
    public float menuSoundTimer;
    public EventInstance menuMusicEvent;
    public ParameterInstance menuSampleInstance;

    // Use this for initialization
    void OnEnable()
    {
        if (songInstance == null)
        {
            songInstance = this;
        }

        else
        {
            UnityEngine.Debug.LogError("More than one Game Manager on the scene!");
        }
    }

    void Start()
    {
        menuMusicEvent = RuntimeManager.CreateInstance(menuSoundtrack);
        menuMusicEvent.getParameter("MenuSampleInstance", out menuSampleInstance);
        menuMusicEvent.start();
    }

    // Update is called once per frame
    void Update()
    {
        menuSoundTimer += Time.deltaTime;
        if (menuSoundTimer >= 7)
        {
            menuSampleInstance.setValue(UnityEngine.Random.Range(0, 3));
            menuSoundTimer = 0;
        }
    }
}
