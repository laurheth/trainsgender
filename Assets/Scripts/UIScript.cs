using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour {
    public GameObject loveObj;
    Text loveTxt;
    int love;
	// Use this for initialization
	void Start () {
        love = 0;
        loveTxt = loveObj.GetComponent<Text>();
	}
	
    public void AddLove() {
        love++;
        loveTxt.text = love.ToString();
    }
}
