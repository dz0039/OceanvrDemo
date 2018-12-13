using UnityEngine;
using System.Collections;

public class EnviroAudioSource : MonoBehaviour {

	public enum AudioSourceFunction
	{
		Weather1,
		Weather2,
		Ambient,
		Ambient2,
		Thunder,
        ZoneAmbient
	}

	public AudioSourceFunction myFunction;
    public AudioSource audiosrc;
	public bool isFadingIn = false;
	public bool isFadingOut = false;


	float currentAmbientVolume;
	float currentWeatherVolume;
    float currentZoneVolume;

    void Start ()
	{
		if (audiosrc == null)
		audiosrc = GetComponent<AudioSource> ();
		
		if (myFunction == AudioSourceFunction.Weather1 || myFunction == AudioSourceFunction.Weather2) 
		{
			audiosrc.loop = true;
			audiosrc.volume = 0f;
		}

		currentAmbientVolume = EnviroSky.instance.Audio.ambientSFXVolume;
		currentWeatherVolume = EnviroSky.instance.Audio.weatherSFXVolume;
    }
		
	public void FadeOut () 
	{
		isFadingOut = true;
		isFadingIn = false;
	}
		
	public void FadeIn (AudioClip clip) 
	{
		isFadingIn = true;
		isFadingOut = false;
		audiosrc.clip = clip;
		audiosrc.Play ();
	}


	void Update ()
	{
		if (!EnviroSky.instance.started || EnviroSky.instance == null)
			return;

		currentAmbientVolume = Mathf.Lerp(currentAmbientVolume,EnviroSky.instance.Audio.ambientSFXVolume + EnviroSky.instance.Audio.ambientSFXVolumeMod,10f * Time.deltaTime);
		currentWeatherVolume = Mathf.Lerp(currentWeatherVolume,EnviroSky.instance.Audio.weatherSFXVolume + EnviroSky.instance.Audio.weatherSFXVolumeMod,10 * Time.deltaTime);

        if (myFunction == AudioSourceFunction.Weather1 || myFunction == AudioSourceFunction.Weather2 || myFunction == AudioSourceFunction.Thunder){
			if (isFadingIn && audiosrc.volume < currentWeatherVolume) {
				audiosrc.volume += EnviroSky.instance.weatherSettings.audioTransitionSpeed * Time.deltaTime;
			} else if (isFadingIn && audiosrc.volume >= currentWeatherVolume - 0.01f) {
				isFadingIn = false;
			}

			if (isFadingOut && audiosrc.volume > 0f) {
				audiosrc.volume -= EnviroSky.instance.weatherSettings.audioTransitionSpeed * Time.deltaTime;
			} else if (isFadingOut && audiosrc.volume == 0f) {
				audiosrc.Stop ();
				isFadingOut = false;
			}

			if (audiosrc.isPlaying && !isFadingOut && !isFadingIn) {
				audiosrc.volume = currentWeatherVolume;
			}
		}
		else if (myFunction == AudioSourceFunction.Ambient || myFunction == AudioSourceFunction.Ambient2)
		{
			if (isFadingIn && audiosrc.volume < currentAmbientVolume) {
				audiosrc.volume += EnviroSky.instance.weatherSettings.audioTransitionSpeed * Time.deltaTime;
			} else if (isFadingIn && audiosrc.volume >= currentAmbientVolume - 0.01f) {
				isFadingIn = false;
			}

			if (isFadingOut && audiosrc.volume > 0f) {
				audiosrc.volume -= EnviroSky.instance.weatherSettings.audioTransitionSpeed * Time.deltaTime;
			} else if (isFadingOut && audiosrc.volume == 0f) {
				audiosrc.Stop ();
				isFadingOut = false;
			}

			if (audiosrc.isPlaying && !isFadingOut && !isFadingIn) {
				audiosrc.volume = currentAmbientVolume;
			}
		}

        else if (myFunction == AudioSourceFunction.ZoneAmbient)
        {
            if (isFadingIn && audiosrc.volume < EnviroSky.instance.currentInteriorZoneAudioVolume)
            {
                audiosrc.volume += EnviroSky.instance.currentInteriorZoneAudioFadingSpeed * Time.deltaTime;
            }
            else if (isFadingIn && audiosrc.volume >= EnviroSky.instance.currentInteriorZoneAudioVolume - 0.01f)
            {
                isFadingIn = false;
            }

            if (isFadingOut && audiosrc.volume > 0f)
            {
                audiosrc.volume -= EnviroSky.instance.currentInteriorZoneAudioFadingSpeed * Time.deltaTime;
            }
            else if (isFadingOut && audiosrc.volume == 0f)
            {
                audiosrc.Stop();
                isFadingOut = false;
            }

            if (audiosrc.isPlaying && !isFadingOut && !isFadingIn)
            {
                audiosrc.volume = EnviroSky.instance.currentInteriorZoneAudioVolume;
            }
        }
    }
}
