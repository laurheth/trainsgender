using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlaceTileCursor : MonoBehaviour {

    SpriteRenderer spriteRend;
    public TileBase tileToPlace;
    public GameObject objToPlace;
    public bool selected;

    private void Start()
    {
        spriteRend = GetComponent<SpriteRenderer>();
    }

    public void SetSprite(Sprite sprite) {
        spriteRend.sprite = sprite;
    }

    public TileBase GetTile() {
        return tileToPlace;
    }

    public GameObject GetObject() {
        return objToPlace;
    }

    public bool IsSelected() {
        return selected;
    }

    public void SetSelected(bool setto) {
        selected = setto;
    }
}
