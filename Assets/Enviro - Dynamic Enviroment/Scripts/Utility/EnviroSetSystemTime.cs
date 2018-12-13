using UnityEngine;
using System.Collections;

public class EnviroSetSystemTime : MonoBehaviour {

	void Start () 
	{
		if (EnviroSky.instance != null) {
			EnviroSky.instance.SetTime (System.DateTime.Now);
		}

	}

}
