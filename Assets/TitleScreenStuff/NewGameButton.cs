using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class NewGameButton : MonoBehaviour, IPointerClickHandler {

    public void OnPointerClick(PointerEventData eventdata) {
        SceneManager.LoadScene("MainGame");
    }
}
