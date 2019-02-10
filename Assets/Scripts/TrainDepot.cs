using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TrainDepot : MonoBehaviour, IPointerClickHandler {
    public GameObject trainPrefab;
    public float trainInterval;
    float nextTrainTime;
	// Use this for initialization
	void Start () {
        nextTrainTime = Time.time + trainInterval;
	}
	
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Time.time>nextTrainTime) {
            nextTrainTime = Time.time + trainInterval;
            Instantiate(trainPrefab, null);
        }
    }
}
