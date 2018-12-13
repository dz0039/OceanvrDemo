using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EnviroSamples
{
public class DemoUI : MonoBehaviour {


	public UnityEngine.UI.Slider sliderTime;
	public UnityEngine.UI.Slider sliderQuality;
	public UnityEngine.UI.Text timeText;
	public UnityEngine.UI.Dropdown weatherDropdown;
	public UnityEngine.UI.Dropdown seasonDropdown;

 	bool seasonmode = true;
	bool fastdays = false;

	private bool started = false;

	void Start () 
	{
		EnviroSky.instance.OnWeatherChanged += (EnviroWeatherPreset type) =>
		{
			UpdateWeatherSlider ();	
		};

		EnviroSky.instance.OnSeasonChanged += (EnviroSeasons.Seasons season) =>
		{
			UpdateSeasonSlider(season);
		};
	}
		
	IEnumerator setupDrodown ()
	{
		started = true;
		yield return new WaitForSeconds (0.1f);

		for (int i = 0; i < EnviroSky.instance.Weather.weatherPresets.Count; i++) {
			UnityEngine.UI.Dropdown.OptionData o = new UnityEngine.UI.Dropdown.OptionData();
			o.text = EnviroSky.instance.Weather.weatherPresets [i].Name;
			weatherDropdown.options.Add (o);
		}

		yield return new WaitForSeconds (0.1f);
		UpdateWeatherSlider ();
	}

	public void ChangeTimeSlider () 
	{
		if (sliderTime.value < 0f)
			sliderTime.value = 0f;
		EnviroSky.instance.SetInternalTimeOfDay (sliderTime.value * 24f);
	}


	public void ChangeTimeLenghtSlider (float value) 
	{
		EnviroSky.instance.GameTime.DayLengthInMinutes = value;
	}

    public void ChangeCloudQuality(int value)
    {
         EnviroSky.instance.cloudsSettings.cloudsQuality = (EnviroCloudSettings.CloudQuality)value;
    }

        public void ChangeQualitySlider () 
	{
		EnviroSky.instance.profile.qualitySettings.GlobalParticleEmissionRates = sliderQuality.value;
	}

	public void ChangeAmbientVolume (float value)
	{
		EnviroSky.instance.Audio.ambientSFXVolume = value;
	}

	public void ChangeWeatherVolume (float value)
	{
		EnviroSky.instance.Audio.weatherSFXVolume = value;
	}


	public void SetWeatherID (int id) 
	{
		EnviroSky.instance.ChangeWeather (id);
	}

    public void SetClouds(int id)
    {
        EnviroSky.instance.cloudsMode = (EnviroSky.EnviroCloudsMode)id;
    }

        public void OverwriteSeason ()
	{
		if (!seasonmode) {
			seasonmode = true;
			EnviroSky.instance.Seasons.calcSeasons = true;
		}
		else {
			seasonmode = false;
			EnviroSky.instance.Seasons.calcSeasons = false;
		}
		
	}

	public void FastDays ()
	{
		if (!fastdays) {
			fastdays = true;
			EnviroSky.instance.GameTime.DayLengthInMinutes = 0.2f;
		}
		else {
			fastdays = false;
			EnviroSky.instance.GameTime.DayLengthInMinutes = 5f;
		}

	}

	public void SetSeason (int id)
	{
		switch (id) 
		{
		case 0:
			EnviroSky.instance.ChangeSeason (EnviroSeasons.Seasons.Spring);
		break;
		case 1:
			EnviroSky.instance.ChangeSeason (EnviroSeasons.Seasons.Summer);
			break;
		case 2:
			EnviroSky.instance.ChangeSeason (EnviroSeasons.Seasons.Autumn);
			break;
		case 3:
			EnviroSky.instance.ChangeSeason (EnviroSeasons.Seasons.Winter);
			break;
		}
	}


	public void SetTimeProgress (int id)
	{
		switch (id) 
		{
		case 0:
			EnviroSky.instance.GameTime.ProgressTime = EnviroTime.TimeProgressMode.None;
			break;
		case 1:
			EnviroSky.instance.GameTime.ProgressTime = EnviroTime.TimeProgressMode.Simulated;
			break;
		case 2:
			EnviroSky.instance.GameTime.ProgressTime = EnviroTime.TimeProgressMode.SystemTime;
			break;
		}
	}
		
	void UpdateWeatherSlider ()
	{
		if (EnviroSky.instance.Weather.currentActiveWeatherPreset != null) {
			for (int i = 0; i < weatherDropdown.options.Count; i++) {
				if (weatherDropdown.options [i].text == EnviroSky.instance.Weather.currentActiveWeatherPreset.Name)
					weatherDropdown.value = i;
			}
		}
	}

	void UpdateSeasonSlider (EnviroSeasons.Seasons s)
	{
		switch (s) {
		case EnviroSeasons.Seasons.Spring:
			seasonDropdown.value = 0;
			break;
		case EnviroSeasons.Seasons.Summer:
			seasonDropdown.value = 1;
			break;
		case EnviroSeasons.Seasons.Autumn:
			seasonDropdown.value = 2;
			break;
		case EnviroSeasons.Seasons.Winter:
			seasonDropdown.value = 3;
			break;
		}
	}

	void Update ()
	{
		if (!EnviroSky.instance.started)
			return;
		else {
			if(!started)
				StartCoroutine(setupDrodown ());
		}

		timeText.text = EnviroSky.instance.GetTimeString ();
		ChangeQualitySlider ();
	}

	void LateUpdate ()
	{
		sliderTime.value = EnviroSky.instance.internalHour / 24f;
	}
}
}