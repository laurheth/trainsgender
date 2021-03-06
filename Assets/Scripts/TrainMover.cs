﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.EventSystems;

public partial class TrainMover : MonoBehaviour  {
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
    float rerunPathTime;
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
    public bool titleScreen;
    float lastSpeed;
    bool curved;
    bool wasJustStopped;
    bool justBorn;
    public bool firstTrain;//=false;
    bool pickingUp;
    bool noProperExit;
    bool onHold;
    bool collided;
    Vector3 pivot;
    Vector3 trackDirection;
    Vector3 nextDirection;
    public Vector3Int startDirection;
    Vector3 nextDirectionRev;
    Dictionary<TurnKey, float> turnLog;
    public float turnAngle;
    int i, j;

    TrainStop TargetStop;
    //List<TrainStop> pickups;
    //List<TrainStop> dropoffs;
    TrainStop StoppedBySignal;
    Vector3Int StoppedByTile;

    TrainsWoman passenger;

    UIScript uIScript;

    // Use this for initialization
    private void Awake()
    {
        wasJustStopped = false;
        justBorn = true;
        collided = false;
        onHold = false;
        noProperExit = false;
        StoppedByTile = Vector3Int.one;
        StoppedBySignal = null;
        TargetStop = null;
        //pickups = null;
        //dropoffs = null;
        turnLog = null;
    }
    void Start () {
        rerunPathTime = 0;
        //trainSprite.SetActive(false);
        FindPathToTarget = false;
        lindir = 1f;
        curved = false;
        if (trackObj==null) {
            trackObj = GameObject.FindGameObjectWithTag("TrackController");
        }
        trackController = trackObj.GetComponent<TrackController>();
        trackController.AddSelfToTrainList(this);
        //transform.position=trackController
        squareDist = 0f;
        //speed = 0f;
        acceleration = GetAcceleration(maxSpeed, speed);
        //speed = desiredSpeed;
        speed += acceleration;
        lastSpeed = speed;

        positions = new Vector3[5];
        pivot = Vector3.zero;
        if (prevCarObj != null)
        {
            prevCar = prevCarObj.GetComponent<TrainMover>();
        }

        //DefineTile(trackController.TileRotation(trackController.GetPosInt(transform.position))*Vector3.left);
        DefineTile(startDirection);
        currentDist = 0f;

        trackDirection = Vector3.zero;
        nextDirection = startDirection;//Vector3.zero;
        nextDirectionRev = Vector3.zero;
        if (head)
        {
            turnLog = new Dictionary<TurnKey, float>();
            turnLog.Add(new TurnKey(trackController.GetPosInt(transform.position),Vector3Int.zero),turnAngle);
            //turnLog.Add(turnAngle);

        }
        TotalLength = GetLength()+0.5f;
        //pickups = trackController.GetStops(TrainStop.StopType.pickUp);
        //dropoffs = trackController.GetStops(TrainStop.StopType.dropOff);
        //SetTargetStop(pickups[0]);
        pickingUp = true;

        if (!titleScreen)
        {
            uIScript = GameObject.FindGameObjectWithTag("GUI").GetComponent<UIScript>();
        }
	}

    public struct TurnKey {
        public readonly Vector3Int position;
        public readonly Vector3Int direction;
        public TurnKey (Vector3Int p1, Vector3Int p2) {
            position = p1;
            direction = p2;
        }
    }

    float GetAcceleration(float Vf, float Vo, float dist=1.6f) {
        //if (currentDist < 4f) { dist = 2f; }
        desiredSpeed = Vf;
        return Mathf.Abs((Vf * Vf - Vo * Vo) / (2f * dist));
    }

    public void SetTargetStop(TrainStop stop) {
        TargetStop = stop;
        if (stop != null)
        {
            if (stop.Connection()==null) {
                onHold = true;
            }
            else {
                ReleaseHold();
            }
            Target = stop.GridPosition();
            FindPathToTarget = true;
        }
        else {
            /*if (StoppedBySignal!=null) {
                onHold = true;
            }*/
            FindPathToTarget = false;
        }
    }

