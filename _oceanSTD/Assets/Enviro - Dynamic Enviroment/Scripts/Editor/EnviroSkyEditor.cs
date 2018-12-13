using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;


[CustomEditor(typeof(EnviroSky))]
public class EnviroSkyEditor : Editor {

	private string latestVersion = "2.0.5";
	// GUI Styles
	private GUIStyle boxStyle;
	private GUIStyle boxStyleModified;
	private GUIStyle wrapStyle;
	private GUIStyle headerStyle;
	//Target
	private EnviroSky myTarget;
	private Color modifiedColor;
    private Color greenColor;
    private bool showWeatherMap;
	//Profile Properties
	SerializedObject serializedObj;
	SerializedProperty Sun,Moon,DirectLight,GlobalReflectionProbe, windZone, LightningGenerator, satellites, starsRotation; 
	SerializedProperty Player,Camera,PlayerTag,CameraTag, AssignOnRuntime, HDR, SatTag, MoonTag, dontDestroy;
	SerializedProperty ProgressMode,Years,Days,Hours,Minutes,Seconds,Longitude,Latitude, DayLength,NightLength,UTC, UpdateSeason, CurrentSeason;
	SerializedProperty UpdateWeather,StartWeather, EnableVolumeLighting,EnableSunShafts,EnableMoonShafts,AmbientVolume,WeatherVolume;
	SerializedProperty angleOffset,lightColorGradient, lightIntensityCurveSun,lightIntensityCurveMoon, shadowStrength, VolumeLightingResolution;
	SerializedProperty ambientMode, ambientIntensityCurve, ambientSkyGradient, ambientEquatorGradient, ambientGroundGradient;
	SerializedProperty globalReflectionsScale,reflectionBool, reflectionIntensity, reflectionUpdate;
    SerializedProperty renderMoon, blackGroundMode,galaxyCubeMap, galaxyIntensity, skyboxMode, customSkyboxMaterial, customSkyboxColor, rayleigh, g, mie, scatteringCurve, scatteringColor, sunMoonPos, sunIntensity, sunDiskScale, sunDiskIntensity, sunDiskColor, moonPhaseMode, moonTexture,moonSize, moonBrightness,moonGlow, startMoonPhase, currentMoonPhase, skyLuminance, skyColorPower, skyExposure, starsCubemap, starsIntensity,moonColor, moonGlowColor;
    SerializedProperty reprojectionPixelSize,stepsInDepthModificator, lodDistance, shadowCookieSize, flatCloudsTextureIteration, flatCloudsScale, flatCloudsResolution, flatCloudsMorphingSpeed, shadowIntensity, cloudsQuality,customWeatherMap,primaryattenuation, secondaryattenuation, weatherAnimSpeedScale, tonemapping,globalCloudCoverage,cirrusCloudsAltitude,flatCloudsAltitude,cloudsWorldScale, BottomCloudHeight,TopCloudHeight, cloudsRenderQuality,raymarchSteps, directlightIntensity,ambientlightIntensity, hgPhase,baseNoiseUV,detailNoiseUV,cloudsExposure,cirrusCloudsColor,flatCloudsColor,volumeCloudsColor,volumeCloudsMoonColor,cirrusCloudsTexture,flatCloudsTexture, weatherMapTiling,detailNoiseQuality;
    SerializedProperty useTag, wetnessAccumulationSpeed,wetnessDryingSpeed, snowAccumulationSpeed,snowMeltingSpeed,cloudTransitionSpeed,fogTransitionSpeed,effectTransitionSpeed,audioTransitionSpeed, useWindZoneDirection,windTimeScale,windIntensity,windDirectionX,windDirectionY;
	SerializedProperty useSimpleFog,fogMie, fogG, fogmode, distanceFog, useRadialFog, startDistance, distanceFogIntensity,maximumFogIntensity, heightFog, height, heightFogIntensity, useNoise, noiseIntensity, noiseIntensityOffset, noiseScale,fogDitheringScale,fogDitheringIntensity;
	SerializedProperty resolution, screenBlendMode, useDepthTexture, lightShaftsColorSun, lightShaftsColorMoon, treshholdColorSun, treshholdColorMoon, blurRadius, shaftsIntensity, maxRadius;
	SerializedProperty daysInSpring, daysInSummer, daysInAutumn,daysInWinter;
	SerializedProperty effectQuality, updateInterval, lightningEffect, lightningRange, lightningHeight;
    SerializedProperty singlePassVR, setCameraClearFlags, renderClouds;//, globalFog;
	SerializedProperty volumeLighting,SampleCount, ScatteringCoef, ExtinctionCoef, Anistropy, MaxRayLength, VolumeResolution,globalVolumeIntensity;

	ReorderableList thunderSFX;

