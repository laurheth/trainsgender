using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TrainTown : MonoBehaviour, IPointerClickHandler {

    TrainTown[] allTowns;
    List<TrainsWoman> residents;
    //TrackController trackController;
    float nextTripRequest;
    bool connected;
    TrainStop stop;
    string name;
    // Use this for initialization
    private void Awake()
    {
        residents = new List<TrainsWoman>();
        connected = false;
        stop = null;
        name = "Town";
    }
    void Start () {
        nextTripRequest = 0;

        //trackController
        List<string> townNames = new List<string>();
        GameObject[] townObjs = GameObject.FindGameObjectsWithTag("Town");
        allTowns = new TrainTown[townObjs.Length];
        for (int i = 0; i < townObjs.Length; i++)
        {
            allTowns[i] = townObjs[i].GetComponent<TrainTown>();
            townNames.Add(allTowns[i].GetName());
        }

        int breaker = 0;
        do
        {
            name = GenTownName();
            breaker++;
        } while (townNames.Contains(name) && breaker<1000);
        gameObject.name = name;

        residents.Add(new TrainsWoman(this));
	}
	
    public string GetName() {
        return name;
    }

    string GenTownName() {
        string newname;// = "";
        switch (Mathf.FloorToInt(Random.Range(0,4)))
        {
            case 0:
                newname = "New ";
                break;
            case 1:
                newname = "Old ";
                break;
            default:
                newname = "";
                break;
        }

        string[] prefix = { "Bark", "Meow", "Furry", "Lake", "Train", "Amazing", "Wood", "Log", "Face", "Gay", "Trans" };
        string[] suffix = { "ton"," Town", "ville"," City"," Hollow", " Hill" };
        newname += prefix[Random.Range(0, prefix.Length)];
        newname += suffix[Random.Range(0, suffix.Length)];
            
        return newname;
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

    public void SetConnected(TrainStop newstop) {
        stop = newstop;
        connected = (stop != null);
    }

    public bool IsConnected() {
        return connected;
    }

    public TrainsWoman GetTraveller() {
        TrainsWoman toReturn=null;
        for (int i = 0; i < residents.Count;i++) {
            if (residents[i].WantsToTravel() && residents[i].GetGF().GetTown().IsConnected()) {
                toReturn = residents[i];
            }
        }
        return toReturn;
    }

    public TrainStop GetStop() {
        return stop;
    }

    public void LeaveTown(TrainsWoman left) {
        residents.Remove(left);
    }
}

public class TrainsWoman {
    TrainTown homeTown;
    string name;
    TrainsWoman girlFriend;
    float nextTripRequest;
    bool wantsToTravel;

    public TrainsWoman(TrainTown home, TrainsWoman gf=null) {
        name = home.GenName();
        homeTown = home;
        if (gf!=null) {
            girlFriend = gf;
            wantsToTravel = false;
        }
        else {
            wantsToTravel = true;
            TrainTown other = home.ChooseOtherTown();
            girlFriend = new TrainsWoman(other, this);
            other.AddResident(girlFriend);
        }
    }

    public string GetName() {
        return name;
    }

    public void LeaveTown() {
        homeTown.LeaveTown(this);
        homeTown = null;
    }

    public bool WantsToTravel() {
        return wantsToTravel;
    }

    public TrainsWoman GetGF() {
        return girlFriend;
    }

    public TrainTown GetTown() {
        return homeTown;
    }

    public string Description() {
        string msg = "";
        msg += name;
        if (wantsToTravel) {
            msg += " wants to go to "+ girlFriend.GetTown().GetName() +" to visit " + girlFriend.GetName()+"!";
        }
        else {
            msg += " is waiting for " + girlFriend.GetName() + " to visit!";
        }
        return msg;
    }
}