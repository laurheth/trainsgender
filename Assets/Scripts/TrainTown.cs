using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TrainTown : MonoBehaviour, IPointerClickHandler {

    TrainTown[] allTowns;
    List<TrainsWoman> residents;
    //TrackController trackController;
    float nextTripRequest;
    // Use this for initialization
    private void Awake()
    {
        residents = new List<TrainsWoman>();
    }
    void Start () {
        nextTripRequest = 0;

        //trackController
        GameObject[] townObjs = GameObject.FindGameObjectsWithTag("Town");
        allTowns = new TrainTown[townObjs.Length];
        for (int i = 0; i < townObjs.Length; i++)
        {
            allTowns[i] = townObjs[i].GetComponent<TrainTown>();
        }

        residents.Add(new TrainsWoman(this));
	}
	
	// Update is called once per frame
	/*void Update () {
		
	}*/
    public void AddResident(TrainsWoman newresident) {
        residents.Add(newresident);
    }

    public string GenName() {
        string[] firstNames = { "Susan", "Zoe", "Jennifer", "Lauren",
            "Alexis", "Alex", "Emma", "Bonnie", "Evelyn", "Christine",
        "Meghan", "Mary", "Carrie", "Daphne", "Bulma", "Velma","Betty","Betty"};
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return firstNames[Random.Range(0,firstNames.Length-1)]+
            " "+chars[Random.Range(0,chars.Length)];
    }

    public TrainTown ChooseOtherTown() {
        TrainTown toReturn;
        int breaker = 0;
        Debug.Log(allTowns.Length);
        do
        {
            breaker++;
            toReturn = allTowns[Random.Range(0, allTowns.Length)];
        } while (toReturn == this && breaker<1000);
        return toReturn;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("click!!");
        //camScript.PlaceTile();
        foreach (TrainsWoman resident in residents)
        {
            Debug.Log(resident.Description());
        }
    }
}

public class TrainsWoman {
    TrainTown homeTown;
    string name;
    TrainsWoman girlFriend;
    float nextTripRequest;
    bool WantsToTravel;

    public TrainsWoman(TrainTown home, TrainsWoman gf=null) {
        name = home.GenName();
        homeTown = home;
        if (gf!=null) {
            girlFriend = gf;
            WantsToTravel = false;
        }
        else {
            WantsToTravel = true;
            TrainTown other = home.ChooseOtherTown();
            girlFriend = new TrainsWoman(other, this);
            other.AddResident(girlFriend);
        }
    }

    public string GetName() {
        return name;
    }

    public string Description() {
        string msg = "";
        msg += name;
        if (WantsToTravel) {
            msg += " wants to go to TownName to visit " + girlFriend.GetName()+"!";
        }
        else {
            msg += " is waiting for " + girlFriend.GetName() + " to visit!";
        }
        return msg;
    }
}