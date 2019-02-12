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
    public GameObject trackArrow;
    CamScript camScript;

    private void Start()
    {
        trackArrow.SetActive(false);
        spriteRend = GetComponent<SpriteRenderer>();
        camScript = cam.GetComponent<CamScript>();
        selected = false;
    }

    public void SetSprite(Sprite sprite) {
        spriteRend.sprite = sprite;
    }

    public void SetArrow(bool arrowNeeded) {
        trackArrow.SetActive(arrowNeeded);
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

    public void Clear () {
        SetSprite(null);
        tileToPlace = null;
        objToPlace = null;
        selected = false;
        trackArrow.SetActive(false);
    }

    public void SetSelected(bool setto) {
        selected = setto;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("click!!");
        if (selected && objToPlace != null)
        {
            camScript.PlaceTile();
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (selected && objToPlace == null)
        {
            camScript.PlacingTiles(true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        camScript.PlacingTiles(false);
    }
}
