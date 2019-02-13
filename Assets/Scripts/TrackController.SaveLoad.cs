using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public partial class TrackController : MonoBehaviour {

    public GameObject[] stopPrefabs;
    public TileBase[] trackOpts;
    public ForestTile forestTile;

    public TileBase TileFromNum(int num) {
        int newnum = num - 2;
        if (newnum < 0) { newnum = 0; }
        if (newnum > trackOpts.Length - 1) { newnum = trackOpts.Length - 1; }
        return trackOpts[newnum];
    }

    public int NumFromTile(Vector3Int pos) {
        TileBase thisTile = tilemap.GetTile(pos);
        if (thisTile is ForestTile) {
            //Debug.Log("forest");
            return 1;
        }
        else {
            switch (thisTile.name) {
                case "straightrail":
                    return 2;
                case "turn":
                    return 3;
                case "double_branchoff":
                    return 4;
                case "leftbranch":
                    return 5;
                case "branchoff":
                    return 6;
                case "crossing":
                    return 7;
                default:
                    return 0;
            }
        }
    }

    public void Save() {
        if (titleScreen) { return; }
        string path = Path.Combine(Application.persistentDataPath, "TrainSave.txt");
        SaveObject saveGame = new SaveObject();
        Vector3Int pos=Vector3Int.zero;
        for (int i = tilemap.cellBounds.xMin; i <= tilemap.cellBounds.xMax;i++) {
            for (int j = tilemap.cellBounds.yMin; j <= tilemap.cellBounds.yMax; j++)
            {
                pos.x = i;
                pos.y = j;
                if (tilemap.GetTile(pos)!=null) {
                    saveGame.AddTile(pos, NumFromTile(pos), TileRotation(pos));
                }
            }
        }
        int inttype;
        foreach (TrainStop stop in GetStops(TrainStop.StopType.signal,false,true)) {
            if (stop.alwaysOff) { continue; } // skip initial stop
            switch (stop.GetStopType()) {
                case TrainStop.StopType.pickUp:
                    inttype = 1;
                    break;
                default:
                    inttype = 0;
                    break;
            }
            saveGame.AddSignal(stop.GridPosition(), inttype, stop.gameObject.transform.rotation,stop.ChainSignal);
        }

        string json = JsonUtility.ToJson(saveGame);
        File.WriteAllText(path,json);
    }

//    public void LoadGame() {
  //      tilemap.ClearAllTiles();

    //}
    public void LoadGame() {
        if (titleScreen) { return; }
        string path = Path.Combine(Application.persistentDataPath, "TrainSave.txt");
        if (File.Exists(path)) {

            SaveObject saveGame = JsonUtility.FromJson<SaveObject>(File.ReadAllText(path));
            tilemap.ClearAllTiles();
            foreach (TrainStop stop in GetStops(TrainStop.StopType.signal, false, true))
            {
                if (stop.alwaysOff) {
                    continue;
                }
                AddObject(stop.GridPosition(), null, Quaternion.identity);
            }


            for (int i = 0; i < saveGame.tilePos.Count;i++) {
                if (saveGame.tileTypes[i] == 1)
                {
                    //Debug.Log("forest");
                    tilemap.SetTile(saveGame.tilePos[i], forestTile);
                }
                else if (saveGame.tileTypes[i] > 1){
                    tilemap.SetTile(saveGame.tilePos[i], TileFromNum(saveGame.tileTypes[i]));
                    tilemap.SetTransformMatrix(saveGame.tilePos[i], Matrix4x4.TRS(Vector3.zero, saveGame.tileRotations[i], Vector3.one));
                }
            }

            for (int i = 0; i < saveGame.signalPos.Count;i++) {
                //GameObject newObject;
                AddObject(saveGame.signalPos[i], stopPrefabs[saveGame.signalType[i]], saveGame.signalDirection[i],i==saveGame.signalPos.Count-1);
            }
        }
    }

    [System.Serializable]
    public class SaveObject
    {
        public List<Vector3Int> tilePos;
        public List<int> tileTypes;
        public List<Quaternion> tileRotations;

        public List<Vector3Int> signalPos;
        public List<int> signalType;
        public List<Quaternion> signalDirection;
        //public List<bool> isChain;

        public SaveObject() {
            tilePos = new List<Vector3Int>();
            tileTypes = new List<int>();
            tileRotations = new List<Quaternion>();

            signalPos = new List<Vector3Int>();
            signalType = new List<int>();
            signalDirection = new List<Quaternion>();
            //isChain = new List<bool>();
        }

        public void AddTile(Vector3Int pos, int tile, Quaternion rot)
        {
            tilePos.Add(pos);
            tileTypes.Add(tile);
            tileRotations.Add(rot);
        }

        public void AddSignal(Vector3Int pos, int thetype, Quaternion rot, bool checkChain) {
            signalPos.Add(pos);
            if (!checkChain)
            {
                signalType.Add(thetype);
            }
            else {
                signalType.Add(2);
            }
            signalDirection.Add(rot);
            //isChain.Add(checkChain);
        }
    }
}
