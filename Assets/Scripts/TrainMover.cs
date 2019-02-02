using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainMover : MonoBehaviour {
    public GameObject trackObj;
    public GameObject prevCarObj;
    public float prevCarDist;
    public GameObject trainSprite;
    public bool showMessages;
    public bool head;
    public float currentDist;
    float distCorrect;
    TrainMover prevCar;
    TrackController trackController;
    Vector3[] positions;
    public Vector3 trainDirection;
    float squareDist;
    float squareLength;
    float lindir;
    float TotalLength;
    public float speed;
    bool curved;
    Vector3 pivot;
    Vector3 trackDirection;
    Vector3 nextDirection;
    Vector3 nextDirectionRev;
    List<float[]> turnLog;
    public float turnAngle;
    int i, j;
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
        currentDist = 0f;
        if (prevCarObj != null) {
            prevCar = prevCarObj.GetComponent<TrainMover>();
        }
        trackDirection = Vector3.zero;
        nextDirection = Vector3.zero;
        nextDirectionRev = Vector3.zero;
        if (head)
        {
            turnLog = new List<float[]>();
            turnLog.Add(new float[2]);
            turnLog[0][0] = currentDist;
            turnLog[0][1] = turnAngle;
            if (prevCar != null) {
                prevCar.PassDownTurnLog(turnLog);
            }
        }
        TotalLength = GetLength()+0.5f;
	}

    public void PassDownTurnLog(List<float[]> turnListRef) {
        turnLog = turnListRef;
        if (prevCar != null) {
            prevCar.PassDownTurnLog(turnListRef);
        }
    }

    public float GetLength() {
        if (prevCar != null) {
            return prevCarDist + prevCar.GetLength();
        }
        else {
            return prevCarDist;
        }
    }

    public void SetBackDist(float headDist, float backDist) {
        distCorrect = (headDist - backDist) - currentDist;
        //currentDist += difference;
        //squareDist += difference;
    }
	
    void DefineTile(Vector3 enterDirection, bool forward=true) {
        Vector3Int[] exits = trackController.ValidExits(transform.position,
                                                        enterDirection);
        if (showMessages) {
            string msg = "";
            for (i = 0; i < exits.Length;i++) {
                msg += exits[i];
            }
            Debug.Log(transform.position);
            Debug.Log(enterDirection.normalized);
            Debug.Log(enterDirection.normalized+transform.position+ Quaternion.Euler(0, 0, turnAngle) * enterDirection.normalized);
            Debug.Log(msg);
        }
        int startInd=0;
        int targInd=exits.Length-1;
        int[] minDists={0,0}; // 0 is start, 1 is end. Indices.
        // Determine relevant exits
        Vector3 checkAheadVector = (enterDirection.normalized
                                  + Quaternion.Euler(0, 0, -Mathf.Sign(speed)*turnAngle) * enterDirection.normalized);
        for (j = 0; j < 2; j++)
        {
            for (i = 0; i < exits.Length; i++)
            {
                if ((transform.position - trackController.GetPos(exits[i]) + j*(checkAheadVector)).sqrMagnitude < 
                    (transform.position - trackController.GetPos(exits[minDists[j]]) + j * (checkAheadVector)).sqrMagnitude ) {
                    if (j>0 && i == minDists[0]) {
                        continue;
                    }
                    minDists[j] = i;
                }
            }
            if (j==0 && minDists[0]==0) {
                minDists[1] = 1;
            }
        }
        startInd = minDists[0];
        targInd = minDists[1];
        if (startInd==targInd) {
            startInd = 0;
            targInd = exits.Length - 1;
        }
        /*if (showMessages) {
            Debug.Log(startInd + " " + targInd);
        }*/
        //transform.position = positions[1];
        positions[2] = trackController.GetPos(transform.position);
        positions[4] = trackController.GetPos(exits[startInd]);
        positions[0] = trackController.GetPos(exits[targInd]);
        if (exits[startInd].x == exits[targInd].x || exits[startInd].y == exits[targInd].y) {
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
        bool startbool=((positions[4] - transform.position).sqrMagnitude) > ((positions[0] - transform.position).sqrMagnitude);
        if (startbool == forward)
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
            positions[0] = trackController.GetPos(exits[startInd]);
            //squareDist += squareLength;
            //trackDirection *= -1;
            //speed *= -1;
        }
        positions[1] = (positions[0] + positions[2]) / 2f;
        positions[3] = (positions[4] + positions[2]) / 2f;
        trackDirection = (positions[3] - positions[1]);
        nextDirection = (positions[4] - positions[2]).normalized;
        nextDirectionRev = (positions[0] - positions[2]).normalized;
        if (forward)
        {
            squareDist -= squareLength;
        }
        else {
            squareDist += squareLength;
        }
        /*if (showMessages)
        {
            Debug.Log(trackDirection);
            Debug.Log(positions[0] + " " + positions[2] + " " + positions[4]);
        }*/
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
        
        if (Mathf.Sign(speed)*(distCorrect-Time.deltaTime*speed)>0) {
            squareDist += 2f*Time.deltaTime * speed;
            currentDist += 2f*Time.deltaTime * speed;
        }
        else if (Mathf.Sign(speed)*(distCorrect + Time.deltaTime*speed) > 0) {
            squareDist += Time.deltaTime * speed;
            currentDist += Time.deltaTime * speed;
        }
        /*else {
            squareDist -= Time.deltaTime * speed;
            currentDist -= Time.deltaTime * speed;
        }*/
        //Debug.Log(lindir+ " "+ trainDirection);
        if (head) {
            // Remove indices ahead of head or behind tail
            if (showMessages) {
                string msg = "";
                for (i = 0; i < turnLog.Count;i++) {
                    msg += turnLog[i][0]+","+turnLog[i][1]+" ";
                }
                Debug.Log(msg);
            }
            if (turnLog.Count == 1 && speed > 0)
            {
                if (Mathf.Approximately(turnLog[0][1], turnAngle))
                {
                    turnLog[0][0] = currentDist-TotalLength+0.1f;
                }
                else
                {
                    turnLog.Add(new float[2]);
                    turnLog[0][0] = currentDist - Time.deltaTime * speed;
                    turnLog[1][0] = currentDist;
                    turnLog[1][1] = turnAngle;
                }
            }
            else if (speed > 0)
            {
                if (!Mathf.Approximately(turnLog[turnLog.Count-1][1], turnAngle)) {
                    turnLog.Add(new float[2]);
                    turnLog[turnLog.Count - 1][0] = currentDist;
                    turnLog[turnLog.Count - 1][1] = turnAngle;
                }
            }
            else if (speed < 0)
            {
                if (turnLog[turnLog.Count - 1][0] < currentDist)
                {
                    turnAngle = turnLog[turnLog.Count - 1][1];
                }
            }

            while (turnLog.Count > 1 && turnLog[turnLog.Count - 1][0] > currentDist)
            {
                turnLog.RemoveAt(turnLog.Count - 1);
            }
            while (turnLog.Count > 1 && turnLog[0][0] < currentDist - TotalLength)
            {
                turnLog.RemoveAt(0);
            }
        }
        else {
            for (i = 0; i < turnLog.Count;i++) {
                if (turnLog[i][0]<currentDist) {
                    turnAngle = turnLog[i][1];
                }
            }
        }

        UpdatePosition();
        if (squareDist > squareLength)
        {
            squareDist -= squareLength;
            DefineTile(nextDirection);
            UpdatePosition();
        }
        else if (squareDist < -squareLength) {
            squareDist += squareLength;
            DefineTile(nextDirectionRev,false);
            UpdatePosition();
        }
        if (prevCar != null) {
            prevCar.SetBackDist(currentDist, prevCarDist);
            prevCar.SetStats(speed, turnAngle);
            if (trainSprite != null) {
                trainSprite.transform.position = transform.position;
                trainSprite.transform.rotation =
                               Quaternion.LookRotation(Vector3.forward,
                                                       Quaternion.Euler(0,0,90)*(prevCarObj.transform.position
                                                                                 - transform.position));
            }
        }
	}
    public void SetStats(float spd, float ang) {
        speed = spd;
        //turnAngle = ang;
    }
}
