using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSpark : MonoBehaviour {

    private Vector3 Destination;
    private Transform child = null;

    public bool PrimeSpark;

	void Start () {
        Destination = transform.position + transform.forward;
        if (transform.childCount > 0) child = transform.GetChild(0);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        transform.position = Vector3.MoveTowards(transform.position, Destination, 0.1f);
        if (transform.position == Destination)
        {
            // Unparent ourselves before annhilating
            if (child != null) child.parent = null;
            Destroy(gameObject);
        }
	}
}
