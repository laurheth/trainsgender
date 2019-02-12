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

    private void Start()
    {
        tileCursor = cursor.GetComponent<PlaceTileCursor>();
    }

    public void OnPointerClick(PointerEventData eventData) {
        //Debug.Log("click!");
        tileCursor.tileToPlace = tileToPlace;
        tileCursor.SetSprite(GetComponent<Image>().sprite);
        tileCursor.selected = true;
        tileCursor.objToPlace = objToPlace;
        tileCursor.SetArrow((objToPlace != null));
    }

}
