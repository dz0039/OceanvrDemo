using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Enviro/Weather Zone")]
public class EnviroZone : MonoBehaviour {

	public enum WeatherUpdateMode
	{
		GameTimeHours,
		RealTimeMinutes
	}

	[Tooltip("Defines the zone name.")]
	public string zoneName;
	[Tooltip("Uncheck to remove OnTriggerExit call when using overlapping zone layout.")]
	public bool ExitToDefault = true;

	public List<EnviroWeatherPrefab> zoneWeather = new List<EnviroWeatherPrefab>();
	public List<EnviroWeatherPrefab> curPossibleZoneWeather;

	[Header("Zone weather settings:")]
	[Tooltip("Add all weather prefabs for this zone here.")]
	public List<EnviroWeatherPreset> zoneWeatherPresets = new List<EnviroWeatherPreset>();
	[Tooltip("Shall weather changes occure based on gametime or realtime?")]
	public WeatherUpdateMode updateMode = WeatherUpdateMode.GameTimeHours;
	[Tooltip("Defines how often (gametime hours or realtime minutes) the system will heck to change the current weather conditions.")]
	public float WeatherUpdateIntervall = 6f;
	[Header("Zone scaling and gizmo:")]
	[Tooltip("Defines the zone scale.")]
	public Vector3 zoneScale = new Vector3 (100f, 100f, 100f);
	[Tooltip("Defines the color of the zone's gizmo in editor mode.")]
	public Color zoneGizmoColor = Color.gray;

	[Header("Current active weather:")]
	[Tooltip("The current active weather conditions.")]
	public EnviroWeatherPrefab currentActiveZoneWeatherPrefab;
	public EnviroWeatherPreset currentActiveZoneWeatherPreset;
	[HideInInspector]public EnviroWeatherPrefab lastActiveZoneWeatherPrefab;
	[HideInInspector]public EnviroWeatherPreset lastActiveZoneWeatherPreset;

	private BoxCollider zoneCollider;
	private double nextUpdate;
	private float nextUpdateRealtime;
	private bool init = false;
	private bool isDefault;


	void Start () 
	{
		if (zoneWeatherPresets.Count > 0)
		{
			zoneCollider = gameObject.AddComponent<BoxCollider> ();
			zoneCollider.isTrigger = true;

			if (!GetComponent<EnviroSky> ())
				EnviroSky.instance.RegisterZone (this);
			else 
				isDefault = true;

			UpdateZoneScale ();
			nextUpdate = EnviroSky.instance.currentTimeInHours + WeatherUpdateIntervall;
			nextUpdateRealtime = Time.time + (WeatherUpdateIntervall * 60f); 
		}
		else
		{
			Debug.LogError("Please add Weather Prefabs to Zone:" + gameObject.name);
		}
	}

	public void UpdateZoneScale ()
	{
		if (!isDefault)
			zoneCollider.size = zoneScale;
		else
			zoneCollider.size = (Vector3.one * (1f / EnviroSky.instance.transform.localScale.y)) * 0.25f;
	}

