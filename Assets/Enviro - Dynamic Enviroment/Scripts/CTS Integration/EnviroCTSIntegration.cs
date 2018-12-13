using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if CTS_PRESENT
using CTS;

[AddComponentMenu("Enviro/Integration/CTS Integration")]
public class EnviroCTSIntegration : MonoBehaviour {

	public CTSWeatherManager ctsWeatherManager;

	public bool updateSnow;
	public bool updateWetness;
	public bool updateSeasons;

	private float daysInYear;

	void Start () 
	{
		if (ctsWeatherManager == null) {
			ctsWeatherManager = GameObject.FindObjectOfType<CTSWeatherManager> ();
		}

		if(ctsWeatherManager == null) {
			Debug.LogWarning("CTS WeatherManager not found! Component -> CTS -> Add Weather Manager");
			return;
		}

		if (EnviroSky.instance == null) {
			Debug.LogWarning("EnviroSky not found! Please add EnviroSky prefab to your scene!");
			return;
		}
	daysInYear = EnviroSky.instance.seasonsSettings.SpringInDays + EnviroSky.instance.seasonsSettings.SummerInDays + EnviroSky.instance.seasonsSettings.AutumnInDays + EnviroSky.instance.seasonsSettings.WinterInDays;
	}
	

	void Update () 
	{
		if (ctsWeatherManager == null || EnviroSky.instance == null)
			return;

		if (updateSnow)
			ctsWeatherManager.SnowPower = EnviroSky.instance.Weather.curSnowStrength;

		if(updateWetness)
			ctsWeatherManager.RainPower = EnviroSky.instance.Weather.curWetness;

		if (updateSeasons) {
			ctsWeatherManager.Season = Mathf.Lerp (0f, 4f, EnviroSky.instance.currentDay / daysInYear);
		}
	}
}
#endif
