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
	// Use this for initialization
	void Start () {
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

    public bool IsPassable() {
        return passable;
    }

}
