using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TrackController : MonoBehaviour
{
    Tilemap tilemap;
    //GameObject underlayobj;
    //Tilemap underlay;
    Grid grid;
    public GameObject gridObj;
    public GameObject camobj;
    public Vector3 direction;
    Dictionary<Vector3Int, TrainStop> trainStops;
    List<StopBlock> stopBlocks;
    Camera cam;
    // Use this for initialization
    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        grid = gridObj.GetComponent<Grid>();
        cam = camobj.GetComponent<Camera>();
        //direction = Vector3.left;
        trainStops = new Dictionary<Vector3Int, TrainStop>();

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("TrainStop"))
        {
            TrainStop newStop = obj.GetComponent<TrainStop>();
            trainStops.Add(grid.WorldToCell(obj.transform.position), newStop);
        }
        stopBlocks = new List<StopBlock>();
    }

    /*private void Start()
    {
        underlay = underlayobj.GetComponent<Tilemap>();
    }*/
    private void Start()
    {
        GenerateStopBlocks();
    }

    void GenerateStopBlocks()
    {
        List<TrainStop> SignalsEnter = GetStops(TrainStop.StopType.signal);
        List<TrainStop> SignalsExit = GetStops(TrainStop.StopType.signal);
        int breaker = 0;
        List<Vector3Int> tiles = new List<Vector3Int>();
        while (breaker<1000 && SignalsEnter.Count>0) {
            stopBlocks.Add(new StopBlock());
            breaker++;
            tiles.Clear();
            tiles.Add(SignalsEnter[0].GridPosition());
            stopBlocks[stopBlocks.Count - 1].AddEntrance(SignalsEnter[0]);
            StopBlockIterate(SignalsEnter[0].GridPosition() + Vector3Int.RoundToInt(SignalsEnter[0].Direction()),
                            tiles,SignalsEnter,SignalsExit, stopBlocks[stopBlocks.Count-1]);
            
            SignalsEnter.RemoveAt(0);
        }
        // Check if it worked
        foreach (StopBlock block in stopBlocks) {
            //Debug.Log(breaker);
            block.Print();
        }
    }

    void StopBlockIterate(Vector3Int thisPos, List<Vector3Int> donetiles,
                          List<TrainStop> SignalsEnter,List<TrainStop> SignalsExit, StopBlock newBlock) {
        //Debug.Log(thisPos);
        Vector3Int[] exits = ValidExits(thisPos,Vector3.zero,true);
        foreach (Vector3Int exit in exits) {
            if (!donetiles.Contains(exit)) {
                donetiles.Add(exit);
                bool IterateMore=true;
                if (GetStop(exit) != null)// && (SignalsEnter.Contains(GetStop(exit)) || ))
                {
                    //Signals.Remove(GetStop(exit));
                    if ((Vector3.Dot(GetStop(exit).Direction(), exit - thisPos) > 0) &&
                        SignalsExit.Contains(GetStop(exit)))
                    {
                        SignalsExit.Remove(GetStop(exit));
                        newBlock.AddExit(GetStop(exit));
                        Debug.Log((exit - thisPos) + " " + GetStop(exit).Direction());
                        Debug.Log("Exit added:" + GetStop(exit).GridPosition());
                        IterateMore = false;
                    }
                    else if ((Vector3.Dot(GetStop(exit).Direction(), exit - thisPos) <= 0) &&
                        SignalsEnter.Contains(GetStop(exit)))
                    {
                        SignalsEnter.Remove(GetStop(exit));
                        newBlock.AddEntrance(GetStop(exit));
                        Debug.Log("Entrance added:" + GetStop(exit).GridPosition());
                        IterateMore = false;
                    }
                }

                if (IterateMore) {
                    StopBlockIterate(exit, donetiles, SignalsEnter, SignalsExit, newBlock);
                }
            }
        }
    }

    public class StopBlock {
        List<TrainStop> Entrances;
        List<TrainStop> Exits;
        bool passable;
        public StopBlock() {
            Entrances = new List<TrainStop>();
            Exits = new List<TrainStop>();
            passable = true;
        }

        public void Print() {
            string msg = "Entrances:\n";
            foreach (TrainStop stop in Entrances) {
                msg += stop.GridPosition()+"\n";
            }
            msg += "Exits:\n";
            foreach (TrainStop stop in Exits)
            {
                msg += stop.GridPosition() + "\n";
            }
            Debug.Log(msg);
        }

        public void AddEntrance(TrainStop newStop) {
            Entrances.Add(newStop);
            newStop.SetPassable(passable);
            newStop.SetAsEntrance(this);
        }

        public void AddExit(TrainStop newStop) {
            Exits.Add(newStop);
            newStop.SetAsExit(this);
            //newStop.SetPassable(passable);
        }

        public void Enter () {
            passable = false;
            SetStatus();
        }

        public void Exit () {
            passable = true;
            SetStatus();
        }

        private void SetStatus() {
            foreach (TrainStop stop in Entrances) {
                stop.SetPassable(passable);
            }
        }
    }

    public List<TrainStop> GetStops(TrainStop.StopType type) {
        List<TrainStop> toreturn = new List<TrainStop>();
        foreach (KeyValuePair<Vector3Int,TrainStop> stop in trainStops) {
            if (stop.Value.GetStopType() == type) {
                toreturn.Add(stop.Value);
            }
        }
        return toreturn;
    }

    public TrainStop GetStop(Vector3Int position) {
        if (trainStops.ContainsKey(position)) {
            return trainStops[position];
        }
        else {
            return null;
        }
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

    public Vector3Int[] ValidExits(Vector3 position, Vector3 direction, bool GetAll=false) {
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
                    toreturn[0] = pos + Vector3Int.RoundToInt(rotation * (flip*Vector3.up));
                    toreturn[1] = pos + Vector3Int.RoundToInt(rotation * (Vector3.right));
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
                    if (GetAll || (Vector3.Dot(rotation * (Vector3.left),direction) > 0.2)) {
                        goto case "double_branchoff";
                    }
                    if ((Vector3.Dot(rotation * (Vector3.left), direction) < -0.2)) {
                        goto case "straightrail";
                    }
                    else {
                        goto case "turn";
                    }
                case "crossing":
                    if (!GetAll)
                    {
                        toreturn = new Vector3Int[2];
                        toreturn[0] = pos + Vector3Int.RoundToInt(direction.normalized);
                        toreturn[1] = pos - Vector3Int.RoundToInt(direction.normalized);
                    }
                    else {
                        toreturn = new Vector3Int[4];
                        toreturn[0] = pos + Vector3Int.up;
                        toreturn[1] = pos + Vector3Int.down;
                        toreturn[2] = pos + Vector3Int.left;
                        toreturn[3] = pos + Vector3Int.right;
                    }
                    break;
                default:
                    toreturn = null;
                    break;
            }
        }
        return toreturn;
    }
}
