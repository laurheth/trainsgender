using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamScript : MonoBehaviour {

    public float speed;
    float vx;
    float hx;
    float camSize;
    public float[] MinMaxSize;
    Camera cam;
	// Use this for initialization
	void Start () {
        vx = 0;
        hx = 0;
        camSize = 5f;
        cam = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
        vx = Input.GetAxis("Vertical") * speed * camSize;
        hx = Input.GetAxis("Horizontal") * speed * camSize;
        transform.Translate(hx*Time.deltaTime, vx* Time.deltaTime, 0);
        camSize -= Input.GetAxis("Mouse ScrollWheel");
        if (camSize < MinMaxSize[0]) { camSize = MinMaxSize[0]; }
        if (camSize > MinMaxSize[1]) { camSize = MinMaxSize[1]; }
        cam.orthographicSize = camSize;
	}
}
