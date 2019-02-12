using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour {
    public GameObject loveObj;
    Text loveTxt;
    int love;

    TrainTown[] allTowns;
    List<TrainMover> allTrains;
    int i,j;

    float specialTime;
    public float[] specialTimeRange;

    public GameObject trackControlObj;
    TrackController trackController;

    public GameObject notificationPanel;
    public GameObject notificationText;

    RectTransform notificationRect;
    Text notifyText;
    float notifyPos;
    float notifyTime;

	// Use this for initialization
	void Start () {
        notifyPos = 110;
        notifyTime = 0;
        notificationRect = notificationPanel.GetComponent<RectTransform>();
        notifyText = notificationText.GetComponent<Text>();

        trackController = trackControlObj.GetComponent<TrackController>();
        love = 0;
        loveTxt = loveObj.GetComponent<Text>();

        GameObject[] townObjs = GameObject.FindGameObjectsWithTag("Town");
        allTowns = new TrainTown[townObjs.Length];
        for (int i = 0; i < townObjs.Length; i++)
        {
            allTowns[i] = townObjs[i].GetComponent<TrainTown>();
        }

        allTrains = new List<TrainMover>();
        TrainMover trainMover;
        foreach (GameObject train in GameObject.FindGameObjectsWithTag("TrainChunk")) {
            trainMover = train.GetComponent<TrainMover>();
            if (trainMover.head) {
                allTrains.Add(trainMover);
            }
        }
        i = 0;
        NextSpecial();
	}

    public void UpdateTrainList() {
        allTrains.Clear();
        TrainMover trainMover;
        foreach (GameObject train in GameObject.FindGameObjectsWithTag("TrainChunk"))
        {
            trainMover = train.GetComponent<TrainMover>();
            if (trainMover.head)
            {
                allTrains.Add(trainMover);
            }
        }
    }

    void NextSpecial() {
        specialTime = Time.time + Random.Range(specialTimeRange[0]/2f,specialTimeRange[1]/2f)
                          + Random.Range(specialTimeRange[0] / 2f, specialTimeRange[1] / 2f);
    }

    void Notification (string txt) {
        notifyText.text = txt;
        notifyTime = Time.time + 20;
    }

    private void Update()
    {
        if (Time.time < notifyTime) {
            notifyPos = Mathf.Max(0, notifyPos - 100 * Time.deltaTime);
            notificationRect.anchoredPosition = notifyPos * Vector3.up;
        }
        else if (notifyPos<110) {
            notifyPos = Mathf.Min(120,notifyPos + 100 * Time.deltaTime);
            notificationRect.anchoredPosition = notifyPos * Vector3.up;
        }
        if (Time.time > specialTime) {
            NextSpecial();
            List<TrainTown> validVenues = new List<TrainTown>();
            for (j = 0; j < allTowns.Length;j++) {
                if (allTowns[j].ContainsHostess()) {
                    validVenues.Add(allTowns[j]);
                }
            }
            if (validVenues.Count>0) {
                TrainTown venue = validVenues[Random.Range(0,validVenues.Count)];
                string[] reasons ={
                    "Boardgame Night",
                    "Potluck",
                    "Videogame Party",
                    "Party",
                    "Movie Night",
                    "Jam Session",
                    "Cuddle Party",
                    "LAN Party",
                    "RPG Night"
                };
                string reason = reasons[Random.Range(0, reasons.Length)];
                Notification("A "+reason+" is being organized at "+venue.GetName()+"!");
                for (j = 0; j < allTowns.Length;j++) {
                    allTowns[j].InviteToParty(venue, reason);
                }
            }
        }
        // Check if any trains need a target
        //for (i = 0; i < allTrains.Count;i++) {

        if (allTrains[i].GetTargetStop() == null || allTrains[i].GetTargetStop().Connection() == null)
        {
            for (j = 0; j < allTowns.Length; j++)
            {
                if (allTowns[j].IsConnected() && allTowns[j].GetBooker() == null)
                {
                    if (allTowns[j].GetTraveller() != null)
                    {
                        allTrains[i].SetTargetStop(allTowns[j].GetStop());
                        allTowns[j].Book(allTrains[i]);
                        allTrains[i].ReleaseHold();
                    }
                }
            }


            // If still null, maybe find a train yard to chill in? (i.e. non-town train stop)
            if (!allTrains[i].OnHold() &&
                (allTrains[i].GetTargetStop() == null || 
                 (allTrains[i].GetTargetStop().GetUser() != null && allTrains[i].GetTargetStop().GetUser() != allTrains[i])))
            {
                allTrains[i].SetTargetStop(trackController.TrainYard());
                //allTrains[i].On
            }
        }

        i++;
        if (i >= allTrains.Count) { i = 0; }
        //}


    }

    public void AddLove() {
        love++;
        loveTxt.text = love.ToString();
    }
}
