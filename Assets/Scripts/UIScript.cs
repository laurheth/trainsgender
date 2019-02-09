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
	}

    private void Update()
    {
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
