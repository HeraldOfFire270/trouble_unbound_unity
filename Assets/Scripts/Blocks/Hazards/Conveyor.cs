using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conveyor : MonoBehaviour
{

    public float Rotation_Speed = 1.0f;
    public float Animation_Speed = 0.1f;

    private Renderer rend;
    private GameObject[] Rollers;

    private GameObject WaypointCollider;

    void Start()
    {
        GameObject obj;
        int r = 0;
        Rollers = new GameObject[4];
        for (int i = 0; i < transform.childCount; i++)
        {
            obj = transform.GetChild(i).gameObject;
            if (obj.tag == "Conveyor")
            {
                rend = obj.GetComponent<Renderer>();
            }
            if (obj.tag == "WPConveyor")
            {
                WaypointCollider = obj;
            }
            if (obj.tag == "Roller")
            {
                if (r < 4) Rollers[r++] = obj;
            }
        }

        if (!rend) Debug.LogError("No Renderer found in " + gameObject.ToString() + "!");
        if (!WaypointCollider) Debug.Log("No Waypoint found in " + gameObject.ToString() + "!");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float offset = Time.time * Animation_Speed;
        rend.material.mainTextureOffset = new Vector2(-offset, 0);

        for (int i=0; i < 4; i++)
        {
            Rollers[i].transform.Rotate(new Vector3(0, 0, Rotation_Speed));
        }
    }

    public GameObject Waypoint
    {
        get { return WaypointCollider; }
    }
}