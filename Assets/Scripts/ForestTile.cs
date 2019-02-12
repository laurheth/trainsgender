using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ForestTile : Tile
{
    public Sprite[] singleSprites;
    public Sprite[] connectedSprites;
    public Sprite m_Preview;

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        Vector3Int checkPos;// = position;
        //base.RefreshTile(position, tilemap);
        for (int i = -1; i < 2;i++) {
            checkPos = position + new Vector3Int(0, i, 0);
            if (HasForest(tilemap, checkPos))
            {
                tilemap.RefreshTile(checkPos);
            }
        }
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        //tileData.sprite = m_Sprites[Random.Range(0, m_Sprites.Length)];
        if (HasForest(tilemap,position+Vector3Int.up) && position.y % 2 == 0) {
            tileData.sprite = connectedSprites[0];
        }
        else if (HasForest(tilemap, position + Vector3Int.down) && position.y % 2 != 0) {
            tileData.sprite = connectedSprites[1];
        }
        else {
            tileData.sprite = singleSprites[Random.Range(0,singleSprites.Length)];
        }
    }

    public bool HasForest(ITilemap tilemap, Vector3Int pos) {
        return tilemap.GetTile(pos) == this;
    }

#if UNITY_EDITOR
    [MenuItem("Assets/Create/ForestTile")]
    public static void CreateForestTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Forest Tile", "New Forest Tile", "Asset", "Save Forest Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ForestTile>(), path);
    }
#endif
}
