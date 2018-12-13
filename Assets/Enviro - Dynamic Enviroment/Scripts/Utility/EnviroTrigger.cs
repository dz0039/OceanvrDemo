using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnviroTrigger : MonoBehaviour {

	public EnviroInterior myZone;
	public string Name;

	//public bool entered = false;

	void Start () 
	{
		
	}
	

	void Update () 
	{
		
	}


	void OnTriggerEnter (Collider col)
	{
		if (EnviroSky.instance.weatherSettings.useTag) {
			if (col.gameObject.tag == EnviroSky.instance.gameObject.tag) {
				EnterExit ();
			}
		} else {
			if (col.gameObject.GetComponent<EnviroSky> ()) {
				EnterExit ();
			}
		}
	}

	void OnTriggerExit (Collider col)
	{

        if (myZone.zoneTriggerType == EnviroInterior.ZoneTriggerType.Zone)
        {
            if (EnviroSky.instance.weatherSettings.useTag)
            {
                if (col.gameObject.tag == EnviroSky.instance.gameObject.tag)
                {
                    EnterExit();
                }
            }
            else
            {
                if (col.gameObject.GetComponent<EnviroSky>())
                {
                    EnterExit();
                }
            }
        }
	}
		



	void EnterExit ()
	{
        if (EnviroSky.instance.lastInteriorZone != myZone)
        {
            if (EnviroSky.instance.lastInteriorZone != null)
                EnviroSky.instance.lastInteriorZone.StopAllFading();

            myZone.Enter();
        }
        else
        {
            if (!EnviroSky.instance.interiorMode)
                myZone.Enter();
            else
                myZone.Exit();
        }
	}

	void OnDrawGizmos () 
	{
		Gizmos.matrix = transform.worldToLocalMatrix;
		Gizmos.color = Color.blue;
		Gizmos.DrawCube (Vector3.zero,Vector3.one);
	}
}
