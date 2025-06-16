using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour {

    private Vector3 Destination;
    private Trapdoor parent;
    private bool hasReceived;
    private const int CheckLayers = (1 << 9);

    private void Awake()
    {
        hasReceived = false;
        parent = transform.parent.GetComponent<Trapdoor>();
        if (!parent)
        {
            Destroy(this);
        }
        else
        {
            Destination = parent.PairedTrapdoor.transform.position + parent.PairedTrapdoor.transform.up;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "BlockDetector")
        {   
            GameObject parentBlock = other.gameObject.transform.parent.gameObject;
            if (!parentBlock) return;
            if (parent.IsNewBlock(parentBlock))
            {
                if (!parent.IsReceiver)
                {
                    if (parentBlock.GetComponent<Pushables>().CanTeleport(parent.PairedTrapdoor.transform.position + parent.PairedTrapdoor.transform.up))
                    {
                        parent.Open();
                        parent.Send(parentBlock);
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (parent.IsReceiver)
        {
            if (hasReceived)
            {
                if (!parent.BoundCheck())
                {
                    parent.ClearBlock();
                    hasReceived = false;
                }
            }
            else
            {
                if(parent.BoundCheck())
                {
                    hasReceived = true;
                    parent.Close();
                }
            }
        }
    }
}
