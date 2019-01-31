using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainMover : MonoBehaviour {
    public GameObject trackObj;
    TrackController trackController;
    Vector3[] positions;
    public Vector3 trainDirection;
    float squareDist;
    float squareLength;
    float lindir;
    float speed;
    bool curved;
    Vector3 pivot;
    Vector3 trackDirection;
	// Use this for initialization
	void Start () {
        lindir = 1f;
        curved = false;
        trackController = trackObj.GetComponent<TrackController>();
        squareDist = 0f;
        speed = 1f;
        positions = new Vector3[5];
        pivot = Vector3.zero;
        DefineTile(trackController.TileRotation(trackController.GetPosInt(transform.position))*Vector3.left);
	}
	
    void DefineTile(Vector3 enterDirection) {
        Vector3Int[] exits = trackController.ValidExits(transform.position, trackDirection);
        //transform.position = positions[1];
        positions[2] = trackController.GetPos(transform.position);
        positions[4] = trackController.GetPos(exits[0]);
        positions[0] = trackController.GetPos(exits[exits.Length - 1]);
        if (exits[0].x == exits[exits.Length -1].x || exits[0].y == exits[exits.Length - 1].y) {
            squareLength = 0.5f;
            curved = false;
        }
        else {
            squareLength = (1.570796f)/4f;
            curved = true;
            //Debug.Log("curved");
        }
        /*trackDirection = trackController.TileRotation(trackController.GetPosInt(positions[1]))
                                            * Vector3.left;*/
        //speed = Mathf.Abs(speed);
        if (((positions[4]-transform.position).sqrMagnitude) > ((positions[0] - transform.position).sqrMagnitude))
        //if (Vector3.Dot(enterDirection,trackController.TileRotation(trackController.GetPosInt(positions[1]))
        //                *Vector3.left) >= -.3)
        {
            //lindir = 1;
            //speed = Mathf.Abs(speed);
        }
        else {
            //lindir = -1;
            //Debug.Log("switch dir?");
            positions[4] = positions[0];
            positions[0] = trackController.GetPos(exits[0]);
            //squareDist += squareLength;
            //trackDirection *= -1;
            //speed *= -1;
        }
        positions[1] = (positions[0] + positions[2]) / 2f;
        positions[3] = (positions[4] + positions[2]) / 2f;
        trackDirection = (positions[3] - positions[1]);
        squareDist -= squareLength;
        Debug.Log(positions[0] + " " + positions[2] + " " + positions[4]);
        if (curved) {
            pivot = (positions[0] + positions[4]) / 2f;
        }
    }

    void UpdatePosition() {
        if (curved) {
            transform.position = pivot
                + Quaternion.Euler(0, 0, -(0.5f + 0.5f * squareDist / squareLength) *
                                   Vector3.SignedAngle(positions[1]-pivot, positions[3]-pivot,Vector3.back)) * (positions[1] - pivot);
        }
        else {
            transform.position = positions[1] + trackDirection * (0.5f + 0.5f * squareDist / squareLength);
        }
        //transform.rotation = Quaternion.LookRotation(Vector3.back,trainDirection);

    }

	// Update is called once per frame
	void Update () {
        squareDist += Time.deltaTime * speed;
        //Debug.Log(lindir+ " "+ trainDirection);
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
