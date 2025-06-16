using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {

    public bool StartOpen = false;
    public LevelFlags TriggerEvent;

    private bool wasTriggered;
    private Renderer[] rends;
    private Collider[] colls;

	// Use this for initialization
	void Start () {
        rends = GetComponentsInChildren<Renderer>();
        colls = GetComponentsInChildren<Collider>();
        if (StartOpen) Open();
	}
	
	// Update is called once per frame
	void Update () {
		if(GameManager.CheckFlag(TriggerEvent))
        {
            Debug.Log("Flag is triggered!");
            if (!wasTriggered)
            {
                wasTriggered = true;
                if (StartOpen)
                {
                    Close();
                }
                else
                {
                    Open();
                }
            }
        }
        else
        {
            if (wasTriggered)
            {
                wasTriggered = false;
                if(StartOpen)
                {
                    Open();
                }
                else
                {
                    Close();
                }
            }
        }
	}

    void Open()
    {
        foreach (Collider c in colls)
        {
            c.enabled = false;
        }

        foreach(Renderer r in rends)
        {
            r.enabled = false;
        }
    }

    void Close()
    {
        foreach (Collider c in colls)
        {
            c.enabled = true;
        }

        foreach (Renderer r in rends)
        {
            r.enabled = true;
        }
    }
}
