using UnityEngine;
using System.Collections;

public class EnviroReflections : MonoBehaviour {

	public ReflectionProbe probe;
	public float ReflectionUpdateInGameHours = 1f;

	private double lastUpdate;

	// Use this for initialization
	void Start () 
	{

	if (probe == null)
			probe = GetComponent<ReflectionProbe> ();
	}

	void  UpdateProbe ()
	{
		probe.RenderProbe ();
		lastUpdate = EnviroSky.instance.currentTimeInHours;
	}

	// Update is called once per frame
	void Update ()
	{
        if (EnviroSky.instance == null)
            return;

        if (EnviroSky.instance.currentTimeInHours > lastUpdate + ReflectionUpdateInGameHours || EnviroSky.instance.currentTimeInHours < lastUpdate - ReflectionUpdateInGameHours)
			UpdateProbe ();
	}
}
