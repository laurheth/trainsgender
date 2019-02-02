using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Pathfinding
public partial class TrainMover : MonoBehaviour {
    
    public void FindPath(Vector3Int target, Vector3Int startDirection) {
        turnLog.Clear();
        int breaker = 0;
        PathTile targetTile = new PathTile(target, null, target);
        List<PathTile> ClosedList = new List<PathTile>();
        List<PathTile> OpenList = new List<PathTile>();
        Vector3Int[] exits;
        ClosedList.Add(new PathTile(trackController.GetPosInt(transform.position), null, target));


/*        exits = trackController.ValidExits(ClosedList[0].GetPos(), startDirection);
        foreach (Vector3Int exit in exits) {
            if (exit != ClosedList[0].GetPos()-startDirection) {
                OpenList.Add(new Pa)
            }
        }*/
        // Do until the tile is found, or it times out
        while (breaker<1000 && !ClosedList.Contains(targetTile)) {
            if (breaker==0) {
                ClosedList.Add(new PathTile(trackController.GetPosInt(transform.position) + startDirection,
                                    ClosedList[0], target));
            }
            else {
                ClosedList.Add(OpenList[0]);
                OpenList.RemoveAt(0);
            }
            exits = trackController.ValidExits(ClosedList[ClosedList.Count-1].GetPos(), startDirection);
            for (i = 0; i < exits.Length;i++) {
                PathTile newTile = new PathTile(exits[i], ClosedList[ClosedList.Count - 1], target);
                if (!ClosedList.Contains(newTile)) {
                    OpenList.Add(newTile);
                }
            }
            OpenList.Sort(PathTile.Compare);
            breaker++;
        }

        if (ClosedList.Contains(targetTile)) {
            PathTile thisTile = ClosedList[ClosedList.Count - 1];
            breaker = 0;
            string msg="";
            while (thisTile!=ClosedList[0] && breaker<1000) {
                msg += thisTile.GetPos();
                thisTile = thisTile.GetPrevious();
            }
            Debug.Log(msg);
            breaker++;
        }
    }

    // Class for defining cost and position of each tile, as well as sorting method
    public class PathTile : IEquatable<PathTile> {
        Vector3Int position;
        PathTile previousTile;
        //Vector3Int[] exits;
        float travelCost;
        float proximityToTarget;

        public PathTile(Vector3Int pos, PathTile previous, Vector3Int target) {
            position = pos;
            SetPrevious(previous);
            proximityToTarget = Mathf.Abs(pos.x - target.x) + Mathf.Abs(pos.y - target.y);
        }

        public Vector3Int GetPos() {
            return position;
        }

        public float GetCost() {
            return travelCost + proximityToTarget;
        }

        public float GetTravel() {
            return travelCost;
        }

        public void SetPrevious (PathTile previous) {
            if (previous != null)
            {
                previousTile = previous;
                travelCost = GetTravel() + 1;
            }
            else {
                previousTile = null;
                travelCost = 0;
            }
        }

        public PathTile GetPrevious() {
            return previousTile;
        }

        public static int Compare(PathTile tile1, PathTile tile2) {
            return tile1.GetCost().CompareTo(tile2.GetCost());
        }

        public bool Equals(PathTile other) {
            if (other==null) {
                return false;
            }
            if (this.GetPos() == other.GetPos()) {
                return true;
            }
            else {
                return false;
            }
        }
    }

}
