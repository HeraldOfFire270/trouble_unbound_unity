using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LECompanion : MonoBehaviour
{
    private Vector3 sPos = Vector3.zero;
    private Vector3 size = new Vector3(1,1,1);
    private Vector3 pivot = new Vector3(0.5f, 0.5f, 0.5f);

    public bool canDraw = false;

    private void OnDrawGizmos()
    {
        if (!canDraw) return;

        transform.position = RoundVectorToInt(transform.position);

        Vector3 position = transform.position - pivot;

        float tX = position.x + size.x;
        float tY = position.y + size.y;
        float tZ = position.z + size.z;

        Gizmos.color = Color.white;
        Gizmos.DrawLine(position, new Vector3(position.x, position.y, tZ));
        Gizmos.DrawLine(position, new Vector3(position.x, tY, position.z));
        Gizmos.DrawLine(position, new Vector3(tX, position.y, position.z));
        Gizmos.DrawLine(new Vector3(tX, position.y, position.z), new Vector3(tX, position.y, tZ));
        Gizmos.DrawLine(new Vector3(tX, position.y, position.z), new Vector3(tX, tY, position.z));
        Gizmos.DrawLine(new Vector3(tX, position.y, tZ), new Vector3(position.x, position.y, tZ));
        Gizmos.DrawLine(new Vector3(tX, position.y, tZ), new Vector3(tX, tY, tZ));
        Gizmos.DrawLine(new Vector3(position.x, position.y, tZ), new Vector3(position.x, tY, tZ));
        Gizmos.DrawLine(new Vector3(tX, tY, tZ), new Vector3(tX, tY, position.z));
        Gizmos.DrawLine(new Vector3(tX, tY, tZ), new Vector3(position.x, tY, tZ));
        Gizmos.DrawLine(new Vector3(position.x, tY, position.z), new Vector3(position.x, tY, tZ));
        Gizmos.DrawLine(new Vector3(position.x, tY, position.z), new Vector3(tX, tY, position.z));
    }

    public void DrawBox( Vector3 dimensions)
    {
        size = dimensions;
        canDraw = true;
    }

    public void Clear()
    {
        sPos = Vector3.zero;
        size = new Vector3(1, 1, 1);
        pivot = new Vector3(0.5f, 0.5f, 0.5f);
        canDraw = false;
    }

    public static Vector3 RoundVectorToInt(Vector3 v)
    {
        Vector3 newVector = new Vector3((float)Mathf.RoundToInt(v.x), (float)Mathf.RoundToInt(v.y), (float)Mathf.RoundToInt(v.z));
        return newVector;
    }
}
