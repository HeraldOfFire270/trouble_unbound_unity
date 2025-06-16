using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPAnimate : MonoBehaviour {

    private Renderer rend;
	void Start () {
        rend = GetComponent<Renderer>();
        if (!rend) Debug.Log("Nothing to render!");
	}

    // Update is called once per frame
    void Update()
    {
        float offset = 0.0f;
        Teleporter tp;
        if (transform.parent)
        {
            tp = transform.parent.GetComponent<Teleporter>();
            if (tp) offset = tp.Offset;
        }
        if (rend)
        {
            rend.material.mainTextureOffset = new Vector2(0, offset);
        }
    }
}
