using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GoHomeButton : MonoBehaviour, IPointerClickHandler {

    public GameObject camObj;
	
    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(FlyHome());
    }

    IEnumerator FlyHome() {
        Transform tf = camObj.transform;
        
        while (tf.position.sqrMagnitude > 2) {
            tf.position = Vector3.Lerp(tf.position, 10*Vector3.back, 3*Time.deltaTime);
            yield return null;
            if (Input.anyKey) { break; }
        }
        yield return null;
    }
}