	public void CreateZoneWeatherTypeList ()
	{
		// Add new WeatherPrefabs
		for ( int i = 0; i < zoneWeatherPresets.Count; i++)
		{
			if (zoneWeatherPresets [i] == null) {
				Debug.Log ("Warning! Missing Weather Preset in Zone: " + this.zoneName);
				return;
			}

			bool addThis = true;
			for (int i2 = 0; i2 < EnviroSky.instance.Weather.weatherPresets.Count; i2++)
			{
				if (zoneWeatherPresets [i] == EnviroSky.instance.Weather.weatherPresets [i2]) 
				{
					addThis = false;
					zoneWeather.Add (EnviroSky.instance.Weather.WeatherPrefabs [i2]);
				}
			}

			if (addThis) {
				GameObject wPrefab = new GameObject ();
				EnviroWeatherPrefab wP = wPrefab.AddComponent<EnviroWeatherPrefab> ();
				wP.weatherPreset = zoneWeatherPresets [i];
				wPrefab.name = wP.weatherPreset.Name;

				// Check and create particle systems.
				for (int w = 0; w < wP.weatherPreset.effectSystems.Count; w++)
				{
					if (wP.weatherPreset.effectSystems [w] == null || wP.weatherPreset.effectSystems [w].prefab == null) {
						Debug.Log ("Warning! Missing Particle System Entry: " + wP.weatherPreset.Name);
						Destroy (wPrefab);
						return;
					}
					GameObject eS = (GameObject)Instantiate (wP.weatherPreset.effectSystems [w].prefab, wPrefab.transform);
					eS.transform.localPosition = wP.weatherPreset.effectSystems [w].localPositionOffset;
					eS.transform.localEulerAngles = wP.weatherPreset.effectSystems [w].localRotationOffset;
					ParticleSystem pS = eS.GetComponent<ParticleSystem> ();

					if (pS != null)
						wP.effectSystems.Add (pS);
					else {
						pS = eS.GetComponentInChildren<ParticleSystem> ();
						if (pS != null)
							wP.effectSystems.Add (pS);
						else {
							Debug.Log ("No Particle System found in prefab in weather preset: " + wP.weatherPreset.Name);
							Destroy (wPrefab);
							return;
						}
					}
				}
				wP.effectEmmisionRates.Clear ();
				wPrefab.transform.parent = EnviroSky.instance.Weather.VFXHolder.transform;
				wPrefab.transform.localPosition = Vector3.zero;
				wPrefab.transform.localRotation = Quaternion.identity;
				zoneWeather.Add(wP);

				EnviroSky.instance.Weather.WeatherPrefabs.Add (wP);
				EnviroSky.instance.Weather.weatherPresets.Add (zoneWeatherPresets [i]);
			}
		}
		
        // Setup Particle Systems Emission Rates
		for (int i = 0; i < zoneWeather.Count; i++)
		{
			for (int i2 = 0; i2 < zoneWeather[i].effectSystems.Count; i2++)
			{
				zoneWeather[i].effectEmmisionRates.Add(EnviroSky.GetEmissionRate(zoneWeather[i].effectSystems[i2]));
				EnviroSky.SetEmissionRate(zoneWeather[i].effectSystems[i2],0f);
			}   
		}
			
        //Set initial weather
		if (isDefault && EnviroSky.instance.Weather.startWeatherPreset != null) 
		{
            EnviroSky.instance.SetWeatherOverwrite(EnviroSky.instance.Weather.startWeatherPreset);

            for (int i = 0; i < zoneWeather.Count; i++)
            {
                if(zoneWeather[i].weatherPreset == EnviroSky.instance.Weather.startWeatherPreset)
                {
                    currentActiveZoneWeatherPrefab = zoneWeather[i];
                    lastActiveZoneWeatherPrefab = zoneWeather[i];
                }
            }
            currentActiveZoneWeatherPreset = EnviroSky.instance.Weather.startWeatherPreset;
            lastActiveZoneWeatherPreset = EnviroSky.instance.Weather.startWeatherPreset;
		} 
		else 
		{
			currentActiveZoneWeatherPrefab = zoneWeather [0];
			lastActiveZoneWeatherPrefab = zoneWeather [0];
			currentActiveZoneWeatherPreset = zoneWeatherPresets [0];
			lastActiveZoneWeatherPreset = zoneWeatherPresets [0];
		}

		nextUpdate = EnviroSky.instance.currentTimeInHours + WeatherUpdateIntervall;
	}
		
	void BuildNewWeatherList ()
	{
		curPossibleZoneWeather = new List<EnviroWeatherPrefab> ();
		for (int i = 0; i < zoneWeather.Count; i++) 
		{
			switch (EnviroSky.instance.Seasons.currentSeasons)
			{
			case EnviroSeasons.Seasons.Spring:
				if (zoneWeather[i].weatherPreset.Spring)
					curPossibleZoneWeather.Add(zoneWeather[i]);
				break;
			case EnviroSeasons.Seasons.Summer:
				if (zoneWeather[i].weatherPreset.Summer)
					curPossibleZoneWeather.Add(zoneWeather[i]);
				break;
			case EnviroSeasons.Seasons.Autumn:
				if (zoneWeather[i].weatherPreset.Autumn)
					curPossibleZoneWeather.Add(zoneWeather[i]);
				break;
			case EnviroSeasons.Seasons.Winter:
				if (zoneWeather[i].weatherPreset.winter)
					curPossibleZoneWeather.Add(zoneWeather[i]);
				break;
			}
		} 
	}

