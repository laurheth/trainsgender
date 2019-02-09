using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TrackController : MonoBehaviour
{
    Tilemap tilemap;
    //GameObject underlayobj;
    //Tilemap underlay;
    TrainTown[] allTowns;
    //List<TrainMover> allTrains;
    Grid grid;
    public GameObject gridObj;
    public GameObject camobj;
    public Vector3 direction;
    Dictionary<Vector3Int, TrainStop> trainStops;
    List<StopBlock> stopBlocks;
    //Camera cam;
    // Use this for initialization
    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        grid = gridObj.GetComponent<Grid>();
        //cam = camobj.GetComponent<Camera>();
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
        GameObject[] townObjs = GameObject.FindGameObjectsWithTag("Town");
        allTowns = new TrainTown[townObjs.Length];
        for (int i = 0; i < townObjs.Length; i++)
        {
            allTowns[i] = townObjs[i].GetComponent<TrainTown>();
        }
    }

    void RefreshBlocks() {
        stopBlocks.Clear();
        List<TrainStop> Signals = GetStops(TrainStop.StopType.signal);
        foreach (TrainStop signal in Signals) {
            signal.Clear();
        }
        GenerateStopBlocks();
    }

    void GenerateStopBlocks()
    {
        List<TrainStop> SignalsEnter = GetStops(TrainStop.StopType.signal);
        List<TrainStop> SignalsExit = GetStops(TrainStop.StopType.signal);
        GameObject[] trainChunks = GameObject.FindGameObjectsWithTag("TrainChunk");
        List<Vector3Int> trainChunkPos = new List<Vector3Int>();
        foreach (GameObject chunk in trainChunks) {
            trainChunkPos.Add(GetPosInt(chunk.transform.position));
        }
        int breaker = 0;
        List<Vector3Int> tiles = new List<Vector3Int>();
        while (breaker<1000 && SignalsEnter.Count>0) {
            stopBlocks.Add(new StopBlock());
            breaker++;
            tiles.Clear();
            tiles.Add(SignalsEnter[0].GridPosition());
            stopBlocks[stopBlocks.Count - 1].AddEntrance(SignalsEnter[0]);
            StopBlockIterate(SignalsEnter[0].GridPosition() + Vector3Int.RoundToInt(SignalsEnter[0].Direction()),
                            tiles,SignalsEnter,SignalsExit, stopBlocks[stopBlocks.Count-1],trainChunkPos);
            
            SignalsEnter.RemoveAt(0);
        }
        // Check if it worked
        foreach (StopBlock block in stopBlocks) {
            //Debug.Log(breaker);
            block.Print();
            block.DetermineChains();
        }

        foreach (TrainStop stop in GetStops(TrainStop.StopType.signal)) {
            stop.CheckChains();
        }

    }

    void StopBlockIterate(Vector3Int thisPos, List<Vector3Int> donetiles,
                          List<TrainStop> SignalsEnter,List<TrainStop> SignalsExit, StopBlock newBlock,
                         List<Vector3Int> trainChunkPos) {
        //Debug.Log(thisPos);
        Vector3Int[] exits = ValidExits(thisPos,Vector3.zero,true);
        if (trainChunkPos.Contains(thisPos)) {
            newBlock.Enter();
        }
        if (exits == null) { return; }
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
                    StopBlockIterate(exit, donetiles, SignalsEnter, SignalsExit, newBlock,trainChunkPos);
                }
            }
        }
    }

    public class StopBlock {
        List<TrainStop> Entrances;
        List<TrainStop> Exits;
        bool passable;
        //bool chainpassable;
        public StopBlock() {
            Entrances = new List<TrainStop>();
            Exits = new List<TrainStop>();
            passable = true;
            //chainpassable = true;
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

        public List<TrainStop> GetEntrances() {
            return Entrances;
        }

        public List<TrainStop> GetExits()
        {
            return Exits;
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

        public bool CurrentState() {
            return passable;
        }

        public void DetermineChains() {
            foreach (TrainStop stop in Entrances) {
                if (stop.ChainSignal) {
                    foreach (TrainStop pollStop in Exits) {
                        pollStop.AddChain(stop);
                    }
                }
            }
        }

        /*public void SetChains(bool chain) {
            chainpassable = chain;
        }*/
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

    public TileBase GetTile(Vector3Int pos) {
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

    public Vector3 ToCellCenter(Vector3 position) {
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

    public void SetTile(Vector3Int pos, TileBase tile, Quaternion rotation) {
        if (tile==null || GetTile(pos) == null || (GetTile(pos).name != tile.name || Quaternion.Angle(TileRotation(pos),rotation)>5))
        {
            tilemap.SetTile(pos, tile);
            if (tile != null)
            {
                tilemap.SetTransformMatrix(pos, Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one));
            }
        }
    }

    public void AddObject(Vector3Int pos, GameObject obj, Quaternion rotation) {
        //if (obj == null) { return; }
        bool changedSignals = false;
        if (trainStops.ContainsKey(pos)) {
            trainStops[pos].Disconnect();
            Destroy(trainStops[pos].gameObject);
            trainStops.Remove(pos);
            changedSignals = true;
        }
        if (obj != null)
        {
            GameObject newobj = Instantiate(obj, ToCellCenter(pos), rotation, gridObj.transform);
            TrainStop newStop = newobj.GetComponent<TrainStop>();
            trainStops.Add(pos, newStop);
            changedSignals = true;


            if (newStop.GetStopType() == TrainStop.StopType.pickUp)
            {
                for (int i = 0; i < allTowns.Length; i++)
                {
                    if ((newStop.transform.position - allTowns[i].transform.position).sqrMagnitude < 9)
                    {
                        newStop.ConnectTo(allTowns[i]);
                        allTowns[i].SetConnected(newStop);
                    }
                }
            }
        }

        if (changedSignals)
        {
            RefreshBlocks();
        }
    }
}
