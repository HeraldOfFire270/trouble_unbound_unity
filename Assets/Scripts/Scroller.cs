using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ScrollType
{
    ScrollLeft,
    ScrollRight,
    ScrollUp,
    ScrollDown,
    ScrollUpLeft,
    ScrollUpRight,
    ScrollDownLeft,
    ScrollDownRight,
}

public class Scroller : MonoBehaviour {

    public ScrollType scroll;
    public float ScrollSpeed;

    private Renderer rend;
    private float Xoffset = 0;
    private float Yoffset = 0;

	// Use this for initialization
	void Start () {
        rend = GetComponent<Renderer>();

        // no renderer attached, so scrolling is meaningless. Remove this component from the object.
        if (!rend) Destroy(this);
	}
	
	// Update is called once per frame
	void Update () {

        // Just in case we don't catch the delete in time, safely exit out of update routine
        if (!rend) return;

		switch(scroll)
        {
            case ScrollType.ScrollLeft:
                Xoffset += ScrollSpeed * Time.deltaTime;
                break;
            case ScrollType.ScrollRight:
                Xoffset -= ScrollSpeed * Time.deltaTime;
                break;
            case ScrollType.ScrollUp:
                Yoffset -= ScrollSpeed * Time.deltaTime;
                break;
            case ScrollType.ScrollDown:
                Yoffset += ScrollSpeed * Time.deltaTime;
                break;
            case ScrollType.ScrollUpLeft:
                Yoffset -= ScrollSpeed * Time.deltaTime;
                Xoffset += ScrollSpeed * Time.deltaTime;
                break;
            case ScrollType.ScrollUpRight:
                Yoffset -= ScrollSpeed * Time.deltaTime;
                Xoffset -= ScrollSpeed * Time.deltaTime;
                break;
            case ScrollType.ScrollDownLeft:
                Yoffset += ScrollSpeed * Time.deltaTime;
                Xoffset += ScrollSpeed * Time.deltaTime;
                break;
            case ScrollType.ScrollDownRight:
                Yoffset += ScrollSpeed * Time.deltaTime;
                Xoffset -= ScrollSpeed * Time.deltaTime;
                break;
        }

        rend.material.mainTextureOffset = new Vector2(Xoffset, Yoffset);
	}
}
