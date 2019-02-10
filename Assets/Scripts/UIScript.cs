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

	// Use this for initialization
	void Start () {
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

    void NextSpecial() {
        specialTime = Time.time + Random.Range(specialTimeRange[0]/2f,specialTimeRange[1]/2f)
                          + Random.Range(specialTimeRange[0] / 2f, specialTimeRange[1] / 2f);
    }

    private void Update()
    {
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
                    "Videogames",
                    "Party",
                    "Movie Night",
                    "Jam Session",
                    "Cuddle Party"
                };
                string reason = reasons[Random.Range(0, reasons.Length)];
                for (j = 0; j < allTowns.Length;j++) {
                    allTowns[j].InviteToParty(venue, reason);
                }
            }
        }
        // Check if any trains need a target
        //for (i = 0; i < allTrains.Count;i++) {
        if (allTrains[i].GetTargetStop() == null)
        {
            for (j = 0; j < allTowns.Length; j++)
            {
                if (allTowns[j].IsConnected() && allTowns[j].GetBooker() == null)
                {
                    if (allTowns[j].GetTraveller() != null)
                    {
                        allTrains[i].SetTargetStop(allTowns[j].GetStop());
                        allTowns[j].Book(allTrains[i]);
                    }
                }
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
