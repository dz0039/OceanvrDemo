/// <summary>
/// This component can be used to synchronize time and weather in games where server is a player too.
/// </summary>

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
[AddComponentMenu("Enviro/Integration/UNet Player")]
[RequireComponent(typeof (NetworkIdentity))]
public class EnviroUNetPlayer : NetworkBehaviour {

	public bool assignOnStart = true;
    public bool findSceneCamera = true;

    public GameObject Player;
	public Camera PlayerCamera;

	public void Start()
	{
		// Deactivate if it isn't ours!
		if (!isLocalPlayer && !isServer) {
			this.enabled = false;
			return;
		}
  
        if (PlayerCamera == null && findSceneCamera)
            PlayerCamera = Camera.main;

        if (isLocalPlayer) 
		{
			if (assignOnStart && Player != null && PlayerCamera != null)
				EnviroSky.instance.AssignAndStart (Player, PlayerCamera);

			Cmd_RequestSeason ();
			Cmd_RequestCurrentWeather ();
		}
	}
		
	[Command]
	void Cmd_RequestSeason ()
	{
		RpcRequestSeason((int)EnviroSky.instance.Seasons.currentSeasons);
	}

	[ClientRpc]
	void RpcRequestSeason (int season)
	{
		EnviroSky.instance.Seasons.currentSeasons = (EnviroSeasons.Seasons)season;
	}

	[Command]
	void Cmd_RequestCurrentWeather ()
	{
		for (int i = 0; i < EnviroSky.instance.Weather.zones.Count; i++) 
		{
			for (int w = 0; w < EnviroSky.instance.Weather.WeatherPrefabs.Count; w++)
			{
				if(EnviroSky.instance.Weather.WeatherPrefabs[w] == EnviroSky.instance.Weather.zones[i].currentActiveZoneWeatherPrefab)
					RpcRequestCurrentWeather(w,i);
			}
		}
	}

	[ClientRpc]
	void RpcRequestCurrentWeather (int weather, int zone)
	{
		EnviroSky.instance.Weather.zones[zone].currentActiveZoneWeatherPrefab = EnviroSky.instance.Weather.WeatherPrefabs[weather];
        EnviroSky.instance.Weather.zones[zone].currentActiveZoneWeatherPreset = EnviroSky.instance.Weather.WeatherPrefabs[weather].weatherPreset;
    }
}
