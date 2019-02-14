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
    public GameObject tileMapForLimits;
    float[] xBounds;
    float[] yBounds;
    TrackController trackController;
    PlaceTileCursor tileCursor;
    Vector3 mousePos;
    Vector3 camPos;
    List<GameObject> trainChunks;
    float horizSize;

    //Vector3Int mousePosInt;
    // Use this for initialization
    void Start()
    {
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
        foreach (GameObject chunk in GameObject.FindGameObjectsWithTag("TrainChunk"))
        {
            trainChunks.Add(chunk);
        }
        camPos = transform.position;
        Tilemap tilemap = tileMapForLimits.GetComponent<Tilemap>();
        xBounds = new float[2];
        yBounds = new float[2];

        xBounds[0] = tilemap.cellBounds.xMin;
        xBounds[1] = tilemap.cellBounds.xMax;//tilemap.origin.x + tilemap.size.x / 2;
        yBounds[0] = tilemap.cellBounds.yMin;//tilemap.origin.y - tilemap.size.y / 2;
        yBounds[1] = tilemap.cellBounds.yMax;//tilemap.origin.y + tilemap.size.y / 2;
    }
	
	// Update is called once per frame
	void Update () {
        camPos = transform.position;
        // Camera movement
        vx = Input.GetAxis("Vertical") * speed * camSize * Time.deltaTime;
        hx = Input.GetAxis("Horizontal") * speed * camSize * Time.deltaTime;
        //transform.Translate(hx*Time.deltaTime, vx* Time.deltaTime, 0);

        // Zoom with scrollwheel
        if (Input.GetKey(KeyCode.Minus)) {
            camSize *= 1 + 2*Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.Equals)) {
            camSize /= 1 + 2*Time.deltaTime;
        }
        camSize *= 1 + Mathf.Clamp(-Input.GetAxis("Mouse ScrollWheel"),-8*Time.deltaTime,8*Time.deltaTime);
        if (camSize < MinMaxSize[0]) { camSize = MinMaxSize[0]; }
        if (camSize > MinMaxSize[1]) { camSize = MinMaxSize[1]; }
        if (2*camSize > yBounds[1]-yBounds[0]) {
            camSize = ((yBounds[1] - yBounds[0])/2);
        }
        horizSize = camSize * Screen.width / Screen.height;

        if (2*horizSize > xBounds[1]-xBounds[0]) {
            horizSize = ((xBounds[1] - xBounds[0])/2);
            camSize = horizSize * Screen.height / Screen.width;
        }

        cam.orthographicSize = Mathf.FloorToInt(camSize);


        camPos.x = Mathf.Clamp(camPos.x + hx,xBounds[0]+horizSize,xBounds[1]-horizSize);
        camPos.y = Mathf.Clamp(camPos.y + vx, yBounds[0]+camSize, yBounds[1]-camSize);

        transform.position = camPos;

        // Cursor position
        mousePos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        //mousePosInt = trackController.ToCellCenter(mousePos);
        if (!trackController.CheckBlocking(trackController.GetPosInt(mousePos)))
        {
            tileToPlace.transform.position = trackController.ToCellCenter(mousePos);
        }
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

        bool okaytoplace = !trackController.CheckBlocking(placeLocation);

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
