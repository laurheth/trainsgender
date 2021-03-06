﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainStop : MonoBehaviour {
    Vector3Int position;
    Grid parentGrid;

    [System.Serializable]
    public enum StopType { pickUp, dropOff, signal };

    public StopType thisType;
    public bool passable;
    public bool ChainSignal;
    public bool chainPassable;
    public bool ShowMessages;
    public SpriteRenderer sprite;
    TrackController.StopBlock IsExitFor;
    TrackController.StopBlock IsEntryFor;
    public GameObject townObj;
    TrainTown town;
    List<TrainStop> ChainsFrom; // Chains that lead to this stop
    List<TrainStop> ChainsInto; // Chains that this stop leads into
    float waitTime;
    public bool alwaysOff;
    TrainMover usedBy;
    // Use this for initialization
    private void Awake()
    {
        alwaysOff = false;
        usedBy = null;
        town = null;
        IsExitFor = null;
        IsEntryFor = null;
    //}
    //void Start () {
        parentGrid = GetComponentInParent<Grid>();
        transform.position = parentGrid.GetCellCenterWorld(parentGrid.WorldToCell(transform.position));
        passable = true;
        chainPassable=true;
        sprite = GetComponent<SpriteRenderer>();
        sprite.color = Color.cyan;
        ChainsFrom = new List<TrainStop>();
        ChainsInto = new List<TrainStop>();
	}

    private void Start()
    {
        if (townObj!=null && town==null) {
            ConnectTo(townObj.GetComponent<TrainTown>());
        }
    }

    public void Clear() {
        IsExitFor = null;
        IsEntryFor = null;
        ChainsFrom.Clear();
        ChainsInto.Clear();
        passable = true;
        chainPassable = true;
    }

    public Vector3Int GridPosition() {
        return parentGrid.WorldToCell(transform.position);
    }

    public Vector3 Direction() {
        return -transform.right;
    }

    public StopType GetStopType() {
        return thisType;
    }

    public void SetAlwaysOff() {
        alwaysOff = true;
    }

    public void SetPassable(bool setTo, List<TrainStop> checkSelfChains=null) {
        passable = setTo && !alwaysOff;
        UpdateColor();
        if (checkSelfChains==null) {
            checkSelfChains = new List<TrainStop>();
        }
        if (setTo && !checkSelfChains.Contains(this)) // Check own chains
        {
            checkSelfChains.Add(this);
            CheckChains(checkSelfChains);
        }

        if (ChainsFrom.Count > 0) // Don't loop forever
        {
            foreach (TrainStop stop in ChainsFrom) // Ask the chains that lead into this one to update
            {
                if (!checkSelfChains.Contains(stop))
                {
                    checkSelfChains.Add(stop);
                    stop.CheckChains(checkSelfChains);
                }
            }
        }
        /*if (ShowMessages) {
            Debug.Log(setTo);
        }*/

    }

    private void UpdateColor() {
        if (!passable) {
            sprite.color = Color.red;
        }
        else {
            if (chainPassable) {
                sprite.color = Color.cyan;
            }
            else {
                sprite.color = Color.yellow;
            }
        }
    }

    public bool IsPassable() {
        return passable && !alwaysOff;
    }

    public bool IsChainPassable() {
        return chainPassable;
    }

    public void SetAsEntrance(TrackController.StopBlock newBlock) {
        IsEntryFor = newBlock;
    }

    public void SetAsExit(TrackController.StopBlock newBlock)
    {
        IsExitFor = newBlock;
    }

    public void Enter () {
        //SetPassable(false);
        if (IsEntryFor != null)
        {
            IsEntryFor.Enter();
        }
    }

    public void Exit () {
        //SetPassable(true);
        if (IsExitFor != null)
        {
            IsExitFor.Exit();
        }
    }

    public void Hold() {
        //Debug.Log("holding");
        waitTime += Time.deltaTime;
    }

    IEnumerator ImpassableTemp(float wait,TrainMover trainUsing) {
        usedBy = trainUsing;
        waitTime = Mathf.Abs(wait);
        SetPassable(false);
        //Enter();
        while (waitTime>0) {
            waitTime -= Time.deltaTime;
            yield return null;
        }
        SetPassable(true);
        usedBy = null;
        //Exit();
    }

    public void ImpassableTemporarily(float wait,TrainMover trainUsing) {
        StartCoroutine(ImpassableTemp(wait,trainUsing));
    }

    public TrainMover GetUser() {
        return usedBy;
    }

    public void CheckChains(List<TrainStop> checkSelfChains=null) {
        if (!ChainSignal || ChainsInto.Count==0) {
            return;
        }
        bool any = true;
        bool all = false;
        //Debug.Log("Chain Chain:");
        foreach (TrainStop stop in ChainsInto) {
            any &= stop.IsPassable();
            all |= stop.IsPassable();
            //Debug.Log(stop.GridPosition() + " " + stop.IsPassable());
        }
        //Debug.Log(any + " " + all);
        SetChain(any);
        if (checkSelfChains == null)
        {
            checkSelfChains = new List<TrainStop>();
            checkSelfChains.Add(this);
        }
        SetPassable(all && IsEntryFor.CurrentState(),checkSelfChains);
    }

    public void SetChain(bool chainbool) {
        chainPassable = chainbool;
        UpdateColor();
    }

    public void AddChain(TrainStop chainFrom) {
        if (!ChainsFrom.Contains(chainFrom)) {
            ChainsFrom.Add(chainFrom);
        }
        chainFrom.AddChainTo(this);
    }

    public void AddChainTo(TrainStop chainTo) {
        if (!ChainsInto.Contains(chainTo))
        {
            ChainsInto.Add(chainTo);
        }
    }

    public void ConnectTo(TrainTown connection) {
        town = connection;
        townObj = town.gameObject;
        town.SetConnected(this);
    }

    public void Disconnect() {
        if (town != null) {
            town.SetConnected(null);
            town = null;
            townObj = null;
        }
    }

    public TrainTown Connection() {
        return town;
    }

    public TrainsWoman GetPassenger() {
        if (town != null) {
            return town.GetTraveller();
        }
        else {
            return null;
        }
    }

    public void DropPassenger(TrainsWoman passenger) {
        if (passenger != null)
        {
            town.AddResident(passenger);
            //passenger.SetTown(town);
            passenger.DoneTravelling();
        }
    }

    public TrainStop NextSignal(Dictionary<TrainMover.TurnKey,float> turnLog) {
        //TrainStop toReturn = null;
        Vector3Int[] dirs = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        for (int i = 0; i < ChainsInto.Count;i++) {
            for (int j = 0; j < 4;j++) {
                TrainMover.TurnKey newKey = new TrainMover.TurnKey(ChainsInto[i].GridPosition(),dirs[j]);
                if (turnLog.ContainsKey(newKey)) {
                    return ChainsInto[i];
                }
            }
        }

        return null;
    }
}
