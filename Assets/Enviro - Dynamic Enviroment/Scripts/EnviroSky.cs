////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////        EnviroSky- Renders sky with sun, moon, clouds and weather.          ////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

[Serializable]
public class EnviroSeasons
{
	public enum Seasons
	{
		Spring,
		Summer,
		Autumn,
		Winter,
	}
	[Tooltip("When enabled the system will change seasons automaticly when enough days passed.")]
	public bool calcSeasons; // if unticked you can manually overwrite current seas. Ticked = automaticly updates seasons
	[Tooltip("The current season.")]
	public Seasons currentSeasons;
	[HideInInspector]
	public Seasons lastSeason;
}

[Serializable]
public class EnviroAudio // AudioSetup variables
{
	[Tooltip("The prefab with AudioSources used by Enviro. Will be instantiated at runtime.")]
	public GameObject SFXHolderPrefab;

	[Header("Volume Settings:")]
	[Range(0f,1f)][Tooltip("The volume of ambient sounds played by enviro.")]
	public float ambientSFXVolume = 0.5f;
	[Range(0f,1f)][Tooltip("The volume of weather sounds played by enviro.")]
	public float weatherSFXVolume = 1.0f;

	[HideInInspector]public EnviroAudioSource currentAmbientSource;
	[HideInInspector]public float ambientSFXVolumeMod = 0f;
	[HideInInspector]public float weatherSFXVolumeMod = 0f;
}



[Serializable]
public class EnviroComponents // References - setup these in inspector! Or use the provided prefab.
{
	[Tooltip("The Enviro sun object.")]
	public GameObject Sun = null;
	[Tooltip("The Enviro moon object.")]
	public GameObject Moon = null;
	[Tooltip("The directional light for direct sun and moon lighting.")]
	public Transform DirectLight;
	[Tooltip("The Enviro global reflection probe for dynamic reflections.")]
	public ReflectionProbe GlobalReflectionProbe;
	[Tooltip("Your WindZone that reflect our weather wind settings.")]
	public WindZone windZone;
	[Tooltip("The Enviro Lighting Flash Component.")]
	public EnviroLightning LightningGenerator; // Creates lightning Flashes
	[Tooltip("Link to the object that hold all additional satellites as childs.")]
	public Transform satellites;
	[Tooltip("Just a transform for stars rotation calculations. ")]
	public Transform starsRotation = null;
    [Tooltip("Plane to cast cloud shadows.")]
    public GameObject cloudsShadowPlane = null;
}

[Serializable]
public class EnviroSatellite 
{
	[Tooltip("Name of this satellite")]
	public string name;
	[Tooltip("Prefab with model that get instantiated.")]
	public GameObject prefab = null;
	[Tooltip("Orbit distance.")]
	public float orbit;
	[Tooltip("Orbit modification on x axis.")]
	public float xRot;
	[Tooltip("Orbit modification on y axis.")]
	public float yRot;
}

[Serializable]
public class EnviroWeather 
{
	[Tooltip("If disabled the weather will never change.")]
	public bool updateWeather = true;
	[HideInInspector]public List<EnviroWeatherPreset> weatherPresets = new List<EnviroWeatherPreset>();
	[HideInInspector]public List<EnviroWeatherPrefab> WeatherPrefabs = new List<EnviroWeatherPrefab>();
	[Tooltip("List of additional zones. Will be updated on startup!")]
	public List<EnviroZone> zones = new List<EnviroZone>();
	public EnviroWeatherPreset startWeatherPreset;
	[Tooltip("The current active zone.")]
	public EnviroZone currentActiveZone;
	[Tooltip("The current active weather conditions.")]
	public EnviroWeatherPrefab currentActiveWeatherPrefab;
	public EnviroWeatherPreset currentActiveWeatherPreset;

	[HideInInspector]public EnviroWeatherPrefab lastActiveWeatherPrefab;
	[HideInInspector]public EnviroWeatherPreset lastActiveWeatherPreset;

	[HideInInspector]public GameObject VFXHolder;
	[HideInInspector]public float wetness;
	[HideInInspector]public float curWetness;
	[HideInInspector]public float snowStrength;
	[HideInInspector]public float curSnowStrength;
	[HideInInspector]public int thundersfx;
	[HideInInspector]public EnviroAudioSource currentAudioSource;
	[HideInInspector]public bool weatherFullyChanged = false;
}

[Serializable]
public class EnviroTime // GameTime variables
{
	public enum TimeProgressMode
	{
		None,
		Simulated,
		OneDay,
		SystemTime
	}

	[Tooltip("None = No time auto time progressing, Simulated = Time calculated with DayLenghtInMinutes, SystemTime = uses your systemTime.")]
	public TimeProgressMode ProgressTime = TimeProgressMode.Simulated;
	[Tooltip("Current Time: minutes")][Range(0,60)]
	public int Seconds  = 0; 
	[Tooltip("Current Time: minutes")][Range(0,60)]
	public int Minutes  = 0; 
	[Tooltip("Current Time: hours")][Range(0,24)]
	public int Hours  = 12; 
	[Tooltip("Current Time: Days")]
	public int Days = 1; 
	[Tooltip("Current Time: Years")]
	public int Years = 1;
	[Space(20)]
	[Tooltip("Day lenght in realtime minutes.")]
	public float DayLengthInMinutes = 5f; // DayLength in realtime minutes
	[Tooltip("Night lenght in realtime minutes.")]
	public float NightLengthInMinutes = 5f; // DayLength in realtime minutes

	[Range(-13,13)][Tooltip("Time offset for timezones")]
	public int utcOffset = 0;
	[Range(-90,90)] [Tooltip("-90,  90   Horizontal earth lines")]
	public float Latitude   = 0f; 
	[Range(-180,180)] [Tooltip("-180, 180  Vertical earth line")]
	public float Longitude  = 0f; 
	[HideInInspector]public float solarTime; 
	[HideInInspector]public float lunarTime;
}



[Serializable]
public class EnviroFogging
{
	[HideInInspector]
	public float skyFogHeight = 1f;
	[HideInInspector]
	public float skyFogStrength = 0.1f;
	[HideInInspector]
	public float scatteringStrenght = 0.5f;
	[HideInInspector]
	public float sunBlocking = 0.5f;
}

[Serializable]
public class EnviroLightshafts 
{
	[Tooltip("Use light shafts?")]
	public bool sunLightShafts = true;
	public bool moonLightShafts = true;
}

[Serializable]
public class EnviroCloudsLayer 
{
	[HideInInspector]
	public GameObject myObj;
	[HideInInspector]
	public Material myMaterial;
	[HideInInspector]
	public Material myShadowMaterial;
	[HideInInspector]
	public float DirectLightIntensity = 10f;
	[HideInInspector][Tooltip("Base color of clouds.")]
	public Color FirstColor = Color.white;
	[HideInInspector][Tooltip("Coverage rate of clouds generated.")]
	public float Coverage = 0f; // 
	[HideInInspector][Tooltip("Density of clouds generated.")]
	public float Density = 0f; 
	[HideInInspector][Tooltip("Clouds detail normal power modificator.")]
	public float DetailPower = 2f;
	[HideInInspector][Tooltip("Clouds alpha modificator.")]
	public float Alpha = 0f;
}



[ExecuteInEditMode]
public class EnviroSky : MonoBehaviour
{
    private static EnviroSky _instance; // Creat a static instance for easy access!

    public static EnviroSky instance
    {
        get
        {
            //If _instance hasn't been set yet, we grab it from the scene!
            //This will only happen the first time this reference is used.
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<EnviroSky>();
            return _instance;
        }
    }

    public string prefabVersion = "2.0.4";

    [Tooltip("Assign your player gameObject here. Required Field! or enable AssignInRuntime!")]
    public GameObject Player;
    [Tooltip("Assign your main camera here. Required Field! or enable AssignInRuntime!")]
    public Camera PlayerCamera;
    [Tooltip("If enabled Enviro will search for your Player and Camera by Tag!")]
    public bool AssignInRuntime;
    [Tooltip("Your Player Tag")]
    public string PlayerTag = "";
    [Tooltip("Your CameraTag")]
    public string CameraTag = "MainCamera";
    [Header("General")]
    [Tooltip("Enable this when using singlepass rendering.")]
    public bool dontDestroy = false;
    [Header("Camera Settings")]
    [Tooltip("Enable HDR Rendering. You want to use a third party tonemapping effect for best results!")]
    public bool HDR = true;
    [Header("Layer Setup")]
    [Tooltip("This is the layer id forfor the moon.")]
    public int moonRenderingLayer = 29;
    [Tooltip("This is the layer id for additional satellites like moons, planets.")]
    public int satelliteRenderingLayer = 30;
    [Tooltip("Activate to set recommended maincamera clear flag.")]
    public bool setCameraClearFlags = true;
    [Header("Virtual Reality")]
    [Tooltip("Enable this when using singlepass rendering.")]
    public bool singlePassVR = false;
    [Tooltip("Enable this to activate volume lighing")]
    [HideInInspector]
    public bool volumeLighting = true;
    [Tooltip("Enable this to activate global scattering fog. Disabled will also disable volume lighting")]
    [HideInInspector]
    public bool globalFog = true;

    [Header("Profile")]
    public EnviroProfile profile = null;
    // Parameters
    [Header("Control")]
    public EnviroTime GameTime = null;
    public EnviroAudio Audio = null;
    public EnviroWeather Weather = null;
    public EnviroSeasons Seasons = null;
    public EnviroFogging Fog = null;
    public EnviroLightshafts LightShafts = null;

    [Header("Components")]
    public EnviroComponents Components = null;

    //Runtime Settings
    [HideInInspector]public bool started;
    [HideInInspector]public bool isNight = true;
    // Runtime profile
    [HideInInspector]public EnviroLightSettings lightSettings = new EnviroLightSettings();
    [HideInInspector]public EnviroVolumeLightingSettings volumeLightSettings = new EnviroVolumeLightingSettings();
    [HideInInspector]public EnviroSkySettings skySettings = new EnviroSkySettings();
    [HideInInspector]public EnviroCloudSettings cloudsSettings = new EnviroCloudSettings();
    [HideInInspector]public EnviroWeatherSettings weatherSettings = new EnviroWeatherSettings();
    [HideInInspector]public EnviroFogSettings fogSettings = new EnviroFogSettings();
    [HideInInspector]public EnviroLightShaftsSettings lightshaftsSettings = new EnviroLightShaftsSettings();
    [HideInInspector]public EnviroSeasonSettings seasonsSettings = new EnviroSeasonSettings();
    [HideInInspector]public EnviroAudioSettings audioSettings = new EnviroAudioSettings();
    [HideInInspector]public EnviroSatellitesSettings satelliteSettings = new EnviroSatellitesSettings();
    [HideInInspector]public EnviroQualitySettings qualitySettings = new EnviroQualitySettings();

    public enum EnviroCloudsMode
    {
        None,
        Both,
        Volume,
        Flat
    }

    public EnviroCloudsMode cloudsMode;

    private EnviroCloudsMode lastCloudsMode;
    private EnviroCloudSettings.CloudQuality lastCloudsQuality;
    private Material cloudShadows;

    // Camera Components
    [HideInInspector]public Camera moonCamera;
    [HideInInspector]public Camera satCamera;
    //[HideInInspector]public GameObject renderCameraHolder;
    [HideInInspector]public EnviroVolumeLight directVolumeLight;
    [HideInInspector]public EnviroLightShafts lightShaftsScriptSun;
    [HideInInspector]public EnviroLightShafts lightShaftsScriptMoon;
    [HideInInspector]public EnviroSkyRendering EnviroSkyRender;
    // Weather SFX
    [HideInInspector]public GameObject EffectsHolder;
    [HideInInspector]public EnviroAudioSource AudioSourceWeather;
    [HideInInspector]public EnviroAudioSource AudioSourceWeather2;
    [HideInInspector]public EnviroAudioSource AudioSourceAmbient;
    [HideInInspector]public EnviroAudioSource AudioSourceAmbient2;
    [HideInInspector]public AudioSource AudioSourceThunder;
    [HideInInspector]public EnviroAudioSource AudioSourceZone;
    // Vegeation Growth
    [HideInInspector]public List<EnviroVegetationInstance> EnviroVegetationInstances = new List<EnviroVegetationInstance>(); // All EnviroInstance that getting updated at the moment.
                                                                                                                             //Sky runtime
    [HideInInspector]public Color currentWeatherSkyMod;
    [HideInInspector]public Color currentWeatherLightMod;
    [HideInInspector]public Color currentWeatherFogMod;
    //Interior Mods
    [HideInInspector]public Color currentInteriorDirectLightMod;
    [HideInInspector]public Color currentInteriorAmbientLightMod;
    [HideInInspector]public Color currentInteriorAmbientEQLightMod;
    [HideInInspector]public Color currentInteriorAmbientGRLightMod;
    [HideInInspector]public Color currentInteriorSkyboxMod;
    [HideInInspector]public Color currentInteriorFogColorMod = new Color(0, 0, 0, 0);
    [HideInInspector]public float currentInteriorFogMod = 1f;
    [HideInInspector]public float currentInteriorWeatherEffectMod = 1f;
    [HideInInspector]public float currentInteriorZoneAudioVolume = 1f;
    [HideInInspector]public float currentInteriorZoneAudioFadingSpeed = 1f;

    //VolueLighting
    [HideInInspector]public float globalVolumeLightIntensity;
    //clouds runtime
    [HideInInspector]public EnviroWeatherCloudsConfig cloudsConfig;
    [HideInInspector]public float thunder = 0f;
    // Satellites
    [HideInInspector]public List<GameObject> satellites = new List<GameObject>();
    [HideInInspector]public List<GameObject> satellitesRotation = new List<GameObject>();
    // Used from other Enviro componets
    [HideInInspector]public DateTime dateTime;
    [HideInInspector]public float internalHour;
    [HideInInspector]public float currentHour;
    [HideInInspector]public float currentDay;
    [HideInInspector]public float currentYear;
    [HideInInspector]public double currentTimeInHours;
    // Render Textures
    [HideInInspector]public RenderTexture cloudsRenderTarget;
    [HideInInspector]public RenderTexture flatCloudsRenderTarget;
    [HideInInspector]public Material flatCloudsMat;
    [HideInInspector]public RenderTexture weatherMap;
    [HideInInspector]public RenderTexture moonRenderTarget;
    [HideInInspector]public RenderTexture satRenderTarget;
    // Moon Phase
    [HideInInspector]public float customMoonPhase = 0.0f;
    //AQUAS Fog Handling
    [HideInInspector]public bool updateFogDensity = true;
    [HideInInspector]public Color customFogColor = Color.black;
    [HideInInspector]public float customFogIntensity = 0f;
    // Profile
    [HideInInspector]public bool profileLoaded = false;
    [HideInInspector]public bool interiorMode = false;
    [HideInInspector]public EnviroInterior lastInteriorZone;
    //Wind
    [HideInInspector]public Vector2 cloudAnim;
    [HideInInspector]public Vector2 cloudAnimNonScaled;

    [HideInInspector]
    public Material skyMat;
    //private
    private Transform DomeTransform;
	private Transform SunTransform;
	private Light MainLight;
	private Transform MoonTransform;
	private Renderer MoonRenderer;
	private Material MoonShader;
	private float lastHourUpdate;
	private float starsRot;
	private float lastHour;
	private double lastRelfectionUpdate;
    private double lastMoonUpdate;
    private float lastAmbientSkyUpdate;
	private float OrbitRadius
	{
		get { return DomeTransform.localScale.x; }
	}
	private bool serverMode = false;
    //Cloud Shadows
    private RenderTexture cloudShadowMap;
    private Material cloudShadowMat;

    // Scattering constants
    const float pi = Mathf.PI;
	private Vector3 K =  new Vector3(686.0f, 678.0f, 666.0f);
	private const float n =  1.0003f;   
	private const float N =  2.545E25f;
	private const float pn =  0.035f;
	private float hourTime;
	private float LST;

    private ParticleSystem lightningEffect;

    //menu
    [HideInInspector]public bool showSettings = false;

