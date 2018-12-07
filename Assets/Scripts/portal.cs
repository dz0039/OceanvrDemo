using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class portal : MonoBehaviour {

	// Use this for initialization
	void Start () {
		//cam is the Camera class reference.
		Camera cam = GetComponent<Camera>();
		cam.depthTextureMode = DepthTextureMode.Depth;

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
