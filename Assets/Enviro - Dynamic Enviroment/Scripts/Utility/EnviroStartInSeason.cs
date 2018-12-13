using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnviroStartInSeason : MonoBehaviour {

	public EnviroSeasons.Seasons startInSeason;

	void Start () {
		if (EnviroSky.instance != null) {
			switch (startInSeason) {
			case EnviroSeasons.Seasons.Spring:
				EnviroSky.instance.GameTime.Days = 1;
			break;
			case EnviroSeasons.Seasons.Summer:
				EnviroSky.instance.GameTime.Days = (int)EnviroSky.instance.seasonsSettings.SpringInDays;
			break;
			case EnviroSeasons.Seasons.Autumn:
				EnviroSky.instance.GameTime.Days = (int)EnviroSky.instance.seasonsSettings.SpringInDays + (int)EnviroSky.instance.seasonsSettings.SummerInDays;
			break;
			case EnviroSeasons.Seasons.Winter:
				EnviroSky.instance.GameTime.Days = (int)EnviroSky.instance.seasonsSettings.SpringInDays + (int)EnviroSky.instance.seasonsSettings.SummerInDays + (int)EnviroSky.instance.seasonsSettings.AutumnInDays;
			break;
			}
		}
	}
}
