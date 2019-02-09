using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CamScript : MonoBehaviour {

    public float speed;
    float vx;
    float hx;
    float camSize;
    bool placingTiles;
    public float[] MinMaxSize;
    Camera cam;
    public GameObject tileToPlace;
    public GameObject trackControlObj;
    TrackController trackController;
    PlaceTileCursor tileCursor;
    Vector3 mousePos;
    List<GameObject> trainChunks;
    //Vector3Int mousePosInt;
	// Use this for initialization
	void Start () {
        placingTiles = false;
        vx = 0;
        hx = 0;
        camSize = 5f;
        cam = GetComponent<Camera>();
        trackController = trackControlObj.GetComponent<TrackController>();
        tileCursor = tileToPlace.GetComponent<PlaceTileCursor>();
        /*tile = tileCursor.GetTile();
        tileSelected = tileCursor.IsSelected();*/
        trainChunks = new List<GameObject>();
        foreach (GameObject chunk in GameObject.FindGameObjectsWithTag("TrainChunk")) {
            trainChunks.Add(chunk);
        }
	}
	
	// Update is called once per frame
	void Update () {

        // Camera movement
        vx = Input.GetAxis("Vertical") * speed * camSize;
        hx = Input.GetAxis("Horizontal") * speed * camSize;
        transform.Translate(hx*Time.deltaTime, vx* Time.deltaTime, 0);

        // Zoom with scrollwheel
        camSize -= Input.GetAxis("Mouse ScrollWheel");
        if (camSize < MinMaxSize[0]) { camSize = MinMaxSize[0]; }
        if (camSize > MinMaxSize[1]) { camSize = MinMaxSize[1]; }
        cam.orthographicSize = camSize;

        // Cursor position
        mousePos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        //mousePosInt = trackController.ToCellCenter(mousePos);
        tileToPlace.transform.position=trackController.ToCellCenter(mousePos);
        if (Input.GetKeyDown(KeyCode.R)) {
            tileToPlace.transform.Rotate(0, 0, 90);
        }

        if (placingTiles) {
            PlaceTile();
        }

        if (Input.GetButtonDown("Cancel")) {
            tileCursor.Clear();
        }
	}

    public float GetScale() {
        return Mathf.Max(1, camSize / 5f);
    }

    public void PlacingTiles(bool setTo) {
        placingTiles = setTo;
    }

    public void PlaceTile() {
        Vector3Int placeLocation = trackController.GetPosInt(mousePos);
        bool okaytoplace = true;
        int i;
        /*for (i = 0; i < trainChunks.Count; i++)
        {
            if (trackController.GetPosInt(trainChunks[i].transform.position) == placeLocation)
            {
                okaytoplace = false;
                break;
            }
        }*/
        if (okaytoplace)
        {
            trackController.SetTile(placeLocation, tileCursor.GetTile(), tileToPlace.transform.rotation);
            trackController.AddObject(placeLocation, tileCursor.GetObject(), tileToPlace.transform.rotation);
            for (i = 0; i < trainChunks.Count; i++)
            {
                if (trainChunks[i].GetComponent<TrainMover>().head)
                {
                    trainChunks[i].GetComponent<TrainMover>().FindPath();
                }
            }
        }
    }
}
