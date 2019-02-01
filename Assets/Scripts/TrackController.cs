using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TrackController : MonoBehaviour {
    Tilemap tilemap;
    Grid grid;
    public GameObject gridObj;
    public GameObject camobj;
    public Vector3 direction;
    Camera cam;
	// Use this for initialization
	void Awake () {
        tilemap = GetComponent<Tilemap>();
        grid=gridObj.GetComponent<Grid>();
        cam = camobj.GetComponent<Camera>();
        //direction = Vector3.left;
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 mousepos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        Vector3Int mouseposInt = grid.WorldToCell(mousepos);
        TileBase tile = GetTile(mouseposInt);
        //Vector3 rotation = TileRotation(mouseposInt);
        if (tile != null) {
            //Debug.Log(tile.name);
            //Debug.Log(mouseposInt);
            //Debug.Log(rotation);
            Vector3Int[] exits = ValidExits(mousepos,direction);
            if (exits != null && exits.Length>0) {
                for (int i = 0; i < exits.Length;i++) {
                    //Debug.Log(exits[i]);
                }
            }
        }
	}

    TileBase GetTile(Vector3Int pos) {
        if (tilemap.HasTile(pos)) {
            return tilemap.GetTile(pos);
        }
        return null;
    }

    public Quaternion TileRotation(Vector3Int pos) {
        if (tilemap.HasTile(pos))
        {
            return tilemap.GetTransformMatrix(pos).rotation;
        }
        return Quaternion.identity;
    }

    public Vector3Int GetPosInt(Vector3 position) {
        return grid.WorldToCell(position);
    }

    public Vector3 GetPos(Vector3 position) {
        return grid.GetCellCenterWorld(grid.WorldToCell(position));
    }

    public Vector3Int[] ValidExits(Vector3 position, Vector3 direction) {
        Vector3Int pos = grid.WorldToCell(position);
        TileBase tiletype = GetTile(pos);
        Quaternion rotation = TileRotation(pos);
        Vector3Int[] toreturn=null;
        float flip = 1;
        if (tiletype != null)
        {
            switch (tiletype.name)
            {
                case "straightrail":
                    toreturn = new Vector3Int[2];
                    toreturn[0] = pos + Vector3Int.RoundToInt(rotation * (Vector3.left));
                    toreturn[1] = pos + Vector3Int.RoundToInt(rotation * (Vector3.right));
                    break;
                case "turn":
                    toreturn = new Vector3Int[2];
                    toreturn[0] = pos + Vector3Int.RoundToInt(rotation * (Vector3.up));
                    toreturn[1] = pos + Vector3Int.RoundToInt(rotation * (flip*Vector3.right));
                    break;
                case "double_branchoff":
                    toreturn = new Vector3Int[3];
                    toreturn[0] = pos + Vector3Int.RoundToInt(rotation * (Vector3.left));
                    toreturn[1] = pos + Vector3Int.RoundToInt(rotation * (flip*Vector3.up));
                    toreturn[2] = pos + Vector3Int.RoundToInt(rotation * (Vector3.right));
                    break;
                case "leftbranch":
                    flip = -1;
                    goto case "branchoff";
                case "branchoff":
                    if (Vector3.Dot(rotation * (Vector3.left),direction) > 0.2*flip) {
                        goto case "double_branchoff";
                    }
                    if ((Vector3.Dot(rotation * (Vector3.left), direction) < -0.2*flip)) {
                        goto case "straightrail";
                    }
                    else {
                        goto case "turn";
                    }
                case "crossing":
                    toreturn = new Vector3Int[2];
                    toreturn[0] = pos + Vector3Int.RoundToInt(direction.normalized);
                    toreturn[1] = pos - Vector3Int.RoundToInt(direction.normalized);
                    break;
                default:
                    toreturn = null;
                    break;
            }
        }
        return toreturn;
    }
}
