using UnityEngine;
using System.Collections;
namespace EnviroSamples
{
public class EventTest : MonoBehaviour {

	void Start ()
	{
		EnviroSky.instance.OnWeatherChanged += (EnviroWeatherPreset type) =>
		{
            DoOnWeatherChange(type);

            Debug.Log("Weather changed to: " + type.Name);
		};


         EnviroSky.instance.OnZoneChanged += (EnviroZone z) =>
          {
              DoOnZoneChange(z);

              Debug.Log("ChangedZone: " + z.zoneName);
          };


       EnviroSky.instance.OnSeasonChanged += (EnviroSeasons.Seasons season) =>
		{
			Debug.Log("Season changed");
		};

		EnviroSky.instance.OnHourPassed += () =>
		{
			Debug.Log("Hour Passed!");
		};

		EnviroSky.instance.OnDayPassed += () =>
		{
			Debug.Log("New Day!");
		};
		EnviroSky.instance.OnYearPassed += () =>
		{
			Debug.Log("New Year!");
		};


	}

    void DoOnWeatherChange (EnviroWeatherPreset type)
    {
        if(type.Name == "Light Rain")
            { 

                //Do something
            }
    }

    void DoOnZoneChange(EnviroZone type)
    {
        if (type.zoneName == "Swamp")
        {

            //Do something
        }
    }

        public void TestEventsWWeather ()
	{
		print("Weather Changed though interface!");
	}

	public void TestEventsNight ()
	{
		print("Night now!!");
	}

	public void TestEventsDay ()
	{
		print("Day now!!");
	}
}
}