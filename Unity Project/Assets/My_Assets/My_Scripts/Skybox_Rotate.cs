using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skybox_Rotate : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        GetComponent<Skybox>().material.SetFloat("_Rotation", Time.time * 10);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
