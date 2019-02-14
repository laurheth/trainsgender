using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadOrNot : MonoBehaviour {
    bool doLoad;
    AudioSource source;
	// Use this for initialization
	void Start () {
        doLoad = false;
        DontDestroyOnLoad(gameObject);
        source = GetComponent<AudioSource>();
	}

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) {
            source.mute=!source.mute;
        }
    }


    public void SetLoad(bool ld) {
        doLoad = ld;
    }

    public bool DoLoad() {
        return doLoad;
    }
}
