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
    List<Transform> allTrains;
    Grid grid;
    //bool DoRefresh;
    public GameObject gridObj;
    public GameObject camobj;
    public Vector3 direction;
    //public GameObject blockingObj;
    Dictionary<Vector3Int, TrainStop> trainStops;
    Dictionary<Vector3Int, StopBlock> stopBlockDict;
    List<StopBlock> stopBlocks;
    public GameObject blockingObj;
    Tilemap blockingMap;
    public GameObject basemapObj;
    Tilemap baseMap;
    List<Vector3Int> rivers;
    float upDate;
    public TileBase[] bridgeTiles;
    public RandomizedTile[] riverTiles;
    public TileBase straightTile;
    //Camera cam;
    // Use this for initialization
    void Awake()
    {
        upDate = 0;
        //DoRefresh = false;
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
        allTrains = new List<Transform>();
        stopBlockDict = new Dictionary<Vector3Int, StopBlock>();
    }

    /*private void Start()
    {
        underlay = underlayobj.GetComponent<Tilemap>();
    }*/
    private void Start()
    {
        
        blockingMap = blockingObj.GetComponent<Tilemap>();
        baseMap = basemapObj.GetComponent<Tilemap>();
        rivers = new List<Vector3Int>();
        //rivers=blockingMa
        Vector3Int blockPos = Vector3Int.zero;
        for (int i = blockingMap.cellBounds.xMin; i <= blockingMap.cellBounds.xMax;i++) {
            for (int j = blockingMap.cellBounds.yMin; j < blockingMap.cellBounds.yMax;j++) {
                blockPos.x = i;
                blockPos.y = j;
                RandomizedTile maybeRiver = blockingMap.GetTile(blockPos) as RandomizedTile;
                if (maybeRiver != null)
                {
                    //Debug.Log("tostring:" + maybeRiver.ToString());
                    //Debug.Log("name:" + maybeRiver.name);
                    if ((blockingMap.GetTile(blockPos).name.Contains("River")))
                    {
                        Debug.Log("river:" + blockPos);
                        rivers.Add(blockPos);
                    }
                }
            }
        }

        GenerateStopBlocks();
        //GenerateStopBlocks();
        RefreshBlocks();
        GameObject[] townObjs = GameObject.FindGameObjectsWithTag("Town");
        allTowns = new TrainTown[townObjs.Length];
        for (int i = 0; i < townObjs.Length; i++)
        {
            allTowns[i] = townObjs[i].GetComponent<TrainTown>();
        }
    }

    public void AddSelfToTrainList(TrainMover mover) {
        allTrains.Add(mover.transform);
    }

    public bool CheckBlocking(Vector3Int pos) {
        return (blockingMap.GetTile(pos) != null ||
                (baseMap.GetTile<RandomizedTile>(pos)!=null 
                 && baseMap.GetTile<RandomizedTile>(pos).colliderType!=0));
    }

    public bool CheckCollision(TrainMover mover) {
        Transform thisOne = mover.transform;
        Vector3Int checkPos=GetPosInt(thisOne.position);
        for (int i = 0; i < allTrains.Count;i++) {
            if (allTrains[i].parent != thisOne.parent) {
                if (GetPosInt(allTrains[i].position) == checkPos) {
                    return true;
                }
            }
        }
        return false;
    }

    private void Update()
    {
        if (Time.time > upDate) {
            UpdateSignals();
            upDate = Time.time + 2.718281828f;
        }
    }

    public void UpdateSignals() {
        int i;
        for (i = 0; i < stopBlocks.Count;i++) {
            stopBlocks[i].Exit();
        }
        for (i = 0; i < allTrains.Count;i++) {
            if (stopBlockDict.ContainsKey(GetPosInt(allTrains[i].position))) {
                stopBlockDict[GetPosInt(allTrains[i].position)].Enter(0);
            }
        }
    }

    void RefreshBlocks() {
        stopBlockDict.Clear();
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
        int i;
        while (breaker<1000 && SignalsEnter.Count>0) {
            stopBlocks.Add(new StopBlock());
            breaker++;
            tiles.Clear();
            tiles.Add(SignalsEnter[0].GridPosition());
            //stopBlockDict.Add(SignalsEnter[0].GridPosition(),stopBlocks[stopBlocks.Count-1]);
            stopBlocks[stopBlocks.Count - 1].AddEntrance(SignalsEnter[0]);
            StopBlockIterate(SignalsEnter[0].GridPosition() + Vector3Int.RoundToInt(SignalsEnter[0].Direction()),
                            tiles,SignalsEnter,SignalsExit, stopBlocks[stopBlocks.Count-1],trainChunkPos);
            
            SignalsEnter.RemoveAt(0);
            for (i = 0; i < stopBlocks[stopBlocks.Count - 1].GetEntrances().Count;i++) {
                tiles.Remove(stopBlocks[stopBlocks.Count - 1].GetEntrances()[i].GridPosition());
            }
            for (i = 0; i < tiles.Count;i++) {
                if (!stopBlockDict.ContainsKey(tiles[i]))
                {
                    stopBlockDict.Add(tiles[i], stopBlocks[stopBlocks.Count - 1]);
                }
            }
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
        float openTime;
        //bool chainpassable;
        public StopBlock() {
            openTime = 0;
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

        public void Enter (float timeMod=1f) {
            openTime = Mathf.Max(Time.time+timeMod,openTime);
            passable = false;
            SetStatus();
        }

        public void Exit () {
            if (Time.time > openTime)
            {
                passable = true;
                SetStatus();
            }
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

    public TrainStop TrainYard() {
        List<TrainStop> yards = GetStops(TrainStop.StopType.pickUp, true);
        TrainStop toReturn = null;
        if (yards.Count>0) {
            int breaker = 0;
            do
            {
                toReturn = yards[Random.Range(0, yards.Count)];
                breaker++;
            } while (breaker < 5 && !toReturn.IsPassable());
        }
        return toReturn;
    }

    public List<TrainStop> GetStops(TrainStop.StopType type, bool onlyDisconnected=false) {
        List<TrainStop> toreturn = new List<TrainStop>();
        foreach (KeyValuePair<Vector3Int,TrainStop> stop in trainStops) {
            if (stop.Value.GetStopType() == type) {
                if (!onlyDisconnected || stop.Value.Connection()==null)
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

    public void SetTile(Vector3Int pos, TileBase tile, Quaternion rotation,bool checkRivers=true) {
        if (tile==null || GetTile(pos) == null || (GetTile(pos).name != tile.name || Quaternion.Angle(TileRotation(pos),rotation)>5))
        {
            tilemap.SetTile(pos, tile);
            if (tile != null)
            {
                tilemap.SetTransformMatrix(pos, Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one));

                for (int i = 0; i < allTrains.Count;i++) {
                    /*if (GetPosInt(allTrains[i].position)==pos) {
                        allTrains[i].gameObject.GetComponent<TrainMover>().ResetTile();
                    }*/
                }
            }
        }
        if (checkRivers)
        {
            CheckRivers();
        }
    }

    public void CheckRivers() {
        int i, j,k;
        int crossings;
        Vector3Int[] checkDirs = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        TileBase thisRiver;// = blockingMap.GetTile(blockPos) as RandomizedTile;
        Vector3Int[] exits;
        for (i = 0; i < rivers.Count;i++) {
            crossings = 0;
            for (j = 0; j < 4;j++) {
                exits = ValidExits(rivers[i] + checkDirs[j], Vector3Int.zero, true);
                if (exits != null) {
                    for (k = 0; k < exits.Length;k++) {
                        if (exits[k]==rivers[i]) {
                            crossings++;
                        }
                    }
                }
                if (crossings >= 2) { break; }
            }
            thisRiver = blockingMap.GetTile(rivers[i]);
            if (thisRiver is RandomizedTile && crossings>=2) {
                if (((RandomizedTile)thisRiver).name == "River_dx") {
                    blockingMap.SetTile(rivers[i], bridgeTiles[0]);
                    SetTile(rivers[i], straightTile, Quaternion.Euler(0, 0, 90),false);
                }
                else {
                    blockingMap.SetTile(rivers[i], bridgeTiles[1]);
                    SetTile(rivers[i], straightTile, Quaternion.identity,false);
                }
            }
            else if (!(thisRiver is RandomizedTile) && crossings<2) {
                if (thisRiver == bridgeTiles[0]) {
                    blockingMap.SetTile(rivers[i], riverTiles[0]);
                }
                else
                {
                    blockingMap.SetTile(rivers[i], riverTiles[1]);
                }
                SetTile(rivers[i], null, Quaternion.identity, false);
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
            StartCoroutine(RefreshLoop());
            //DoRefresh = true;
            RefreshBlocks();
        }
    }

    IEnumerator RefreshLoop() {
        GameObject[] movers = GameObject.FindGameObjectsWithTag("TrainChunk");
        List<TrainStop> stops = GetStops(TrainStop.StopType.signal);
        int i, j;
        bool noCollisions;
        do
        {
            noCollisions = true;
            for (i = 0; i < movers.Length; i++)
            {
                if (movers[i] == null) { continue; }
                for (j = 0; j < stops.Count; j++)
                {
                    if (stops[j] == null) { continue; }
                    if (GetPosInt(movers[i].transform.position) == GetPosInt(stops[j].transform.position))
                    {
                        noCollisions = false;
                    }
                }
            }
            yield return null;
        } while (!noCollisions);
        RefreshBlocks();
        yield return null;
    }
}
