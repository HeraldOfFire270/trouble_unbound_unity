using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour {

    public Transform destination;
    public float animationSpeed = 0.1f;
    public LevelFlags StartTrigger;
    public LevelFlags TriggerEvent;

    private BoxCollider coll;
    private bool active;

    private float _offset;

    public float Offset
    {
        get { return _offset; }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (destination) Gizmos.DrawLine(transform.position, destination.position);
    }

    void Start()
    {
        foreach(Transform child in transform)
        {
            if (!coll) coll = child.GetComponent<BoxCollider>();

            if (StartTrigger != LevelFlags.None)
            {
                child.gameObject.SetActive(false);
                if(coll) coll.enabled = false;
                active = false;
            }
        }
        
    }

    void FixedUpdate()
    {
        if(!active)
        {
            if (GameManager.CheckFlag(StartTrigger))
            {
                coll.enabled = true;

                foreach(Transform child in transform)
                {
                    child.gameObject.SetActive(true);
                }

                active = false;
            }
        }
        _offset += animationSpeed;
    }

    public void ResetTeleport()
    {
        if (StartTrigger != LevelFlags.None)
        {
            if (coll) coll.enabled = false;
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
                active = false;
            }
        }
    }

    public void Trigger()
    {
        if(TriggerEvent != LevelFlags.None)
        {
            GameManager.AddFlag(TriggerEvent);
        }
    }
}
