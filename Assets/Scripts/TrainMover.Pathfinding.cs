using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Pathfinding
public partial class TrainMover : MonoBehaviour {
    
    public void FindPath(Vector3Int target) {
        turnLog.Clear();

    }

    // Class for defining cost and position of each tile, as well as sorting method
    public class PathTile {
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

        public static int Compare(PathTile tile1, PathTile tile2) {
            return tile1.GetCost().CompareTo(tile2.GetCost());
        }
    }

}
