using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Pathfinding
public partial class TrainMover : MonoBehaviour {
    
    public void FindPath(Vector3Int target, Vector3Int startDirection) {
        turnLog.Clear();
        int breaker = 0;
        PathTile targetTile = new PathTile(target, null, target,false,Vector3Int.zero,null);
        List<PathTile> ClosedList = new List<PathTile>();
        List<PathTile> OpenList = new List<PathTile>();
        Vector3Int[] exits;
        ClosedList.Add(new PathTile(trackController.GetPosInt(transform.position),
                                    null, target,true,startDirection,
                                    trackController.GetStop(trackController.GetPosInt(transform.position))));


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
                                    ClosedList[0], target,true,startDirection,
                                            trackController.GetStop(trackController.GetPosInt(transform.position) + startDirection)));
            }
            else {
                if (OpenList.Count==0) {
                    break;
                }
                ClosedList.Add(OpenList[0]);
                //Debug.Log(OpenList[0].GetPos() + " "+OpenList.Count);
                OpenList.RemoveAt(0);
                startDirection = ClosedList[ClosedList.Count - 1].GetPos()
                                                    - ClosedList[ClosedList.Count - 1].GetPrevious().GetPos();
            }
            exits = trackController.ValidExits(ClosedList[ClosedList.Count-1].GetPos(), startDirection);
            for (i = 0; i < exits.Length;i++) {
                if (exits[i] == ClosedList[ClosedList.Count - 1].GetPos()-startDirection) { continue; }
                if (trackController.GetStop(exits[i]) != null) {
                    if (Vector3.Dot(trackController.GetStop(exits[i]).Direction(),startDirection)<0) {
                        //Debug.Log(breaker + " " + ClosedList[ClosedList.Count - 1].GetPos());
                        continue;
                    }
                }
                PathTile newTile = new PathTile(exits[i], ClosedList[ClosedList.Count - 1], target,true,
                                                exits[i]-ClosedList[ClosedList.Count - 1].GetPos(),
                                                trackController.GetStop(exits[i]));
                if (!ClosedList.Contains(newTile)) {
                    OpenList.Add(newTile);
                }
            }
            OpenList.Sort(PathTile.Compare);
            //Debug.Log(OpenList.)
            breaker++;
        }
        //Debug.Log(breaker);
        if (ClosedList.Contains(targetTile)) {
            PathTile thisTile = ClosedList[ClosedList.Count - 1];
            Vector3Int ForwardDir;
            float angle = 0;
            breaker = 0;
            string msg="";
            while (thisTile.GetPrevious()!=null && breaker<1000) {
                msg += thisTile.GetPos();
                ForwardDir = thisTile.GetPos();
                turnLog.Add(new TurnKey(thisTile.GetPos(), thisTile.GetDirection()), angle);
                //turnLog.Add(thisTile.GetPos(), angle);
                thisTile = thisTile.GetPrevious();
                if (thisTile.GetPrevious()!=null) {
                    angle = Vector3.SignedAngle(thisTile.GetPos() - thisTile.GetPrevious().GetPos(),
                                              ForwardDir - thisTile.GetPos(), Vector3.back);
                }
                breaker++;
            }
            //Debug.Log(msg);

        }
    }

    // Class for defining cost and position of each tile, as well as sorting method
    public class PathTile : IEquatable<PathTile> {
        Vector3Int position;
        Vector3Int direction;
        PathTile previousTile;
        //Vector3Int[] exits;
        float travelCost;
        float proximityToTarget;
        bool hasdirection;
        float tileCost;

        public PathTile(Vector3Int pos, PathTile previous, Vector3Int target, bool directional, Vector3Int dir, TrainStop stop) {
            position = pos;
            direction = dir;

            tileCost = 1;
            //if (track)
            if (stop != null) {
                if (!stop.IsPassable()) {
                    tileCost = 10;
                }
            }
            SetPrevious(previous);
            hasdirection = directional;
            proximityToTarget = Mathf.Abs(pos.x - target.x) + Mathf.Abs(pos.y - target.y);
        }

        public Vector3Int GetPos() {
            return position;
        }

        public Vector3Int GetDirection() {
            return Vector3Int.RoundToInt(direction);
        }

        public float GetCost() {
            return travelCost + proximityToTarget;
        }

        public float GetTravel() {
            return travelCost;
        }

        public bool HasDirection() {
            return hasdirection;
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
                if (this.HasDirection() && other.HasDirection())
                {
                    return this.GetDirection() == other.GetDirection();
                }
                else
                {
                    return true;
                }
            }
            else {
                return false;
            }
        }
    }

}