	// Events
	public delegate void HourPassed();
	public delegate void DayPassed();
	public delegate void YearPassed();
	public delegate void WeatherChanged(EnviroWeatherPreset weatherType);
	public delegate void ZoneWeatherChanged(EnviroWeatherPreset weatherType,EnviroZone zone);
	public delegate void SeasonChanged(EnviroSeasons.Seasons season);
	public delegate void isNightE();
	public delegate void isDay();
	public delegate void ZoneChanged(EnviroZone zone);
	public event HourPassed OnHourPassed;
	public event DayPassed OnDayPassed;
	public event YearPassed OnYearPassed;
	public event WeatherChanged OnWeatherChanged;
	public event ZoneWeatherChanged OnZoneWeatherChanged;
	public event SeasonChanged OnSeasonChanged;
	public event isNightE OnNightTime;
	public event isDay OnDayTime;
	public event ZoneChanged OnZoneChanged;
	///

	// Events:
	public virtual void NotifyHourPassed()
	{
		if(OnHourPassed != null)
			OnHourPassed();
	}
	public virtual void NotifyDayPassed()
	{
		if(OnDayPassed != null)
			OnDayPassed();
	}
	public virtual void NotifyYearPassed()
	{
		if(OnYearPassed != null)
			OnYearPassed();
	}
	public virtual void NotifyWeatherChanged(EnviroWeatherPreset type)
	{
		if(OnWeatherChanged != null)
			OnWeatherChanged (type);
	}
	public virtual void NotifyZoneWeatherChanged(EnviroWeatherPreset type, EnviroZone zone)
	{
		if(OnZoneWeatherChanged != null)
			OnZoneWeatherChanged (type,zone);
	}
	public virtual void NotifySeasonChanged(EnviroSeasons.Seasons season)
	{
		if(OnSeasonChanged != null)
			OnSeasonChanged (season);
	}
	public virtual void NotifyIsNight()
	{
		if(OnNightTime != null)
			OnNightTime ();
	} 
	public virtual void NotifyIsDay()
	{
		if(OnDayTime != null)
			OnDayTime ();
	}
	public virtual void NotifyZoneChanged(EnviroZone zone)
	{
		if(OnZoneChanged != null)
			OnZoneChanged (zone);
	}

	void Start()
	{
		//Time
		SetTime (GameTime.Years, GameTime.Days, GameTime.Hours, GameTime.Minutes, GameTime.Seconds);
		lastHourUpdate = Mathf.RoundToInt(internalHour);
		currentTimeInHours = GetInHours (internalHour, GameTime.Days, GameTime.Years);
		Weather.weatherFullyChanged = false;
		thunder = 0f;
        lastCloudsQuality = cloudsSettings.cloudsQuality;

        if (Application.isPlaying && dontDestroy)
            DontDestroyOnLoad(gameObject);

        //Create material
        if (weatherMapMat == null)
            weatherMapMat = new Material(Shader.Find("Enviro/WeatherMap"));

        // Check for Profile
        if (profileLoaded) {
			InvokeRepeating ("UpdateEnviroment", 0, qualitySettings.UpdateInterval);// Vegetation Updates
			CreateEffects ();  //Create Weather Effects Holder
			if (PlayerCamera != null && Player != null && AssignInRuntime == false && profile != null) {
				Init ();
			}
        }


    }

	void OnEnable()
	{
		// Add VolumeShader to always Included
		#if UNITY_EDITOR


		#endif

		DomeTransform = transform;

		//Set Weather
		Weather.currentActiveWeatherPreset = Weather.zones[0].currentActiveZoneWeatherPreset;
		Weather.lastActiveWeatherPreset = Weather.currentActiveWeatherPreset;

		if (profile == null) {
			Debug.LogError ("No profile assigned!");
			return;
		}

		// Auto Load profile
		if (profileLoaded == false)
			ApplyProfile (profile);

		PreInit ();

		if (AssignInRuntime) {
			started = false;	//Wait for assignment
		} else if (PlayerCamera != null && Player != null){
			Init ();
		}
	}

	/// <summary>
	/// Loads a profile into system.
	/// </summary>
	public void ApplyProfile(EnviroProfile p)
	{
		profile = p;
		lightSettings = JsonUtility.FromJson<EnviroLightSettings> (JsonUtility.ToJson(p.lightSettings));
        volumeLightSettings = JsonUtility.FromJson<EnviroVolumeLightingSettings>(JsonUtility.ToJson(p.volumeLightSettings));
        skySettings = JsonUtility.FromJson<EnviroSkySettings> (JsonUtility.ToJson(p.skySettings));
		cloudsSettings = JsonUtility.FromJson<EnviroCloudSettings> (JsonUtility.ToJson(p.cloudsSettings));
		weatherSettings = JsonUtility.FromJson<EnviroWeatherSettings> (JsonUtility.ToJson(p.weatherSettings));
		fogSettings = JsonUtility.FromJson<EnviroFogSettings> (JsonUtility.ToJson(p.fogSettings));
		lightshaftsSettings = JsonUtility.FromJson<EnviroLightShaftsSettings> (JsonUtility.ToJson(p.lightshaftsSettings));
		audioSettings = JsonUtility.FromJson<EnviroAudioSettings> (JsonUtility.ToJson(p.audioSettings));
		satelliteSettings = JsonUtility.FromJson<EnviroSatellitesSettings> (JsonUtility.ToJson(p.satelliteSettings));
		qualitySettings = JsonUtility.FromJson<EnviroQualitySettings> (JsonUtility.ToJson(p.qualitySettings));
		seasonsSettings = JsonUtility.FromJson<EnviroSeasonSettings> (JsonUtility.ToJson(p.seasonsSettings));
		profileLoaded = true;
	}

	/// <summary>
	/// Saves current settings in assigned profile.
	/// </summary>
	public void SaveProfile()
	{
		profile.lightSettings = JsonUtility.FromJson<EnviroLightSettings> (JsonUtility.ToJson(lightSettings));
        profile.volumeLightSettings = JsonUtility.FromJson<EnviroVolumeLightingSettings>(JsonUtility.ToJson(volumeLightSettings));
        profile.skySettings = JsonUtility.FromJson<EnviroSkySettings> (JsonUtility.ToJson(skySettings));
		profile.cloudsSettings = JsonUtility.FromJson<EnviroCloudSettings> (JsonUtility.ToJson(cloudsSettings));
		profile.weatherSettings = JsonUtility.FromJson<EnviroWeatherSettings> (JsonUtility.ToJson(weatherSettings));
		profile.fogSettings = JsonUtility.FromJson<EnviroFogSettings> (JsonUtility.ToJson(fogSettings));
		profile.lightshaftsSettings = JsonUtility.FromJson<EnviroLightShaftsSettings> (JsonUtility.ToJson(lightshaftsSettings));
		profile.audioSettings = JsonUtility.FromJson<EnviroAudioSettings> (JsonUtility.ToJson(audioSettings));
		profile.satelliteSettings = JsonUtility.FromJson<EnviroSatellitesSettings> (JsonUtility.ToJson(satelliteSettings));
		profile.qualitySettings = JsonUtility.FromJson<EnviroQualitySettings> (JsonUtility.ToJson(qualitySettings));
		profile.seasonsSettings = JsonUtility.FromJson<EnviroSeasonSettings> (JsonUtility.ToJson(seasonsSettings));
	
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(profile);
		UnityEditor.AssetDatabase.SaveAssets();
		#endif
	}

	/// <summary>
	/// Re-Initilize the system.
	/// </summary>
	public void ReInit ()
	{
		OnEnable ();
	}

	/// <summary>
	/// Pee-Initilize the system.
	/// </summary>
	private void PreInit ()
	{
		// Check time
		if (GameTime.solarTime < 0.45f)
			isNight = true;
		else
			isNight = false;

		//return when in server mode!
		if (serverMode)
			return;

		CheckSatellites ();

		// Setup Fog Mode
		RenderSettings.fogMode = fogSettings.Fogmode;

        // Setup Skybox Material
        SetupSkybox();

        // Set ambient mode
        RenderSettings.ambientMode = lightSettings.ambientMode;

        // Set Fog
        RenderSettings.fogDensity = 0f;
        RenderSettings.fogStartDistance = 0f;
        RenderSettings.fogEndDistance = 1000f;

        // Setup ReflectionProbe
        Components.GlobalReflectionProbe.size = transform.localScale;
		Components.GlobalReflectionProbe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;

		if (Components.Sun)
        {
            SunTransform = Components.Sun.transform;
        } 
		else
        {
            Debug.LogError("Please set sun object in inspector!");
        }

		if (Components.Moon){
			MoonTransform = Components.Moon.transform;
			MoonRenderer = Components.Moon.GetComponent<Renderer>();

			if (MoonRenderer == null)
				MoonRenderer = Components.Moon.AddComponent<MeshRenderer> ();

			MoonRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			MoonRenderer.receiveShadows = false;

			if (MoonRenderer.sharedMaterial != null)
				DestroyImmediate (MoonRenderer.sharedMaterial);

			if(skySettings.moonPhaseMode == EnviroSkySettings.MoonPhases.Realistic)
				MoonShader = new Material (Shader.Find ("Enviro/MoonShader"));
			else
				MoonShader = new Material (Shader.Find ("Enviro/MoonShaderPhased"));

			MoonShader.SetTexture ("_MainTex", skySettings.moonTexture);

			MoonRenderer.sharedMaterial = MoonShader;
			// Set start moon phase
			customMoonPhase = skySettings.startMoonPhase;
		}
		else { Debug.LogError("Please set moon object in inspector!"); }

        // Destroy old Shadowplane. Only needed to not break when updating enviro from 2.0.1. This will be removed in a future update.
        if(Components.cloudsShadowPlane != null)
        {
            DestroyImmediate(Components.cloudsShadowPlane);
        }

        if (weatherMap == null)
        {
            weatherMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.Default);
            weatherMap.wrapMode = TextureWrapMode.Repeat;
        }

        if (cloudShadows != null && weatherMap != null)
            cloudShadows.SetTexture("_MainTex", weatherMap);

        if (Components.DirectLight) 
		{
            if (Components.DirectLight.name == "Direct Lght")
            {
                DestroyImmediate(Components.DirectLight.gameObject);
                Components.DirectLight = CreateDirectionalLight();
            }

			MainLight = Components.DirectLight.GetComponent<Light>(); 

			if (directVolumeLight == null)
                directVolumeLight = Components.DirectLight.GetComponent<EnviroVolumeLight> ();

			if (directVolumeLight == null)
				directVolumeLight = Components.DirectLight.gameObject.AddComponent<EnviroVolumeLight> ();

            if (dontDestroy && Application.isPlaying)
                DontDestroyOnLoad(Components.DirectLight);
        } 
		else 
		{
            GameObject oldLight = GameObject.Find("Enviro Directional Light");

            if(oldLight != null)
               Components.DirectLight = oldLight.transform;
            else
               Components.DirectLight = CreateDirectionalLight();

            MainLight = Components.DirectLight.GetComponent<Light>();

            if (directVolumeLight == null)
                directVolumeLight = Components.DirectLight.GetComponent<EnviroVolumeLight>();

            if (directVolumeLight == null)
                directVolumeLight = Components.DirectLight.gameObject.AddComponent<EnviroVolumeLight>();

            if (dontDestroy && Application.isPlaying)
                DontDestroyOnLoad(Components.DirectLight);
        }

        if (cloudShadowMap != null)
            DestroyImmediate(cloudShadowMap);

        cloudShadowMap = new RenderTexture(2048, 2048, 0,RenderTextureFormat.Default);
        cloudShadowMap.wrapMode = TextureWrapMode.Repeat;

        if (cloudShadowMat != null)
            DestroyImmediate(cloudShadowMat);

        cloudShadowMat = new Material(Shader.Find("Enviro/ShadowCookie"));

