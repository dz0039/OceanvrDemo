using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
[AddComponentMenu("Enviro/AddionalCamera")]
public class EnviroAdditionalCamera : MonoBehaviour {

    private Camera myCam;
    private EnviroSkyRendering skyRender;
    private EnviroLightShafts lightShaftsScriptSun;
    private EnviroLightShafts lightShaftsScriptMoon;

    private void OnEnable()
    {
        myCam = GetComponent<Camera>();

        if (myCam != null)
            InitImageEffects();
    }


	void Update ()
    {
        UpdateCameraComponents();
    }

    private void InitImageEffects()
    {
        skyRender = myCam.gameObject.GetComponent<EnviroSkyRendering>();

        if (skyRender == null)
            skyRender = myCam.gameObject.AddComponent<EnviroSkyRendering>();

        skyRender.isAddionalCamera = true;

#if UNITY_EDITOR
        string[] assets = UnityEditor.AssetDatabase.FindAssets("enviro_spot_cookie", null);
        for (int idx = 0; idx < assets.Length; idx++)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(assets[idx]);
            if (path.Length > 0)
            {
                skyRender.DefaultSpotCookie = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(path);
            }
        }
#endif

        EnviroLightShafts[] shaftScripts = myCam.gameObject.GetComponents<EnviroLightShafts>();

        if (shaftScripts.Length > 0)
            lightShaftsScriptSun = shaftScripts[0];

        if (lightShaftsScriptSun != null)
        {
            DestroyImmediate(lightShaftsScriptSun.sunShaftsMaterial);
            DestroyImmediate(lightShaftsScriptSun.simpleClearMaterial);
            lightShaftsScriptSun.sunShaftsMaterial = new Material(Shader.Find("Enviro/Effects/LightShafts"));
            lightShaftsScriptSun.sunShaftsShader = lightShaftsScriptSun.sunShaftsMaterial.shader;
            lightShaftsScriptSun.simpleClearMaterial = new Material(Shader.Find("Enviro/Effects/ClearLightShafts"));
            lightShaftsScriptSun.simpleClearShader = lightShaftsScriptSun.simpleClearMaterial.shader;
        }
        else
        {
            lightShaftsScriptSun = myCam.gameObject.AddComponent<EnviroLightShafts>();
            lightShaftsScriptSun.sunShaftsMaterial = new Material(Shader.Find("Enviro/Effects/LightShafts"));
            lightShaftsScriptSun.sunShaftsShader = lightShaftsScriptSun.sunShaftsMaterial.shader;
            lightShaftsScriptSun.simpleClearMaterial = new Material(Shader.Find("Enviro/Effects/ClearLightShafts"));
            lightShaftsScriptSun.simpleClearShader = lightShaftsScriptSun.simpleClearMaterial.shader;
        }

        if (shaftScripts.Length > 1)
            lightShaftsScriptMoon = shaftScripts[1];

