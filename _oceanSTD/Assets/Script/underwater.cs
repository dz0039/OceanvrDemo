using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Underwater : MonoBehaviour
{

    //This script enables underwater effects. Attach to main camera.

    //Define variable
    public bool underwater;

    //The scene's default fog settings
    private bool defaultFog;
    private Color defaultFogColor;
    private float defaultFogDensity;
    private Material defaultSkybox;
    private Material noSkybox;

    void Start()
    {
        defaultFog = RenderSettings.fog;
        defaultFogColor = RenderSettings.fogColor;
        defaultFogDensity = RenderSettings.fogDensity;
        defaultSkybox = RenderSettings.skybox;
        //Set the background color
        GetComponent<Camera>().backgroundColor = new Color(.2f, .3f, .5f, 1);
    }

    void Update()
    {
        if (underwater)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(.2f, .3f, .5f, .6f);
            RenderSettings.fogDensity = 0.04f;
            RenderSettings.skybox = noSkybox;
        }
        else
        {
            RenderSettings.fog = defaultFog;
            RenderSettings.fogColor = defaultFogColor;
            RenderSettings.fogDensity = defaultFogDensity;
            RenderSettings.skybox = defaultSkybox;
        }
    }
}