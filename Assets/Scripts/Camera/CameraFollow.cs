using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public GameObject mainPlayer;
    public GameObject pivot;
    public float ZoomSpeed = 0.01f;
    public float MaxZoomOut = 5.0f;

    public float turnSpeed = 128;
    private float zoom = 0;
    private float MaxZoomIn;

    private Vector3 camPos;

    // Use this for initialization
    void Start () {
        float distance;
        camPos = transform.GetChild(0).position;
		if (!mainPlayer)
        {
            mainPlayer = GameObject.Find("Player");

            distance = (mainPlayer.transform.position - camPos).magnitude;
            MaxZoomIn = -(distance + 1.0f);
        }
	}

    // Update is called once per frame
    void Update() {
        float distance;
        float speed;

        float Ltrig, Rtrig;

        distance = Vector3.Distance(mainPlayer.transform.position, gameObject.transform.position);

        speed = mainPlayer.GetComponent<Player>().runSpeed * (distance * Time.deltaTime);
        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, mainPlayer.transform.position, speed);

        UpdateRotation();

        Rtrig = Input.GetAxis("Right Trigger");
        Ltrig = Input.GetAxis("Left Trigger");

        Debug.Log("Left Trigger: " + Ltrig + " Right Trigger: " + Rtrig);

        if (Ltrig != Rtrig)
        {
            if (Rtrig == 1)
            {
                zoom += ZoomSpeed * Time.deltaTime;
                if (zoom > MaxZoomIn) zoom = MaxZoomIn;
            }

            if (Ltrig == 1)
            {
                zoom -= ZoomSpeed * Time.deltaTime;
                if (zoom < MaxZoomOut) zoom = MaxZoomOut;

            }
        }

        Debug.Log("Camera Zoom: " + zoom);
       
        GameManager.MainCamera.transform.localPosition = camPos - (GameManager.MainCamera.transform.forward * zoom);
    }

    void UpdateRotation()
    {
        float step = turnSpeed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, pivot.gameObject.transform.rotation, step);
    }


    bool PlayerVisible()
    {
        int intLayerMask = (int)PhysicsLayer.AllVisible;
        float fHeight = mainPlayer.GetComponent<Renderer>().bounds.size.y;
        int rayHits = 0;

        Vector3 _playerTop = mainPlayer.transform.position + (Vector3.up * fHeight);
        Vector3 _playerBot = mainPlayer.transform.position - (Vector3.up * fHeight);

        // Three point check system: Top, Mid, Bottom. If any one is false, we can see the player

        if (Physics.Linecast(GameManager.MainCamera.transform.position, _playerTop, intLayerMask))
        {
            rayHits++;
        }

        if (Physics.Linecast(GameManager.MainCamera.transform.position, mainPlayer.transform.position, intLayerMask))
        {
            rayHits++;
        }

        if (Physics.Linecast(GameManager.MainCamera.transform.position, _playerBot, intLayerMask))
        {
            rayHits++;
        }

        if(rayHits >= 2)
        {
            return false;
        }

        return true;
    }

}