	void OnEnable()
	{
		myTarget = (EnviroSky)target;
		serializedObj = new SerializedObject (myTarget);
		//Components
		Sun = serializedObj.FindProperty ("Components.Sun");
		Moon = serializedObj.FindProperty ("Components.Moon");
		DirectLight = serializedObj.FindProperty ("Components.DirectLight");
		GlobalReflectionProbe = serializedObj.FindProperty ("Components.GlobalReflectionProbe");
		windZone = serializedObj.FindProperty ("Components.windZone");
		LightningGenerator = serializedObj.FindProperty ("Components.LightningGenerator");
		satellites = serializedObj.FindProperty ("Components.satellites");
		starsRotation = serializedObj.FindProperty ("Components.starsRotation");

        /// Setup
        Player = serializedObj.FindProperty ("Player");
		Camera = serializedObj.FindProperty ("PlayerCamera");
		PlayerTag = serializedObj.FindProperty ("PlayerTag");
		CameraTag = serializedObj.FindProperty ("CameraTag");
		AssignOnRuntime = serializedObj.FindProperty ("AssignInRuntime");
        dontDestroy = serializedObj.FindProperty("dontDestroy");
        HDR = serializedObj.FindProperty ("HDR"); 
		MoonTag = serializedObj.FindProperty ("moonRenderingLayer"); 
		SatTag = serializedObj.FindProperty ("satelliteRenderingLayer"); 
		singlePassVR = serializedObj.FindProperty ("singlePassVR"); 
		setCameraClearFlags = serializedObj.FindProperty ("setCameraClearFlags"); 
		// Weather Controls
		UpdateWeather = serializedObj.FindProperty ("Weather.updateWeather");
		StartWeather = serializedObj.FindProperty ("Weather.startWeatherPreset");
		//Feature Controls:
		EnableVolumeLighting = serializedObj.FindProperty ("volumeLighting");
		EnableSunShafts = serializedObj.FindProperty ("LightShafts.sunLightShafts");
		EnableMoonShafts = serializedObj.FindProperty ("LightShafts.moonLightShafts");
        //globalFog = serializedObj.FindProperty("globalFog");
        // Audio Controls
        AmbientVolume = serializedObj.FindProperty ("Audio.ambientSFXVolume");
		WeatherVolume = serializedObj.FindProperty ("Audio.weatherSFXVolume");
		// Time Controls
		ProgressMode = serializedObj.FindProperty ("GameTime.ProgressTime");
		DayLength = serializedObj.FindProperty ("GameTime.DayLengthInMinutes");
		NightLength = serializedObj.FindProperty ("GameTime.NightLengthInMinutes");
		UpdateSeason = serializedObj.FindProperty ("Seasons.calcSeasons");
		CurrentSeason = serializedObj.FindProperty ("Seasons.currentSeasons");
		Years = serializedObj.FindProperty ("GameTime.Years");
		Days = serializedObj.FindProperty ("GameTime.Days");
		Hours = serializedObj.FindProperty ("GameTime.Hours");
		Minutes = serializedObj.FindProperty ("GameTime.Minutes");
		Seconds = serializedObj.FindProperty ("GameTime.Seconds");
		Longitude = serializedObj.FindProperty ("GameTime.Longitude");
		Latitude = serializedObj.FindProperty ("GameTime.Latitude");
		UTC = serializedObj.FindProperty ("GameTime.utcOffset");
		//Lighting Category
		lightColorGradient = serializedObj.FindProperty ("lightSettings.LightColor");
		lightIntensityCurveSun = serializedObj.FindProperty ("lightSettings.directLightSunIntensity");
		lightIntensityCurveMoon = serializedObj.FindProperty ("lightSettings.directLightMoonIntensity");
		shadowStrength = serializedObj.FindProperty ("lightSettings.shadowIntensity");
        angleOffset = serializedObj.FindProperty("lightSettings.directLightAngleOffset");
        ambientMode = serializedObj.FindProperty ("lightSettings.ambientMode");
		ambientIntensityCurve = serializedObj.FindProperty ("lightSettings.ambientIntensity");
		ambientSkyGradient = serializedObj.FindProperty ("lightSettings.ambientSkyColor");
		ambientEquatorGradient = serializedObj.FindProperty ("lightSettings.ambientEquatorColor");
		ambientGroundGradient = serializedObj.FindProperty ("lightSettings.ambientGroundColor");
		reflectionBool = serializedObj.FindProperty ("lightSettings.globalReflections");
		reflectionIntensity = serializedObj.FindProperty ("lightSettings.globalReflectionsIntensity");
		reflectionUpdate = serializedObj.FindProperty ("lightSettings.globalReflectionsUpdate");
        globalReflectionsScale = serializedObj.FindProperty("lightSettings.globalReflectionsScale");
        //Volume Lighting
        VolumeLightingResolution = serializedObj.FindProperty("volumeLightSettings.Resolution");
        volumeLighting = serializedObj.FindProperty ("volumeLightSettings.dirVolumeLighting");
		SampleCount = serializedObj.FindProperty ("volumeLightSettings.SampleCount");
		ScatteringCoef = serializedObj.FindProperty ("volumeLightSettings.ScatteringCoef");
		ExtinctionCoef = serializedObj.FindProperty ("volumeLightSettings.ExtinctionCoef");
		Anistropy = serializedObj.FindProperty ("volumeLightSettings.Anistropy");
		MaxRayLength = serializedObj.FindProperty ("volumeLightSettings.MaxRayLength");
		useNoise = serializedObj.FindProperty ("volumeLightSettings.directLightNoise"); 
		noiseIntensity = serializedObj.FindProperty ("volumeLightSettings.noiseIntensity");
		noiseScale = serializedObj.FindProperty ("volumeLightSettings.noiseScale");
		noiseIntensityOffset = serializedObj.FindProperty ("volumeLightSettings.noiseIntensityOffset");
		//Sky Category
		skyboxMode = serializedObj.FindProperty ("skySettings.skyboxMode");
		customSkyboxMaterial = serializedObj.FindProperty ("skySettings.customSkyboxMaterial");
		customSkyboxColor = serializedObj.FindProperty ("skySettings.customSkyboxColor");
        blackGroundMode = serializedObj.FindProperty("skySettings.blackGroundMode");
        rayleigh = serializedObj.FindProperty ("skySettings.rayleigh");
		g = serializedObj.FindProperty ("skySettings.g");
		mie = serializedObj.FindProperty ("skySettings.mie");
		scatteringCurve = serializedObj.FindProperty ("skySettings.scatteringCurve");
		scatteringColor = serializedObj.FindProperty ("skySettings.scatteringColor");
		sunMoonPos = serializedObj.FindProperty ("skySettings.sunAndMoonPosition");
		sunIntensity = serializedObj.FindProperty ("skySettings.sunIntensity");
		sunDiskScale = serializedObj.FindProperty ("skySettings.sunDiskScale");
		sunDiskIntensity = serializedObj.FindProperty ("skySettings.sunDiskIntensity");
		sunDiskColor = serializedObj.FindProperty ("skySettings.sunDiskColor");
        renderMoon = serializedObj.FindProperty("skySettings.renderMoon");
        moonPhaseMode = serializedObj.FindProperty ("skySettings.moonPhaseMode");
		moonTexture = serializedObj.FindProperty ("skySettings.moonTexture");
		moonBrightness = serializedObj.FindProperty ("skySettings.moonBrightness");
		moonGlow = serializedObj.FindProperty ("skySettings.moonGlow");
		startMoonPhase = serializedObj.FindProperty ("skySettings.startMoonPhase");
		currentMoonPhase = serializedObj.FindProperty ("customMoonPhase");
		skyLuminance = serializedObj.FindProperty ("skySettings.skyLuminence");
		skyColorPower = serializedObj.FindProperty ("skySettings.skyColorPower");
		skyExposure = serializedObj.FindProperty ("skySettings.skyExposure");
		starsCubemap = serializedObj.FindProperty ("skySettings.starsCubeMap");
		starsIntensity = serializedObj.FindProperty ("skySettings.starsIntensity");
		moonGlowColor = serializedObj.FindProperty ("skySettings.moonGlowColor");
		moonColor = serializedObj.FindProperty ("skySettings.moonColor");
		moonSize = serializedObj.FindProperty ("skySettings.moonSize");
        //skyNoiseScale = serializedObj.FindProperty("skySettings.noiseScale");
       // skyNoiseIntensity = serializedObj.FindProperty("skySettings.noiseIntensity");

        galaxyCubeMap = serializedObj.FindProperty("skySettings.galaxyCubeMap");
        galaxyIntensity = serializedObj.FindProperty("skySettings.galaxyIntensity");
        //Clouds Category
        cloudsWorldScale = serializedObj.FindProperty ("cloudsSettings.cloudsWorldScale");
		BottomCloudHeight = serializedObj.FindProperty ("cloudsSettings.bottomCloudHeight");
		TopCloudHeight = serializedObj.FindProperty ("cloudsSettings.topCloudHeight");
		weatherAnimSpeedScale = serializedObj.FindProperty ("cloudsSettings.weatherAnimSpeedScale");
		cloudsRenderQuality = serializedObj.FindProperty ("cloudsSettings.cloudsRenderResolution");
		raymarchSteps = serializedObj.FindProperty ("cloudsSettings.raymarchSteps");
        stepsInDepthModificator = serializedObj.FindProperty("cloudsSettings.stepsInDepthModificator");
        reprojectionPixelSize = serializedObj.FindProperty("cloudsSettings.reprojectionPixelSize");
        hgPhase = serializedObj.FindProperty ("cloudsSettings.hgPhase");
		detailNoiseUV = serializedObj.FindProperty ("cloudsSettings.detailNoiseUV");
		baseNoiseUV = serializedObj.FindProperty ("cloudsSettings.baseNoiseUV");
		weatherMapTiling = serializedObj.FindProperty ("cloudsSettings.weatherMapTiling");
		detailNoiseQuality = serializedObj.FindProperty ("cloudsSettings.detailQuality");
		volumeCloudsColor = serializedObj.FindProperty ("cloudsSettings.volumeCloudsColor");
		directlightIntensity = serializedObj.FindProperty ("cloudsSettings.directLightIntensity");
		ambientlightIntensity = serializedObj.FindProperty ("cloudsSettings.ambientLightIntensity");
		cloudsExposure = serializedObj.FindProperty ("cloudsSettings.cloudsExposure");
		volumeCloudsMoonColor = serializedObj.FindProperty ("cloudsSettings.volumeCloudsMoonColor");
        lodDistance = serializedObj.FindProperty("cloudsSettings.lodDistance");
        cirrusCloudsTexture = serializedObj.FindProperty ("cloudsSettings.cirrusCloudsTexture");
		cirrusCloudsAltitude = serializedObj.FindProperty ("cloudsSettings.cirrusCloudsAltitude");
		cirrusCloudsColor = serializedObj.FindProperty ("cloudsSettings.cirrusCloudsColor"); 
		flatCloudsTexture = serializedObj.FindProperty ("cloudsSettings.flatCloudsNoiseTexture");
		flatCloudsAltitude = serializedObj.FindProperty ("cloudsSettings.flatCloudsAltitude");
		flatCloudsColor = serializedObj.FindProperty ("cloudsSettings.flatCloudsColor"); 
		globalCloudCoverage = serializedObj.FindProperty ("cloudsSettings.globalCloudCoverage"); 
		tonemapping = serializedObj.FindProperty ("cloudsSettings.tonemapping");
        primaryattenuation = serializedObj.FindProperty("cloudsSettings.primaryAttenuation");
        customWeatherMap = serializedObj.FindProperty("cloudsSettings.customWeatherMap");
        cloudsQuality = serializedObj.FindProperty("cloudsSettings.cloudsQuality");
        secondaryattenuation = serializedObj.FindProperty("cloudsSettings.secondaryAttenuation");
        shadowIntensity = serializedObj.FindProperty("cloudsSettings.shadowIntensity");
        shadowCookieSize = serializedObj.FindProperty("cloudsSettings.shadowCookieSize");
        flatCloudsResolution = serializedObj.FindProperty("cloudsSettings.flatCloudsResolution");
        flatCloudsScale = serializedObj.FindProperty("cloudsSettings.flatCloudsScale");
        flatCloudsTextureIteration = serializedObj.FindProperty("cloudsSettings.flatCloudsNoiseOctaves");
        flatCloudsMorphingSpeed = serializedObj.FindProperty("cloudsSettings.flatCloudsMorphingSpeed");
        // Weather Category
        useTag = serializedObj.FindProperty ("weatherSettings.useTag");
		wetnessAccumulationSpeed = serializedObj.FindProperty ("weatherSettings.wetnessAccumulationSpeed");
		wetnessDryingSpeed = serializedObj.FindProperty ("weatherSettings.wetnessDryingSpeed");
		snowAccumulationSpeed = serializedObj.FindProperty ("weatherSettings.snowAccumulationSpeed");
		snowMeltingSpeed = serializedObj.FindProperty ("weatherSettings.snowMeltingSpeed");
		cloudTransitionSpeed = serializedObj.FindProperty ("weatherSettings.cloudTransitionSpeed");
		fogTransitionSpeed = serializedObj.FindProperty ("weatherSettings.fogTransitionSpeed");
		effectTransitionSpeed = serializedObj.FindProperty ("weatherSettings.effectTransitionSpeed");
		audioTransitionSpeed = serializedObj.FindProperty ("weatherSettings.audioTransitionSpeed");
        lightningEffect = serializedObj.FindProperty("weatherSettings.lightningEffect");
        lightningRange = serializedObj.FindProperty("weatherSettings.lightningRange");
        lightningHeight = serializedObj.FindProperty("weatherSettings.lightningHeight");

        useWindZoneDirection = serializedObj.FindProperty ("cloudsSettings.useWindZoneDirection");
		renderClouds = serializedObj.FindProperty ("cloudsMode");
		windTimeScale = serializedObj.FindProperty ("cloudsSettings.cloudsTimeScale");
		windIntensity = serializedObj.FindProperty ("cloudsSettings.cloudsWindStrengthModificator");
		windDirectionX = serializedObj.FindProperty ("cloudsSettings.cloudsWindDirectionX");
		windDirectionY = serializedObj.FindProperty ("cloudsSettings.cloudsWindDirectionY");
		fogmode = serializedObj.FindProperty ("fogSettings.Fogmode");
		distanceFog = serializedObj.FindProperty ("fogSettings.distanceFog");
		useRadialFog = serializedObj.FindProperty ("fogSettings.useRadialDistance");
		startDistance = serializedObj.FindProperty ("fogSettings.startDistance");
		distanceFogIntensity = serializedObj.FindProperty ("fogSettings.distanceFogIntensity");
		maximumFogIntensity = serializedObj.FindProperty ("fogSettings.maximumFogDensity");
		heightFog = serializedObj.FindProperty ("fogSettings.heightFog");
		height = serializedObj.FindProperty ("fogSettings.height");
		heightFogIntensity = serializedObj.FindProperty ("fogSettings.heightFogIntensity");

        noiseIntensity = serializedObj.FindProperty("fogSettings.noiseIntensity");
        noiseIntensityOffset = serializedObj.FindProperty("fogSettings.noiseIntensityOffset");
        noiseScale = serializedObj.FindProperty("fogSettings.noiseScale");

        fogDitheringScale = serializedObj.FindProperty ("fogSettings.fogDitheringScale");
		fogDitheringIntensity = serializedObj.FindProperty ("fogSettings.fogDitheringIntensity");
		fogMie = serializedObj.FindProperty ("fogSettings.mie"); 
		fogG = serializedObj.FindProperty ("fogSettings.g");
        useSimpleFog = serializedObj.FindProperty("fogSettings.useSimpleFog");

        //LightShafts
        resolution = serializedObj.FindProperty ("lightshaftsSettings.resolution");
		screenBlendMode = serializedObj.FindProperty ("lightshaftsSettings.screenBlendMode");
		useDepthTexture = serializedObj.FindProperty ("lightshaftsSettings.useDepthTexture");
		lightShaftsColorSun = serializedObj.FindProperty ("lightshaftsSettings.lightShaftsColorSun");
		lightShaftsColorMoon = serializedObj.FindProperty ("lightshaftsSettings.lightShaftsColorMoon");
		treshholdColorSun = serializedObj.FindProperty ("lightshaftsSettings.thresholdColorSun");
		treshholdColorMoon = serializedObj.FindProperty ("lightshaftsSettings.thresholdColorMoon");
		blurRadius = serializedObj.FindProperty ("lightshaftsSettings.blurRadius");
		shaftsIntensity = serializedObj.FindProperty ("lightshaftsSettings.intensity");
		maxRadius = serializedObj.FindProperty ("lightshaftsSettings.maxRadius");
		//Season
		daysInSpring = serializedObj.FindProperty ("seasonsSettings.SpringInDays");
		daysInSummer = serializedObj.FindProperty ("seasonsSettings.SummerInDays");
		daysInAutumn = serializedObj.FindProperty ("seasonsSettings.AutumnInDays");
		daysInWinter = serializedObj.FindProperty ("seasonsSettings.WinterInDays");
		//Quality
		effectQuality= serializedObj.FindProperty ("qualitySettings.GlobalParticleEmissionRates");
		updateInterval= serializedObj.FindProperty ("qualitySettings.UpdateInterval");
		//Audio
		thunderSFX = new ReorderableList(serializedObject, 
			serializedObject.FindProperty("audioSettings.ThunderSFX"), 
			true, true, true, true);

		thunderSFX.drawHeaderCallback = (Rect rect) =>
		{
			EditorGUI.LabelField(rect, "Thunder SFX");
		};
		thunderSFX.drawElementCallback =
			(Rect rect, int index, bool isActive, bool isFocused) =>
		{
			var element = thunderSFX.serializedProperty.GetArrayElementAtIndex(index);
			rect.y += 2;
			EditorGUI.PropertyField(
				new Rect(rect.x, rect.y, Screen.width*.8f, EditorGUIUtility.singleLineHeight),
				element, GUIContent.none);
		};
		thunderSFX.onAddCallback = (ReorderableList l) =>
		{
			var index = l.serializedProperty.arraySize;
			l.serializedProperty.arraySize++;
			l.index = index;
			//var element = l.serializedProperty.GetArrayElementAtIndex(index);
		};

		modifiedColor = Color.red;
		modifiedColor.a = 0.5f;

        greenColor = Color.green;
        greenColor.a = 0.5f;
        ////
    }
	/// <summary>
	/// Applies the changes and set profile to modifed but not saved.
	/// </summary>
	private void ApplyChanges ()
	{
		if (EditorGUI.EndChangeCheck ()) {
			serializedObj.ApplyModifiedProperties ();
			myTarget.profile.modified = true;
		}
	}

