using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TrackController : MonoBehaviour {
    Tilemap tilemap;
    Grid grid;
    public GameObject camobj;
    public Vector3 direction;
    Camera cam;
	// Use this for initialization
	void Start () {
        tilemap = GetComponent<Tilemap>();
        grid=GetComponentInParent<Grid>();
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
            Debug.Log(tile.name);
            Debug.Log(mouseposInt);
            //Debug.Log(rotation);
            Vector3Int[] exits = ValidExits(mousepos,direction);
            if (exits != null && exits.Length>0) {
                for (int i = 0; i < exits.Length;i++) {
                    Debug.Log(exits[i]);
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

    Quaternion TileRotation(Vector3Int pos) {
        if (tilemap.HasTile(pos))
        {
            return tilemap.GetTransformMatrix(pos).rotation;
        }
        return Quaternion.identity;
    }

    Vector3Int[] ValidExits(Vector3 position, Vector3 direction) {
        Vector3Int pos = grid.WorldToCell(position);
        TileBase tiletype = GetTile(pos);
        Quaternion rotation = TileRotation(pos);
        Vector3Int[] toreturn=null;
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
                    toreturn[1] = pos + Vector3Int.RoundToInt(rotation * (Vector3.right));
                    break;
                case "double_branchoff":
                    toreturn = new Vector3Int[3];
                    toreturn[0] = pos + Vector3Int.RoundToInt(rotation * (Vector3.left));
                    toreturn[1] = pos + Vector3Int.RoundToInt(rotation * (Vector3.up));
                    toreturn[2] = pos + Vector3Int.RoundToInt(rotation * (Vector3.right));
                    break;
                case "branchoff":
                    if (Vector3.Dot(rotation * (Vector3.left),direction) > 0) {
                        goto case "double_branchoff";
                    }
                    else {
                        goto case "straightrail";
                    }
                    break;
                case "crossing":
                    toreturn = new Vector3Int[2];
                    toreturn[0] = pos + Vector3Int.RoundToInt(direction);
                    toreturn[1] = pos - Vector3Int.RoundToInt(direction);
                    break;
                default:
                    toreturn = null;
                    break;
            }
        }
        return toreturn;
    }
}
