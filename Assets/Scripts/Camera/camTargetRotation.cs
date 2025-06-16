using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camTargetRotation : MonoBehaviour {


	// Use this for initialization
	void Start () {
        float y = transform.eulerAngles.y;
        transform.eulerAngles = (new Vector3(0, Mathf.Round((y / 90) * 90), 0));
        y = transform.eulerAngles.y;
        GameManager.CameraDir = 0;
        if (y == 90) GameManager.CameraDir = 1;
        if (y == 180) GameManager.CameraDir = 2;
        if (y == 270) GameManager.CameraDir = 3;
	}
	
	// Update is called once per frame
	void Update () {

        bool left = false;
        bool right = false;

        if (Input.GetKeyDown(KeyCode.Joystick1Button4))
        {
            right = true;
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button5))
        {
            left = true;
        }

        if (Input.GetKeyDown(KeyCode.Comma))
        {
            right = true;
            
        }

        if (Input.GetKeyDown(KeyCode.Period))
        {
            left = true;
        }

        if (!(left && right))
        {
            if (left)
            {
                transform.Rotate(0, -90, 0);
                GameManager.CameraDir--;
            }
            if (right)
            {
                transform.Rotate(0, 90, 0);
                GameManager.CameraDir++;
            }
        }
    }
}
