using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanCamera : MonoBehaviour {

    [SerializeField, Tooltip("Main camera")]
    private GameObject PlayerCamera;
    [SerializeField, Tooltip("Player character")]
    private GameObject player;
    [SerializeField, Tooltip("Speed of the pan.")]
    private float PanningSpeed = 0.1f;
    [SerializeField, Tooltip("Speed of rotation at end of pan to match main camera.")]
    private float RotationSpeed = 5.0f;
    [SerializeField, Tooltip("Flag to set upon starting the pan.")]
    private LevelFlags PanningStartEvent;
    [SerializeField, Tooltip("Flag to set upon reaching last waypoint.")]
    private LevelFlags PanningEndEvent;
    [SerializeField, Tooltip("Waypoints for panning.")]
    private Transform[] Waypoints;
    [SerializeField, Tooltip("Focal points during pan.")]
    private Transform[] FocalPoints;
    [SerializeField, Tooltip("Waypoints at which a focal shift occurs.")]
    private int[] FocalShiftWaypoints;

    private int current_waypoint = 0;
    private int current_focus = 0;
    private bool Lastwaypoint = false;

	// Use this for initialization
	void Start () {
        if (Waypoints.Length == 0)
        {
            if (PanningEndEvent != LevelFlags.None) GameManager.AddFlag(PanningEndEvent);
            PlayerCamera.GetComponent<Camera>().enabled = true;
            GetComponent<Camera>().enabled = false;
            Debug.Log("Destroying Panning camera");
            Destroy(gameObject);
        }
        else
        {
            Reset();
        }
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (Lastwaypoint)
        {
            transform.position = Vector3.MoveTowards(transform.position, PlayerCamera.transform.position, PanningSpeed * Time.deltaTime);
            if (player) transform.LookAt(player.transform);

            if (transform.position == PlayerCamera.transform.position)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, PlayerCamera.transform.rotation, RotationSpeed * Time.deltaTime);
                if (transform.rotation == PlayerCamera.transform.rotation)
                {
                    if (PanningEndEvent != LevelFlags.None) { GameManager.AddFlag(PanningEndEvent); }
                    PlayerCamera.GetComponent<Camera>().enabled = true;
                    GetComponent<Camera>().enabled = false;
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, Waypoints[current_waypoint].position, PanningSpeed * Time.deltaTime);
            if (transform.position == Waypoints[current_waypoint].position)
            {
                current_waypoint++;
                if (current_waypoint >= Waypoints.Length)
                {
                    Lastwaypoint = true;
                    current_waypoint--;
                }
                else
                {
                    if (current_focus != -1)
                    {
                        foreach (int i in FocalShiftWaypoints)
                        {
                            if (i == current_waypoint)
                            {
                                current_focus++;
                                break;
                            }
                        }
                    }
                }
            }
            if (current_focus != -1)
            {
                if (current_focus < FocalPoints.Length)
                {
                    transform.LookAt(FocalPoints[current_focus]);
                }
            }
        }
	}

    public void Reset()
    {

        if (!PlayerCamera)
        {
            Debug.LogError("No Main Camera assigned to PanCamera!");
            Destroy(gameObject);
        }
        else
        {
            PlayerCamera.GetComponent<Camera>().enabled = false;
        }

        if (PanningStartEvent != LevelFlags.None) { GameManager.AddFlag(PanningStartEvent); }
        current_focus = 0;
        current_waypoint = 0;
        Lastwaypoint = false;
        Debug.Log("Length of waypoint array: " + Waypoints.Length);
        transform.position = Waypoints[0].position;
        if (PanningSpeed == 0) PanningSpeed = 3.0f;
        if (RotationSpeed == 0) RotationSpeed = 20.0f;
        if (FocalShiftWaypoints.Length == 0)
        {
            current_focus = -1;
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 1; i < Waypoints.Length; i++)
        {
            if (Waypoints[i - 1] == null) return;
            if (Waypoints[i] == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Waypoints[i - 1].position, Waypoints[i].position);
        }
    }
}
