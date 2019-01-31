using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainMover : MonoBehaviour {
    public GameObject trackObj;
    TrackController trackController;
    Vector3[] positions;
    Vector3 trainDirection;
    float squareDist;
    float squareLength;
    float speed;
    bool curved;
    Vector3 trackDirection;
	// Use this for initialization
	void Start () {
        curved = false;
        trackController = trackObj.GetComponent<TrackController>();
        squareDist = 0f;
        speed = 4f;
        positions = new Vector3[3];
        DefineTile(trackController.TileRotation(trackController.GetPosInt(transform.position))*Vector3.left);
	}
	
    void DefineTile(Vector3 enterDirection) {
        Vector3Int[] exits = trackController.ValidExits(transform.position, trackDirection);
        //transform.position = positions[1];
        positions[1] = trackController.GetPos(transform.position);
        positions[2] = exits[0];
        positions[0] = exits[exits.Length - 1];
        if (exits[0].x == exits[exits.Length -1].x || exits[0].y == exits[exits.Length - 1].y) {
            squareLength = 0.5f;
            curved = false;
        }
        else {
            squareLength = (1.570796f)/2f;
            curved = true;
        }
        trackDirection = trackController.TileRotation(trackController.GetPosInt(positions[1]))
                                            * Vector3.left;
        speed = Mathf.Abs(speed);
        if (Vector3.Dot(enterDirection,trackController.TileRotation(trackController.GetPosInt(positions[1]))
                        *Vector3.left) >= 0) {
            squareDist -= squareLength;
            speed = Mathf.Abs(speed);
        }
        else {
            squareDist += squareLength;
            //trackDirection *= -1;
            speed *= -1;
        }
    }

    void UpdatePosition() {
        if (!curved) {
            transform.position = positions[1] + trackDirection * squareDist;
            trainDirection = speed*trackDirection;
        }
        else {
            transform.position = positions[1] - trackDirection / 2f + Quaternion.Euler(0, 0, -90) * trackDirection / 2f
                +Quaternion.Euler(0,0,45-45f * (squareDist/squareLength))*trackDirection/2f;
            trainDirection = speed * (Quaternion.Euler(0, 0, -90 * (squareDist / squareLength)) * trackDirection);
        }
        transform.rotation = Quaternion.LookRotation(Vector3.back,trainDirection);
    }

	// Update is called once per frame
	void Update () {
        squareDist += Time.deltaTime * speed;
        Debug.Log(squareDist);
        UpdatePosition();
        if (squareDist > squareLength)
        {
            squareDist -= squareLength;
            DefineTile(trainDirection);
            UpdatePosition();
        }
        else if (squareDist < -squareLength) {
            squareDist += squareLength;
            DefineTile(trainDirection);
            UpdatePosition();
        }
	}
}