         if (cloudsSettings.shadowIntensity > 0)
         {
             Graphics.Blit(weatherMap, cloudShadowMap, cloudShadowMat);

             MainLight.cookie = cloudShadowMap;
             MainLight.cookieSize = 10000;
         }
         else
             MainLight.cookie = null;
             
    }


    private void SetupSkybox()
    { 
        if (skySettings.skyboxMode == EnviroSkySettings.SkyboxModi.Default)
        {
            if (skyMat != null)
                DestroyImmediate(skyMat);

            if (cloudsMode == EnviroCloudsMode.None || cloudsMode == EnviroCloudsMode.Volume)
                skyMat = new Material(Shader.Find("Enviro/Skybox"));
            else
                skyMat = new Material(Shader.Find("Enviro/SkyboxFlatClouds"));

            if (skySettings.starsCubeMap != null)
                skyMat.SetTexture("_Stars", skySettings.starsCubeMap);
            if(skySettings.galaxyCubeMap != null)
                skyMat.SetTexture("_Galaxy", skySettings.galaxyCubeMap);

            RenderSettings.skybox = skyMat;
        }
        else if (skySettings.skyboxMode == EnviroSkySettings.SkyboxModi.CustomSkybox)
        {
            if (skySettings.customSkyboxMaterial != null)
                RenderSettings.skybox = skySettings.customSkyboxMaterial;

            skyMat = skySettings.customSkyboxMaterial;
        }

        lastCloudsMode = cloudsMode;

        //Update environment texture in next frame!
        if (lightSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Skybox)
            StartCoroutine(UpdateAmbientLightWithDelay());
    }

    /// <summary>
    /// Update the environment texture for skybox ambient mode with one frame delay. Somehow not working in same frame as we create the skybox material.
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateAmbientLightWithDelay()
    {
        yield return 0;
        DynamicGI.UpdateEnvironment();

    }

    /// <summary>
    /// Final Initilization and startup.
    /// </summary>
    private void Init ()
	{
		if (profile == null)
			return;

		if (serverMode) {
			started = true;
			return;
		}

		InitImageEffects ();

		// Setup Camera
		if (PlayerCamera != null) 
		{

			if (setCameraClearFlags)
				PlayerCamera.clearFlags = CameraClearFlags.Skybox;
	
			// Workaround for deferred forve HDR...
			if (PlayerCamera.actualRenderingPath == RenderingPath.DeferredShading)
				SetCameraHDR (PlayerCamera, true);
			else
				SetCameraHDR (PlayerCamera, HDR);

			Components.GlobalReflectionProbe.farClipPlane = PlayerCamera.farClipPlane;

            if (moonCamera == null)
                CreateMoonCamera();
            else
            {
                moonCamera.cullingMask = (1 << moonRenderingLayer);
                moonCamera.farClipPlane = PlayerCamera.farClipPlane * 0.5f;

                Camera[] cams = GameObject.FindObjectsOfType<Camera>();
                for (int i = 0; i < cams.Length; i++)
                {
                    if (cams[i] != moonCamera)
                        cams[i].cullingMask &= ~(1 << moonRenderingLayer);
                }

                //Set moon camera defaults
                moonCamera.transform.SetParent(Components.Moon.transform, false);
                moonCamera.transform.localPosition = new Vector3(0f, 0f, 1.5f);
                moonCamera.transform.localEulerAngles = new Vector3(-180f, 0f, 45f);
                moonCamera.transform.localScale = Vector3.one;
                moonCamera.enabled = false;
            }
        }

        CreateMoonTexture ();
        Components.Moon.layer = moonRenderingLayer;

        //Destroy old Cameras not needed for 2.0
        DestroyImmediate(GameObject.Find ("Enviro Cameras"));

		if(satelliteSettings.additionalSatellites.Count > 0)
            InitSatCamera();

        started = true;

        //Render moon once always on start
        if (MoonShader != null)
        {
            MoonShader.SetFloat("_Phase", customMoonPhase);
            MoonShader.SetColor("_Color", skySettings.moonColor);
            MoonShader.SetFloat("_Brightness", skySettings.moonBrightness * (1 - GameTime.solarTime));
        }

        if (moonCamera != null)
            moonCamera.Render();
    }
	/// <summary>
	/// Helper function to set camera hdr for different unity versions.
	/// </summary>
	public void SetCameraHDR (Camera cam, bool hdr)
	{
		#if UNITY_5_6_OR_NEWER
		cam.allowHDR = hdr;
		#else
		cam.hdr = hdr;
		#endif
	}
	/// <summary>
	/// Helper function to get camera hdr bool for different unity versions.
	/// </summary>
	public bool GetCameraHDR (Camera cam)
	{
		#if UNITY_5_6_OR_NEWER
		return cam.allowHDR;
		#else
		return cam.hdr;
		#endif
	}

    private Transform CreateDirectionalLight ()
    {
        GameObject newGO = new GameObject();
        newGO.name = "Enviro Directional Light";
        newGO.transform.parent = transform;
        newGO.transform.parent = null;
        Light newLight = newGO.AddComponent<Light>();
        newLight.type = LightType.Directional;
        newLight.shadows = LightShadows.Soft;
        return newGO.transform;
    }

	private void InitImageEffects ()
	{
        EnviroSkyRender = PlayerCamera.gameObject.GetComponent<EnviroSkyRendering>();

        if (EnviroSkyRender == null)
            EnviroSkyRender = PlayerCamera.gameObject.AddComponent<EnviroSkyRendering>();

        #if UNITY_EDITOR
        string[] assets = UnityEditor.AssetDatabase.FindAssets("enviro_spot_cookie", null);
        for (int idx = 0; idx < assets.Length; idx++)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(assets[idx]);
            if (path.Length > 0)
            {
                EnviroSkyRender.DefaultSpotCookie = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(path);
            }
        }
		#endif

		EnviroLightShafts[] shaftScripts = PlayerCamera.gameObject.GetComponents<EnviroLightShafts>();

		if(shaftScripts.Length > 0)
			lightShaftsScriptSun = shaftScripts [0];

		if (lightShaftsScriptSun != null) 
		{
			DestroyImmediate (lightShaftsScriptSun.sunShaftsMaterial);
			DestroyImmediate (lightShaftsScriptSun.simpleClearMaterial);
			lightShaftsScriptSun.sunShaftsMaterial = new Material (Shader.Find ("Enviro/Effects/LightShafts"));
			lightShaftsScriptSun.sunShaftsShader = lightShaftsScriptSun.sunShaftsMaterial.shader;
			lightShaftsScriptSun.simpleClearMaterial = new Material (Shader.Find ("Enviro/Effects/ClearLightShafts"));
			lightShaftsScriptSun.simpleClearShader = lightShaftsScriptSun.simpleClearMaterial.shader;
		}
		else
		{
			lightShaftsScriptSun = PlayerCamera.gameObject.AddComponent<EnviroLightShafts> ();
			lightShaftsScriptSun.sunShaftsMaterial = new Material (Shader.Find ("Enviro/Effects/LightShafts"));
			lightShaftsScriptSun.sunShaftsShader = lightShaftsScriptSun.sunShaftsMaterial.shader;
			lightShaftsScriptSun.simpleClearMaterial = new Material (Shader.Find ("Enviro/Effects/ClearLightShafts"));
			lightShaftsScriptSun.simpleClearShader = lightShaftsScriptSun.simpleClearMaterial.shader;
		}

		if(shaftScripts.Length > 1)
			lightShaftsScriptMoon = shaftScripts [1];

		if (lightShaftsScriptMoon != null) 
		{
			DestroyImmediate (lightShaftsScriptMoon.sunShaftsMaterial);
			DestroyImmediate (lightShaftsScriptMoon.simpleClearMaterial);
			lightShaftsScriptMoon.sunShaftsMaterial = new Material (Shader.Find ("Enviro/Effects/LightShafts"));
			lightShaftsScriptMoon.sunShaftsShader = lightShaftsScriptMoon.sunShaftsMaterial.shader;
			lightShaftsScriptMoon.simpleClearMaterial = new Material (Shader.Find ("Enviro/Effects/ClearLightShafts"));
			lightShaftsScriptMoon.simpleClearShader = lightShaftsScriptMoon.simpleClearMaterial.shader;
		}
		else
		{
			lightShaftsScriptMoon = PlayerCamera.gameObject.AddComponent<EnviroLightShafts> ();
			lightShaftsScriptMoon.sunShaftsMaterial = new Material (Shader.Find ("Enviro/Effects/LightShafts"));
			lightShaftsScriptMoon.sunShaftsShader = lightShaftsScriptMoon.sunShaftsMaterial.shader;
			lightShaftsScriptMoon.simpleClearMaterial = new Material (Shader.Find ("Enviro/Effects/ClearLightShafts"));
			lightShaftsScriptMoon.simpleClearShader = lightShaftsScriptMoon.simpleClearMaterial.shader;
		}
	}
	
	/// <summary>
	/// Re-create the camera and render texture for satellite rendering
	/// </summary>
	public void InitSatCamera ()
	{
		Camera[] cams = GameObject.FindObjectsOfType<Camera> ();
		for (int i = 0; i < cams.Length; i++) 
		{
			cams[i].cullingMask &= ~(1 << satelliteRenderingLayer);
		}

		DestroyImmediate(GameObject.Find ("Enviro Sat Camera"));

		GameObject camObj = new GameObject ();	

		camObj.name = "Enviro Sat Camera";
		camObj.transform.position = PlayerCamera.transform.position;
		camObj.transform.rotation = PlayerCamera.transform.rotation;
        camObj.hideFlags = HideFlags.DontSave;
        satCamera = camObj.AddComponent<Camera> ();
        satCamera.farClipPlane = PlayerCamera.farClipPlane;
        satCamera.nearClipPlane = PlayerCamera.nearClipPlane;
        satCamera.aspect = PlayerCamera.aspect;
		SetCameraHDR (satCamera, HDR);
        satCamera.useOcclusionCulling = false;
        satCamera.renderingPath = RenderingPath.Forward;
        satCamera.fieldOfView = PlayerCamera.fieldOfView;
        satCamera.clearFlags = CameraClearFlags.SolidColor;
        satCamera.backgroundColor = new Color(0f,0f,0f,0f);
        satCamera.cullingMask = (1 << satelliteRenderingLayer);
        satCamera.depth = PlayerCamera.depth + 1;
        satCamera.enabled = true;
		PlayerCamera.cullingMask &= ~(1 << satelliteRenderingLayer);

		var format = GetCameraHDR(satCamera) ? RenderTextureFormat.DefaultHDR: RenderTextureFormat.Default;

		satRenderTarget = new RenderTexture (Screen.currentResolution.width, Screen.currentResolution.height,16,format);
        satCamera.targetTexture = satRenderTarget;
        satCamera.enabled = false;
	}

	/// <summary>
	/// Re-create the camera and render texture for background rendering
	/// </summary>
	private void CreateMoonCamera ()
	{
		Camera[] cams = GameObject.FindObjectsOfType<Camera> ();
		for (int i = 0; i < cams.Length; i++) 
		{
			cams[i].cullingMask &= ~(1 << moonRenderingLayer);
		}

		DestroyImmediate(GameObject.Find ("Enviro Moon Render Cam"));

		GameObject camObj = new GameObject ();	

		camObj.name = "Enviro Moon Render Cam";
        camObj.transform.SetParent (Components.Moon.transform,false);
        camObj.transform.localPosition = new Vector3(0f, 0f, 1.5f);
        camObj.transform.localEulerAngles = new Vector3(-180f, 0f, 45f);
        camObj.transform.localScale = Vector3.one;
        moonCamera = camObj.AddComponent<Camera> ();
		moonCamera.farClipPlane = PlayerCamera.farClipPlane * 0.5f;
		moonCamera.nearClipPlane = 0.01f;
		moonCamera.aspect = PlayerCamera.aspect;
		SetCameraHDR (moonCamera, HDR);
		moonCamera.renderingPath = RenderingPath.Forward;
		moonCamera.fieldOfView = PlayerCamera.fieldOfView;
		moonCamera.clearFlags = CameraClearFlags.SolidColor;
		moonCamera.backgroundColor = Color.black;
		moonCamera.cullingMask = (1 << moonRenderingLayer);
		PlayerCamera.cullingMask &= ~(1 << moonRenderingLayer);
        moonCamera.enabled = false;
    }

    private void RenderMoon ()
    {
        if ((currentTimeInHours > lastMoonUpdate + 0.1) || (currentTimeInHours < lastMoonUpdate - 0.1) && skySettings.renderMoon)
        {
            moonCamera.Render();
            lastMoonUpdate = currentTimeInHours;
        }
    }

    /// <summary>
    /// Re-create the camera and render texture for background rendering
    /// </summary>
    private void CreateMoonTexture ()
	{
		if (moonRenderTarget != null && moonCamera != null) {
			moonCamera.targetTexture = null;
			DestroyImmediate (moonRenderTarget);
		}

		var format = GetCameraHDR(moonCamera) ? RenderTextureFormat.DefaultHDR: RenderTextureFormat.Default;
		moonRenderTarget = new RenderTexture (512, 512, 0 ,format);
		moonCamera.targetTexture = moonRenderTarget;
	}


	/// <summary>
	/// Create Effect Holder Gmaeobjec and adds audiofeatures
	/// </summary>
	public void CreateEffects ()
	{
		EffectsHolder = GameObject.Find ("Enviro Effects");

		if (EffectsHolder == null)
        {
            EffectsHolder = new GameObject();
            EffectsHolder.name = "Enviro Effects";
            // Fix from Thorskin - Make sure that Effect Object will be moved to same scene as envirosky object.
            EffectsHolder.transform.parent = transform;
            EffectsHolder.transform.parent = null;
        }
        else
        {
            int childs = EffectsHolder.transform.childCount;

            for (int i = childs - 1; i >= 0; i--)
            {
                DestroyImmediate(EffectsHolder.transform.GetChild(i).gameObject);
            }
        }

        CreateWeatherEffectHolder();

        if (Application.isPlaying && dontDestroy)
            DontDestroyOnLoad(EffectsHolder);

		if(Player != null)
            EffectsHolder.transform.position = Player.transform.position;
		else
            EffectsHolder.transform.position = transform.position;

		GameObject SFX = (GameObject)Instantiate (Audio.SFXHolderPrefab, Vector3.zero, Quaternion.identity);

		SFX.transform.parent = EffectsHolder.transform;

		EnviroAudioSource[] srcs = SFX.GetComponentsInChildren<EnviroAudioSource> ();

		for (int i = 0; i < srcs.Length; i++) 
		{
			switch (srcs [i].myFunction) {
			case EnviroAudioSource.AudioSourceFunction.Weather1:
				AudioSourceWeather = srcs [i];
				break;
			case EnviroAudioSource.AudioSourceFunction.Weather2:
				AudioSourceWeather2 = srcs [i];
				break;
			case EnviroAudioSource.AudioSourceFunction.Ambient:
				AudioSourceAmbient = srcs [i];
				break;
			case EnviroAudioSource.AudioSourceFunction.Ambient2:
				AudioSourceAmbient2 = srcs [i];
				break;
			case EnviroAudioSource.AudioSourceFunction.Thunder:
				AudioSourceThunder = srcs [i].audiosrc;
				break;
            case EnviroAudioSource.AudioSourceFunction.ZoneAmbient:
                AudioSourceZone = srcs[i];
            break;
            }
		}

		Weather.currentAudioSource = AudioSourceWeather; 
		Audio.currentAmbientSource = AudioSourceAmbient;
		TryPlayAmbientSFX ();

        // Instantiate Lightning Effect
        if (weatherSettings.lightningEffect != null)
            lightningEffect = Instantiate(weatherSettings.lightningEffect, EffectsHolder.transform).GetComponent<ParticleSystem>();
    }

	/// <summary>
	/// Called internaly from growth objects
	/// </summary>
	/// <param name="season">Season.</param>
	public int RegisterMe (EnviroVegetationInstance me) 
	{
		EnviroVegetationInstances.Add (me);
		return EnviroVegetationInstances.Count - 1;
	}

	/// <summary>
	/// Manual change of Season
	/// </summary>
	/// <param name="season">Season.</param>
	public void ChangeSeason (EnviroSeasons.Seasons season)
	{
		Seasons.currentSeasons = season;
		NotifySeasonChanged (season);
	}

	// Update the Season according gameDays
	private void UpdateSeason ()
	{

		if (currentDay >= 0 && currentDay < seasonsSettings.SpringInDays)
		{
			Seasons.currentSeasons = EnviroSeasons.Seasons.Spring;

			if (Seasons.lastSeason != Seasons.currentSeasons)
				NotifySeasonChanged (EnviroSeasons.Seasons.Spring);

			Seasons.lastSeason = Seasons.currentSeasons;
		} 
		else if (currentDay >= seasonsSettings.SpringInDays && currentDay < (seasonsSettings.SpringInDays + seasonsSettings.SummerInDays))
		{
			Seasons.currentSeasons = EnviroSeasons.Seasons.Summer;

			if (Seasons.lastSeason != Seasons.currentSeasons)
				NotifySeasonChanged (EnviroSeasons.Seasons.Summer);

			Seasons.lastSeason = Seasons.currentSeasons;
		} 
		else if (currentDay >= (seasonsSettings.SpringInDays + seasonsSettings.SummerInDays) && currentDay < (seasonsSettings.SpringInDays + seasonsSettings.SummerInDays + seasonsSettings.AutumnInDays)) 
		{
			Seasons.currentSeasons = EnviroSeasons.Seasons.Autumn;

			if (Seasons.lastSeason != Seasons.currentSeasons)
				NotifySeasonChanged (EnviroSeasons.Seasons.Autumn);

			Seasons.lastSeason = Seasons.currentSeasons;
		}
		else if(currentDay >= (seasonsSettings.SpringInDays + seasonsSettings.SummerInDays + seasonsSettings.AutumnInDays) && currentDay <= (seasonsSettings.SpringInDays + seasonsSettings.SummerInDays + seasonsSettings.AutumnInDays + seasonsSettings.WinterInDays))
		{
			Seasons.currentSeasons = EnviroSeasons.Seasons.Winter;

			if (Seasons.lastSeason != Seasons.currentSeasons)
				NotifySeasonChanged (EnviroSeasons.Seasons.Winter);

			Seasons.lastSeason = Seasons.currentSeasons;
		}
	}

	private void PlayAmbient (AudioClip sfx)
	{
		if (sfx == Audio.currentAmbientSource.audiosrc.clip) {
            if (!Audio.currentAmbientSource.audiosrc.isPlaying)
                 Audio.currentAmbientSource.audiosrc.Play();
			return;
		}
		if (Audio.currentAmbientSource == AudioSourceAmbient){
			AudioSourceAmbient.FadeOut();
			AudioSourceAmbient2.FadeIn(sfx);
			Audio.currentAmbientSource = AudioSourceAmbient2;
		}
		else if (Audio.currentAmbientSource == AudioSourceAmbient2){
			AudioSourceAmbient2.FadeOut();
			AudioSourceAmbient.FadeIn(sfx);
			Audio.currentAmbientSource = AudioSourceAmbient;
		}
	}


	private void TryPlayAmbientSFX ()
	{
		if (Weather.currentActiveWeatherPreset == null)
			return;

		if (isNight) 
		{
			switch (Seasons.currentSeasons)
			{
			case EnviroSeasons.Seasons.Spring:
				if (Weather.currentActiveWeatherPreset.SpringNightAmbient != null)
					PlayAmbient (Weather.currentActiveWeatherPreset.SpringNightAmbient);
				else {
					AudioSourceAmbient.FadeOut ();
					AudioSourceAmbient2.FadeOut ();
				}
				break;

			case EnviroSeasons.Seasons.Summer:
				if (Weather.currentActiveWeatherPreset.SummerNightAmbient != null)
					PlayAmbient (Weather.currentActiveWeatherPreset.SummerNightAmbient);
				else {
					AudioSourceAmbient.FadeOut ();
					AudioSourceAmbient2.FadeOut ();
				}
				break;
			case EnviroSeasons.Seasons.Autumn:
				if (Weather.currentActiveWeatherPreset.AutumnNightAmbient != null)
					PlayAmbient (Weather.currentActiveWeatherPreset.AutumnNightAmbient);
				else {
					AudioSourceAmbient.FadeOut ();
					AudioSourceAmbient2.FadeOut ();
				}
				break;
			case EnviroSeasons.Seasons.Winter:
				if (Weather.currentActiveWeatherPreset.WinterNightAmbient != null)
					PlayAmbient (Weather.currentActiveWeatherPreset.WinterNightAmbient);
				else {
					AudioSourceAmbient.FadeOut ();
					AudioSourceAmbient2.FadeOut ();
				}
				break;
			}
		} 
		else 
		{
			switch (Seasons.currentSeasons)
			{
			case EnviroSeasons.Seasons.Spring:
				if (Weather.currentActiveWeatherPreset.SpringDayAmbient != null)
					PlayAmbient (Weather.currentActiveWeatherPreset.SpringDayAmbient);
				else {
					AudioSourceAmbient.FadeOut ();
					AudioSourceAmbient2.FadeOut ();
				}
				break;
			case EnviroSeasons.Seasons.Summer:
				if (Weather.currentActiveWeatherPreset.SummerDayAmbient != null)
					PlayAmbient (Weather.currentActiveWeatherPreset.SummerDayAmbient);
				else {
					AudioSourceAmbient.FadeOut ();
					AudioSourceAmbient2.FadeOut ();
				}
				break;
			case EnviroSeasons.Seasons.Autumn:
				if (Weather.currentActiveWeatherPreset.AutumnDayAmbient != null)
					PlayAmbient (Weather.currentActiveWeatherPreset.AutumnDayAmbient);
				else {
					AudioSourceAmbient.FadeOut ();
					AudioSourceAmbient2.FadeOut ();
				}
				break;
			case EnviroSeasons.Seasons.Winter:
				if (Weather.currentActiveWeatherPreset.WinterDayAmbient != null)
					PlayAmbient (Weather.currentActiveWeatherPreset.WinterDayAmbient);
				else {
					AudioSourceAmbient.FadeOut ();
					AudioSourceAmbient2.FadeOut ();
				}
				break;
			}
		}
	}

    private void UpdateEnviroment() // Update the all GrowthInstances
    {
        // Set correct Season.
        if (Seasons.calcSeasons)
            UpdateSeason();

        // Update all EnviroGrowInstancesSeason in scene!
        if (EnviroVegetationInstances.Count > 0)
        {
            for (int i = 0; i < EnviroVegetationInstances.Count; i++)
            {
                if (EnviroVegetationInstances[i] != null)
                    EnviroVegetationInstances[i].UpdateInstance();

            }
        }
    }

    /// <summary>
    /// Instantiates a new satellite
    /// </summary>
    /// <param name="id">Identifier.</param>
    private void CreateSatellite (int id)
	{
		if (satelliteSettings.additionalSatellites [id].prefab == null) {
			Debug.Log ("Satellite without prefab! Pleae assign a prefab to all satellites.");
			return;
		}
		GameObject satRot = new GameObject ();
		satRot.name = satelliteSettings.additionalSatellites [id].name;
		satRot.transform.parent = Components.satellites;
		satellitesRotation.Add (satRot);
		GameObject sat = (GameObject)Instantiate (satelliteSettings.additionalSatellites [id].prefab,satRot.transform);
		sat.layer = satelliteRenderingLayer;
		satellites.Add (sat);
	}

	/// <summary>
	/// Destroy and recreate all satellites
	/// </summary>
	public void CheckSatellites ()
	{
		satellites = new List<GameObject> ();

		int childs = Components.satellites.childCount;
		for (int i = childs-1; i >= 0; i--) 
		{
			DestroyImmediate (Components.satellites.GetChild (i).gameObject);
		}

		satellites.Clear ();
		satellitesRotation.Clear ();

		for (int i = 0; i < satelliteSettings.additionalSatellites.Count; i++) 
		{
			CreateSatellite (i);
		}
	}


	private void CalculateSatPositions (float siderealTime)
	{
		for (int i = 0; i < satelliteSettings.additionalSatellites.Count; i++)
		{
			Quaternion satRotation = Quaternion.Euler (90 - GameTime.Latitude, GameTime.Longitude, 0);
			satRotation *= Quaternion.Euler(satelliteSettings.additionalSatellites[i].yRot, siderealTime, satelliteSettings.additionalSatellites[i].xRot);

			if(satellites.Count >= i)
				satellites [i].transform.localPosition = new Vector3 (0f, satelliteSettings.additionalSatellites[i].orbit, 0f);
			if(satellitesRotation.Count >= i)
				satellitesRotation[i].transform.localRotation = satRotation;
		}
	}


	private void UpdateCameraComponents()
	{
		//Update Fog
		if (EnviroSkyRender != null) 
		{

        #if UNITY_2017_3_OR_NEWER
            EnviroSkyRender.volumeLighting = volumeLighting;
        #else

            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D9)
                EnviroSkyRender.volumeLighting = false;
            else
                EnviroSkyRender.volumeLighting = volumeLighting;
        #endif

            EnviroSkyRender.dirVolumeLighting = volumeLightSettings.dirVolumeLighting;
            EnviroSkyRender.simpleFog = fogSettings.useSimpleFog;
            EnviroSkyRender.distanceFog = fogSettings.distanceFog;
            EnviroSkyRender.heightFog = fogSettings.heightFog;
            EnviroSkyRender.height = fogSettings.height;
            EnviroSkyRender.heightDensity = fogSettings.heightDensity;
            EnviroSkyRender.useRadialDistance = fogSettings.useRadialDistance;
            EnviroSkyRender.startDistance = fogSettings.startDistance;
		}

		//Update LightShafts
		if (lightShaftsScriptSun != null) 
		{
			lightShaftsScriptSun.resolution = lightshaftsSettings.resolution;
			lightShaftsScriptSun.screenBlendMode = lightshaftsSettings.screenBlendMode;
			lightShaftsScriptSun.useDepthTexture = lightshaftsSettings.useDepthTexture;
			lightShaftsScriptSun.sunThreshold = lightshaftsSettings.thresholdColorSun.Evaluate (GameTime.solarTime);

			lightShaftsScriptSun.sunShaftBlurRadius = lightshaftsSettings.blurRadius;
			lightShaftsScriptSun.sunShaftIntensity = lightshaftsSettings.intensity;
			lightShaftsScriptSun.maxRadius = lightshaftsSettings.maxRadius;
			lightShaftsScriptSun.sunColor = lightshaftsSettings.lightShaftsColorSun.Evaluate (GameTime.solarTime);
			lightShaftsScriptSun.sunTransform = Components.Sun.transform;

			if (LightShafts.sunLightShafts) {
				lightShaftsScriptSun.enabled = true;
			} else {
				lightShaftsScriptSun.enabled = false;
			}
		}

		if (lightShaftsScriptMoon != null) 
		{
			lightShaftsScriptMoon.resolution = lightshaftsSettings.resolution;
			lightShaftsScriptMoon.screenBlendMode = lightshaftsSettings.screenBlendMode;
			lightShaftsScriptMoon.useDepthTexture = lightshaftsSettings.useDepthTexture;
			lightShaftsScriptMoon.sunThreshold = lightshaftsSettings.thresholdColorMoon.Evaluate (GameTime.lunarTime);


			lightShaftsScriptMoon.sunShaftBlurRadius = lightshaftsSettings.blurRadius;
			lightShaftsScriptMoon.sunShaftIntensity = Mathf.Clamp ((lightshaftsSettings.intensity - GameTime.solarTime),0,100);
			lightShaftsScriptMoon.maxRadius = lightshaftsSettings.maxRadius;
			lightShaftsScriptMoon.sunColor = lightshaftsSettings.lightShaftsColorMoon.Evaluate (GameTime.lunarTime);
			lightShaftsScriptMoon.sunTransform = Components.Moon.transform;

			if (LightShafts.moonLightShafts) {
				lightShaftsScriptMoon.enabled = true;
			} else {
				lightShaftsScriptMoon.enabled = false;
			}
		}
	}

	private Vector3 CalculatePosition ()
	{
		Vector3 newPosition;
		newPosition.x = Player.transform.position.x;
		newPosition.z = Player.transform.position.z;
		newPosition.y = Player.transform.position.y;

		return newPosition;
	}

    private void RenderFlatCloudsMap()
    {
        if (flatCloudsMat == null)
        flatCloudsMat = new Material(Shader.Find("Enviro/FlatCloudMap"));

        flatCloudsRenderTarget = RenderTexture.GetTemporary(512 * ((int)cloudsSettings.flatCloudsResolution + 1), 512 * ((int)cloudsSettings.flatCloudsResolution + 1), 0, RenderTextureFormat.DefaultHDR);
        flatCloudsRenderTarget.wrapMode = TextureWrapMode.Repeat;
        flatCloudsMat.SetVector("_CloudAnimation", cloudAnimNonScaled);
        flatCloudsMat.SetTexture("_NoiseTex", cloudsSettings.flatCloudsNoiseTexture);
        flatCloudsMat.SetFloat("_CloudScale", cloudsSettings.flatCloudsScale);
        flatCloudsMat.SetFloat("_Coverage", cloudsConfig.flatCoverage);
        flatCloudsMat.SetInt("noiseOctaves", cloudsSettings.flatCloudsNoiseOctaves);
        flatCloudsMat.SetFloat("_Softness", cloudsConfig.flatSoftness);
        flatCloudsMat.SetFloat("_Brightness", cloudsConfig.flatBrightness);
        flatCloudsMat.SetFloat("_MorphingSpeed", cloudsSettings.flatCloudsMorphingSpeed);
        Graphics.Blit(null, flatCloudsRenderTarget, flatCloudsMat);
        RenderTexture.ReleaseTemporary(flatCloudsRenderTarget);
    }


    private Material weatherMapMat;

    private void RenderWeatherMap()
    {
        if (cloudsSettings.customWeatherMap == null)
        {
            weatherMapMat.SetVector("_WindDir", cloudAnimNonScaled);
            weatherMapMat.SetFloat("_AnimSpeedScale", cloudsSettings.weatherAnimSpeedScale);
            weatherMapMat.SetInt("_Tiling", cloudsSettings.weatherMapTiling);
            weatherMapMat.SetVector("_Location", cloudsSettings.locationOffset);
            double cov = EnviroSky.instance.cloudsConfig.coverage * cloudsSettings.globalCloudCoverage;
            weatherMapMat.SetFloat("_Coverage", (float)System.Math.Round(cov, 4));
            Graphics.Blit(null, weatherMap, weatherMapMat);
        }
    }

    private void RenderCloudMaps ()
    {
        switch (cloudsMode)
        {
            case EnviroCloudsMode.None:
                break;
            case EnviroCloudsMode.Flat:
                RenderFlatCloudsMap();
                RenderWeatherMap();
                break;
            case EnviroCloudsMode.Volume:
                RenderWeatherMap();
                break;
            case EnviroCloudsMode.Both:
                RenderFlatCloudsMap();
                RenderWeatherMap();
                break;
        }
    }

    void Update()
	{
		if (profile == null) {
			Debug.Log ("No profile applied! Please create and assign a profile.");
			return;
		}

        if (!started && !serverMode) 
		{
            UpdateTime();
            UpdateSunAndMoonPosition();
            CalculateDirectLight();
            UpdateAmbientLight();
            UpdateReflections();
            RenderMoon();
  
            if (AssignInRuntime && PlayerTag != "" && CameraTag != "" && Application.isPlaying) {
               
                // Search for Player by tag
                GameObject plr = GameObject.FindGameObjectWithTag(PlayerTag);
                if (plr != null)
                    Player = plr;
               
                // Search for camera by tag
                for(int i = 0; i < Camera.allCameras.Length; i++)
                {
                    if (Camera.allCameras[i].tag == CameraTag)
                        PlayerCamera = Camera.allCameras[i];
                }

                if (Player != null && PlayerCamera != null) {
					Init ();
					started = true;
				}
				else  {started = false; return;}
			} else {started = false; return;}
		}

		UpdateTime ();
		ValidateParameters();

		if (!serverMode) {

            //Check if cloudmode changed
            if (lastCloudsMode != cloudsMode)
                SetupSkybox();

            RenderCloudMaps();
            UpdateCameraComponents ();
			UpdateAmbientLight ();
			UpdateReflections ();
			UpdateWeather ();
            UpdateCloudShadows();
            UpdateSkyRenderingComponent ();
            RenderMoon();
            UpdateSunAndMoonPosition();
            CalculateDirectLight();
            CalculateSatPositions(LST);

			if (!isNight && GameTime.solarTime < 0.45f) {
				isNight = true;
				if (AudioSourceAmbient != null)
					TryPlayAmbientSFX ();
				NotifyIsNight ();
			} else if (isNight && GameTime.solarTime >= 0.45f) {
				isNight = false;
				if (AudioSourceAmbient != null)
					TryPlayAmbientSFX ();
				NotifyIsDay ();
			}

            //Change Clouds Quality Settings
            if (lastCloudsQuality != cloudsSettings.cloudsQuality && (cloudsMode == EnviroCloudsMode.Volume || cloudsMode == EnviroCloudsMode.Both))
                ChangeCloudsQuality(cloudsSettings.cloudsQuality);
        } 
		else 
		{
            UpdateWeather();

            if (!isNight && GameTime.solarTime < 0.45f)
            {
                isNight = true;
                NotifyIsNight();
            }
            else if (isNight && GameTime.solarTime >= 0.45f)
            {
                isNight = false;
                NotifyIsDay();
            }
		}
	}

	void LateUpdate()
	{
		if (!serverMode && PlayerCamera != null && Player != null) {
			transform.position = Player.transform.position;
			transform.localScale = new Vector3 (PlayerCamera.farClipPlane, PlayerCamera.farClipPlane, PlayerCamera.farClipPlane);

            if (EffectsHolder != null)
                EffectsHolder.transform.position = Player.transform.position;
        }
	}

    private void UpdateCloudShadows ()
    {
        if (cloudsSettings.shadowIntensity == 0 || cloudsMode == EnviroCloudsMode.None || cloudsMode == EnviroCloudsMode.Flat)
        {
            if (MainLight.cookie != null)
                MainLight.cookie = null;
        }
        else if (cloudsSettings.shadowIntensity > 0)
        {
            cloudShadowMap.DiscardContents(true,true);

            cloudShadowMat.SetFloat("_shadowIntensity", cloudsSettings.shadowIntensity);

            if (cloudsMode == EnviroCloudsMode.Volume || cloudsMode == EnviroCloudsMode.Both)
            {
                cloudShadowMat.SetTexture("_MainTex", weatherMap);
            //    cloudShadowMat.SetInt("_shadowMode", 1);
                Graphics.Blit(weatherMap, cloudShadowMap, cloudShadowMat);
            }
            /* else
             {
                 cloudShadowMat.SetTexture("_MainTex", flatCloudsRenderTarget);
                 cloudShadowMat.SetInt("_shadowMode", 0);
                 Graphics.Blit(flatCloudsRenderTarget, cloudShadowMap, cloudShadowMat);
             }*/

            if (Application.isPlaying)
                MainLight.cookie = cloudShadowMap;
            else
                MainLight.cookie = null;

            MainLight.cookieSize = cloudsSettings.shadowCookieSize;
        }
    }


	public Vector3 BetaRay() {
		Vector3 Br;

		Vector3 realWavelength = skySettings.waveLength * 1.0e-9f;

		Br.x = (((8.0f * Mathf.Pow(pi, 3.0f) * (Mathf.Pow(Mathf.Pow(n, 2.0f) - 1.0f, 2.0f)))*(6.0f+3.0f*pn) ) / ((3.0f * N * Mathf.Pow(realWavelength.x, 4.0f))*(6.0f-7.0f*pn) ))* 2000f;
		Br.y = (((8.0f * Mathf.Pow(pi, 3.0f) * (Mathf.Pow(Mathf.Pow(n, 2.0f) - 1.0f, 2.0f)))*(6.0f+3.0f*pn) ) / ((3.0f * N * Mathf.Pow(realWavelength.y, 4.0f))*(6.0f-7.0f*pn) ))* 2000f;
		Br.z = (((8.0f * Mathf.Pow(pi, 3.0f) * (Mathf.Pow(Mathf.Pow(n, 2.0f) - 1.0f, 2.0f)))*(6.0f+3.0f*pn) ) / ((3.0f * N * Mathf.Pow(realWavelength.z, 4.0f))*(6.0f-7.0f*pn) ))* 2000f;

		return Br;
	}


	public Vector3 BetaMie() {
		Vector3 Bm;

		float c = (0.2f * skySettings.turbidity ) * 10.0f;

		Bm.x = (434.0f * c * pi * Mathf.Pow((2.0f * pi) / skySettings.waveLength.x, 2.0f) * K.x);
		Bm.y = (434.0f * c * pi * Mathf.Pow((2.0f * pi) / skySettings.waveLength.y, 2.0f) * K.y);
		Bm.z = (434.0f * c * pi * Mathf.Pow((2.0f * pi) / skySettings.waveLength.z, 2.0f) * K.z);

		Bm.x=Mathf.Pow(Bm.x,-1.0f);
		Bm.y=Mathf.Pow(Bm.y,-1.0f);
		Bm.z=Mathf.Pow(Bm.z,-1.0f);

		return Bm;
	}

	public Vector3 GetMieG() {
		return new Vector3(1.0f - skySettings.g * skySettings.g, 1.0f + skySettings.g * skySettings.g, 2.0f * skySettings.g);
	}

	public Vector3 GetMieGScene() {
		return new Vector3(1.0f - fogSettings.g * fogSettings.g, 1.0f + fogSettings.g * fogSettings.g, 2.0f * fogSettings.g);
	}

	// Setup the Shaders with correct information
	private void SetupShader(float setup)
	{
        if (skyMat != null)
        {
            skyMat.SetVector("_SunDir", -SunTransform.transform.forward);
            skyMat.SetVector("_MoonDir", Components.Moon.transform.forward);
            skyMat.SetColor("_MoonColor", skySettings.moonColor);
            skyMat.SetFloat("_MoonSize", skySettings.moonSize);
            skyMat.SetFloat("_MoonBrightness", skySettings.moonBrightness);
            if (skySettings.renderMoon)
                skyMat.SetTexture("_MoonTex", moonRenderTarget);
            else
                skyMat.SetTexture("_MoonTex", null);
            skyMat.SetColor("_scatteringColor", skySettings.scatteringColor.Evaluate(GameTime.solarTime));
            skyMat.SetColor("_sunDiskColor", skySettings.sunDiskColor.Evaluate(GameTime.solarTime));
            skyMat.SetColor("_weatherSkyMod", Color.Lerp(currentWeatherSkyMod, currentInteriorSkyboxMod, currentInteriorSkyboxMod.a));
            skyMat.SetColor("_weatherFogMod", Color.Lerp(currentWeatherFogMod, currentInteriorFogColorMod, currentInteriorFogColorMod.a));
            skyMat.SetVector("_Bm", BetaMie() * (skySettings.mie * Fog.scatteringStrenght));
            skyMat.SetVector("_Br", BetaRay() * skySettings.rayleigh);
            skyMat.SetVector("_mieG", GetMieG());
            skyMat.SetFloat("_SunIntensity", skySettings.sunIntensity);
            skyMat.SetFloat("_SunDiskSize", skySettings.sunDiskScale);
            skyMat.SetFloat("_SunDiskIntensity", skySettings.sunDiskIntensity);
            skyMat.SetFloat("_SunDiskSize", skySettings.sunDiskScale);
            skyMat.SetFloat("_Exposure", skySettings.skyExposure);
            skyMat.SetFloat("_SkyLuminance", skySettings.skyLuminence.Evaluate(GameTime.solarTime));
            skyMat.SetFloat("_scatteringPower", skySettings.scatteringCurve.Evaluate(GameTime.solarTime));
            skyMat.SetFloat("_SkyColorPower", skySettings.skyColorPower.Evaluate(GameTime.solarTime));
            skyMat.SetFloat("_StarsIntensity", skySettings.starsIntensity.Evaluate(GameTime.solarTime));
            skyMat.SetFloat("_GalaxyIntensity", skySettings.galaxyIntensity.Evaluate(GameTime.solarTime));
            skyMat.SetColor("_moonGlowColor", skySettings.moonGlowColor);

            if (skySettings.blackGroundMode)
                skyMat.SetInt("_blackGround", 1);
            else
                skyMat.SetInt("_blackGround", 0);

            float hdr = HDR ? 1f : 0f;
            skyMat.SetFloat("_hdr", hdr);
            skyMat.SetFloat("_moonGlowStrenght", skySettings.moonGlow.Evaluate(GameTime.solarTime));

            //Clouds
            skyMat.SetVector("_CloudAnimation", cloudAnim);

            //cirrus
            if (cloudsSettings.cirrusCloudsTexture != null)
                skyMat.SetTexture("_CloudMap", cloudsSettings.cirrusCloudsTexture);

            skyMat.SetColor("_CloudColor", cloudsSettings.cirrusCloudsColor.Evaluate(GameTime.solarTime));
            skyMat.SetFloat("_CloudAltitude", cloudsSettings.cirrusCloudsAltitude);
            skyMat.SetFloat("_CloudAlpha", cloudsConfig.cirrusAlpha);
            skyMat.SetFloat("_CloudCoverage", cloudsConfig.cirrusCoverage);
            skyMat.SetFloat("_CloudColorPower", cloudsConfig.cirrusColorPow);

            //flat procedural
            if (flatCloudsRenderTarget != null)
            {
                skyMat.SetTexture("_Cloud1Map", flatCloudsRenderTarget);
                skyMat.SetColor("_Cloud1Color", cloudsSettings.flatCloudsColor.Evaluate(GameTime.solarTime));
                skyMat.SetFloat("_Cloud1Altitude", cloudsSettings.flatCloudsAltitude);
                skyMat.SetFloat("_Cloud1Alpha", cloudsConfig.flatAlpha);
                skyMat.SetFloat("_Cloud1ColorPower", cloudsConfig.flatColorPow);
            }
        }
       //RenderSettings.skybox.SetFloat("_noiseScale", skySettings.noiseScale);
       //RenderSettings.skybox.SetFloat("_noiseIntensity", skySettings.noiseIntensity);

        Shader.SetGlobalVector ("_SunDir", -Components.Sun.transform.forward);
		Shader.SetGlobalVector ("_MoonDir", -Components.Moon.transform.forward);
		Shader.SetGlobalColor("_scatteringColor", skySettings.scatteringColor.Evaluate(GameTime.solarTime));
		Shader.SetGlobalColor("_sunDiskColor", skySettings.sunDiskColor.Evaluate(GameTime.solarTime));
		Shader.SetGlobalColor("_weatherSkyMod", Color.Lerp(currentWeatherSkyMod, currentInteriorSkyboxMod, currentInteriorSkyboxMod.a));
        Shader.SetGlobalColor("_weatherFogMod", Color.Lerp(currentWeatherFogMod, currentInteriorFogColorMod, currentInteriorFogColorMod.a));

		Shader.SetGlobalFloat ("_gameTime", Mathf.Clamp(1f-GameTime.solarTime,0.5f,1f));
		Shader.SetGlobalFloat ("_SkyFogHeight", Fog.skyFogHeight);
		Shader.SetGlobalFloat ("_scatteringStrenght", Fog.scatteringStrenght);
		Shader.SetGlobalFloat ("_skyFogIntensity", fogSettings.skyFogIntensity);
		Shader.SetGlobalFloat ("_SunBlocking", Fog.sunBlocking);

		Shader.SetGlobalVector ("_EnviroParams", new Vector4(Mathf.Clamp(1f-GameTime.solarTime,0.5f,1f),fogSettings.distanceFog ? 1f:0f,fogSettings.heightFog ? 1f:0f,HDR ? 1f:0f));

		Shader.SetGlobalVector ("_Bm", BetaMie () * (skySettings.mie * (Fog.scatteringStrenght * GameTime.solarTime)));
		Shader.SetGlobalVector ("_BmScene", BetaMie () * (fogSettings.mie * (Fog.scatteringStrenght * GameTime.solarTime)));
		Shader.SetGlobalVector ("_Br", BetaRay() * skySettings.rayleigh);
		Shader.SetGlobalVector ("_mieG", GetMieG ());
		Shader.SetGlobalVector ("_mieGScene", GetMieGScene ());
		Shader.SetGlobalFloat ("_SunIntensity",  skySettings.sunIntensity);

		Shader.SetGlobalFloat ("_SunDiskSize",  skySettings.sunDiskScale);
		Shader.SetGlobalFloat ("_SunDiskIntensity",  skySettings.sunDiskIntensity);
		Shader.SetGlobalFloat ("_SunDiskSize", skySettings.sunDiskScale);

		Shader.SetGlobalFloat ("_Exposure", skySettings.skyExposure);
		Shader.SetGlobalFloat ("_SkyLuminance", skySettings.skyLuminence.Evaluate(GameTime.solarTime));
		Shader.SetGlobalFloat ("_scatteringPower", skySettings.scatteringCurve.Evaluate(GameTime.solarTime));
		Shader.SetGlobalFloat ("_SkyColorPower", skySettings.skyColorPower.Evaluate(GameTime.solarTime));

		Shader.SetGlobalFloat ("_heightFogIntensity", fogSettings.heightFogIntensity);
		Shader.SetGlobalFloat ("_distanceFogIntensity", fogSettings.distanceFogIntensity);
        if (Application.isPlaying)
            Shader.SetGlobalFloat ("_maximumFogDensity", 1 - fogSettings.maximumFogDensity);
        else
            Shader.SetGlobalFloat("_maximumFogDensity", 1f);
        Shader.SetGlobalFloat ("_lightning", thunder);

		float windStrenght = 0;

		if (Weather.currentActiveWeatherPreset != null)
			windStrenght = Weather.currentActiveWeatherPreset.WindStrenght;

		if (cloudsSettings.useWindZoneDirection) {
			cloudsSettings.cloudsWindDirectionX = -Components.windZone.transform.forward.x;
			cloudsSettings.cloudsWindDirectionY = -Components.windZone.transform.forward.z;
		}

		cloudAnim += new Vector2(((cloudsSettings.cloudsTimeScale * (windStrenght * cloudsSettings.cloudsWindDirectionX)) * cloudsSettings.cloudsWindStrengthModificator) * Time.deltaTime,((cloudsSettings.cloudsTimeScale * (windStrenght * cloudsSettings.cloudsWindDirectionY)) * cloudsSettings.cloudsWindStrengthModificator) * Time.deltaTime);
        cloudAnimNonScaled += new Vector2(((cloudsSettings.cloudsTimeScale * (windStrenght * cloudsSettings.cloudsWindDirectionX)) * cloudsSettings.cloudsWindStrengthModificator) * Time.deltaTime * 0.1f, ((cloudsSettings.cloudsTimeScale * (windStrenght * cloudsSettings.cloudsWindDirectionY)) * cloudsSettings.cloudsWindStrengthModificator) * Time.deltaTime * 0.1f);

        if (cloudAnim.x > 1f)
			cloudAnim.x = -1f;
		else if (cloudAnim.x < -1f)
			cloudAnim.x = 1f;

		if (cloudAnim.y > 1f)
			cloudAnim.y = -1f;
		else if (cloudAnim.y < -1f)
			cloudAnim.y = 1f;

		if (MoonShader != null)
		{
			MoonShader.SetFloat("_Phase", customMoonPhase);
			MoonShader.SetColor ("_Color", skySettings.moonColor);
			MoonShader.SetFloat("_Brightness", skySettings.moonBrightness * (1-GameTime.solarTime));
		}
	}

	void UpdateSkyRenderingComponent ()
	{
		if (EnviroSkyRender == null)
			return;

        EnviroSkyRender.Resolution = volumeLightSettings.Resolution;

        if (EnviroSkyRender._volumeRenderingMaterial != null) {
            EnviroSkyRender._volumeRenderingMaterial.SetTexture ("_Clouds", cloudsRenderTarget);
			float hdr = HDR ? 1f : 0f;
            EnviroSkyRender._volumeRenderingMaterial.SetFloat ("_hdr", hdr);
		}
	}

	DateTime CreateSystemDate ()
	{
		DateTime date = new DateTime ();

		date = date.AddYears (GameTime.Years - 1);
		date = date.AddDays (GameTime.Days - 1);

		return date;
	}

	void UpdateSunAndMoonPosition ()
	{
		DateTime date = CreateSystemDate ();
		float d = 367 * date.Year - 7 * ( date.Year + (date.Month / 12 + 9) / 12 ) / 4 + 275 * date.Month/9 + date.Day - 730530;
		d += (GetUniversalTimeOfDay() / 24f);

		float ecl = 23.4393f - 3.563E-7f * d;

        if (skySettings.sunAndMoonPosition == EnviroSkySettings.SunAndMoonCalc.Realistic)
        {
            CalculateSunPosition(d, ecl, false);
            CalculateMoonPosition(d, ecl);
        }
        else
        {
            CalculateSunPosition(d, ecl, true);
        }

        CalculateStarsPosition(LST);

    }


	private float Remap (float value, float from1, float to1, float from2, float to2) {
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}

	void CalculateSunPosition (float d, float ecl, bool simpleMoon)
	{
		/////http://www.stjarnhimlen.se/comp/ppcomp.html#5////
		///////////////////////// SUN ////////////////////////
		float w = 282.9404f + 4.70935E-5f * d;
		float e = 0.016709f - 1.151E-9f * d;
		float M = 356.0470f + 0.9856002585f * d;

		float E = M + e * Mathf.Rad2Deg * Mathf.Sin(Mathf.Deg2Rad * M) * (1 + e * Mathf.Cos(Mathf.Deg2Rad * M));

		float xv = Mathf.Cos(Mathf.Deg2Rad * E) - e;
		float yv = Mathf.Sin(Mathf.Deg2Rad * E) * Mathf.Sqrt(1 - e*e);

		float v = Mathf.Rad2Deg * Mathf.Atan2(yv, xv);
		float r = Mathf.Sqrt(xv*xv + yv*yv);

		float l = v + w;

		float xs = r * Mathf.Cos(Mathf.Deg2Rad * l);
		float ys = r * Mathf.Sin(Mathf.Deg2Rad * l);

		float xe = xs;
		float ye = ys * Mathf.Cos(Mathf.Deg2Rad * ecl);
		float ze = ys * Mathf.Sin(Mathf.Deg2Rad * ecl);

		float decl_rad = Mathf.Atan2(ze, Mathf.Sqrt(xe*xe + ye*ye));
		float decl_sin = Mathf.Sin(decl_rad);
		float decl_cos = Mathf.Cos(decl_rad);

		float GMST0 = (l + 180);
		float GMST  = GMST0 + GetUniversalTimeOfDay() * 15;
		LST = GMST + GameTime.Longitude;
        
        float HA_deg = LST - Mathf.Rad2Deg * Mathf.Atan2(ye, xe);
		float HA_rad = Mathf.Deg2Rad * HA_deg;
		float HA_sin = Mathf.Sin(HA_rad);
		float HA_cos = Mathf.Cos(HA_rad);

        float x = HA_cos * decl_cos;
		float y = HA_sin * decl_cos;
		float z = decl_sin;

		float sin_Lat = Mathf.Sin(Mathf.Deg2Rad * GameTime.Latitude);
		float cos_Lat = Mathf.Cos(Mathf.Deg2Rad * GameTime.Latitude);

		float xhor = x * sin_Lat - z * cos_Lat;
		float yhor = y;
		float zhor = x * cos_Lat + z * sin_Lat;

		float azimuth  = Mathf.Atan2(yhor, xhor) + Mathf.Deg2Rad * 180;
		float altitude = Mathf.Atan2(zhor, Mathf.Sqrt(xhor*xhor + yhor*yhor));

		float sunTheta = (90 * Mathf.Deg2Rad) - altitude;
		float sunPhi   = azimuth;

		//Set SolarTime: 1 = mid-day (sun directly above you), 0.5 = sunset/dawn, 0 = midnight;
		GameTime.solarTime = Mathf.Clamp01(Remap (sunTheta, -1.5f, 0f, 1.5f, 1f));

		SunTransform.localPosition = OrbitalToLocal(sunTheta, sunPhi);
		SunTransform.LookAt(DomeTransform.position);

        if(simpleMoon)
        {
          MoonTransform.localPosition = OrbitalToLocal(sunTheta - pi, sunPhi);
          MoonTransform.LookAt(DomeTransform.position);
        }

        SetupShader(sunTheta);
	}

    void CalculateMoonPosition(float d, float ecl)
    {

        float N = 125.1228f - 0.0529538083f * d;
        float i = 5.1454f;
        float w = 318.0634f + 0.1643573223f * d;
        float a = 60.2666f;
        float e = 0.054900f;
        float M = 115.3654f + 13.0649929509f * d;

        float rad_M = Mathf.Deg2Rad * M;

        float E = rad_M + e * Mathf.Sin(rad_M) * (1f + e * Mathf.Cos(rad_M));

        float xv = a * (Mathf.Cos(E) - e);
        float yv = a * (Mathf.Sqrt(1f - e * e) * Mathf.Sin(E));

        float v = Mathf.Rad2Deg * Mathf.Atan2(yv, xv);
        float r = Mathf.Sqrt(xv * xv + yv * yv);

        float rad_N = Mathf.Deg2Rad * N;
        float sin_N = Mathf.Sin(rad_N);
        float cos_N = Mathf.Cos(rad_N);

        float l = Mathf.Deg2Rad * (v + w);
        float sin_l = Mathf.Sin(l);
        float cos_l = Mathf.Cos(l);

        float rad_i = Mathf.Deg2Rad * i;
        float cos_i = Mathf.Cos(rad_i);

        float xh = r * (cos_N * cos_l - sin_N * sin_l * cos_i);
        float yh = r * (sin_N * cos_l + cos_N * sin_l * cos_i);
        float zh = r * (sin_l * Mathf.Sin(rad_i));

        float cos_ecl = Mathf.Cos(Mathf.Deg2Rad * ecl);
        float sin_ecl = Mathf.Sin(Mathf.Deg2Rad * ecl);

        float xe = xh;
        float ye = yh * cos_ecl - zh * sin_ecl;
        float ze = yh * sin_ecl + zh * cos_ecl;

        float ra = Mathf.Atan2(ye, xe);
        float decl = Mathf.Atan2(ze, Mathf.Sqrt(xe * xe + ye * ye));

        float HA = Mathf.Deg2Rad * LST - ra;

        float x = Mathf.Cos(HA) * Mathf.Cos(decl);
        float y = Mathf.Sin(HA) * Mathf.Cos(decl);
        float z = Mathf.Sin(decl);

        float latitude = Mathf.Deg2Rad * GameTime.Latitude;
        float sin_latitude = Mathf.Sin(latitude);
        float cos_latitude = Mathf.Cos(latitude);

        float xhor = x * sin_latitude - z * cos_latitude;
        float yhor = y;
        float zhor = x * cos_latitude + z * sin_latitude;

        float azimuth = Mathf.Atan2(yhor, xhor) + Mathf.Deg2Rad * 180f;
        float altitude = Mathf.Atan2(zhor, Mathf.Sqrt(xhor * xhor + yhor * yhor));

        float MoonTheta = (90f * Mathf.Deg2Rad) - altitude;
        float MoonPhi = azimuth;

        MoonTransform.localPosition = OrbitalToLocal(MoonTheta, MoonPhi);
        GameTime.lunarTime = Mathf.Clamp01(Remap(MoonTheta, -1.5f, 0f, 1.5f, 1f));
        MoonTransform.LookAt(DomeTransform.position);
    }


	void CalculateStarsPosition (float siderealTime)
	{
      
        if (siderealTime > 24) siderealTime -= 24;
        else if (siderealTime < 0) siderealTime += 24;

        Quaternion starsRotation = Quaternion.Euler (90 - GameTime.Latitude, Mathf.Deg2Rad * GameTime.Longitude, 0); 
		starsRotation *= Quaternion.Euler(0, siderealTime, 0);

		Components.starsRotation.localRotation = starsRotation;

        if(RenderSettings.skybox != null)
		    RenderSettings.skybox.SetMatrix ("_StarsMatrix", Components.starsRotation.worldToLocalMatrix);
	}

	Vector3 UpdateSatellitePosition (float orbit,float orbit2,float speed)
	{
		// Calculates the Solar latitude
		float latitudeRadians = Mathf.Deg2Rad * GameTime.Latitude;
		float latitudeRadiansSin = Mathf.Sin(latitudeRadians);
		float latitudeRadiansCos = Mathf.Cos(latitudeRadians);

		// Calculates the Solar longitude
		float longitudeRadians = Mathf.Deg2Rad * GameTime.Longitude;

		// Solar declination - constant for the whole globe at any given day
		float solarDeclination = orbit2 * Mathf.Sin(2f * pi / 368f * (GameTime.Days - 81f));
		float solarDeclinationSin = Mathf.Sin(solarDeclination);
		float solarDeclinationCos = Mathf.Cos(solarDeclination);

		// Calculate Solar time
		float timeZone = (int)(GameTime.Longitude / 15f);
		float meridian = Mathf.Deg2Rad * 15f * timeZone;

		float solarTime = GetUniversalTimeOfDay() + orbit * Mathf.Sin(4f * pi / 377f * (GameTime.Days - 80f)) - speed * Mathf.Sin(1f * pi / 355f * (GameTime.Days - 8f))  + 12f / pi * (meridian - longitudeRadians);

		float solarTimeRadians = pi / 12f * solarTime;
		float solarTimeSin = Mathf.Sin(solarTimeRadians);
		float solarTimeCos = Mathf.Cos(solarTimeRadians);

		// Solar altitude angle between the sun and the horizon
		float solarAltitudeSin = latitudeRadiansSin * solarDeclinationSin - latitudeRadiansCos * solarDeclinationCos * solarTimeCos;
		float solarAltitude = Mathf.Asin(solarAltitudeSin);

		// Solar azimuth angle of the sun around the horizon
		float solarAzimuthY = -solarDeclinationCos * solarTimeSin;
		float solarAzimuthX = latitudeRadiansCos * solarDeclinationSin - latitudeRadiansSin * solarDeclinationCos * solarTimeCos;
		float solarAzimuth = Mathf.Atan2(solarAzimuthY, solarAzimuthX);

		// Convert to spherical coords
		float theta = pi / 2 - solarAltitude;
		float phi = solarAzimuth;

		// Send local position
		return OrbitalToLocal(theta, phi);
	}

	Vector3 OrbitalToLocal(float theta, float phi)
	{
		Vector3 pos;

		float sinTheta = Mathf.Sin(theta);
		float cosTheta = Mathf.Cos(theta);
		float sinPhi   = Mathf.Sin(phi);
		float cosPhi   = Mathf.Cos(phi);

        pos.z = sinTheta * cosPhi;
        pos.y = cosTheta;
        pos.x = sinTheta * sinPhi;

		return pos;
	}

	void UpdateReflections ()
	{
		Components.GlobalReflectionProbe.intensity = lightSettings.globalReflectionsIntensity;
        Components.GlobalReflectionProbe.size = transform.localScale * lightSettings.globalReflectionsScale;

        if ((currentTimeInHours > lastRelfectionUpdate + lightSettings.globalReflectionsUpdate || currentTimeInHours < lastRelfectionUpdate - lightSettings.globalReflectionsUpdate) && lightSettings.globalReflections) {
			Components.GlobalReflectionProbe.enabled = true;
			lastRelfectionUpdate = currentTimeInHours;
			Components.GlobalReflectionProbe.RenderProbe ();
		} else if (!lightSettings.globalReflections) {
			Components.GlobalReflectionProbe.enabled = false;
		}
	}

	// Update the GameTime
	void UpdateTime()
	{
		if (Application.isPlaying) {

			float t = 0f;

			if(!isNight)
				t = (24.0f / 60.0f) / GameTime.DayLengthInMinutes;
			else
				t = (24.0f / 60.0f) / GameTime.NightLengthInMinutes;

			hourTime = t * Time.deltaTime;

			switch (GameTime.ProgressTime) {
			case EnviroTime.TimeProgressMode.None://Set Time over editor or other scripts.
				SetTime (GameTime.Years, GameTime.Days, GameTime.Hours, GameTime.Minutes, GameTime.Seconds);
				break;
			case EnviroTime.TimeProgressMode.Simulated:
				internalHour += hourTime;
				SetGameTime ();
				customMoonPhase += Time.deltaTime / (30f * (GameTime.DayLengthInMinutes * 60f)) * 2f;
				break;
			case EnviroTime.TimeProgressMode.OneDay:
				internalHour += hourTime;
				SetGameTime ();
				customMoonPhase += Time.deltaTime / (30f * (GameTime.DayLengthInMinutes * 60f)) * 2f;
				break;
			case EnviroTime.TimeProgressMode.SystemTime:
				SetTime (System.DateTime.Now);
				customMoonPhase += Time.deltaTime / (30f * (1440f * 60f)) * 2f;
				break;
			}
		} 
		else 
		{
			SetTime (GameTime.Years, GameTime.Days, GameTime.Hours, GameTime.Minutes, GameTime.Seconds);
		}

		if (customMoonPhase < -1) customMoonPhase += 2;
		else if (customMoonPhase > 1) customMoonPhase -= 2;

		//Fire OnHour Event
		if (internalHour > (lastHourUpdate + 1f)) {
			lastHourUpdate = internalHour;
			NotifyHourPassed ();
		}

		// Check Days
		if(GameTime.Days >= (seasonsSettings.SpringInDays + seasonsSettings.SummerInDays + seasonsSettings.AutumnInDays + seasonsSettings.WinterInDays)){
			GameTime.Years = GameTime.Years + 1;
			GameTime.Days = 0;
			NotifyYearPassed ();
		}

		currentHour = internalHour;
		currentDay = GameTime.Days;
		currentYear = GameTime.Years;

		currentTimeInHours = GetInHours (internalHour, currentDay, currentYear);
	}

	private void SetInternalTime(int year, int dayOfYear, int hour, int minute, int seconds)
	{
		GameTime.Years = year;
		GameTime.Days = dayOfYear;
		GameTime.Minutes = minute;
		GameTime.Hours = hour;
		internalHour = hour + (minute * 0.0166667f) + (seconds * 0.000277778f);
	}

	/// <summary>
	/// Set the time of day in hours. (12.5 = 12:30)
	/// </summary>
	private void SetGameTime()
	{ 
		if (internalHour >= 24f) {
			internalHour = internalHour - 24f;
			NotifyHourPassed ();
			lastHourUpdate = internalHour;
			if (GameTime.ProgressTime != EnviroTime.TimeProgressMode.OneDay) {
				GameTime.Days = GameTime.Days + 1;
				NotifyDayPassed ();
			}
		} else if (internalHour < 0f) {
			internalHour = 24f + internalHour;
			lastHourUpdate = internalHour;

			if (GameTime.ProgressTime != EnviroTime.TimeProgressMode.OneDay) {
				GameTime.Days = GameTime.Days - 1;
				NotifyDayPassed ();
			}
		}

		float inHours = internalHour;
		GameTime.Hours = (int)(inHours);
		inHours -= GameTime.Hours;
		GameTime.Minutes = (int)(inHours * 60f);
		inHours -= GameTime.Minutes * 0.0166667f;
		GameTime.Seconds = (int)(inHours * 3600f);
	}


	void UpdateAmbientLight ()
	{
		switch (lightSettings.ambientMode) {
		case UnityEngine.Rendering.AmbientMode.Flat:
			Color lightClr = Color.Lerp(lightSettings.ambientSkyColor.Evaluate (GameTime.solarTime),currentWeatherLightMod,currentWeatherLightMod.a) * lightSettings.ambientIntensity.Evaluate(GameTime.solarTime);
			RenderSettings.ambientSkyColor = Color.Lerp (lightClr, currentInteriorAmbientLightMod, currentInteriorAmbientLightMod.a);
		break;

		case UnityEngine.Rendering.AmbientMode.Trilight:
			Color lClr = Color.Lerp(lightSettings.ambientSkyColor.Evaluate (GameTime.solarTime),currentWeatherLightMod,currentWeatherLightMod.a) * lightSettings.ambientIntensity.Evaluate(GameTime.solarTime);
			RenderSettings.ambientSkyColor = Color.Lerp (lClr, currentInteriorAmbientLightMod, currentInteriorAmbientLightMod.a);
			Color eqClr = Color.Lerp(lightSettings.ambientEquatorColor.Evaluate (GameTime.solarTime),currentWeatherLightMod,currentWeatherLightMod.a) * lightSettings.ambientIntensity.Evaluate(GameTime.solarTime);
			RenderSettings.ambientEquatorColor =  Color.Lerp (eqClr, currentInteriorAmbientEQLightMod, currentInteriorAmbientEQLightMod.a);
			Color grClr = Color.Lerp(lightSettings.ambientGroundColor.Evaluate (GameTime.solarTime),currentWeatherLightMod,currentWeatherLightMod.a) * lightSettings.ambientIntensity.Evaluate(GameTime.solarTime);
			RenderSettings.ambientGroundColor = Color.Lerp (grClr, currentInteriorAmbientGRLightMod, currentInteriorAmbientGRLightMod.a);
		break;
              
		case UnityEngine.Rendering.AmbientMode.Skybox:
                RenderSettings.ambientIntensity = lightSettings.ambientIntensity.Evaluate(GameTime.solarTime);
                if (lastAmbientSkyUpdate < internalHour || lastAmbientSkyUpdate > internalHour + 0.101f)
                {
			        DynamicGI.UpdateEnvironment ();
                    lastAmbientSkyUpdate = internalHour + 0.1f;
                }
             break;
		}
	}

	// Calculate sun and moon light intensity and color
	private void CalculateDirectLight()
	{ 
		Color lightClr = Color.Lerp(lightSettings.LightColor.Evaluate (GameTime.solarTime),currentWeatherLightMod,currentWeatherLightMod.a);
		MainLight.color = Color.Lerp (lightClr, currentInteriorDirectLightMod, currentInteriorDirectLightMod.a);
         
		Shader.SetGlobalColor ("_EnviroLighting", lightSettings.LightColor.Evaluate (GameTime.solarTime));
		Shader.SetGlobalVector ("_SunDirection", -Components.Sun.transform.forward);

		Shader.SetGlobalVector ("_SunPosition", Components.Sun.transform.localPosition + (-Components.Sun.transform.forward * 10000f));
		Shader.SetGlobalVector ("_MoonPosition", Components.Moon.transform.localPosition);
        //MOON
       // Shader.SetGlobalVector("_lightVector", (Components.Sun.transform.localPosition + (-Components.Sun.transform.forward * 10000f)) - Components.Moon.transform.localPosition);

        float lightIntensity;

		// Set sun and moon intensity
		if (!isNight)
		{
			lightIntensity = lightSettings.directLightSunIntensity.Evaluate (GameTime.solarTime);
            Components.Sun.transform.LookAt(new Vector3(DomeTransform.position.x, DomeTransform.position.y - lightSettings.directLightAngleOffset, DomeTransform.position.z));
            Components.DirectLight.rotation = Components.Sun.transform.rotation;
        }
		else
		{
			lightIntensity = lightSettings.directLightMoonIntensity.Evaluate (GameTime.lunarTime);
            Components.Moon.transform.LookAt(new Vector3(DomeTransform.position.x, DomeTransform.position.y - lightSettings.directLightAngleOffset, DomeTransform.position.z));
            Components.DirectLight.rotation = Components.Moon.transform.rotation;
        }

        // Set the light and shadow intensity
      //  MainLight.intensity = Mathf.Lerp (MainLight.intensity, lightIntensity, 5f * Time.deltaTime);
        MainLight.intensity = lightIntensity;
       MainLight.shadowStrength = lightSettings.shadowIntensity.Evaluate(GameTime.solarTime);
	}

    private Quaternion LightLookAt (Quaternion inputRotation, Quaternion newRotation)
    {
        return Quaternion.Lerp(inputRotation, newRotation, 500f * Time.deltaTime);
    }


	// Make the parameters stay in reasonable range
	private void ValidateParameters()
	{
		// Keep GameTime Parameters right!
		internalHour = Mathf.Repeat(internalHour, 24f);
		GameTime.Longitude = Mathf.Clamp(GameTime.Longitude, -180, 180);
		GameTime.Latitude = Mathf.Clamp(GameTime.Latitude, -90, 90);
		#if UNITY_EDITOR
		if (GameTime.DayLengthInMinutes <= 0f || GameTime.NightLengthInMinutes<= 0f)
		{
		if (GameTime.DayLengthInMinutes < 0f)
		GameTime.DayLengthInMinutes = 0f;

		if (GameTime.NightLengthInMinutes < 0f)
		GameTime.NightLengthInMinutes = 0f;
		internalHour = 12f;
		customMoonPhase = 0f;
		}

		if(GameTime.Days < 0)
		GameTime.Days = 0;

		if(GameTime.Years < 0)
		GameTime.Years = 0;

		// Moon
		customMoonPhase = Mathf.Clamp(customMoonPhase, -1f, 1f);
		#endif
	}

	///////////////////////////////////////////////////////////////////WEATHER SYSTEM /////////////////////////////////////////////////////////////////////////
	public void RegisterZone (EnviroZone zoneToAdd)
	{
		Weather.zones.Add (zoneToAdd);
	}


	public void EnterZone (EnviroZone zone)
	{
		Weather.currentActiveZone = zone;
	}

	public void ExitZone ()
	{

	}

	public void CreateWeatherEffectHolder()
	{
		if (Weather.VFXHolder == null) {
			GameObject VFX = new GameObject ();
			VFX.name = "VFX";
			VFX.transform.parent = EffectsHolder.transform;
			VFX.transform.localPosition = Vector3.zero;
			Weather.VFXHolder = VFX;
		}
	}

	private void UpdateAudioSource (EnviroWeatherPreset i)
	{
		if (i != null && i.weatherSFX != null)
		{
			if (i.weatherSFX == Weather.currentAudioSource.audiosrc.clip)
			{
				if(Weather.currentAudioSource.audiosrc.volume < 0.1f)
					Weather.currentAudioSource.FadeIn(i.weatherSFX);

				return;
			}

			if (Weather.currentAudioSource == AudioSourceWeather)
			{
				AudioSourceWeather.FadeOut();
				AudioSourceWeather2.FadeIn(i.weatherSFX);
				Weather.currentAudioSource = AudioSourceWeather2;
			}
			else if (Weather.currentAudioSource == AudioSourceWeather2)
			{
				AudioSourceWeather2.FadeOut();
				AudioSourceWeather.FadeIn(i.weatherSFX);
				Weather.currentAudioSource = AudioSourceWeather;
			}
		} 
		else
		{
			AudioSourceWeather.FadeOut();
			AudioSourceWeather2.FadeOut();
		}
	}

	private void UpdateClouds (EnviroWeatherPreset i, bool withTransition)
	{
		if (i == null)
			return;

		float speed = 500f * Time.deltaTime;

		if (withTransition)
			speed = weatherSettings.cloudTransitionSpeed * Time.deltaTime;

		cloudsConfig.topColor = Color.Lerp (cloudsConfig.topColor, i.cloudsConfig.topColor, speed);
		cloudsConfig.bottomColor = Color.Lerp (cloudsConfig.bottomColor, i.cloudsConfig.bottomColor, speed);
		cloudsConfig.coverage = Mathf.Lerp (cloudsConfig.coverage, i.cloudsConfig.coverage, speed);
		cloudsConfig.coverageHeight = Mathf.Lerp (cloudsConfig.coverageHeight, i.cloudsConfig.coverageHeight, speed);
        cloudsConfig.raymarchingScale = Mathf.Lerp(cloudsConfig.raymarchingScale, i.cloudsConfig.raymarchingScale, speed);
        cloudsConfig.skyBlending = Mathf.Lerp(cloudsConfig.skyBlending, i.cloudsConfig.skyBlending, speed);

        cloudsConfig.density = Mathf.Lerp (cloudsConfig.density, i.cloudsConfig.density, speed);
		cloudsConfig.alphaCoef = Mathf.Lerp (cloudsConfig.alphaCoef, i.cloudsConfig.alphaCoef, speed);
		cloudsConfig.scatteringCoef = Mathf.Lerp (cloudsConfig.scatteringCoef, i.cloudsConfig.scatteringCoef, speed);
		cloudsConfig.cloudType = Mathf.Lerp (cloudsConfig.cloudType, i.cloudsConfig.cloudType, speed);

		cloudsConfig.cirrusAlpha = Mathf.Lerp (cloudsConfig.cirrusAlpha, i.cloudsConfig.cirrusAlpha, speed);
		cloudsConfig.cirrusCoverage = Mathf.Lerp (cloudsConfig.cirrusCoverage, i.cloudsConfig.cirrusCoverage, speed);
		cloudsConfig.cirrusColorPow = Mathf.Lerp (cloudsConfig.cirrusColorPow, i.cloudsConfig.cirrusColorPow, speed);

		cloudsConfig.flatAlpha = Mathf.Lerp (cloudsConfig.flatAlpha, i.cloudsConfig.flatAlpha, speed);
		cloudsConfig.flatCoverage = Mathf.Lerp (cloudsConfig.flatCoverage, i.cloudsConfig.flatCoverage, speed);
		cloudsConfig.flatColorPow = Mathf.Lerp (cloudsConfig.flatColorPow, i.cloudsConfig.flatColorPow, speed);

        cloudsConfig.flatSoftness = Mathf.Lerp(cloudsConfig.flatSoftness, i.cloudsConfig.flatSoftness, speed);
        cloudsConfig.flatBrightness = Mathf.Lerp(cloudsConfig.flatBrightness, i.cloudsConfig.flatBrightness, speed);

        globalVolumeLightIntensity = Mathf.Lerp(globalVolumeLightIntensity, i.volumeLightIntensity, speed);

        currentWeatherSkyMod = Color.Lerp (currentWeatherSkyMod, i.weatherSkyMod.Evaluate(GameTime.solarTime), speed);
		currentWeatherFogMod = Color.Lerp (currentWeatherFogMod, i.weatherFogMod.Evaluate(GameTime.solarTime), speed * 10);
		currentWeatherLightMod = Color.Lerp (currentWeatherLightMod, i.weatherLightMod.Evaluate(GameTime.solarTime), speed);
	}


	void UpdateFog (EnviroWeatherPreset i, bool withTransition)
	{
		if (i != null) {

			float speed = 500f * Time.deltaTime;

			if (withTransition)
				speed = weatherSettings.fogTransitionSpeed * Time.deltaTime;

			if (fogSettings.Fogmode == FogMode.Linear) {
				RenderSettings.fogEndDistance = Mathf.Lerp (RenderSettings.fogEndDistance, i.fogDistance, speed);
				RenderSettings.fogStartDistance = Mathf.Lerp (RenderSettings.fogStartDistance, i.fogStartDistance, speed);
			} else {
				if(updateFogDensity)
					RenderSettings.fogDensity = Mathf.Lerp (RenderSettings.fogDensity, i.fogDensity, speed) * currentInteriorFogMod;
			}

			// Set the Fog color to light color to match Day-Night cycle and weather
			Color fogClr = Color.Lerp(lightSettings.ambientSkyColor.Evaluate(GameTime.solarTime),customFogColor,customFogIntensity);
			RenderSettings.fogColor = Color.Lerp(fogClr,currentWeatherFogMod,currentWeatherFogMod.a);

			fogSettings.heightDensity = Mathf.Lerp (fogSettings.heightDensity, i.heightFogDensity, speed);
			Fog.skyFogHeight = Mathf.Lerp (Fog.skyFogHeight, i.SkyFogHeight, speed);
			Fog.skyFogStrength = Mathf.Lerp (Fog.skyFogStrength, i.SkyFogIntensity, speed);
			fogSettings.skyFogIntensity = Mathf.Lerp (fogSettings.skyFogIntensity, i.SkyFogIntensity, speed);
			Fog.scatteringStrenght = Mathf.Lerp (Fog.scatteringStrenght, i.FogScatteringIntensity, speed);
			Fog.sunBlocking = Mathf.Lerp (Fog.sunBlocking, i.fogSunBlocking, speed);
		}
	}

	void UpdateEffectSystems (EnviroWeatherPrefab id, bool withTransition)
	{
		if (id != null) {

			float speed = 500f * Time.deltaTime;

			if (withTransition)
				speed = weatherSettings.effectTransitionSpeed * Time.deltaTime;

			for (int i = 0; i < id.effectSystems.Count; i++) {

                if (id.effectSystems[i].isStopped)
                    id.effectSystems[i].Play();

                // Set EmissionRate
                float val = Mathf.Lerp (GetEmissionRate (id.effectSystems [i]), id.effectEmmisionRates [i] * qualitySettings.GlobalParticleEmissionRates, speed ) * currentInteriorWeatherEffectMod;
				SetEmissionRate (id.effectSystems [i], val);
			}

			for (int i = 0; i < Weather.WeatherPrefabs.Count; i++) {
				if (Weather.WeatherPrefabs [i].gameObject != id.gameObject) {
					for (int i2 = 0; i2 < Weather.WeatherPrefabs [i].effectSystems.Count; i2++) {
						float val2 = Mathf.Lerp (GetEmissionRate (Weather.WeatherPrefabs [i].effectSystems [i2]), 0f, speed);

						if (val2 < 1f)
							val2 = 0f;

						SetEmissionRate (Weather.WeatherPrefabs [i].effectSystems [i2], val2);

                        if (val2 == 0f && !Weather.WeatherPrefabs[i].effectSystems[i2].isStopped)
                        {
                            Weather.WeatherPrefabs[i].effectSystems[i2].Stop();
                        }
                    }
				}
			}

            UpdateWeatherVariables(id.weatherPreset);
        }
	}


    private void UpdateWeatherVariables(EnviroWeatherPreset p)
    {
        Components.windZone.windMain = p.WindStrenght; // Set Wind Strenght

        // The wetness raise
        if (Weather.wetness < p.wetnessLevel)
        {
            Weather.wetness = Mathf.Lerp(Weather.curWetness, p.wetnessLevel, weatherSettings.wetnessAccumulationSpeed * Time.deltaTime);
        }
        else
        { // Drying
            Weather.wetness = Mathf.Lerp(Weather.curWetness, p.wetnessLevel, weatherSettings.wetnessDryingSpeed * Time.deltaTime);
        }

        Weather.wetness = Mathf.Clamp(Weather.wetness, 0f, 1f);
        Weather.curWetness = Weather.wetness;

        //Snowing
        if (Weather.snowStrength < p.snowLevel)
            Weather.snowStrength = Mathf.Lerp(Weather.curSnowStrength, p.snowLevel, weatherSettings.snowAccumulationSpeed * Time.deltaTime);
        else //Melting
            Weather.snowStrength = Mathf.Lerp(Weather.curSnowStrength, p.snowLevel, weatherSettings.snowMeltingSpeed * Time.deltaTime);

        Weather.snowStrength = Mathf.Clamp(Weather.snowStrength, 0f, 1f);
        Weather.curSnowStrength = Weather.snowStrength;

        Shader.SetGlobalFloat("_EnviroGrassSnow", Weather.curSnowStrength);
    }


	public static float GetEmissionRate (ParticleSystem system)
	{
		return system.emission.rateOverTime.constantMax;
	}


	public static void SetEmissionRate (ParticleSystem sys, float emissionRate)
	{
		var emission = sys.emission;
		var rate = emission.rateOverTime;
		rate.constantMax = emissionRate;
		emission.rateOverTime = rate;
	}

	IEnumerator PlayThunderRandom()
	{
		yield return new WaitForSeconds(UnityEngine.Random.Range(Weather.currentActiveWeatherPreset.lightningInterval,Weather.currentActiveWeatherPreset.lightningInterval * 2));

		if (Weather.currentActiveWeatherPrefab.weatherPreset.isLightningStorm) 
		{
			if(Weather.weatherFullyChanged)
                PlayLightning ();

			StartCoroutine (PlayThunderRandom ());
		}
		else 
		{
			StopCoroutine (PlayThunderRandom ());
			Components.LightningGenerator.StopLightning ();
		}
	}


    public IEnumerator PlayLightningEffect(Vector3 position)
    {
        lightningEffect.transform.position = position;
        lightningEffect.transform.eulerAngles = new Vector3(UnityEngine.Random.Range(-80f, -100f), 0f, 0f);
        lightningEffect.Play();
        yield return new WaitForSeconds(0.5f);
        lightningEffect.Stop();
    }

    public void PlayLightning ()
	{
        if(lightningEffect != null)
        StartCoroutine(PlayLightningEffect(new Vector3(transform.position.x + UnityEngine.Random.Range(-weatherSettings.lightningRange, weatherSettings.lightningRange), weatherSettings.lightningHeight, transform.position.z + UnityEngine.Random.Range(-weatherSettings.lightningRange, weatherSettings.lightningRange))));

        int i = UnityEngine.Random.Range(0,audioSettings.ThunderSFX.Count);
		AudioSourceThunder.clip = audioSettings.ThunderSFX [i];
		AudioSourceThunder.loop = false;
		AudioSourceThunder.Play ();
		Components.LightningGenerator.Lightning ();
	}

	void UpdateWeather ()
	{	
		//Current active weather not matching current zones weather
		if(Weather.currentActiveWeatherPreset != Weather.currentActiveZone.currentActiveZoneWeatherPreset)
		{
			Weather.lastActiveWeatherPreset = Weather.currentActiveWeatherPreset;
			Weather.lastActiveWeatherPrefab = Weather.currentActiveWeatherPrefab;
			Weather.currentActiveWeatherPreset = Weather.currentActiveZone.currentActiveZoneWeatherPreset;
			Weather.currentActiveWeatherPrefab = Weather.currentActiveZone.currentActiveZoneWeatherPrefab;
			if (Weather.currentActiveWeatherPreset != null) {
				NotifyWeatherChanged (Weather.currentActiveWeatherPreset);
				Weather.weatherFullyChanged = false;
				if (!serverMode) {
					TryPlayAmbientSFX ();
					UpdateAudioSource (Weather.currentActiveWeatherPreset);

					if (Weather.currentActiveWeatherPrefab.weatherPreset.isLightningStorm)
						StartCoroutine (PlayThunderRandom ());
					else {
						StopCoroutine (PlayThunderRandom ());
						Components.LightningGenerator.StopLightning ();
					}
				}
			}
		}

		if (Weather.currentActiveWeatherPrefab != null && !serverMode) 
		{
			UpdateClouds (Weather.currentActiveWeatherPreset, true);
			UpdateFog (Weather.currentActiveWeatherPreset, true);
			UpdateEffectSystems (Weather.currentActiveWeatherPrefab, true);
			if(!Weather.weatherFullyChanged)
				CalcWeatherTransitionState ();
		}
        else if(Weather.currentActiveWeatherPrefab != null)
        {
            UpdateWeatherVariables(Weather.currentActiveWeatherPrefab.weatherPreset);
        }
	}
    /// <summary>
    /// Forces a internal Weather Update and applies current active weatherpreset values and send out a weather changed event!
    /// </summary>
    public void ForceWeatherUpdate ()
    {
        Weather.lastActiveWeatherPreset = Weather.currentActiveWeatherPreset;
        Weather.lastActiveWeatherPrefab = Weather.currentActiveWeatherPrefab;
        Weather.currentActiveWeatherPreset = Weather.currentActiveZone.currentActiveZoneWeatherPreset;
        Weather.currentActiveWeatherPrefab = Weather.currentActiveZone.currentActiveZoneWeatherPrefab;
        if (Weather.currentActiveWeatherPreset != null)
        {
            NotifyWeatherChanged(Weather.currentActiveWeatherPreset);
            Weather.weatherFullyChanged = false;
            if (!serverMode)
            {
             //   TryPlayAmbientSFX();
             //   UpdateAudioSource(Weather.currentActiveWeatherPreset);

                if (Weather.currentActiveWeatherPrefab.weatherPreset.isLightningStorm)
                    StartCoroutine(PlayThunderRandom());
                else
                {
                    StopCoroutine(PlayThunderRandom());
                    Components.LightningGenerator.StopLightning();
                }
            }
        }
    }
	/// <summary>
	/// Check if clouds already full rolled up to start thunder effects.
	/// </summary>
	void CalcWeatherTransitionState ()
	{
		bool changed = false;

		if(cloudsConfig.coverage >= Weather.currentActiveWeatherPreset.cloudsConfig.coverage - 0.01f)
			changed = true;
		else
			changed = false;

		Weather.weatherFullyChanged = changed;
	}

	/// <summary>
	/// Set weather directly with list id of Weather.WeatherTemplates. No transtions!
	/// </summary>
	public void SetWeatherOverwrite (int weatherId)
	{
		if (weatherId < 0 || weatherId > Weather.WeatherPrefabs.Count)
			return;

		if (Weather.WeatherPrefabs[weatherId] != Weather.currentActiveWeatherPrefab)
		{
			Weather.currentActiveZone.currentActiveZoneWeatherPrefab = Weather.WeatherPrefabs[weatherId];
			Weather.currentActiveZone.currentActiveZoneWeatherPreset = Weather.WeatherPrefabs[weatherId].weatherPreset;
			EnviroSky.instance.NotifyZoneWeatherChanged (Weather.WeatherPrefabs[weatherId].weatherPreset, Weather.currentActiveZone);
		}

		UpdateClouds (Weather.currentActiveZone.currentActiveZoneWeatherPreset, false);
		UpdateFog (Weather.currentActiveZone.currentActiveZoneWeatherPreset, false);
		UpdateEffectSystems (Weather.currentActiveZone.currentActiveZoneWeatherPrefab, false);
	}
	/// <summary>
	/// Set weather directly with preset of Weather.WeatherTemplates. No transtions!
	/// </summary>
	public void SetWeatherOverwrite (EnviroWeatherPreset preset)
	{
		if (preset == null)
			return;

		if (preset != Weather.currentActiveWeatherPreset)
		{
			for (int i = 0; i < Weather.WeatherPrefabs.Count; i++) {
				if (preset == Weather.WeatherPrefabs [i].weatherPreset) {
					Weather.currentActiveZone.currentActiveZoneWeatherPrefab = Weather.WeatherPrefabs[i];
					Weather.currentActiveZone.currentActiveZoneWeatherPreset = preset;
					EnviroSky.instance.NotifyZoneWeatherChanged (preset, Weather.currentActiveZone);
				}
			}
		}

		UpdateClouds (Weather.currentActiveZone.currentActiveZoneWeatherPreset, false);
		UpdateFog (Weather.currentActiveZone.currentActiveZoneWeatherPreset, false);
		UpdateEffectSystems (Weather.currentActiveZone.currentActiveZoneWeatherPrefab, false);
	}

	/// <summary>
	/// Set weather over id with smooth transtion.
	/// </summary>
	public void ChangeWeather (int weatherId)
	{
		if (weatherId < 0 || weatherId > Weather.WeatherPrefabs.Count)
			return;

		if (Weather.WeatherPrefabs[weatherId] != Weather.currentActiveWeatherPrefab)
		{
			Weather.currentActiveZone.currentActiveZoneWeatherPrefab = Weather.WeatherPrefabs[weatherId];
			Weather.currentActiveZone.currentActiveZoneWeatherPreset = Weather.WeatherPrefabs[weatherId].weatherPreset;
			EnviroSky.instance.NotifyZoneWeatherChanged (Weather.WeatherPrefabs[weatherId].weatherPreset, Weather.currentActiveZone);
		}
	}

	/// <summary>
	/// Set weather over name.
	/// </summary>
	public void ChangeWeather (string weatherName)
	{
		for (int i = 0; i < Weather.WeatherPrefabs.Count; i++) {
			if (Weather.WeatherPrefabs [i].weatherPreset.Name == weatherName && Weather.WeatherPrefabs [i] != Weather.currentActiveWeatherPrefab) {
				ChangeWeather (i);
				EnviroSky.instance.NotifyZoneWeatherChanged (Weather.WeatherPrefabs [i].weatherPreset, Weather.currentActiveZone);
			}
		}
	}

    /// <summary>
    /// Change volume clouds quality mode and apply settings.
    /// </summary>
    /// <param name="q"></param>
    public void ChangeCloudsQuality(EnviroCloudSettings.CloudQuality q)
    {
        if (q == EnviroCloudSettings.CloudQuality.Custom)
            return;

        switch (q)
        { 

         case EnviroCloudSettings.CloudQuality.Lowest:
            cloudsSettings.bottomCloudHeight = 2000f;
            cloudsSettings.topCloudHeight = 4000f;
            cloudsSettings.cloudsWorldScale = 120000f;
            cloudsSettings.raymarchSteps = 75;
            cloudsSettings.stepsInDepthModificator = 0.7f;
            cloudsSettings.cloudsRenderResolution = 2;
            cloudsSettings.reprojectionPixelSize = EnviroCloudSettings.ReprojectionPixelSize.Medium;
            cloudsSettings.baseNoiseUV = 26f;
            cloudsSettings.detailNoiseUV = 1f;
            cloudsSettings.detailQuality = EnviroCloudSettings.CloudDetailQuality.Low;
        break;

        case EnviroCloudSettings.CloudQuality.Low:
                cloudsSettings.bottomCloudHeight = 2000f;
                cloudsSettings.topCloudHeight = 4000f;
                cloudsSettings.cloudsWorldScale = 120000f;
                cloudsSettings.raymarchSteps = 90;
                cloudsSettings.stepsInDepthModificator = 0.7f;
                cloudsSettings.cloudsRenderResolution = 2;
                cloudsSettings.reprojectionPixelSize = EnviroCloudSettings.ReprojectionPixelSize.Low;
                cloudsSettings.baseNoiseUV = 30f;
                cloudsSettings.detailNoiseUV = 1f;
                cloudsSettings.detailQuality = EnviroCloudSettings.CloudDetailQuality.Low;
                break;

            case EnviroCloudSettings.CloudQuality.Medium:
                cloudsSettings.bottomCloudHeight = 2000f;
                cloudsSettings.topCloudHeight = 4500f;
                cloudsSettings.cloudsWorldScale = 120000f;
                cloudsSettings.raymarchSteps = 100;
                cloudsSettings.stepsInDepthModificator = 0.7f;
                cloudsSettings.cloudsRenderResolution = 1;
                cloudsSettings.reprojectionPixelSize = EnviroCloudSettings.ReprojectionPixelSize.Medium;
                cloudsSettings.baseNoiseUV = 35f;
                cloudsSettings.detailNoiseUV = 50f;
                cloudsSettings.detailQuality = EnviroCloudSettings.CloudDetailQuality.Low;
                break;

            case EnviroCloudSettings.CloudQuality.High:
                cloudsSettings.bottomCloudHeight = 2000f;
                cloudsSettings.topCloudHeight = 5000f;
                cloudsSettings.cloudsWorldScale = 120000f;
                cloudsSettings.raymarchSteps = 128;
                cloudsSettings.stepsInDepthModificator = 0.6f;
                cloudsSettings.cloudsRenderResolution = 1;
                cloudsSettings.reprojectionPixelSize = EnviroCloudSettings.ReprojectionPixelSize.Medium;
                cloudsSettings.baseNoiseUV = 40f;
                cloudsSettings.detailNoiseUV = 50f;
                cloudsSettings.detailQuality = EnviroCloudSettings.CloudDetailQuality.Low;
                break;

            case EnviroCloudSettings.CloudQuality.Ultra:
                cloudsSettings.bottomCloudHeight = 2000f;
                cloudsSettings.topCloudHeight = 5500f;
                cloudsSettings.cloudsWorldScale = 120000f;
                cloudsSettings.raymarchSteps = 150;
                cloudsSettings.stepsInDepthModificator = 0.5f;
                cloudsSettings.cloudsRenderResolution = 1;
                cloudsSettings.reprojectionPixelSize = EnviroCloudSettings.ReprojectionPixelSize.Low;
                cloudsSettings.baseNoiseUV = 40f;
                cloudsSettings.detailNoiseUV = 70f;
                cloudsSettings.detailQuality = EnviroCloudSettings.CloudDetailQuality.Low;
                break;

            case EnviroCloudSettings.CloudQuality.VR_Low:
                cloudsSettings.bottomCloudHeight = 3000f;
                cloudsSettings.topCloudHeight = 4200f;
                cloudsSettings.cloudsWorldScale = 30000f;
                cloudsSettings.raymarchSteps = 60;
                cloudsSettings.cloudsRenderResolution = 2;
                cloudsSettings.reprojectionPixelSize = EnviroCloudSettings.ReprojectionPixelSize.Low;
                cloudsSettings.baseNoiseUV = 20f;
                cloudsSettings.detailNoiseUV = 1f;
                cloudsSettings.detailQuality = EnviroCloudSettings.CloudDetailQuality.Low;
                break;

            case EnviroCloudSettings.CloudQuality.VR_Medium:
                cloudsSettings.bottomCloudHeight = 3000f;
                cloudsSettings.topCloudHeight = 4500f;
                cloudsSettings.cloudsWorldScale = 30000f;
                cloudsSettings.raymarchSteps = 75;
                cloudsSettings.cloudsRenderResolution = 1;
                cloudsSettings.reprojectionPixelSize = EnviroCloudSettings.ReprojectionPixelSize.Medium;
                cloudsSettings.baseNoiseUV = 22f;
                cloudsSettings.detailNoiseUV = 1f;
                cloudsSettings.detailQuality = EnviroCloudSettings.CloudDetailQuality.Low;
                break;

            case EnviroCloudSettings.CloudQuality.VR_High:
                cloudsSettings.bottomCloudHeight = 3000f;
                cloudsSettings.topCloudHeight = 4500f;
                cloudsSettings.cloudsWorldScale = 30000f;
                cloudsSettings.raymarchSteps = 80;
                cloudsSettings.cloudsRenderResolution = 1;
                cloudsSettings.reprojectionPixelSize = EnviroCloudSettings.ReprojectionPixelSize.Medium;
                cloudsSettings.baseNoiseUV = 23f;
                cloudsSettings.detailNoiseUV = 1f;
                cloudsSettings.detailQuality = EnviroCloudSettings.CloudDetailQuality.Low;
                break;
        }

        lastCloudsQuality = q;
        cloudsSettings.cloudsQuality = q;
    }

    /// <summary>
    /// Get Active Weather ID
    /// </summary>
    public int GetActiveWeatherID ()
	{
		for (int i = 0; i < Weather.WeatherPrefabs.Count; i++) 
		{
			if (Weather.WeatherPrefabs [i].weatherPreset == Weather.currentActiveWeatherPreset)
				return i;
		}
		return -1;
	}

	/// <summary>
	/// Saves the current time and weather in Playerprefs.
	/// </summary>
	public void Save ()
	{
		PlayerPrefs.SetFloat("Time_Hours",internalHour);
		PlayerPrefs.SetInt("Time_Days",GameTime.Days);
		PlayerPrefs.SetInt("Time_Years",GameTime.Years);
		for (int i = 0; i < Weather.WeatherPrefabs.Count; i++) {
			if(Weather.WeatherPrefabs[i] == Weather.currentActiveWeatherPrefab)
				PlayerPrefs.SetInt("currentWeather",i);
		}
	}

	/// <summary>
	/// Loads the saved time and weather from Playerprefs.
	/// </summary>
	public void Load ()
	{
		if (PlayerPrefs.HasKey ("Time_Hours"))
			internalHour = PlayerPrefs.GetFloat ("Time_Hours");
		if (PlayerPrefs.HasKey ("Time_Days"))
			GameTime.Days = PlayerPrefs.GetInt ("Time_Days");
		if (PlayerPrefs.HasKey ("Time_Years"))
			GameTime.Years = PlayerPrefs.GetInt ("Time_Years");
		if (PlayerPrefs.HasKey ("currentWeather"))
			SetWeatherOverwrite(PlayerPrefs.GetInt("currentWeather"));
	}

	/// <summary>
	/// Set the exact date. by DateTime
	/// </summary>
	public void SetTime(DateTime date)
	{
		GameTime.Years = date.Year;
		GameTime.Days = date.DayOfYear;
		GameTime.Minutes = date.Minute;
		GameTime.Seconds = date.Second;
		GameTime.Hours = date.Hour;
		internalHour = date.Hour + (date.Minute * 0.0166667f) + (date.Second * 0.000277778f);
	}

	/// <summary>
	/// Set the exact date.
	/// </summary>
	public void SetTime(int year, int dayOfYear, int hour, int minute, int seconds)
	{
		GameTime.Years = year;
		GameTime.Days = dayOfYear;
		GameTime.Minutes = minute;
		GameTime.Hours = hour;
		internalHour = hour + (minute * 0.0166667f) + (seconds * 0.000277778f);
	}

	/// <summary>
	/// Set the time of day in hours. (12.5 = 12:30)
	/// </summary>
	public void SetInternalTimeOfDay(float inHours)
	{ 
		internalHour = inHours;
		GameTime.Hours = (int)(inHours);
		inHours -= GameTime.Hours;
		GameTime.Minutes = (int)(inHours * 60f);
		inHours -= GameTime.Minutes * 0.0166667f;
		GameTime.Seconds = (int)(inHours * 3600f);
	}

	/// <summary>
	/// Get current time in a nicely formatted string with seconds!
	/// </summary>
	/// <returns>The time string.</returns>
	public string GetTimeStringWithSeconds ()
	{
		return string.Format ("{0:00}:{1:00}:{2:00}", GameTime.Hours, GameTime.Minutes, GameTime.Seconds);
	}

	/// <summary>
	/// Get current time in a nicely formatted string!
	/// </summary>
	/// <returns>The time string.</returns>
	public string GetTimeString ()
	{
		return string.Format ("{0:00}:{1:00}", GameTime.Hours, GameTime.Minutes);
	}
    
	/// <summary>
	/// Get current time in hours. UTC0 (12.5 = 12:30)
	/// </summary>
	/// <returns>The the current time of day in hours.</returns>
	public float GetUniversalTimeOfDay()
	{
		return internalHour - GameTime.utcOffset;
    }

	/// <summary>
	/// Calculate total time in hours.
	/// </summary>
	/// <returns>The the current date in hours.</returns>
	public double GetInHours (float hours,float days, float years)
	{
		double inHours  = hours + (days * 24f) + ((years * (seasonsSettings.SpringInDays + seasonsSettings.SummerInDays + seasonsSettings.AutumnInDays + seasonsSettings.WinterInDays)) * 24f);
		return inHours;
	}

	/// <summary>
	/// Assign your Player and Camera and Initilize.////
	/// </summary>
	public void AssignAndStart (GameObject player, Camera Camera)
	{
		this.Player = player;
		PlayerCamera = Camera;
		Init ();
		started = true;
	}

	/// <summary>
	/// Assign your Player and Camera and Initilize.////
	/// </summary>
	public void StartAsServer ()
	{
		Player = gameObject;
		serverMode = true;
		Init ();
	}

	/// <summary>
	/// Changes focus on other Player or Camera on runtime.////
	/// </summary>
	/// <param name="Player">Player.</param>
	/// <param name="Camera">Camera.</param>
	public void ChangeFocus (GameObject player, Camera Camera)
	{
		this.Player = player;
		RemoveEnviroCameraComponents (PlayerCamera);
		PlayerCamera = Camera;
        InitImageEffects();
	}
	/// <summary>
	/// Destroy all enviro related camera components on this camera.
	/// </summary>
	/// <param name="cam">Cam.</param>
	private void RemoveEnviroCameraComponents (Camera cam)
	{
		EnviroFog oldFog;
		EnviroSkyRendering renderComponent;

		oldFog = cam.GetComponent<EnviroFog> ();
		if (oldFog != null)
			Destroy (oldFog);

        EnviroLightShafts[] ls = cam.GetComponents<EnviroLightShafts>();

        for (int i = 0; i < ls.Length; i++)
        {
            Destroy(ls[i]);
        }

		renderComponent = cam.GetComponent<EnviroSkyRendering> (); 
		if(renderComponent != null)
			Destroy (renderComponent);
	}

    public void Play(EnviroTime.TimeProgressMode progressMode = EnviroTime.TimeProgressMode.Simulated)
    {
        SetupSkybox();

        if (!Components.DirectLight.gameObject.activeSelf)
           Components.DirectLight.gameObject.SetActive(true);

        GameTime.ProgressTime = progressMode;
        EffectsHolder.SetActive(true);
        EnviroSkyRender.enabled = true;
        started = true;
    }

    public void Stop(bool disableLight = false, bool stopTime = true)
    {
        if(disableLight)
            Components.DirectLight.gameObject.SetActive(false);
        if (stopTime)
            GameTime.ProgressTime = EnviroTime.TimeProgressMode.None;

        EffectsHolder.SetActive(false);
        EnviroSkyRender.enabled = false;
        lightShaftsScriptSun.enabled = false;
        lightShaftsScriptMoon.enabled = false;
        started = false;
    }
}


