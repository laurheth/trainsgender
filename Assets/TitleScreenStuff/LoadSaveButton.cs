using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LoadSaveButton : MonoBehaviour, IPointerClickHandler {

    public GameObject loadOrNotObj;
    LoadOrNot loadOrNot;

    private void Start()
    {
        loadOrNot = loadOrNotObj.GetComponent<LoadOrNot>();
    }

    public void OnPointerClick(PointerEventData eventdata) {
        //DontDestroyOnLoad(loadOrNotObj);
        loadOrNot.SetLoad(true);
        SceneManager.LoadScene("MainGame");
    }
}
