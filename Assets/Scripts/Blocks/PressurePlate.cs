using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour {

    public bool isToggle;
    public LevelFlags ToggleEvent;
    public LevelFlags OnPadDepress;
    public LevelFlags OnPadRise;

    public float PressSpeed = 0.02f;

    private bool isActive = false;
    private float startY;
    private bool wasTriggered = false;

	// Use this for initialization
	void Start () {
        startY = transform.localPosition.y;
        if(!transform.parent)
        {
            Destroy(this);
        }
	}

    private void FixedUpdate()
    {
        RaycastHit ray;
        int layerMask = 1 << 9;
        
        if (Physics.Raycast(transform.parent.position, transform.parent.TransformDirection(Vector3.up), out ray, 1.0f, layerMask))
        {
            if(ray.collider.tag == "Pushables")
            {
                isActive = true;
            }
        }
        else
        {
            if (isActive)
            {
                if(!isToggle && (OnPadRise != LevelFlags.None))
                {
                    GameManager.AddFlag(OnPadRise);
                }
            }
            isActive = false;
        }
    }

    void Update () {

        float y = transform.localPosition.y;
        float targetY = y;

		if(isActive)
        {
            targetY = startY - 0.2f;
            if(isToggle)
            {
                GameManager.AddFlag(ToggleEvent);
            }
            else
            {
                GameManager.AddFlag(OnPadDepress);
            }
            wasTriggered = true;
        }
        else
        {
            targetY = startY;
            if(isToggle)
            {
                GameManager.ClearFlag(ToggleEvent);
            }
            else
            {
                GameManager.AddFlag(OnPadRise);
            }
        }

        if (y != targetY)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, new Vector3(transform.localPosition.x, targetY, transform.localPosition.z), PressSpeed);
        }
    }
}
