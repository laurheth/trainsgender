using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainStop : MonoBehaviour {
    Vector3Int position;
    Grid parentGrid;
    public enum StopType { pickUp, dropOff, signal };
    public StopType thisType;
    public bool passable;
    public bool ChainSignal;
    public bool chainPassable;
    public bool ShowMessages;
    SpriteRenderer sprite;
    TrackController.StopBlock IsExitFor;
    TrackController.StopBlock IsEntryFor;
    public GameObject townObj;
    TrainTown town;
    List<TrainStop> ChainsFrom;
    List<TrainStop> ChainsInto;
    float waitTime;
    TrainMover usedBy;
    // Use this for initialization
    private void Awake()
    {
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

    public void SetPassable(bool setTo, bool checkSelfChains=true) {
        passable = setTo;
        UpdateColor();
        if (ChainsFrom.Count > 0)
        {
            foreach (TrainStop stop in ChainsFrom)
            {
                stop.CheckChains();
            }
        }
        /*if (ShowMessages) {
            Debug.Log(setTo);
        }*/
        if (setTo && checkSelfChains)
        {
            CheckChains();
        }
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
        return passable;
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
        Debug.Log("holding");
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

    public void CheckChains() {
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
        SetPassable(all && IsEntryFor.CurrentState(),false);
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
        town.AddResident(passenger);
        //passenger.SetTown(town);
        passenger.DoneTravelling();
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
