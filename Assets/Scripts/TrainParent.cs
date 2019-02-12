using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TrainParent : MonoBehaviour, IPointerClickHandler {

    public void OnPointerClick(PointerEventData eventData)
    {
        if (transform.parent!=null) {
            TrainDepot depot = transform.parent.gameObject.GetComponent<TrainDepot>();
            if (depot!=null) {
                depot.OnPointerClick(eventData);
                return;
                //transform.SetParent(null);
            }
        }
        //Debug.Log("dsfgsdfg");
        foreach (TrainMover mover in GetComponentsInChildren<TrainMover>()) {
            mover.Kick();
        }
    }
}
