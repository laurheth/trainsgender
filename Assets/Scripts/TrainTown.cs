﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrainTown : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    TrainTown[] allTowns;
    List<TrainsWoman> residents;
    //TrackController trackController;
    float nextTripRequest;
    bool connected;
    //bool infoPanelOpen;
    TrainStop stop;
    string name;
    float nameBoxTargSize;
    float nameBoxSize;
    float infoPanelSize;
    float infoPanelTargSize;
    public GameObject namePanel;
    public GameObject infoPanelTextObj;
    public GameObject infoPanel;
    public GameObject nameTextObj;
    RectTransform namePanelTransform;
    RectTransform infoPanelTransform;
    Text nameText;
    Text infoPanelText;
    TrainMover bookedBy;
    Image namePanelImage;

    CamScript camScript;
    Vector3 panelScale;
    Vector3 infoPanelVectSize;
    // Use this for initialization
    private void Awake()
    {
        bookedBy = null;
        //infoPanelOpen = false;
        residents = new List<TrainsWoman>();
        connected = false;
        stop = null;
        name = "Town";
        nameBoxTargSize = 0;
        nameBoxSize = 0;
        infoPanelSize = 0;
        infoPanelVectSize = Vector3.zero;
        infoPanelTargSize = 0;
    }
    void Start () {

        nextTripRequest = 0;
        panelScale = Vector3.one;
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
        namePanelTransform = namePanel.GetComponent<RectTransform>();
        nameText = nameTextObj.GetComponent<Text>();
        namePanelImage = namePanel.GetComponent<Image>();
        nameText.text = name;
        camScript = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CamScript>();
        infoPanelTransform = infoPanel.GetComponent<RectTransform>();
        infoPanelText = infoPanelTextObj.GetComponent<Text>();
	}

    private void Update()
    {
        if (Mathf.Approximately(nameBoxSize,0) && Mathf.Approximately(nameBoxSize, nameBoxTargSize)) {
            namePanelImage.enabled = false;
        }
        if (Mathf.Abs(nameBoxSize-nameBoxTargSize)>0.1) {
            namePanelImage.enabled = true;
            //Time.deltaTime*(nameBoxTargSize - nameBoxSize);
            nameBoxSize += 4*Time.deltaTime * ((nameBoxTargSize - nameBoxSize));
            if (Mathf.Abs((nameBoxTargSize - nameBoxSize))<0.5f) {
                nameBoxSize = nameBoxTargSize;
            }
            namePanelTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, nameBoxSize);

            panelScale = Vector3.one * camScript.GetScale();
            namePanelTransform.localScale = panelScale;
        }
        if (!Mathf.Approximately(infoPanelSize,infoPanelTargSize)) {
            infoPanelSize += 4 * Time.deltaTime * (infoPanelTargSize - infoPanelSize);
            if (Mathf.Abs(infoPanelTargSize-infoPanelSize)<0.01) {
                infoPanelSize = infoPanelTargSize;
            }
            infoPanelVectSize = camScript.GetScale() * infoPanelSize * Vector3.one;
            infoPanelTransform.localScale = infoPanelVectSize;
            //namePanelTransform.SetPositionAndRotation(new Vector3(0,infoPanelVectSize[1]), Quaternion.identity);
            namePanelTransform.anchoredPosition = new Vector3(0, infoPanelVectSize[1]*infoPanelTransform.sizeDelta.y);
        }

        if (bookedBy!=null) {
            if (stop != null)
            {
                if (bookedBy.GetTargetStop() == null || bookedBy.GetTargetStop().GridPosition() != stop.GridPosition())
                {
                    bookedBy = null;
                }
            }
        }
    }

    public void Book(TrainMover booker) {
        bookedBy = booker;
    }

    public TrainMover GetBooker() {
        return bookedBy;
    }

    public string GetName() {
        return name;
    }

    string GenTownName() {
        string newname;// = "";
        switch (Mathf.FloorToInt(Random.Range(0,10)))
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

        string[] prefix = { "Bark", "Meow", "Furry", "Lake", "Train", "Amazing",
            "Wood", "Log", "Face", "Gay", "Trans", "Chicken",
            "Pelican", "Ostrich", "Goose", "Well", "Cloud", "Cat", "Dog", "Ferret",
            "Dinosaur", "Music", "Anime"
        };
        string[] suffix = {
            "ton"," Town", "ville"," City"," Hollow", " Hill",
            " Village", " Coop", "fax", "don", "castle"
        };
        newname += prefix[Random.Range(0, prefix.Length)];
        newname += suffix[Random.Range(0, suffix.Length)];
            
        return newname;
    }
	// Update is called once per frame
	/*void Update () {
		
	}*/
    public void AddResident(TrainsWoman newresident) {
        residents.Add(newresident);
        newresident.SetTown(this);
    }

    public string GenName() {
        string[] firstNames = { "Susan", "Zoe", "Jennifer", "Lauren",
            "Alexis", "Alex", "Emma", "Bonnie", "Evelyn", "Christine",
        "Meghan", "Mary", "Carrie", "Daphne", "Bulma", "Velma","Betty","Betty",
        "Jade","Deborah","Laura","Diana","Athena","Alana","Natasha","Miriam",
        "Calliope","Jane","Marnie","Josephine","Diane","Katie","Sarah","Beatrice"};
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return firstNames[Random.Range(0,firstNames.Length-1)]+
            " "+chars[Random.Range(0,chars.Length)];
    }

    public TrainTown ChooseOtherTown() {
        TrainTown toReturn;
        int breaker = 0;
        float townRadius = transform.position.sqrMagnitude;
        //Debug.Log(allTowns.Length);
        do
        {
            breaker++;
            toReturn = allTowns[Random.Range(0, allTowns.Length)];
            townRadius *= 1.2f;
        } while ((toReturn == this || toReturn.transform.position.sqrMagnitude>townRadius) && breaker < 1000);
        return toReturn;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("click!!");
        //camScript.PlaceTile();
        string msg = "";

        if (residents.Count > 0)
        {
            for (int i = 0; i < residents.Count; i++)
            {
                //Debug.Log(resident.Description());
                if (i > 0)
                {
                    msg += "\n";
                }
                msg += residents[i].Description();
            }
        }
        else {
            msg = "Nobody is home.";
        }
        infoPanelTargSize=1;
        infoPanelText.text = msg;
        //infoPanelTargSize//
    }

    public void OnPointerEnter(PointerEventData eventData) {
        nameBoxTargSize = 32;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        nameBoxTargSize = 0;
        infoPanelTargSize = 0;
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
            if (residents[i].WantsToTravel() && residents[i].TargTown().IsConnected()) {
                toReturn = residents[i];
            }
        }
        return toReturn;
    }

    public bool ContainsHostess() {
        TrainsWoman toReturn = null;
        for (int i = 0; i < residents.Count; i++)
        {
            if (residents[i].HomeBody() && residents[i].HomeTown().IsConnected())
            {
                toReturn = residents[i];
            }
        }
        //return toReturn;
        return toReturn != null;
    }

    public void InviteToParty(TrainTown venue, string reason) {
        if (IsConnected()) {
            for (int i = 0; i < residents.Count; i++)
            {
                residents[i].SetSpecial(venue, "for the " + reason + "!");
                StartCoroutine(PartyOver(residents[i]));
            }
        }
    }

    IEnumerator PartyOver(TrainsWoman trainsWoman) {
        yield return new WaitForSeconds(120);
        trainsWoman.CancelSpecial();
        yield return null;
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
    TrainTown currentTown;
    TrainTown specialVisit;
    string specialVisitText;
    string name;
    TrainsWoman girlFriend;
    float nextTripRequest;
    bool wantsToTravel;
    bool homeBody;

    public TrainsWoman(TrainTown home, TrainsWoman gf=null, bool likesHome=false) {
        name = home.GenName();
        homeTown = home;
        currentTown = home;
        homeBody = likesHome;
        if (gf!=null) {
            girlFriend = gf;
            wantsToTravel = false;
        }
        else {
            wantsToTravel = true;
            TrainTown other = home.ChooseOtherTown();
            girlFriend = new TrainsWoman(other, this,true);
            other.AddResident(girlFriend);
        }
        specialVisit = null;
        specialVisitText = "";
    }

    public string GetName() {
        return name;
    }

    public void LeaveTown() {
        currentTown.LeaveTown(this);
        currentTown = null;
    }

    public bool WantsToTravel() {
        if (!wantsToTravel)
        {
            
            if (Time.time > nextTripRequest)
            {
                if (!homeBody || (currentTown != null && currentTown.GetName() != homeTown.GetName()))
                {
                    wantsToTravel = true;
                }
            }
        }
        if (specialVisit != null && currentTown != null) {
            if (specialVisit.GetName() != currentTown.GetName())
            {
                wantsToTravel = true;
            }
            else {
                wantsToTravel = false;
            }

        }
        return wantsToTravel;
    }

    public void SetSpecial(TrainTown tovisit, string reason) {
        
        specialVisit = tovisit;
        specialVisitText = reason;
        //StartCoroutine(this.CancelSpecial());
    }

    public void CancelSpecial() {
        //yield return new WaitForSeconds(120);
        specialVisit = null;
        specialVisitText = "";
        //yield return null;
    }

    public TrainsWoman GetGF() {
        return girlFriend;
    }

    public TrainTown GetTown() {
        return currentTown;
    }

    public TrainTown HomeTown() {
        return homeTown;
    }

    public bool HomeBody() {
        return homeBody;
    }

    public TrainTown TargTown() {
        if (specialVisit != null) {
            return specialVisit;
        }
        else if (currentTown != null && currentTown.GetName()!=homeTown.GetName()) {
            return homeTown;
        }
        else if ((girlFriend.GetTown() != null && currentTown != null) && girlFriend.GetTown().GetName() != currentTown.GetName()) {
            return girlFriend.GetTown();
        }
        else {
            return homeTown;
        }
    }

    public void SetTown(TrainTown newTown) {
        currentTown = newTown;
        if (specialVisit != null && newTown.GetName()==specialVisit.GetName()) {
            specialVisit = null;
        }
    }

    public void DoneTravelling() {
        wantsToTravel = false;
        nextTripRequest = Time.time + 60f;
    }

    public string Description() {
        string msg = "";
        msg += name;
        if (WantsToTravel()) {
            msg += " wants to go to " + TargTown().GetName();// +" to visit " + girlFriend.GetName()+"!";
            if (specialVisit != null) {
                msg += " " + specialVisitText;
            }
            else if (girlFriend.GetTown()!= null && TargTown().GetName() == girlFriend.GetTown().GetName()) {
                msg += " to visit " + girlFriend.GetName() + "!";
            }
            else {
                msg += " to get some rest!";
            }
        }
        else {
            if (currentTown!=null && currentTown.GetName()==homeTown.GetName() && specialVisit==null) {
                if (girlFriend.GetTown() != null && girlFriend.GetTown().GetName() == currentTown.GetName())
                {
                    msg += " is happy in " + currentTown.GetName() + " with " + girlFriend.GetName() + "!";
                }
                else {
                    if (homeBody)
                    {
                        msg += " is waiting for " + girlFriend.GetName() + " to visit from " + girlFriend.HomeTown().GetName() + " !";
                    }
                    else {
                        msg += " is getting some sleep before wanting to visit " + girlFriend.GetName() + " again!";
                    }
                }
            }
            else  {
                msg += " is having a very nice time!";
            }
        }
        return msg;
    }
}