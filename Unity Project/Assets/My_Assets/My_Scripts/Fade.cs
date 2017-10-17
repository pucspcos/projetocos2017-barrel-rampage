using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Fade : MonoBehaviour
{
    Image painelFade;
    public string LevelToLoad;
    public static Fade fade;
    public float fadeTimeInSeconds;

    private void Awake()
    {
        fade = this;
    }

    // Use this for initialization
    void Start()
    {
        painelFade = GetComponent<Image>();
        painelFade.enabled = true;
        painelFade.CrossFadeAlpha(0.01f, 2, true);
        Invoke("fadeOut", 3);
    }

    void fadeOut()
    {
        painelFade.CrossFadeAlpha(1, 2, true);
        Invoke("ChangeScene", 2);
    }

    void ChangeScene()
    {
        SceneManager.LoadScene(LevelToLoad);
    }
}
