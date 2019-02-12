using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RandomizedTile : Tile
{
    public Sprite[] m_Sprites;
    public Sprite m_Preview;

    public override void GetTileData(Vector3Int vector3Int, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = m_Sprites[Random.Range(0, m_Sprites.Length)];
    }

#if UNITY_EDITOR
    [MenuItem("Assets/Create/RandomizedTile")]
    public static void CreateRandomizedTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Randomized Tile", "New Randomized Tile", "Asset", "Save Randomized Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<RandomizedTile>(), path);
    }
#endif
}
