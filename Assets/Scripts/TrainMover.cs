using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class TrainMover : MonoBehaviour {
    public GameObject trackObj;
    public GameObject prevCarObj;
    public float prevCarDist;
    public GameObject trainSprite;
    public bool showMessages;
    public bool head;
    public float currentDist;
    public Vector3Int Target;
    public bool FindPathToTarget;
    public Vector3Int TestPosition;
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
    public float desiredSpeed;
    public float maxSpeed;
    public float acceleration;
    float lastSpeed;
    bool curved;
    bool pickingUp;
    Vector3 pivot;
    Vector3 trackDirection;
    Vector3 nextDirection;
    Vector3 nextDirectionRev;
    Dictionary<TurnKey, float> turnLog;
    public float turnAngle;
    int i, j;

    TrainStop TargetStop;
    List<TrainStop> pickups;
    List<TrainStop> dropoffs;
    TrainStop StoppedBySignal;

    // Use this for initialization
    private void Awake()
    {
        StoppedBySignal = null;
        TargetStop = null;
        pickups = null;
        dropoffs = null;
        turnLog = null;
    }
    void Start () {
        FindPathToTarget = false;
        lindir = 1f;
        curved = false;
        trackController = trackObj.GetComponent<TrackController>();
        squareDist = 0f;
        speed = 0f;
        acceleration = GetAcceleration(maxSpeed, speed);
        //speed = desiredSpeed;
        speed += acceleration;
        lastSpeed = speed;

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
            turnLog = new Dictionary<TurnKey, float>();
            turnLog.Add(new TurnKey(trackController.GetPosInt(transform.position),Vector3Int.zero),turnAngle);
            //turnLog.Add(turnAngle);

        }
        TotalLength = GetLength()+0.5f;
        pickups = trackController.GetStops(TrainStop.StopType.pickUp);
        dropoffs = trackController.GetStops(TrainStop.StopType.dropOff);
        SetTargetStop(pickups[0]);
        pickingUp = true;
	}

    public struct TurnKey {
        public readonly Vector3Int position;
        public readonly Vector3Int direction;
        public TurnKey (Vector3Int p1, Vector3Int p2) {
            position = p1;
            direction = p2;
        }
    }

    float GetAcceleration(float Vf, float Vo) {
        desiredSpeed = Vf;
        return Mathf.Abs((Vf * Vf - Vo * Vo) / (2f * 1.9f));
    }

    void SetTargetStop(TrainStop stop) {
        TargetStop = stop;
        Target = stop.GridPosition();
        FindPathToTarget = true;
    }

    public void PassDownTurnLog(Dictionary<TurnKey,float> turnDictRef) {
        turnLog = turnDictRef;
        //turnLog = turnListRefAngles;
        if (prevCar != null) {
            prevCar.PassDownTurnLog(turnDictRef);
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
        Vector3Int[] exits;
        //float turnAngleOverride = turnAngle;
        positions[2] = trackController.GetPos(transform.position);
        //int turnLogIndex = -1;//turnLog.(trackController.GetPosInt(transform.position));
        //float useTurn = 1;
        //bool noSwitch = false;
        if (showMessages && trackController.GetPosInt(transform.position) == TestPosition)
        {
            Debug.Log(Vector3Int.RoundToInt(enterDirection));
            Debug.Log(speed);
            Debug.Log(head);
            Debug.Log(prevCar == null);
            //Debug.Log(turnLog[thisKey]);
        }
        if (turnLog != null)
        {
            TurnKey thisKey = new TurnKey(trackController.GetPosInt(transform.position), Vector3Int.RoundToInt(enterDirection));

            if (turnLog.ContainsKey(thisKey)) {
                turnAngle = turnLog[thisKey];
                if ((speed>0 && prevCar==null) || (speed < 0 && head)) {
                    turnLog.Remove(thisKey);
                }
            }
            else {
                if ((speed > 0 && head) || (speed < 0 && prevCar==null))
                {
                    turnLog.Add(thisKey, turnAngle);
                }
            }
        }

        exits = trackController.ValidExits(transform.position, enterDirection);

        if (showMessages) {
            string msg = "";
            for (i = 0; i < exits.Length;i++) {
                msg += exits[i];
            }
            //Debug.Log(transform.position);
            //Debug.Log(enterDirection.normalized);
            //Debug.Log(enterDirection.normalized+transform.position+ Quaternion.Euler(0, 0, turnAngle) * enterDirection.normalized);
            //Debug.Log(gameObject.name + " " +msg);
            //Debug.Log(positions[2]);
        }

        int startInd=0;
        int targInd=exits.Length-1;


        int[] minDists = { 0, 0 }; // 0 is start, 1 is end. Indices.
                                   // Determine relevant exits
        Vector3 checkAheadVector = (enterDirection.normalized
                                  + Quaternion.Euler(0, 0, -Mathf.Sign(speed) * turnAngle) * enterDirection.normalized);
        for (j = 0; j < 2; j++)
        {
            for (i = 0; i < exits.Length; i++)
            {
                if ((transform.position - trackController.GetPos(exits[i]) + j * (checkAheadVector)).sqrMagnitude <
                    (transform.position - trackController.GetPos(exits[minDists[j]]) + j * (checkAheadVector)).sqrMagnitude)
                {
                    if (j > 0 && i == minDists[0])
                    {
                        continue;
                    }
                    minDists[j] = i;
                }
            }
            if (j == 0 && minDists[0] == 0)
            {
                minDists[1] = 1;
            }
        }
        startInd = minDists[0];
        targInd = minDists[1];
        if (startInd == targInd)
        {
            startInd = 0;
            targInd = exits.Length - 1;
        }
        
        /*if (showMessages) {
            Debug.Log(startInd + " " + targInd);
        }*/
        //transform.position = positions[1];

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
        if ((startbool == forward))
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
        TrainStop checkforstop = trackController.GetStop(trackController.GetPosInt(transform.position)
                                                       + Vector3Int.RoundToInt(nextDirection));
        if ( checkforstop != null)
        {
            if (head)
            {
                if (checkforstop.IsPassable() && (TargetStop == null  || checkforstop.GridPosition() != TargetStop.GridPosition()))
                {
                    
                    checkforstop.Enter();
                }
                else
                {
                    //if (checkforstop == )
                    if (checkforstop.GridPosition() == TargetStop.GridPosition())
                    {
                        TargetStop.ImpassableTemporarily(5f);
                    }
                    //Debug.Log("Stop??");
                    //speed = 0f;
                    acceleration = GetAcceleration(0f, speed);
                    StoppedBySignal = checkforstop;
                }
            }
            else if (prevCar == null)
            {
                //Debug.Log("Deactivating:");
                //Debug.Log(trackController.GetPosInt(transform.position));
                //Debug.Log(Vector3Int.RoundToInt(nextDirection));
                checkforstop.Exit();
            }
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

    IEnumerator PauseThenContinue (float wait, TrainStop nextTarget) {
        wait = Mathf.Abs(wait);
        while (wait>0) {
            wait -= Time.deltaTime;
            yield return null;
        }
        SetTargetStop(nextTarget);
    }

	// Update is called once per frame
	void Update () {
        if (StoppedBySignal != null) {
            if (StoppedBySignal.IsPassable() == true) {
                //speed = desiredSpeed;
                acceleration = GetAcceleration(maxSpeed, speed);
                StoppedBySignal.Enter();
                StoppedBySignal = null;
            }
        }
        if (FindPathToTarget && head && currentDist>TotalLength) {
            FindPathToTarget = false;
            speed = Mathf.Abs(speed);
            FindPath(Target, Vector3Int.RoundToInt(nextDirection.normalized));
        }
        if (head && trackController.GetPosInt(transform.position)==Target) {
            if (pickingUp) {
                pickingUp = false;
                //StartCoroutine(PauseThenContinue(5f, dropoffs[0]));
                SetTargetStop(dropoffs[0]);
            }
            else {
                pickingUp = true;
                SetTargetStop(pickups[0]);
                //StartCoroutine(PauseThenContinue(5f, pickups[0]));
            }
        }
        if (head && currentDist < TotalLength) {
            if (prevCar != null)
            {
                prevCar.PassDownTurnLog(turnLog);
            }
        }

        if (!Mathf.Approximately(speed,desiredSpeed)) {
            if (speed - Time.deltaTime*acceleration>desiredSpeed) {
                speed -= Time.deltaTime * acceleration;
            }
            else if (speed + Time.deltaTime * acceleration < desiredSpeed) {
                speed += Time.deltaTime * acceleration;
            }
            else {
                speed = desiredSpeed;
            }
        }
        /*
        if (!Mathf.Approximately(Mathf.Sign(speed),Mathf.Sign(lastSpeed))) {
            lastSpeed = speed;
            if (speed < 0 && head) {
                turnLog.Remove(new TurnKey(trackController.GetPosInt(transform.position), Vector3Int.RoundToInt(nextDirectionRev)));
            }
            else if (speed > 0 && prevCar == null) {
                turnLog.Remove(new TurnKey(trackController.GetPosInt(transform.position), Vector3Int.RoundToInt(nextDirection)));
            }
        }*/

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
