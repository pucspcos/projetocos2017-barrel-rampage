using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu_Botoes : MonoBehaviour
{

    public void Play()
    {
        MenuSongManager.songInstance.menuMusicEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        SceneManager.LoadScene("Loading");
    }

    public void Credits()
    {
        MenuSongManager.songInstance.menuMusicEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        SceneManager.LoadScene("Credits");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Back()
    {
        MenuSongManager.songInstance.menuMusicEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        SceneManager.LoadScene("Menu");
    }
}
