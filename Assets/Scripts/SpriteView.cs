using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteView : MonoBehaviour {

    private GameObject panning;

    private void Start()
    {
        panning = GameObject.Find("Panning Camera");
    }

    // Update is called once per frame
    void Update () {
        Vector3 targetVector;
        if (panning != null)
        {
            targetVector = this.transform.position - panning.transform.position;
            transform.rotation = Quaternion.LookRotation(targetVector, panning.GetComponent<Camera>().transform.rotation * Vector3.up);
        }
        else
        {
            targetVector = this.transform.position - GameManager.MainCamera.transform.position;
            transform.rotation = Quaternion.LookRotation(targetVector, GameManager.MainCamera.transform.rotation * Vector3.up);
        }

        
	}
}