	public override void OnInspectorGUI ()
	{
		myTarget = (EnviroSky)target;
		//int daysInyear = (int)(myTarget.seasonsSettings.SpringInDays + myTarget.seasonsSettings.SummerInDays + myTarget.seasonsSettings.AutumnInDays + myTarget.seasonsSettings.WinterInDays);
		//Set up the box style
		if (boxStyle == null)
		{
			boxStyle = new GUIStyle(GUI.skin.box);
			boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
			boxStyle.fontStyle = FontStyle.Bold;
			boxStyle.alignment = TextAnchor.UpperLeft;
		}

		if (boxStyleModified == null)
		{
			boxStyleModified = new GUIStyle(GUI.skin.box);
			boxStyleModified.normal.textColor = GUI.skin.label.normal.textColor;
			boxStyleModified.fontStyle = FontStyle.Bold;
			boxStyleModified.alignment = TextAnchor.UpperLeft;
		}

		//Setup the wrap style
		if (wrapStyle == null)
		{
			wrapStyle = new GUIStyle(GUI.skin.label);
			wrapStyle.fontStyle = FontStyle.Bold;
			wrapStyle.wordWrap = true;
		}

		if (headerStyle == null) {
			headerStyle = new GUIStyle(GUI.skin.label);
			headerStyle.fontStyle = FontStyle.Bold;
			headerStyle.alignment = TextAnchor.UpperLeft;
		}

		GUILayout.BeginVertical("Enviro - Sky and Weather " + latestVersion, boxStyle);
		GUILayout.Space(20);
		GUILayout.BeginVertical("Profile", boxStyle);
		GUILayout.Space(20);
		myTarget.profile = (EnviroProfile)EditorGUILayout.ObjectField (myTarget.profile, typeof(EnviroProfile), false);
		GUILayout.Space(10);
		if (myTarget.profile != null)
			EditorGUILayout.LabelField ("Profile Version:", myTarget.profile.version);
		else
			EditorGUILayout.LabelField ("No Profile Assigned!");
		EditorGUILayout.LabelField ("Prefab Version:", myTarget.prefabVersion);
		GUILayout.Space(10);

		if(myTarget.profile.version != latestVersion)
		if (GUILayout.Button ("Update Profile")) {
			if (EnviroProfileCreation.UpdateProfile (myTarget.profile, myTarget.profile.version, latestVersion) == true)
				myTarget.ApplyProfile (myTarget.profile);
		}
		// Runtime Settings
		if (GUILayout.Button ("Apply all Settings")) {
			myTarget.enabled = false;
			myTarget.enabled = true;
		}
		GUILayout.EndHorizontal ();
		if (myTarget.profile != null) {
			if (myTarget.profile.modified) // Change color when modified
				GUI.backgroundColor = modifiedColor;
			GUILayout.BeginVertical ("", boxStyle);
			if(myTarget.profile.modified)
				GUI.backgroundColor = Color.white;
			#if UNITY_5_6_OR_NEWER
			serializedObj.UpdateIfRequiredOrScript ();
			#else
			serializedObj.UpdateIfDirtyOrScript ();
			#endif
			myTarget.showSettings = EditorGUILayout.BeginToggleGroup (" Edit Profile", myTarget.showSettings);
			if (myTarget.showSettings) {
				GUILayout.BeginVertical ("", boxStyle);
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Save to Profile")) {
					myTarget.SaveProfile ();
					myTarget.profile.modified = false;
				}
				if (GUILayout.Button ("Load from Profile")) {
					myTarget.ApplyProfile (myTarget.profile);
					myTarget.profile.modified = false;
					#if UNITY_5_6_OR_NEWER
					serializedObj.UpdateIfRequiredOrScript ();
					#else
					serializedObj.UpdateIfDirtyOrScript ();
					#endif
				}
				GUILayout.EndHorizontal ();
				GUILayout.Space (10);
				EditorGUILayout.LabelField ("Category", headerStyle);
				myTarget.profile.viewMode =	(EnviroProfile.settingsMode)EditorGUILayout.EnumPopup (myTarget.profile.viewMode);
				GUILayout.EndVertical ();

				switch (myTarget.profile.viewMode) {
				case EnviroProfile.settingsMode.Lighting:
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (lightColorGradient, true, null);
					EditorGUILayout.PropertyField (lightIntensityCurveSun, true, null);
					EditorGUILayout.PropertyField (lightIntensityCurveMoon, true, null);
                    EditorGUILayout.PropertyField(shadowStrength, true, null);
                    EditorGUILayout.PropertyField(angleOffset, true, null);
                    EditorGUILayout.PropertyField (ambientMode, true, null);
					EditorGUILayout.PropertyField (ambientIntensityCurve, true, null);
					EditorGUILayout.PropertyField (ambientSkyGradient, true, null);
					EditorGUILayout.PropertyField (ambientEquatorGradient, true, null);
					EditorGUILayout.PropertyField (ambientGroundGradient, true, null);
					EditorGUILayout.PropertyField (reflectionBool, true, null);
					EditorGUILayout.PropertyField (reflectionIntensity, true, null);
					EditorGUILayout.PropertyField (reflectionUpdate, true, null);
                    EditorGUILayout.PropertyField(globalReflectionsScale, true, null);
                    ApplyChanges ();
					break;
				case EnviroProfile.settingsMode.Sky:
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (skyboxMode, true, null);
					EditorGUILayout.PropertyField (customSkyboxMaterial, true, null);
					EditorGUILayout.PropertyField (customSkyboxColor, true, null);
                    EditorGUILayout.PropertyField(blackGroundMode, true, null);
                    GUILayout.Space (10);
					EditorGUILayout.LabelField ("Scattering", headerStyle);
					myTarget.skySettings.waveLength = EditorGUILayout.Vector3Field ("Wave Length", myTarget.skySettings.waveLength);
					EditorGUILayout.PropertyField (rayleigh, true, null);
					EditorGUILayout.PropertyField (g, true, null);
					EditorGUILayout.PropertyField (mie, true, null);
					EditorGUILayout.PropertyField (scatteringCurve, true, null);
					EditorGUILayout.PropertyField (scatteringColor, true, null);
					EditorGUILayout.PropertyField (sunMoonPos, true, null);
					EditorGUILayout.PropertyField (sunIntensity, true, null);
					EditorGUILayout.PropertyField (sunDiskScale, true, null);
					EditorGUILayout.PropertyField (sunDiskIntensity, true, null);
					EditorGUILayout.PropertyField (sunDiskColor, true, null);       
                    EditorGUILayout.PropertyField(renderMoon, true, null);
                    EditorGUILayout.PropertyField (moonPhaseMode, true, null);
					EditorGUILayout.PropertyField (moonTexture, true, null);
					EditorGUILayout.PropertyField (moonColor, true, null);
					EditorGUILayout.PropertyField (moonBrightness, true, null);
					EditorGUILayout.PropertyField (moonSize, true, null);
					EditorGUILayout.PropertyField (moonGlow, true, null);
					EditorGUILayout.PropertyField (moonGlowColor, true, null);
					if (myTarget.skySettings.moonPhaseMode == EnviroSkySettings.MoonPhases.Custom) {
						EditorGUILayout.PropertyField (startMoonPhase, true, null);
						EditorGUILayout.PropertyField (currentMoonPhase, true, null);
					}
					EditorGUILayout.PropertyField (skyLuminance, true, null);
					EditorGUILayout.PropertyField (skyColorPower, true, null);
					EditorGUILayout.PropertyField (skyExposure, true, null);
					EditorGUILayout.PropertyField (starsCubemap, true, null);
					EditorGUILayout.PropertyField (starsIntensity, true, null);
                    EditorGUILayout.PropertyField(galaxyCubeMap, true, null);
                    EditorGUILayout.PropertyField(galaxyIntensity, true, null);
                        //  EditorGUILayout.PropertyField(skyNoiseScale, true, null);
                        //  EditorGUILayout.PropertyField(skyNoiseIntensity, true, null);
                        ApplyChanges ();
					break;
					// CLouds Category
				case EnviroProfile.settingsMode.Clouds:	
					EditorGUI.BeginChangeCheck ();
					GUILayout.BeginVertical ("Volume Clouds", boxStyle);
					GUILayout.Space (20);
                        
                    EditorGUILayout.PropertyField(cloudsQuality, true, null);
                    EditorGUILayout.PropertyField(lodDistance, true, null);
                        if (myTarget.cloudsSettings.cloudsQuality == EnviroCloudSettings.CloudQuality.Custom)
                        {
                            GUILayout.BeginVertical("", boxStyle);
                            EditorGUILayout.PropertyField(cloudsWorldScale, true, null);
                            EditorGUILayout.PropertyField(BottomCloudHeight, true, null);
                            EditorGUILayout.PropertyField(TopCloudHeight, true, null);

                            EditorGUILayout.PropertyField(raymarchSteps, true, null);
                            
                            EditorGUILayout.PropertyField(stepsInDepthModificator, true, null);
                            
                            EditorGUILayout.PropertyField(cloudsRenderQuality, true, null);
                            EditorGUILayout.PropertyField(reprojectionPixelSize, true, null);

                            EditorGUILayout.PropertyField(baseNoiseUV, true, null);
                            EditorGUILayout.PropertyField(detailNoiseUV, true, null);
                            EditorGUILayout.PropertyField(detailNoiseQuality, true, null);
                            EditorGUILayout.EndVertical();
                        }
                        GUILayout.BeginVertical("", boxStyle);
                        EditorGUILayout.PropertyField(hgPhase, true, null);
                        EditorGUILayout.PropertyField(primaryattenuation, true, null);
                        EditorGUILayout.PropertyField(secondaryattenuation, true, null);
                        EditorGUILayout.PropertyField(volumeCloudsColor, true, null);
                        EditorGUILayout.PropertyField(volumeCloudsMoonColor, true, null);
                        EditorGUILayout.PropertyField(directlightIntensity, true, null);
                        EditorGUILayout.PropertyField(ambientlightIntensity, true, null);
                        
                        EditorGUILayout.PropertyField(tonemapping, true, null);
                        EditorGUILayout.PropertyField(cloudsExposure, true, null);
                        EditorGUILayout.PropertyField (weatherMapTiling, true, null);
                        EditorGUILayout.PropertyField(customWeatherMap, true, null);
                        
                     myTarget.cloudsSettings.locationOffset = EditorGUILayout.Vector2Field("Location Offset", myTarget.cloudsSettings.locationOffset);
                        EditorGUILayout.PropertyField (weatherAnimSpeedScale, true, null);

					showWeatherMap = EditorGUILayout.BeginToggleGroup ("Show Weather Map Preview", showWeatherMap);
					if (myTarget.weatherMap != null && showWeatherMap)
						EditorGUI.DrawPreviewTexture (GUILayoutUtility.GetAspectRect (1f), myTarget.weatherMap);
					EditorGUILayout.EndToggleGroup ();

					EditorGUILayout.PropertyField (globalCloudCoverage, true, null);
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndVertical ();

                    GUILayout.BeginVertical("Clouds Shadows", boxStyle);
                    GUILayout.Space(20);
                    EditorGUILayout.PropertyField(shadowIntensity, true, null);
                    EditorGUILayout.PropertyField(shadowCookieSize, true, null);
                    EditorGUILayout.EndVertical();
                    GUILayout.BeginVertical ("Cirrus Clouds", boxStyle);
					GUILayout.Space (20);
					EditorGUILayout.PropertyField (cirrusCloudsTexture, true, null);
					EditorGUILayout.PropertyField (cirrusCloudsColor, true, null);
					EditorGUILayout.PropertyField (cirrusCloudsAltitude, true, null);
                    
                    EditorGUILayout.EndVertical ();
					GUILayout.BeginVertical ("Flat Clouds", boxStyle);
					GUILayout.Space (20);
            
                    EditorGUILayout.PropertyField(flatCloudsTexture, true, null);
                    EditorGUILayout.PropertyField (flatCloudsResolution, true, null);
                    EditorGUILayout.PropertyField(flatCloudsTextureIteration, true, null);
                    EditorGUILayout.PropertyField (flatCloudsColor, true, null);
                    EditorGUILayout.PropertyField(flatCloudsScale, true, null);
                    EditorGUILayout.PropertyField (flatCloudsAltitude, true, null);
                        EditorGUILayout.PropertyField(flatCloudsMorphingSpeed, true, null);
                        EditorGUILayout.EndVertical ();

					EditorGUILayout.PropertyField (useWindZoneDirection, true, null);
					EditorGUILayout.PropertyField (windTimeScale, true, null);
					EditorGUILayout.PropertyField (windIntensity, true, null);
					if (useWindZoneDirection.boolValue == false) {
						EditorGUILayout.PropertyField (windDirectionX, true, null);
						EditorGUILayout.PropertyField (windDirectionY, true, null);
					}
					ApplyChanges ();
					break;

				case EnviroProfile.settingsMode.Weather:
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (useTag, true, null);
					EditorGUILayout.PropertyField (wetnessAccumulationSpeed, true, null);
					EditorGUILayout.PropertyField (wetnessDryingSpeed, true, null);
					EditorGUILayout.PropertyField (snowAccumulationSpeed, true, null);
					EditorGUILayout.PropertyField (snowMeltingSpeed, true, null);
					GUILayout.Space (10);
					EditorGUILayout.PropertyField (cloudTransitionSpeed, true, null);
					EditorGUILayout.PropertyField (fogTransitionSpeed, true, null);
					EditorGUILayout.PropertyField (effectTransitionSpeed, true, null);
					EditorGUILayout.PropertyField (audioTransitionSpeed, true, null);
                    EditorGUILayout.PropertyField(lightningEffect, true, null);
                    EditorGUILayout.PropertyField(lightningRange, true, null);
                    EditorGUILayout.PropertyField(lightningHeight, true, null);
                    ApplyChanges ();
					break;

				case EnviroProfile.settingsMode.Season:
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (daysInSpring, true, null);
					EditorGUILayout.PropertyField (daysInSummer, true, null);
					EditorGUILayout.PropertyField (daysInAutumn, true, null);
					EditorGUILayout.PropertyField (daysInWinter, true, null);
					ApplyChanges ();
					break;

				case EnviroProfile.settingsMode.Fog:
					EditorGUI.BeginChangeCheck ();
                    EditorGUILayout.PropertyField(useSimpleFog, true, null);
                    EditorGUILayout.PropertyField (fogmode, true, null);
					EditorGUILayout.PropertyField (distanceFog, true, null);
					EditorGUILayout.PropertyField (useRadialFog, true, null);
					EditorGUILayout.PropertyField (startDistance, true, null);
					EditorGUILayout.PropertyField (distanceFogIntensity, true, null);
					EditorGUILayout.PropertyField (maximumFogIntensity, true, null);
					EditorGUILayout.PropertyField (heightFog, true, null);
					EditorGUILayout.PropertyField (height, true, null);
					EditorGUILayout.PropertyField (heightFogIntensity, true, null);
                    if (!myTarget.fogSettings.useSimpleFog)
                    {
                        EditorGUILayout.PropertyField(noiseIntensity, true, null);
                        EditorGUILayout.PropertyField(noiseIntensityOffset, true, null);
                        EditorGUILayout.PropertyField(noiseScale, true, null);
                        myTarget.fogSettings.noiseVelocity = EditorGUILayout.Vector2Field("Noise Velocity", myTarget.fogSettings.noiseVelocity);
                        EditorGUILayout.PropertyField(fogMie, true, null);
                        EditorGUILayout.PropertyField(fogG, true, null);
                    }
					EditorGUILayout.PropertyField (fogDitheringScale, true, null);
					EditorGUILayout.PropertyField (fogDitheringIntensity, true, null);
					ApplyChanges ();
					break;


                    case EnviroProfile.settingsMode.VolumeLighting:
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(VolumeLightingResolution, true, null);
                        EditorGUILayout.LabelField("Directional Volume Lighting", headerStyle);
                        EditorGUILayout.PropertyField(volumeLighting, true, null);
                        EditorGUILayout.PropertyField(SampleCount, true, null);
                        EditorGUILayout.PropertyField(ScatteringCoef, true, null);
                        EditorGUILayout.PropertyField(ExtinctionCoef, true, null);
                        EditorGUILayout.PropertyField(Anistropy, true, null);
                        EditorGUILayout.PropertyField(MaxRayLength, true, null);
                        EditorGUILayout.PropertyField(useNoise, true, null);
                        EditorGUILayout.PropertyField(noiseIntensity, true, null);
                        EditorGUILayout.PropertyField(noiseIntensityOffset, true, null);
                        EditorGUILayout.PropertyField(noiseScale, true, null);
                        myTarget.volumeLightSettings.noiseVelocity = EditorGUILayout.Vector2Field("Noise Velocity", myTarget.volumeLightSettings.noiseVelocity);
                        ApplyChanges();
                    break;

                    case EnviroProfile.settingsMode.Lightshafts:
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (resolution, true, null);
					EditorGUILayout.PropertyField (screenBlendMode, true, null);
					EditorGUILayout.PropertyField (useDepthTexture, true, null);
					EditorGUILayout.PropertyField (lightShaftsColorSun, true, null);
					EditorGUILayout.PropertyField (lightShaftsColorMoon, true, null);
					EditorGUILayout.PropertyField (treshholdColorSun, true, null);
					EditorGUILayout.PropertyField (treshholdColorMoon, true, null);
					EditorGUILayout.PropertyField (blurRadius, true, null);
					EditorGUILayout.PropertyField (shaftsIntensity, true, null);
					EditorGUILayout.PropertyField (maxRadius, true, null);
					ApplyChanges ();
					break;

				case EnviroProfile.settingsMode.Audio:
					myTarget.Audio.SFXHolderPrefab = (GameObject)EditorGUILayout.ObjectField ("SFX Prefab:", myTarget.Audio.SFXHolderPrefab, typeof(GameObject), false);
					serializedObject.Update ();
					thunderSFX.DoLayoutList ();
					serializedObject.ApplyModifiedProperties ();
					break;

				case EnviroProfile.settingsMode.Satellites:
					GUILayout.BeginVertical (" Layer Setup", boxStyle);
					GUILayout.Space (20);
                    if (GUILayout.Button ("Add Satellite")) {
				    myTarget.satelliteSettings.additionalSatellites.Add (new EnviroSatellite ());
					}

					if (GUILayout.Button ("Apply Changes")) {
						myTarget.CheckSatellites ();
					}
					for (int i = 0; i < myTarget.satelliteSettings.additionalSatellites.Count; i++) {
						GUILayout.BeginVertical ("", boxStyle);
						GUILayout.Space (10);
						myTarget.satelliteSettings.additionalSatellites [i].name = EditorGUILayout.TextField ("Name", myTarget.satelliteSettings.additionalSatellites [i].name);
						GUILayout.Space (10);
						myTarget.satelliteSettings.additionalSatellites [i].prefab = (GameObject)EditorGUILayout.ObjectField ("Prefab", myTarget.satelliteSettings.additionalSatellites [i].prefab, typeof(GameObject), false);
						myTarget.satelliteSettings.additionalSatellites [i].orbit = EditorGUILayout.Slider ("OrbitDistance", myTarget.satelliteSettings.additionalSatellites [i].orbit,0f,myTarget.transform.localScale.y);
						myTarget.satelliteSettings.additionalSatellites [i].xRot = EditorGUILayout.Slider ("XRot", myTarget.satelliteSettings.additionalSatellites [i].xRot,0f,360f);
						myTarget.satelliteSettings.additionalSatellites [i].yRot = EditorGUILayout.Slider ("YRot", myTarget.satelliteSettings.additionalSatellites [i].yRot,0f,360f);
						if (GUILayout.Button ("Remove")) 
						{
							myTarget.satelliteSettings.additionalSatellites.Remove (myTarget.satelliteSettings.additionalSatellites [i]);
							myTarget.CheckSatellites ();
						}
						GUILayout.EndVertical ();
					}
					serializedObj.Update ();
					GUILayout.EndVertical ();
					break;

				case EnviroProfile.settingsMode.Quality:
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (effectQuality, true, null);
					EditorGUILayout.PropertyField (updateInterval, true, null);
					ApplyChanges ();
					break;
				}
			}
			GUILayout.EndVertical ();
			EditorGUILayout.EndToggleGroup ();
		}
		GUILayout.EndVertical ();

