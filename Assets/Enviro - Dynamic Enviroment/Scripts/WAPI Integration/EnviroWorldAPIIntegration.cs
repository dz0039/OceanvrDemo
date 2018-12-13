using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if WORLDAPI_PRESENT
using WAPI;

[AddComponentMenu("Enviro/WAPI Integration")]
[ExecuteInEditMode]
public class EnviroWorldAPIIntegration : MonoBehaviour, IWorldApiChangeHandler
{
	public enum GetSet
	{
		None,
		GetFromWAPI,
		SendToWAPI
	}

	public enum Get
	{
		None,
		GetFromWAPI
	}
		
	public enum Set
	{
		None,
		SendToWAPI
	}

	// Controls
	public GetSet snowPower;
	public GetSet wetnessPower;
	public GetSet fogPower;
	public float fogPowerMult = 1000f;
	public Set windDirection;
	public Set windSpeed;
	public GetSet seasons;
	public GetSet time;
	public GetSet cloudCover;
	public GetSet location;

	// Privates
    private float daysInYear;
    private float timeOfDay;

	private List<EnviroWeatherPreset> weatherPresets = new List<EnviroWeatherPreset>();
	private List<EnviroWeatherPreset> clearWeatherPresets = new List<EnviroWeatherPreset>();
	private List<EnviroWeatherPreset> cloudyWeatherPresets = new List<EnviroWeatherPreset>();
	private List<EnviroWeatherPreset> rainWeatherPresets = new List<EnviroWeatherPreset>();
	private List<EnviroWeatherPreset> snowWeatherPresets = new List<EnviroWeatherPreset>();

    void OnEnable()
    {
        ConnectToWorldAPI();
    }

    void OnDisable()
    {
        DisconnectFromWorldAPI();
    }

    void Start()
    {
        if (EnviroSky.instance == null)
        {
            Debug.LogWarning("EnviroSky not found! Please add EnviroSky prefab to your scene!");
            return;
        }

        daysInYear = EnviroSky.instance.seasonsSettings.SpringInDays + EnviroSky.instance.seasonsSettings.SummerInDays + EnviroSky.instance.seasonsSettings.AutumnInDays + EnviroSky.instance.seasonsSettings.WinterInDays;
        timeOfDay = EnviroSky.instance.internalHour;

		//Create Lists of weather presets
		for (int i = 0; i < EnviroSky.instance.Weather.zones [0].zoneWeatherPresets.Count; i++) {
			weatherPresets.Add(EnviroSky.instance.Weather.zones [0].zoneWeatherPresets[i]);
		}
		
		for (int i = 0; i < weatherPresets.Count; i++) 
		{
			//Clear Weather List
			if (weatherPresets [i].cloudsConfig.coverage <= -0.5)
				clearWeatherPresets.Add (weatherPresets [i]);

			//Cloudy Weather List
			if (weatherPresets [i].cloudsConfig.coverage >= -0.5) {
				if (weatherPresets [i].wetnessLevel == 0f && weatherPresets [i].snowLevel == 0f)
					cloudyWeatherPresets.Add (weatherPresets [i]);
			}

			// Rainy Weather List
			if (weatherPresets [i].wetnessLevel > 0f)
				rainWeatherPresets.Add (weatherPresets [i]);
			//Snowy Weather List
			if (weatherPresets [i].snowLevel > 0f)
				snowWeatherPresets.Add (weatherPresets [i]);
		}

        ConnectToWorldAPI();
    }

