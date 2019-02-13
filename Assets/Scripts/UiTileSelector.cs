using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiTileSelector : MonoBehaviour, IPointerClickHandler {

    public GameObject cursor;
    PlaceTileCursor tileCursor;
    public TileBase tileToPlace;
    public GameObject objToPlace;
    public KeyCode button;

    private void Start()
    {
        tileCursor = cursor.GetComponent<PlaceTileCursor>();
    }

    private void Update()
    {
        if (Input.GetKey(button)) {
            DoClick();
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        //Debug.Log("click!");
        DoClick();
    }

    public void DoClick() {
        tileCursor.tileToPlace = tileToPlace;
        tileCursor.SetSprite(GetComponent<Image>().sprite);
        tileCursor.selected = true;
        tileCursor.objToPlace = objToPlace;
        tileCursor.SetArrow((objToPlace != null));
    }

}
