using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockDetector : MonoBehaviour {

    public LevelFlags EventOnLock;

    private Pushables parent;

    void Start () {
        parent = transform.parent.GetComponent<Pushables>();
        if (!parent) Destroy(gameObject);
    }
	
    private void OnTriggerEnter(Collider cl)
    {
        if (parent.LockStatus)
        {
            Debug.Log("Parent is locked!");
            return;
        }
        switch (cl.gameObject.tag)
        {
            case "TargetDetector":
                // If we have a matching colour - lock us into place!
                if (cl.gameObject.GetComponent<TargetDetector>().targetColor == parent.blockColor)
                {
                    // Set parent block to the exact position of the target
                    parent.LockStatus = true;
                    parent.MoveStatus = false;
                    transform.parent.position = cl.gameObject.transform.position;
                    if (EventOnLock != LevelFlags.None)
                    {
                        GameManager.AddFlag(EventOnLock);
                    }

                    GameManager.LockedBlocks++;
                    GameManager.CheckWin();
                }
                break;
            case "WPConveyor":
                Vector3 angle = cl.gameObject.transform.forward;
                parent.CheckAndMove(cl.gameObject.transform.position + cl.gameObject.transform.forward, true);
                break;
            case "Lava":
                // Fire blocks are resistant to lava and merely sink; all others burn or melt.
                if (parent.blockColor == BlockColor.Ice) cl.gameObject.GetComponent<Magma>().Solidify();
                if (parent.blockColor != BlockColor.Fire) parent.Melt();
                break;
        }
    }

    private HitDirection GetConveyorDirection(Vector3 v)
    {
        if (v.x < 0) return HitDirection.Right;
        if (v.x > 0) return HitDirection.Left;
        if (v.z < 0) return HitDirection.Forward;
        if (v.z > 0) return HitDirection.Back;
        return HitDirection.None;
    }
}
