using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TrainDepot : MonoBehaviour, IPointerClickHandler {
    public GameObject trainPrefab;
    public float trainInterval;
    public GameObject UIObj;
    public GameObject depotSignal;
    GameObject newTrain;
    UIScript uIScript;
    float nextTrainTime;
    bool firstTrainFree;
	// Use this for initialization
	void Start () {
        firstTrainFree = false;
        nextTrainTime = Time.time + trainInterval;
        uIScript = UIObj.GetComponent<UIScript>();
        depotSignal.GetComponent<TrainStop>().SetAlwaysOff();
        newTrain = transform.Find("Train").gameObject;
	}

    private void Update()
    {
        if (firstTrainFree && newTrain==null && Time.time>nextTrainTime) {
            newTrain=Instantiate(trainPrefab,null);
            newTrain.transform.SetParent(transform);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Depot click");
        if (newTrain!=null) {
            newTrain.transform.SetParent(null);
            nextTrainTime = Time.time + trainInterval;
            uIScript.UpdateTrainList();
            TrainMover[] movers = newTrain.GetComponentsInChildren<TrainMover>();
            foreach (TrainMover mover in movers) {
                if (mover.head) {
                    mover.Kick();
                }
            }
            newTrain = null;
            firstTrainFree = true;
        }
    }
}
