using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour {

    [SerializeField]
    private Transform[] Waypoints;
    [SerializeField]
    private float PlatformSpeed;
    [SerializeField]
    private bool ReverseAtEnd;
    private int current = 1;
    private bool reverse = false;
    private int childCount = 0;

    // Use this for initialization
    void Start () {
        if (Waypoints.Length < 2) Destroy(this);
        childCount = transform.childCount;
        transform.position = Waypoints[0].position;
	}
	
	void FixedUpdate () {
        float speed = PlatformSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, Waypoints[current].position, speed);
        if (transform.position == Waypoints[current].position)
        {
            if (reverse)
            {
                if (--current < 0) { current = 1; reverse = false; }
            }
            else
            {
                if (ReverseAtEnd)
                {
                    if (++current >= Waypoints.Length) { current = Waypoints.Length - 2; reverse = true; }
                    if (current < 0) { Debug.LogError("Trying to access negatives in waypoint array!"); current = 0; }
                }
                else
                {
                    if (++current >= Waypoints.Length) current = 0;
                }
            }
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.transform.root.gameObject;
        if (obj == gameObject) return; 
        switch (other.tag)
        {
            case "Pushables":
                Pushables p = obj.GetComponent<Pushables>();
                Vector3 closestPoint = Vector3.up;
                other.transform.root.parent = transform;              
                p.Transporting(closestPoint);
                break;

            case "Player":
                other.transform.root.parent = transform;
                break;
            default:
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        bool unparent = false;
        switch (other.gameObject.tag)
        {
            case "Pushables":
                unparent = true;
                break;
            case "Player":
                unparent = true;
                break;
            default:
                break;
        }

        if (unparent)
        {
            foreach (Transform t in transform)
            {
                if (t.gameObject == other.gameObject)
                {
                    if (t.parent = transform) t.parent = null;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {

    }
}