        if (lightShaftsScriptMoon != null)
        {
            DestroyImmediate(lightShaftsScriptMoon.sunShaftsMaterial);
            DestroyImmediate(lightShaftsScriptMoon.simpleClearMaterial);
            lightShaftsScriptMoon.sunShaftsMaterial = new Material(Shader.Find("Enviro/Effects/LightShafts"));
            lightShaftsScriptMoon.sunShaftsShader = lightShaftsScriptMoon.sunShaftsMaterial.shader;
            lightShaftsScriptMoon.simpleClearMaterial = new Material(Shader.Find("Enviro/Effects/ClearLightShafts"));
            lightShaftsScriptMoon.simpleClearShader = lightShaftsScriptMoon.simpleClearMaterial.shader;
        }
        else
        {
            lightShaftsScriptMoon = myCam.gameObject.AddComponent<EnviroLightShafts>();
            lightShaftsScriptMoon.sunShaftsMaterial = new Material(Shader.Find("Enviro/Effects/LightShafts"));
            lightShaftsScriptMoon.sunShaftsShader = lightShaftsScriptMoon.sunShaftsMaterial.shader;
            lightShaftsScriptMoon.simpleClearMaterial = new Material(Shader.Find("Enviro/Effects/ClearLightShafts"));
            lightShaftsScriptMoon.simpleClearShader = lightShaftsScriptMoon.simpleClearMaterial.shader;
        }
    }


    private void UpdateCameraComponents()
    {
        if (EnviroSky.instance == null)
            return;

        //Update Fog
        if (skyRender != null)
        {
            skyRender.dirVolumeLighting = EnviroSky.instance.volumeLightSettings.dirVolumeLighting;
            skyRender.volumeLighting = EnviroSky.instance.volumeLighting;
            skyRender.distanceFog = EnviroSky.instance.fogSettings.distanceFog;
            skyRender.heightFog = EnviroSky.instance.fogSettings.heightFog;
            skyRender.height = EnviroSky.instance.fogSettings.height;
            skyRender.heightDensity = EnviroSky.instance.fogSettings.heightDensity;
            skyRender.useRadialDistance = EnviroSky.instance.fogSettings.useRadialDistance;
            skyRender.startDistance = EnviroSky.instance.fogSettings.startDistance;
        }

        //Update LightShafts
        if (lightShaftsScriptSun != null)
        {
            lightShaftsScriptSun.resolution = EnviroSky.instance.lightshaftsSettings.resolution;
            lightShaftsScriptSun.screenBlendMode = EnviroSky.instance.lightshaftsSettings.screenBlendMode;
            lightShaftsScriptSun.useDepthTexture = EnviroSky.instance.lightshaftsSettings.useDepthTexture;
            lightShaftsScriptSun.sunThreshold = EnviroSky.instance.lightshaftsSettings.thresholdColorSun.Evaluate(EnviroSky.instance.GameTime.solarTime);

            lightShaftsScriptSun.sunShaftBlurRadius = EnviroSky.instance.lightshaftsSettings.blurRadius;
            lightShaftsScriptSun.sunShaftIntensity = EnviroSky.instance.lightshaftsSettings.intensity;
            lightShaftsScriptSun.maxRadius = EnviroSky.instance.lightshaftsSettings.maxRadius;
            lightShaftsScriptSun.sunColor = EnviroSky.instance.lightshaftsSettings.lightShaftsColorSun.Evaluate(EnviroSky.instance.GameTime.solarTime);
            lightShaftsScriptSun.sunTransform = EnviroSky.instance.Components.Sun.transform;

            if (EnviroSky.instance.LightShafts.sunLightShafts)
            {
                lightShaftsScriptSun.enabled = true;
            }
            else
            {
                lightShaftsScriptSun.enabled = false;
            }
        }

        if (lightShaftsScriptMoon != null)
        {
            lightShaftsScriptMoon.resolution = EnviroSky.instance.lightshaftsSettings.resolution;
            lightShaftsScriptMoon.screenBlendMode = EnviroSky.instance.lightshaftsSettings.screenBlendMode;
            lightShaftsScriptMoon.useDepthTexture = EnviroSky.instance.lightshaftsSettings.useDepthTexture;
            lightShaftsScriptMoon.sunThreshold = EnviroSky.instance.lightshaftsSettings.thresholdColorMoon.Evaluate(EnviroSky.instance.GameTime.lunarTime);


            lightShaftsScriptMoon.sunShaftBlurRadius = EnviroSky.instance.lightshaftsSettings.blurRadius;
            lightShaftsScriptMoon.sunShaftIntensity = Mathf.Clamp((EnviroSky.instance.lightshaftsSettings.intensity - EnviroSky.instance.GameTime.solarTime), 0, 100);
            lightShaftsScriptMoon.maxRadius = EnviroSky.instance.lightshaftsSettings.maxRadius;
            lightShaftsScriptMoon.sunColor = EnviroSky.instance.lightshaftsSettings.lightShaftsColorMoon.Evaluate(EnviroSky.instance.GameTime.lunarTime);
            lightShaftsScriptMoon.sunTransform = EnviroSky.instance.Components.Moon.transform;

            if (EnviroSky.instance.LightShafts.moonLightShafts)
            {
                lightShaftsScriptMoon.enabled = true;
            }
            else
            {
                lightShaftsScriptMoon.enabled = false;
            }
        }
    }


}
