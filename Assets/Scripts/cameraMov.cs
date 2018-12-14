using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMov : MonoBehaviour
{
    // Start is called before the first frame update
    float yaw = 0;  
    float pitch =0;
    float speedH =3;
    float speedV=3;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        yaw += speedH*Input.GetAxis("Mouse X");
        pitch -= speedV*Input.GetAxis("Mouse Y");
        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }
}
