using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadOrNot : MonoBehaviour {
    bool doLoad;
	// Use this for initialization
	void Start () {
        doLoad = false;
        DontDestroyOnLoad(gameObject);
	}
	
    public void SetLoad(bool ld) {
        doLoad = ld;
    }

    public bool DoLoad() {
        return doLoad;
    }
}
