using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainStop : MonoBehaviour {
    Vector3Int position;
    Grid parentGrid;
    public enum StopType { pickUp, dropOff, signal };
    public StopType thisType;
    bool passable;
    SpriteRenderer sprite;
    TrackController.StopBlock IsExitFor;
    TrackController.StopBlock IsEntryFor;
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
        sprite = GetComponent<SpriteRenderer>();
        sprite.color = Color.cyan;
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

    public void SetPassable(bool setTo) {
        passable = setTo;
        if (passable) {
            sprite.color = Color.cyan;
        }
        else {
            sprite.color = Color.red;
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

}