    void Update()
    {
        if (EnviroSky.instance == null)
            return;

		if (EnviroSky.instance.Weather.currentActiveWeatherPreset != null) {
			if (snowPower == GetSet.SendToWAPI) {
				WorldManager.Instance.Snow = new Vector4 (EnviroSky.instance.Weather.currentActiveWeatherPreset.snowLevel, EnviroSky.instance.Weather.curSnowStrength, WorldManager.Instance.SnowMinHeight, WorldManager.Instance.SnowAge);
			}

			if (wetnessPower == GetSet.SendToWAPI) {
				WorldManager.Instance.Rain = new Vector4 (EnviroSky.instance.Weather.currentActiveWeatherPreset.wetnessLevel, EnviroSky.instance.Weather.curWetness, WorldManager.Instance.RainMinHeight, WorldManager.Instance.RainMaxHeight);
			}

			if (fogPower == GetSet.SendToWAPI) {
				WorldManager.Instance.Fog = new Vector4 (EnviroSky.instance.Weather.currentActiveWeatherPreset.heightFogDensity * 0.1f, EnviroSky.instance.fogSettings.height, Mathf.Clamp01 (RenderSettings.fogDensity * fogPowerMult), WorldManager.Instance.FogDistanceMax);
			}
		}
		if (windDirection == Set.SendToWAPI) {
			if (EnviroSky.instance.cloudsSettings.useWindZoneDirection)
				WorldManager.Instance.WindDirection = EnviroSky.instance.Components.windZone.transform.eulerAngles.y;
		}

		if (windSpeed == Set.SendToWAPI) {
			WorldManager.Instance.WindSpeed = EnviroSky.instance.Components.windZone.windMain;
		}

		if (seasons  == GetSet.SendToWAPI)
        {
            WorldManager.Instance.Season = Mathf.Lerp(0f, 4f, EnviroSky.instance.currentDay/daysInYear);
        }

		if (time == GetSet.SendToWAPI)
        {
            timeOfDay = EnviroSky.instance.GetUniversalTimeOfDay();
            WorldManager.Instance.SetDecimalTime(timeOfDay);
        }

		if (location == GetSet.SendToWAPI)
        {
            WorldManager.Instance.Latitude = EnviroSky.instance.GameTime.Latitude;
            WorldManager.Instance.Longitude = EnviroSky.instance.GameTime.Longitude;
        }

		if (cloudCover == GetSet.SendToWAPI)
        {
                WorldManager.Instance.CloudPower = Mathf.Clamp01(EnviroSky.instance.cloudsConfig.coverage * EnviroSky.instance.cloudsSettings.globalCloudCoverage);
        }
    }

    void ConnectToWorldAPI()
    {
        WorldManager.Instance.AddListener(this);
    }

    void DisconnectFromWorldAPI()
    {
        WorldManager.Instance.RemoveListener(this);
    }

    /// <summary>
    /// Handle updates from world manager
    /// </summary>
    /// <param name="changeArgs">Change to time of day</param>
    public void OnWorldChanged(WorldChangeArgs changeArgs)
    {
        if (EnviroSky.instance == null)
        {
            return;
        }
			
		// Get Time from WAPI
		if (changeArgs.HasChanged(WorldConstants.WorldChangeEvents.GameTimeChanged) && time == GetSet.GetFromWAPI)
        {
            float newTimeOfDay = (float) changeArgs.manager.GetTimeDecimal();
            if (newTimeOfDay != timeOfDay)
            {
                timeOfDay = newTimeOfDay;
                EnviroSky.instance.SetInternalTimeOfDay(timeOfDay);
            }
        }

		//Get Season from WAPI
		if (changeArgs.HasChanged(WorldConstants.WorldChangeEvents.SeasonChanged) && seasons == GetSet.GetFromWAPI)
		{
			if (WorldManager.Instance.Season < 1f)
				EnviroSky.instance.Seasons.currentSeasons = EnviroSeasons.Seasons.Winter;
			else if (WorldManager.Instance.Season < 2f)
				EnviroSky.instance.Seasons.currentSeasons = EnviroSeasons.Seasons.Spring;
			else if (WorldManager.Instance.Season < 3f)
				EnviroSky.instance.Seasons.currentSeasons = EnviroSeasons.Seasons.Summer;
			else
				EnviroSky.instance.Seasons.currentSeasons = EnviroSeasons.Seasons.Autumn;
		}
			
		// Set Lat/Lng from WAPI
		if (changeArgs.HasChanged(WorldConstants.WorldChangeEvents.LatLngChanged) && location == GetSet.GetFromWAPI)
        {
            EnviroSky.instance.GameTime.Latitude = WorldManager.Instance.Latitude;
            EnviroSky.instance.GameTime.Longitude = WorldManager.Instance.Longitude;
        }

		// Set Distance and Height Fog from WAPI
		if (changeArgs.HasChanged(WorldConstants.WorldChangeEvents.FogChanged) && fogPower == GetSet.GetFromWAPI)
		{
			EnviroSky.instance.fogSettings.distanceFogIntensity = WorldManager.Instance.FogDistancePower * 10f;
			EnviroSky.instance.fogSettings.heightFogIntensity = WorldManager.Instance.FogHeightPower;
			EnviroSky.instance.fogSettings.height = WorldManager.Instance.FogHeightMax;
		}
			
		// Change Weather Based on WAPI Rain and Snow
		if (Application.isPlaying && EnviroSky.instance.Weather.currentActiveWeatherPreset != null) 
		{
			// Cloudy
			if (changeArgs.HasChanged(WorldConstants.WorldChangeEvents.CloudsChanged) && cloudCover == GetSet.GetFromWAPI){
				ChangeWeatherOnCloudCoverChanged ();
			}

			//Rain
			if (changeArgs.HasChanged (WorldConstants.WorldChangeEvents.RainChanged) && wetnessPower == GetSet.GetFromWAPI) {
				ChangeWeatherOnRainChanged (WorldManager.Instance.RainPower,WorldManager.Instance.SnowPower);
			}

			//Snow
			if (changeArgs.HasChanged (WorldConstants.WorldChangeEvents.SnowChanged) && snowPower == GetSet.GetFromWAPI) {
				ChangeWeatherOnSnowChanged (WorldManager.Instance.RainPower,WorldManager.Instance.SnowPower);
			}
		}
    }


