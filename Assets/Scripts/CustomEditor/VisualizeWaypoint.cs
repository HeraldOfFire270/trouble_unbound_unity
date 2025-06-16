using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum WaypointType
{
    Waypoint,
    SnapPoint,
}

public class VisualizeWaypoint : MonoBehaviour {

    [SerializeField]
    private WaypointType __w = WaypointType.Waypoint;

    private void OnDrawGizmos()
    {
        if (__w == WaypointType.SnapPoint)
        {
            Gizmos.color = Color.cyan;
        }
        else
        {
            Gizmos.color = Color.blue;
        }
        Gizmos.DrawCube(transform.position, new Vector3(0.1f, 0.1f, 0.1f));
    }

}