		if (myTarget.profile != null) {
			EditorGUI.BeginChangeCheck ();
			// Begin Setup
			GUILayout.BeginVertical ("", boxStyle);
            // Player Setup
            if ((myTarget.Player == null || myTarget.PlayerCamera == null) && !myTarget.AssignInRuntime)
                GUI.backgroundColor = modifiedColor;
            else if ((myTarget.Player != null && myTarget.PlayerCamera != null) || myTarget.AssignInRuntime)
                GUI.backgroundColor = greenColor;

            GUILayout.BeginVertical ("", boxStyle);
            GUI.backgroundColor = Color.white;
            myTarget.profile.showPlayerSetup = EditorGUILayout.BeginToggleGroup ("Player & Camera Setup", myTarget.profile.showPlayerSetup);
			if (myTarget.profile.showPlayerSetup) {
				GUILayout.Space (20);
				EditorGUILayout.PropertyField (Player, true, null);
				EditorGUILayout.PropertyField (Camera, true, null);
				GUILayout.Space (20);
				AssignOnRuntime.boolValue = EditorGUILayout.BeginToggleGroup ("Assign On Runtime", AssignOnRuntime.boolValue);
				PlayerTag.stringValue = EditorGUILayout.TagField ("Player Tag", PlayerTag.stringValue);
				CameraTag.stringValue = EditorGUILayout.TagField ("Camera Tag", CameraTag.stringValue);
				EditorGUILayout.EndToggleGroup ();
			}
			EditorGUILayout.EndToggleGroup ();
			GUILayout.EndVertical ();
          

            /// Render Setup
            GUILayout.BeginVertical ("", boxStyle);
			myTarget.profile.showRenderingSetup = EditorGUILayout.BeginToggleGroup ("Rendering Setup", myTarget.profile.showRenderingSetup);
			if (myTarget.profile.showRenderingSetup) {
                EditorGUILayout.PropertyField(dontDestroy, true, null);
                EditorGUILayout.PropertyField (HDR, true, null);
				EditorGUILayout.PropertyField (setCameraClearFlags, true, null);
				GUILayout.Space (10);
				EditorGUILayout.LabelField ("Layer Setup",headerStyle,null);
				MoonTag.intValue = EditorGUILayout.LayerField ("Moon Layer", MoonTag.intValue);
                //SkyTag.intValue = EditorGUILayout.LayerField ("Satellite Layer", SkyTag.intValue);
                SatTag.intValue = EditorGUILayout.LayerField("Satellites Layers", SatTag.intValue);
                EditorGUILayout.PropertyField (singlePassVR, true, null);
			}
			EditorGUILayout.EndToggleGroup ();
			GUILayout.EndVertical ();

			/// Components Setup
			GUILayout.BeginVertical ("", boxStyle);
			myTarget.profile.showComponentsSetup = EditorGUILayout.BeginToggleGroup ("Component Setup", myTarget.profile.showComponentsSetup);
			if (myTarget.profile.showComponentsSetup) 
			{
				EditorGUILayout.PropertyField (Sun, true, null);
				EditorGUILayout.PropertyField (Moon, true, null);
				EditorGUILayout.PropertyField (DirectLight, true, null);
				EditorGUILayout.PropertyField (LightningGenerator, true, null);
				EditorGUILayout.PropertyField (windZone, true, null);
				EditorGUILayout.PropertyField (GlobalReflectionProbe, true, null);
				EditorGUILayout.PropertyField (satellites, true, null);
				EditorGUILayout.PropertyField (starsRotation, true, null);
            }
			EditorGUILayout.EndToggleGroup ();
			GUILayout.EndVertical ();
			GUILayout.EndVertical ();



			////////////
			// Begin Controls
			GUILayout.BeginVertical ("", boxStyle);
			// Time Control
			GUILayout.BeginVertical ("", boxStyle);
			myTarget.profile.showTimeUI = EditorGUILayout.BeginToggleGroup ("Time and Location Controls", myTarget.profile.showTimeUI);
			if (myTarget.profile.showTimeUI) {
				GUILayout.Space (20);
				GUILayout.BeginVertical ("Time", boxStyle);
				GUILayout.Space (20);
				EditorGUILayout.PropertyField (ProgressMode, true, null);
				GUILayout.Space (20);
				EditorGUILayout.PropertyField (Seconds, true, null);
				EditorGUILayout.PropertyField (Minutes, true, null);
				EditorGUILayout.PropertyField (Hours, true, null);
				EditorGUILayout.PropertyField (Days, true, null);
				EditorGUILayout.PropertyField (Years, true, null);
				GUILayout.Space (10);
				EditorGUILayout.PropertyField (DayLength, true, null);
				EditorGUILayout.PropertyField (NightLength, true, null);
				GUILayout.EndVertical ();
				GUILayout.BeginVertical ("Season", boxStyle);
				GUILayout.Space (20);
				EditorGUILayout.PropertyField (UpdateSeason, true, null);
				EditorGUILayout.PropertyField (CurrentSeason, true, null);
				GUILayout.EndVertical ();
				GUILayout.BeginVertical ("Location", boxStyle);
				GUILayout.Space (20);
				EditorGUILayout.PropertyField (UTC, true, null);
				GUILayout.Space (10);
				EditorGUILayout.PropertyField (Latitude, true, null);
				EditorGUILayout.PropertyField (Longitude, true, null);
				GUILayout.EndVertical ();
			}
			EditorGUILayout.EndToggleGroup ();
			GUILayout.EndVertical ();
			// Time End
			// Weather Control
			GUILayout.BeginVertical ("", boxStyle);
			myTarget.profile.showWeatherUI = EditorGUILayout.BeginToggleGroup ("Weather Controls", myTarget.profile.showWeatherUI);
			if (myTarget.profile.showWeatherUI) {
				EditorGUILayout.PropertyField (UpdateWeather, true, null);
				GUILayout.BeginVertical ("Weather", boxStyle);
				GUILayout.Space (20);
				EditorGUILayout.PropertyField (StartWeather, true, null);
				GUILayout.Space (15);
				if (Application.isPlaying) {
					if (myTarget.Weather.weatherPresets.Count > 0) {
						GUIContent[] zonePrefabs = new GUIContent[myTarget.Weather.weatherPresets.Count];
						for (int idx = 0; idx < zonePrefabs.Length; idx++) {
							zonePrefabs [idx] = new GUIContent (myTarget.Weather.weatherPresets [idx].Name);
						}
						int weatherID = EditorGUILayout.Popup (new GUIContent ("Current Weather"), myTarget.GetActiveWeatherID (), zonePrefabs);
						myTarget.ChangeWeather (weatherID);
					}
				} else
					EditorGUILayout.LabelField ("Weather can only be changed in runtime!");

				if (GUILayout.Button ("Edit current Weather Preset")) {
					if(myTarget.Weather.currentActiveWeatherPreset != null)
						Selection.activeObject = myTarget.Weather.currentActiveWeatherPreset;
					else if(myTarget.Weather.startWeatherPreset != null)
						Selection.activeObject = myTarget.Weather.startWeatherPreset;
				}
				GUILayout.EndVertical ();
				GUILayout.BeginVertical ("Zones", boxStyle);
				GUILayout.Space (20);
				myTarget.Weather.currentActiveZone = (EnviroZone)EditorGUILayout.ObjectField ("Current Zone", myTarget.Weather.currentActiveZone, typeof(EnviroZone), true);
				GUILayout.EndVertical ();
			}
			EditorGUILayout.EndToggleGroup ();
			GUILayout.EndVertical ();
			// Weather End
			// Effects Control
			GUILayout.BeginVertical ("", boxStyle);
			myTarget.profile.showEffectsUI = EditorGUILayout.BeginToggleGroup ("Feature Controls", myTarget.profile.showEffectsUI);
			if (myTarget.profile.showEffectsUI) {
              //  EditorGUILayout.PropertyField (globalFog, true, null);
                EditorGUILayout.PropertyField (EnableVolumeLighting, true, null);
				EditorGUILayout.PropertyField (renderClouds, true, null);
                EditorGUILayout.PropertyField (EnableSunShafts, true, null);
				EditorGUILayout.PropertyField (EnableMoonShafts, true, null);
			}
			EditorGUILayout.EndToggleGroup ();
			GUILayout.EndVertical ();
			// Effects End
			// Audio Control
			GUILayout.BeginVertical ("", boxStyle);
			myTarget.profile.showAudioUI = EditorGUILayout.BeginToggleGroup ("Audio Controls", myTarget.profile.showAudioUI);
			if (myTarget.profile.showAudioUI) {
				GUILayout.BeginVertical ("", boxStyle);
				EditorGUILayout.PropertyField (AmbientVolume, true, null);
				EditorGUILayout.PropertyField (WeatherVolume, true, null);
				GUILayout.EndVertical ();
			}
			EditorGUILayout.EndToggleGroup ();
			GUILayout.EndVertical ();
			// Audio End
			/////////////
			if (EditorGUI.EndChangeCheck ())
				serializedObj.ApplyModifiedProperties ();
			EditorGUILayout.EndVertical ();
		} else {
			GUILayout.BeginVertical ("", boxStyle);
			EditorGUILayout.LabelField ("No profile assigned!");
			if (GUILayout.Button ("Create and assign new profile!")) {
				myTarget.profile = EnviroProfileCreation.CreateNewEnviroProfile ();
				myTarget.ApplyProfile (myTarget.profile);
				myTarget.ReInit ();
			}
			GUILayout.EndVertical ();
		}
		EditorUtility.SetDirty (target);
	}
}
