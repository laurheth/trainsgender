using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class QuitButton : MonoBehaviour, IPointerClickHandler {

    public void OnPointerClick(PointerEventData eventdata) {
        Application.Quit();
    }
}
