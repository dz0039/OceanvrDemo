using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;

[AddComponentMenu("Enviro/Utility/Audio Mixer Support")]
public class EnviroAudioMixerSupport : MonoBehaviour {

	[Header("Mixer")]
	public AudioMixer audioMixer;

	[Header("Group Names")]
	public string ambientMixerGroup;
	public string weatherMixerGroup;
	public string thunderMixerGroup;

	void Start () 
	{
		if(audioMixer!= null && EnviroSky.instance != null)
			StartCoroutine (Setup ());
	}
	

	IEnumerator Setup ()
	{
		yield return 0;
		if (EnviroSky.instance.started) {

			if(ambientMixerGroup != "")
			{
				EnviroSky.instance.AudioSourceAmbient.audiosrc.outputAudioMixerGroup = audioMixer.FindMatchingGroups (ambientMixerGroup) [0];
				EnviroSky.instance.AudioSourceAmbient2.audiosrc.outputAudioMixerGroup = audioMixer.FindMatchingGroups (ambientMixerGroup) [0];
			}

			if(weatherMixerGroup != "")
			{
				EnviroSky.instance.AudioSourceWeather.audiosrc.outputAudioMixerGroup = audioMixer.FindMatchingGroups (weatherMixerGroup) [0];
				EnviroSky.instance.AudioSourceWeather2.audiosrc.outputAudioMixerGroup = audioMixer.FindMatchingGroups (weatherMixerGroup) [0];
			}

			if(thunderMixerGroup != "")
			{
				EnviroSky.instance.AudioSourceThunder.outputAudioMixerGroup = audioMixer.FindMatchingGroups (thunderMixerGroup) [0];
			}
		} else {
			StartCoroutine (Setup ());
		}
	}
}