    private void ChangeWeatherOnCloudCoverChanged()
    {
        if (WorldManager.Instance.RainPower > 0.01f)
            return;

        if (WorldManager.Instance.SnowPower > 0.01f)
            return;

        float cloudCover = WorldManager.Instance.CloudPower;

        if (cloudCover <= 0.1f)
        {
            if (clearWeatherPresets.Count > 0 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != clearWeatherPresets[0].Name)
                EnviroSky.instance.ChangeWeather(clearWeatherPresets[0].Name);

        }
        else if (cloudCover > 0.1f && cloudCover <= 0.3f)
        {
            if (cloudyWeatherPresets.Count > 0 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != cloudyWeatherPresets[0].Name)
                EnviroSky.instance.ChangeWeather(cloudyWeatherPresets[0].Name);

        }
        else if (cloudCover > 0.3f && cloudCover <= 0.7f)
        {
            if (cloudyWeatherPresets.Count > 1 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != cloudyWeatherPresets[1].Name)
                EnviroSky.instance.ChangeWeather(cloudyWeatherPresets[1].Name);

        }
        else if (cloudCover > 0.7f)
        {
            if (cloudyWeatherPresets.Count > 2 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != cloudyWeatherPresets[2].Name)
                EnviroSky.instance.ChangeWeather(cloudyWeatherPresets[2].Name);

        }
    }



    private void ChangeWeatherOnRainChanged(float r, float s)
    {
        if (r < s || r == 0f)
        {
            if (s > 0)
                ChangeWeatherOnSnowChanged(r, s);
            else
                ChangeWeatherOnCloudCoverChanged();
            return;
        }

        float rainPower = r;

        if (rainPower < 0.1f)
        {
            ChangeWeatherOnCloudCoverChanged();
        }
        else if (rainPower > 0.1f && rainPower <= 0.4f)
        {
            if (rainWeatherPresets.Count > 0 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != rainWeatherPresets[0].Name)
                EnviroSky.instance.ChangeWeather(rainWeatherPresets[0].Name);

        }
        else if (rainPower > 0.4f && rainPower < 0.7f)
        {
            if (rainWeatherPresets.Count > 1 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != rainWeatherPresets[1].Name)
                EnviroSky.instance.ChangeWeather(rainWeatherPresets[1].Name);

        }
        else if (rainPower > 0.7f)
        {
            if (rainWeatherPresets.Count > 2 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != rainWeatherPresets[2].Name)
                EnviroSky.instance.ChangeWeather(rainWeatherPresets[2].Name);
        }
    }

    private void ChangeWeatherOnSnowChanged(float r, float s)
    {
        if (s < r || s == 0f)
        {
            if (r > 0)
                ChangeWeatherOnRainChanged(r, s);
            else
                ChangeWeatherOnCloudCoverChanged();

            return;
        }

        float snowPower = s;

        if (snowPower <= 0.1f)
        {
            ChangeWeatherOnCloudCoverChanged();
        }
        else if (snowPower > 0.1f && snowPower <= 0.5f)
        {
            if (snowWeatherPresets.Count > 0 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != snowWeatherPresets[0].Name)
                EnviroSky.instance.ChangeWeather(snowWeatherPresets[0].Name);

        }
        else if (snowPower > 0.5f)
        {
            if (snowWeatherPresets.Count > 1 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != snowWeatherPresets[1].Name)
                EnviroSky.instance.ChangeWeather(snowWeatherPresets[1].Name);

        }
    }




