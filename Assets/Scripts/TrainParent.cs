using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TrainParent : MonoBehaviour, IPointerClickHandler {

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("dsfgsdfg");
        foreach (TrainMover mover in GetComponentsInChildren<TrainMover>()) {
            mover.Kick();
        }
    }
}
