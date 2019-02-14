using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class QuitButton : MonoBehaviour, IPointerClickHandler {

    private void Start()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer) {
            gameObject.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventdata) {
        Application.Quit();
    }
}