    /*


    private void ChangeWeatherOnCloudCoverChanged ()
	{
		if (WorldManager.Instance.RainPower > 0.01f && wetnessPower == GetSet.GetFromWAPI)
			return;
		
		if (WorldManager.Instance.SnowPower > 0.01f && snowPower == GetSet.GetFromWAPI)
			return;

		float cloudCover = WorldManager.Instance.CloudPower;

		if (cloudCover < 0.1f) {
			if (clearWeatherPresets.Count > 0 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != clearWeatherPresets[0].Name)
				EnviroSky.instance.ChangeWeather (clearWeatherPresets[0].Name);

		} else if (cloudCover < 0.15f) {
			if (cloudyWeatherPresets.Count > 0 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != cloudyWeatherPresets [0].Name)
				EnviroSky.instance.ChangeWeather (cloudyWeatherPresets [0].Name);

		} else if (cloudCover < 0.3f) {
			if (cloudyWeatherPresets.Count > 1 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != cloudyWeatherPresets[1].Name)
				EnviroSky.instance.ChangeWeather (cloudyWeatherPresets[1].Name);

		} else if (cloudCover < 0.6f) {
			if (cloudyWeatherPresets.Count > 2 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != cloudyWeatherPresets[2].Name)
				EnviroSky.instance.ChangeWeather (cloudyWeatherPresets[2].Name);
		}
	}



	private void ChangeWeatherOnRainChanged (float r, float s)
	{
		if (r < s || r == 0f)
		return;


		float rainPower = r;

		if (rainPower < 0.1f) {
            if (clearWeatherPresets.Count > 0 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != clearWeatherPresets[0].Name)
                //EnviroSky.instance.ChangeWeather (clearWeatherPresets[0].Name);
                ChangeWeatherOnCloudCoverChanged();


        } else if (rainPower < 0.15f) {
			if (cloudyWeatherPresets.Count > 0 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != cloudyWeatherPresets [0].Name)
				EnviroSky.instance.ChangeWeather (cloudyWeatherPresets [0].Name);

		} else if (rainPower < 0.25f) {
			if (rainWeatherPresets.Count > 0 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != rainWeatherPresets[0].Name)
				EnviroSky.instance.ChangeWeather (rainWeatherPresets[0].Name);

		} else if (rainPower < 0.5f) {
			if (rainWeatherPresets.Count > 1 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != rainWeatherPresets[1].Name)
				EnviroSky.instance.ChangeWeather (rainWeatherPresets[1].Name);

		} else if (rainPower < 0.75f) {
			if (rainWeatherPresets.Count > 2 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != rainWeatherPresets[2].Name)
				EnviroSky.instance.ChangeWeather (rainWeatherPresets[2].Name);
		}
	}

	private void ChangeWeatherOnSnowChanged (float r,float s)
	{
		if (s < r || s == 0f)
		return;

		float snowPower = s;

		if (snowPower < 0.1f) {
            if (clearWeatherPresets.Count > 0 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != clearWeatherPresets[0].Name)
                //EnviroSky.instance.ChangeWeather (clearWeatherPresets[0].Name);
                ChangeWeatherOnCloudCoverChanged();

        }
        else if (snowPower < 0.15f) {
			if (cloudyWeatherPresets.Count > 0 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != cloudyWeatherPresets [0].Name)
			  EnviroSky.instance.ChangeWeather (cloudyWeatherPresets [0].Name);

		} else if (snowPower < 0.25f) {
			if (snowWeatherPresets.Count > 0 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != snowWeatherPresets[0].Name)
				EnviroSky.instance.ChangeWeather (snowWeatherPresets[0].Name);

		} else if (snowPower < 0.5f) {
			if (snowWeatherPresets.Count > 1 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != snowWeatherPresets[1].Name)
				EnviroSky.instance.ChangeWeather (snowWeatherPresets[1].Name);

		} else if (snowPower < 0.75f) {
			if (snowWeatherPresets.Count > 2 && EnviroSky.instance.Weather.currentActiveWeatherPreset.Name != snowWeatherPresets[2].Name)
				EnviroSky.instance.ChangeWeather (snowWeatherPresets[2].Name);
		}
	}

    */
}
#endif