	EnviroWeatherPrefab PossibiltyCheck ()
	{
		List<EnviroWeatherPrefab> over = new List<EnviroWeatherPrefab> ();

		for (int i = 0 ; i < curPossibleZoneWeather.Count;i++)
		{
			int würfel = UnityEngine.Random.Range (0,100);

			if (EnviroSky.instance.Seasons.currentSeasons == EnviroSeasons.Seasons.Spring)
			{
				if (würfel <= curPossibleZoneWeather[i].weatherPreset.possibiltyInSpring)
					over.Add(curPossibleZoneWeather[i]);
			}else
			if (EnviroSky.instance.Seasons.currentSeasons == EnviroSeasons.Seasons.Summer)
			{
					if (würfel <= curPossibleZoneWeather[i].weatherPreset.possibiltyInSummer)
					over.Add(curPossibleZoneWeather[i]);
			}else
			if (EnviroSky.instance.Seasons.currentSeasons == EnviroSeasons.Seasons.Autumn)
			{
						if (würfel <= curPossibleZoneWeather[i].weatherPreset.possibiltyInAutumn)
					over.Add(curPossibleZoneWeather[i]);
			}else
			if (EnviroSky.instance.Seasons.currentSeasons == EnviroSeasons.Seasons.Winter)
			{
							if (würfel <= curPossibleZoneWeather[i].weatherPreset.possibiltyInWinter)
					over.Add(curPossibleZoneWeather[i]);
			}
		} 

		if (over.Count > 0)
		{		
			EnviroSky.instance.NotifyZoneWeatherChanged (over [0].weatherPreset, this);
			return over [0];
		}
		else
			return currentActiveZoneWeatherPrefab;
	}
		
	void WeatherUpdate ()
	{
		nextUpdate = EnviroSky.instance.currentTimeInHours + WeatherUpdateIntervall;
		nextUpdateRealtime = Time.time + (WeatherUpdateIntervall * 60f); 

		BuildNewWeatherList ();

		lastActiveZoneWeatherPrefab = currentActiveZoneWeatherPrefab;
		lastActiveZoneWeatherPreset = currentActiveZoneWeatherPreset;
		currentActiveZoneWeatherPrefab = PossibiltyCheck ();
		currentActiveZoneWeatherPreset = currentActiveZoneWeatherPrefab.weatherPreset;
		EnviroSky.instance.NotifyZoneWeatherChanged (currentActiveZoneWeatherPreset, this);
	}

    IEnumerator CreateWeatherListLate ()
	{
		yield return 0;
		CreateZoneWeatherTypeList ();
		init = true;
	}

	void LateUpdate () 
	{
        if (EnviroSky.instance == null)
        {
            Debug.Log("No EnviroSky instance found!");
            return;
        }

        if (EnviroSky.instance.started && !init) 
		{
			if (isDefault) {
				CreateZoneWeatherTypeList ();          
                init = true;
            } else
				StartCoroutine (CreateWeatherListLate ());
		}

		if (updateMode == WeatherUpdateMode.GameTimeHours) {
			if (EnviroSky.instance.currentTimeInHours > nextUpdate && EnviroSky.instance.Weather.updateWeather && EnviroSky.instance.started)
				WeatherUpdate ();
		} else {
			if (Time.time > nextUpdateRealtime && EnviroSky.instance.Weather.updateWeather && EnviroSky.instance.started)
				WeatherUpdate ();
		}

        if (EnviroSky.instance.Player == null)
        {
            // Debug.Log("No Player Assigned in EnviroSky object!");
            return;
        }

        if (isDefault && init)                               
			zoneCollider.center = new Vector3(0f,(EnviroSky.instance.Player.transform.position.y-EnviroSky.instance.transform.position.y) / EnviroSky.instance.transform.lossyScale.y,0f);
	}


	/// Triggers
	void OnTriggerEnter (Collider col)
	{
		if (EnviroSky.instance == null)
			return;

		if (EnviroSky.instance.profile.weatherSettings.useTag) {
			if (col.gameObject.tag == EnviroSky.instance.gameObject.tag) {
				EnviroSky.instance.Weather.currentActiveZone = this;
				EnviroSky.instance.NotifyZoneChanged (this);
			}
		} else {
			if (col.gameObject.GetComponent<EnviroSky> ()) {
				EnviroSky.instance.Weather.currentActiveZone = this;
				EnviroSky.instance.NotifyZoneChanged (this);
			}
		}
	}

	void OnTriggerExit (Collider col)
	{
		if (ExitToDefault == false || EnviroSky.instance == null)
			return;
		
		if (EnviroSky.instance.profile.weatherSettings.useTag) {
			if (col.gameObject.tag == EnviroSky.instance.gameObject.tag) {
				EnviroSky.instance.Weather.currentActiveZone = EnviroSky.instance.Weather.zones[0];
				EnviroSky.instance.NotifyZoneChanged (EnviroSky.instance.Weather.zones[0]);
			}
		} else {
			if (col.gameObject.GetComponent<EnviroSky> ()) {
				EnviroSky.instance.Weather.currentActiveZone = EnviroSky.instance.Weather.zones[0];
				EnviroSky.instance.NotifyZoneChanged (EnviroSky.instance.Weather.zones[0]);
			}
		}
	}


	void OnDrawGizmos () 
	{
		Gizmos.color = zoneGizmoColor;
		Gizmos.DrawCube (transform.position, new Vector3(zoneScale.x,zoneScale.y,zoneScale.z));
	}
}
