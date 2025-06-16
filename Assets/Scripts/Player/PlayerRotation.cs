using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotation : MonoBehaviour {

    [SerializeField]
    private GameObject playerOne;
    private Rigidbody playerChar;
    private Quaternion InitialState;
    private bool Initialised = false;

    // Use this for initialization
	void Start () {
        playerChar = playerOne.GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        float x, z;
        x = playerChar.velocity.x;
        z = playerChar.velocity.z;

        if (x != 0 || z != 0)
        {
            Vector3 direction;

            direction.x = x;
            direction.y = 0;
            direction.z = z;

            if(direction != Vector3.zero) transform.rotation = Quaternion.LookRotation(direction);
        }
	}

    public void SetInitialState(Quaternion _rotation)
    {
        // Once called, ignore further calls.
        if(!Initialised)
        {
            InitialState = _rotation;
            transform.rotation = _rotation;
            Initialised = true;
        }
    }

    public void ResetRotation()
    {
        transform.rotation = InitialState;
    }
}
