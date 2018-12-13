#if AQUAS_PRESENT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[AddComponentMenu("Enviro/Integration/AQUAS Integration")]
public class EnviroAquasIntegration : MonoBehaviour {

	[Header("AQUAS Water Plane")]
	public GameObject waterObject;

	[Header("Setup")]
	public bool deactivateAquasReflectionProbe = true;
	public bool deactivateEnviroFogUnderwater = true;

	[Header("Settings")]
	[Range(0f,1f)]
	public float underwaterFogColorInfluence = 0.3f;
	//
	//private GameObject enviroWaterDepth;
	private AQUAS_LensEffects aquasUnderWater;
	private bool isUnderWater;
	//

	private bool defaultDistanceFog;
	private bool defaultHeightFog;

	void Start () 
	{
		if (EnviroSky.instance == null) 
		{
			Debug.Log ("No EnviroSky in scene! This component will be disabled!");
			this.enabled = false;
			return;
		}

		if(GameObject.Find ("UnderWaterCameraEffects") != null)
			aquasUnderWater = GameObject.Find ("UnderWaterCameraEffects").GetComponent<AQUAS_LensEffects> ();
	
		defaultDistanceFog = EnviroSky.instance.fogSettings.distanceFog;
		defaultHeightFog = EnviroSky.instance.fogSettings.heightFog;

		SetupEnviroWithAQUAS ();
	}

	void Update () 
	{
         if (EnviroSky.instance == null)
            return;

		//Check if we are underwater! Deactivate the workaround plane and enviro fog.
		if (waterObject != null && aquasUnderWater != null) {
			if (aquasUnderWater.underWater && !isUnderWater) {
				if (deactivateEnviroFogUnderwater) {
					EnviroSky.instance.fogSettings.distanceFog = false;
					EnviroSky.instance.fogSettings.heightFog = false;
					EnviroSky.instance.customFogIntensity = underwaterFogColorInfluence;
                    
				}
				EnviroSky.instance.updateFogDensity = false;
                EnviroSky.instance.Audio.ambientSFXVolumeMod = -1f;
                EnviroSky.instance.Audio.weatherSFXVolumeMod = -1f;
				isUnderWater = true;
			} else if (!aquasUnderWater.underWater && isUnderWater) {
				if (deactivateEnviroFogUnderwater) {
					EnviroSky.instance.updateFogDensity = true;
					EnviroSky.instance.fogSettings.distanceFog = defaultDistanceFog;
					EnviroSky.instance.fogSettings.heightFog = defaultHeightFog;
					RenderSettings.fogDensity = EnviroSky.instance.Weather.currentActiveWeatherPreset.fogDensity;
					EnviroSky.instance.customFogColor = aquasUnderWater.underWaterParameters.fogColor;
					EnviroSky.instance.customFogIntensity = 0f;
				}
                EnviroSky.instance.Audio.ambientSFXVolumeMod = 0f;
                EnviroSky.instance.Audio.weatherSFXVolumeMod = 0f;
				isUnderWater = false;
			}
		}
	}

	public void SetupEnviroWithAQUAS ()
	{
		if (waterObject != null) {

			if (deactivateAquasReflectionProbe)
				DeactivateReflectionProbe (waterObject);

			if (EnviroSky.instance.fogSettings.distanceFog == false && EnviroSky.instance.fogSettings.heightFog == false)
				deactivateEnviroFogUnderwater = false;

			if (aquasUnderWater != null)
				aquasUnderWater.setAfloatFog = false;
			
			} else {
				Debug.Log ("AQUAS Object not found! This component will be disabled!");
				this.enabled = false;
				return;
			}
	}


	private void DeactivateReflectionProbe (GameObject aquas)
	{
		GameObject probe = GameObject.Find (aquas.name + "/Reflection Probe");
		if (probe != null)
			probe.GetComponent<ReflectionProbe> ().enabled = false;
		else
		Debug.Log ("Cannot find AQUAS Reflection Probe!");
	}
}
#endif