using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class PlaceTileCursor : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler {

    SpriteRenderer spriteRend;
    public TileBase tileToPlace;
    public GameObject objToPlace;
    public bool selected;
    public GameObject cam;
    CamScript camScript;

    private void Start()
    {
        spriteRend = GetComponent<SpriteRenderer>();
        camScript = cam.GetComponent<CamScript>();
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

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("click!!");
        if (objToPlace != null)
        {
            camScript.PlaceTile();
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (objToPlace == null)
        {
            camScript.PlacingTiles(true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        camScript.PlacingTiles(false);
    }
}