    public TrainStop GetTargetStop() {
        return TargetStop;
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
        if (showMessages)// && trackController.GetPosInt(transform.position) == TestPosition)
        {
            Debug.Log("Def tile"+Vector3Int.RoundToInt(enterDirection));
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
            Debug.Log(gameObject.name + " " +msg);
            //Debug.Log(positions[2]);
        }

        int startInd=0;
        noProperExit = true;
        //if (exits.L)
        if (exits==null) {
            Debug.Log("No proper exit!!!!");
            positions[2] = (trackController.GetPosInt(transform.position) - Vector3Int.RoundToInt(enterDirection));
            return;
        }

        int targInd=exits.Length-1;


        int[] minDists = { 0, 0 }; // 0 is start, 1 is end. Indices.
                                   // Determine relevant exits
        Vector3 checkAheadVector = (enterDirection.normalized
                                  + Quaternion.Euler(0, 0, -Mathf.Sign(speed) * turnAngle) * enterDirection.normalized);
        bool checkExit;

        for (j = 0; j < 2; j++)
        {
            for (i = 0; i < exits.Length; i++)
            {
                if (j==0) {
                    checkExit = ((trackController.GetPosInt(transform.position) - Vector3Int.RoundToInt(enterDirection))==exits[i]);
                    if (checkExit)
                    {
                        noProperExit = false;
                    }
                    if (showMessages) {
                        Debug.Log((trackController.GetPosInt(transform.position) - Vector3Int.RoundToInt(enterDirection)));
                    }
                }
                else {
                    checkExit = ((transform.position - trackController.GetPos(exits[i]) + Mathf.Max(j,1) * (checkAheadVector)).sqrMagnitude <
                                 (transform.position - trackController.GetPos(exits[minDists[j]]) + Mathf.Max(j, 1) * (checkAheadVector)).sqrMagnitude);
                }
                if (checkExit)
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

        if (noProperExit) {
            Debug.Log("No proper exit!!!!");
            positions[2] = (trackController.GetPosInt(transform.position) - Vector3Int.RoundToInt(enterDirection));
            return;
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
        if ((startbool == forward) || currentDist<2)
        //if (Vector3.Dot(enterDirection,trackController.TileRotation(trackController.GetPosInt(positions[1]))
        //                *Vector3.left) >= -.3)
        {
            //lindir = 1;
            //speed = Mathf.Abs(speed);
        }
        else {

            //lindir = -1;
            if (showMessages) { Debug.Log("switch dir?"); }
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

        if (head && trackController.GetTile(trackController.GetPosInt(transform.position)
                                                       + Vector3Int.RoundToInt(nextDirection))==null) {
            StoppedByTile = trackController.GetPosInt(transform.position)
                                         + Vector3Int.RoundToInt(nextDirection);
            acceleration = GetAcceleration(0f, speed,0.8f);
            Debug.Log("Stop for empty space");
        }
        TrainStop checkforstop;
        int minI = 1;
        if (wasJustStopped) { minI = 0; }
        for (i = minI; i < 2; i++)
        {
            if (StoppedBySignal) { break; }
            if (i == 0)
            {
                checkforstop = trackController.GetStop(trackController.GetPosInt(transform.position));
                wasJustStopped = false;
            }
            else
            {
                checkforstop = trackController.GetStop(trackController.GetPosInt(transform.position)
                                                               + Vector3Int.RoundToInt(nextDirection));
            }
            if (checkforstop != null && StoppedBySignal == null) // don't override existing stop!
            {
                if ((TargetStop != null && checkforstop.GridPosition() == Target) ||
                    (checkforstop.GetStopType()==TrainStop.StopType.pickUp && checkforstop.Connection()==null && onHold))
                {
                    checkforstop.ImpassableTemporarily(5f, this);
                    wasJustStopped=true;
                }
                else if (checkforstop.IsPassable() && !checkforstop.IsChainPassable())
                {
                    TrainStop aheadStop = checkforstop.NextSignal(turnLog);
                    if (aheadStop != null)
                    {
                        checkforstop = aheadStop;
                    }
                }

                if (checkforstop.IsPassable())// && (TargetStop == null || checkforstop.GridPosition() != TargetStop.GridPosition()))
                {

                    checkforstop.Enter();
                }
                else if (head)
                {
                    if (i > 0)
                    {
                        acceleration = GetAcceleration(0f, speed);
                    }
                    else {
                        acceleration = GetAcceleration(0f, speed,0.9f); 
                    }
                    StoppedBySignal = checkforstop;
                }
            }
        }
        if (trackController.CheckCollision(this))
        {
            collided = true;
        }
        trackController.UpdateSignals();
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

    // Reset Position if fucked up
    void ResetPositions() {
        TrainMover backPart = prevCar;
        while (backPart != null)
        {
            backPart.transform.position = transform.position;
            backPart.currentDist = currentDist;
            backPart.squareDist = squareDist;
            backPart.squareLength = squareLength;
            backPart.curved = curved;
            backPart.desiredSpeed = desiredSpeed;
            backPart.speed = speed;
            backPart.acceleration = acceleration;
            backPart.StoppedByTile = StoppedByTile;
            backPart.StoppedBySignal = StoppedBySignal;
            backPart.noProperExit = noProperExit;
            backPart.nextDirection = nextDirection;
            backPart.nextDirectionRev = nextDirectionRev;
            backPart.trackDirection = trackDirection;
            for (i = 0; i < positions.Length; i++)
            {
                backPart.positions[i] = positions[i];
            }
            backPart = backPart.prevCar;
        }
    }

    public bool OnHold() {
        return onHold;
    }

    public void ReleaseHold() {
        Debug.Log("hold released");
        onHold = false;
    }

    public Vector3Int GridPosition() {
        return trackController.GetPosInt(transform.position);
    }

    public void Kick() {
        if (noProperExit || StoppedByTile.z == 0) {
            Vector3 scratch = nextDirectionRev+Vector3Int.zero;
            nextDirectionRev = nextDirection + Vector3Int.zero;
            nextDirection = scratch + Vector3Int.zero;
            //float scrtch;
            for (int i = 0; i < 2;i++) {
                scratch = positions[i]+Vector3Int.zero;
                positions[i] = positions[4 - i]+ Vector3Int.zero;
                positions[4-i]=scratch+ Vector3Int.zero;
            }
            //ResetPositions();
            noProperExit = false;
            StoppedByTile = Vector3Int.one;
            DefineTile(nextDirection);
        }
        acceleration = GetAcceleration(maxSpeed, speed);
        if (StoppedBySignal != null)
        {
            StoppedBySignal.Enter();
        }
        StoppedBySignal = null;
    }

    public void ResetTile() {
        DefineTile(nextDirection);
    }

    // Frame update
    private void Update()
    {
        if (trainSprite != null)
        {
            trainSprite.transform.position = transform.position;
            trainSprite.transform.rotation =
                           Quaternion.LookRotation(Vector3.forward,
                                                   Quaternion.Euler(0, 0, 90) * (prevCarObj.transform.position
                                                                             - transform.position));
        }
    }

    // Physics step updated
    void FixedUpdate () {
        if (currentDist > 1)
        {
            if (collided)
            {
                collided = trackController.CheckCollision(this);
                speed = 0;
            }
            if (noProperExit)
            {
                acceleration = GetAcceleration(maxSpeed, speed);
                speed = 0;
            }
        }
        if (StoppedByTile.z==0){
            if (trackController.GetTile(StoppedByTile) != null) {
                StoppedByTile = Vector3Int.one;
                acceleration = GetAcceleration(maxSpeed, speed);
            }
        }

        if (prevCar != null)
        {
            if ((transform.position - prevCar.transform.position).sqrMagnitude > 2.5*prevCarDist)
            {
                ResetPositions();
            }
        }

        if (StoppedBySignal != null) {
            //Debug.Log("signaled");
            if ((onHold && StoppedBySignal.Connection()==null) || TargetStop==null) {
                StoppedBySignal.Hold();
            }
            if (StoppedBySignal.IsPassable() == true) {
                //speed = desiredSpeed;
                acceleration = GetAcceleration(maxSpeed, speed);
                StoppedBySignal.Enter();
                StoppedBySignal = null;
            }
            if (justBorn && !firstTrain)
            {
                Kick();
                justBorn = false;
            }
        }

        if (head && trackController.GetPosInt(transform.position)==Target) {
            
            if (TargetStop!=null) {
                
                if (TargetStop.Connection() != null)
                {
                    TargetStop.Connection().Book(null);
                }
                else {
                    Debug.Log("Hold Activated");
                    onHold = true;
                }
                //Debug.Log("what the fuck");
                if (passenger != null) {
                    TargetStop.DropPassenger(passenger);
                    uIScript.AddLove();
                    passenger = null;
                    //TargetStop = null;
                }
                if (passenger==null) {
                    //Debug.Log("pickup?");
                    passenger = TargetStop.GetPassenger();
                    if (passenger != null)
                    {
                        //Debug.Log("Picked up!");
                        SetTargetStop(passenger.TargTown().GetStop());
                        passenger.LeaveTown();
                    }
                    else {
                        //Debug.Log("failed");
                        TargetStop = null;
                    }
                }
                //if
                //TargetStop = null;
            }
        }
        if (head && currentDist < TotalLength) {
            if (prevCar != null)
            {
                prevCar.PassDownTurnLog(turnLog);
            }
        }

        if (!Mathf.Approximately(speed,desiredSpeed)) {
            if (speed - Time.fixedDeltaTime*acceleration>desiredSpeed) {
                speed -= Time.fixedDeltaTime * acceleration;
            }
            else if (speed + Time.fixedDeltaTime * acceleration < desiredSpeed) {
                speed += Time.fixedDeltaTime * acceleration;
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

        if (Mathf.Sign(speed)*(distCorrect-Time.fixedDeltaTime*speed)>0) {
            squareDist += 2f*Time.fixedDeltaTime * speed;
            currentDist += 2f*Time.fixedDeltaTime * speed;
        }
        else if (Mathf.Sign(speed)*(distCorrect + Time.fixedDeltaTime*speed) > 0) {
            squareDist += Time.fixedDeltaTime * speed;
            currentDist += Time.fixedDeltaTime * speed;
        }

        if (head && Time.time > rerunPathTime)
        {
            rerunPathTime = Time.time + 10;
            Debug.Log("Rerun pathfinding");
            FindPathToTarget = true;
        }

        if (FindPathToTarget && head && currentDist > TotalLength)
        {
            FindPathToTarget = false;
            speed = Mathf.Abs(speed);
            FindPath(Target, Vector3Int.RoundToInt(nextDirection.normalized), TargetStop);
        }
        /*else {
            squareDist -= Time.deltaTime * speed;
            currentDist -= Time.deltaTime * speed;
        }*/
        //Debug.Log(lindir+ " "+ trainDirection);


        UpdatePosition();
        if (squareDist > squareLength)
        {
            if (trackController.GetTile(trackController.GetPosInt(transform.position)) == null && !noProperExit)
            {
                currentDist -= (squareDist - squareLength);
                squareDist = squareLength;
            }
            else
            {
                squareDist -= squareLength;
                DefineTile(nextDirection);
            }
            UpdatePosition();
        }
        else if (squareDist < -squareLength) {
            if (trackController.GetTile(trackController.GetPosInt(transform.position)) == null && !noProperExit)
            {
                currentDist -= (squareDist - squareLength);
                squareDist = -squareLength;
            }
            else
            {
                squareDist += squareLength;
                DefineTile(nextDirectionRev, false);
            }
            UpdatePosition();
        }
        if (prevCar != null) {
            prevCar.SetBackDist(currentDist, prevCarDist);
            prevCar.SetStats(speed, turnAngle);
        }
	}
    public void SetStats(float spd, float ang) {
        speed = spd;
        //turnAngle = ang;
    }

    public bool UnleashedYet() {
        return currentDist > 5f;
    }
}
