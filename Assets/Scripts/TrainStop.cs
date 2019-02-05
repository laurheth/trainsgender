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
    List<TrainStop> ChainsFrom;
    List<TrainStop> ChainsInto;
    // Use this for initialization
    private void Awake()
    {
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
        if (ShowMessages) {
            Debug.Log(setTo);
        }
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

    IEnumerator ImpassableTemp(float wait) {
        wait = Mathf.Abs(wait);
        SetPassable(false);
        //Enter();
        while (wait>0) {
            wait -= Time.deltaTime;
            yield return null;
        }
        SetPassable(true);
        //Exit();
    }

    public void ImpassableTemporarily(float wait) {
        StartCoroutine(ImpassableTemp(wait));
    }

    public void CheckChains() {
        if (!ChainSignal || ChainsInto.Count==0) {
            return;
        }
        bool any = true;
        bool all = false;
        Debug.Log("Chain Chain:");
        foreach (TrainStop stop in ChainsInto) {
            any &= stop.IsPassable();
            all |= stop.IsPassable();
            Debug.Log(stop.GridPosition() + " " + stop.IsPassable());
        }
        Debug.Log(any + " " + all);
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

}
