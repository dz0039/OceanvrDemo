/// <summary>
/// This component can be used to synchronize time and weather.
/// </summary>

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
[AddComponentMenu("Enviro/Integration/UNet Server Component")]
[RequireComponent(typeof (NetworkIdentity))]
public class EnviroUNetServer : NetworkBehaviour {

	public float updateSmoothing = 15f;

	[SyncVar] private float networkHours;
	[SyncVar] private int networkDays;
	[SyncVar] private int networkYears;

	public bool isHeadless = true;

	public override void OnStartServer()
	{
		if (isHeadless) {
			EnviroSky.instance.StartAsServer();
		}
			
		EnviroSky.instance.Weather.updateWeather = true;
			
		EnviroSky.instance.OnSeasonChanged += (EnviroSeasons.Seasons season) => {
			SendSeasonToClient (season);
		};
		EnviroSky.instance.OnZoneWeatherChanged += (EnviroWeatherPreset type, EnviroZone zone) => {
			SendWeatherToClient (type, zone);
		};
	}

	public void Start ()
	{
		if (!isServer) {
			EnviroSky.instance.GameTime.ProgressTime = EnviroTime.TimeProgressMode.None;
			EnviroSky.instance.Weather.updateWeather = false;
		}
	}

	void SendWeatherToClient (EnviroWeatherPreset w, EnviroZone z)
	{
		int zoneID = 0;

		for (int i = 0; i < EnviroSky.instance.Weather.zones.Count; i++) 
		{
			if (EnviroSky.instance.Weather.zones [i] == z)
				zoneID = i;

		}

		for (int i = 0; i < EnviroSky.instance.Weather.weatherPresets.Count; i++) {

			if (EnviroSky.instance.Weather.weatherPresets [i] == w)
				RpcWeatherUpdate (i,zoneID);
		}
	}

	void SendSeasonToClient (EnviroSeasons.Seasons s)
	{
		RpcSeasonUpdate((int)s);
	}

	[ClientRpc]
	void RpcSeasonUpdate (int season)
	{
		EnviroSky.instance.Seasons.currentSeasons = (EnviroSeasons.Seasons)season;
	}

	[ClientRpc]
	void RpcWeatherUpdate (int weather, int zone)
	{
		EnviroSky.instance.Weather.zones[zone].currentActiveZoneWeatherPrefab = EnviroSky.instance.Weather.WeatherPrefabs [weather];
		EnviroSky.instance.Weather.zones[zone].currentActiveZoneWeatherPreset = EnviroSky.instance.Weather.WeatherPrefabs [weather].weatherPreset;
	}


	void Update ()
	{
        if (EnviroSky.instance == null)
            return;

        if (!isServer) 
		{
			if (networkHours < 1f && EnviroSky.instance.internalHour > 23f)
				EnviroSky.instance.SetInternalTimeOfDay(networkHours);

			EnviroSky.instance.SetInternalTimeOfDay(Mathf.Lerp (EnviroSky.instance.internalHour, (float)networkHours, Time.deltaTime * updateSmoothing));
			EnviroSky.instance.GameTime.Days = networkDays;
			EnviroSky.instance.GameTime.Years = networkYears;

		} else {
			networkHours = EnviroSky.instance.internalHour;
			networkDays = EnviroSky.instance.GameTime.Days;
			networkYears = EnviroSky.instance.GameTime.Years;
		}

	}
}

