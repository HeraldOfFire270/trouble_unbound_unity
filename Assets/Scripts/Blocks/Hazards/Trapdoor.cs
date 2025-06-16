using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trapdoor : MonoBehaviour {

    public GameObject PairedTrapdoor;        // Which door are we paired to?
    public Color color;
    private BoxCollider coll;               // Collider to disable
    private Animator anim;                  // Animator
    private Vector3 Destination;
    private HitDirection destDir;           // Which direction is the destination door facing?
    private bool Receiver;                  // Are we receiving a block?
    private GameObject receivingBlock;      // Which block are we receiving?
    private GameObject currentBlock;        // Which block are we handling currently?
    private GameObject lastBlock;           // what was the last block we received?
    private Collider BoundingBox;
    private TextMesh txt;

    void Awake()
    {
        anim = GetComponent<Animator>();
        coll = GetComponent<BoxCollider>();
        txt = GetComponent<TextMesh>();
        Receiver = false;
        if (PairedTrapdoor == gameObject) Debug.LogError("Cannot pair trapdoor to itself!");
        if (PairedTrapdoor) Destination = PairedTrapdoor.transform.position + PairedTrapdoor.transform.up;
        if (Destination == Vector3.zero) Debug.Log("No Destination!");

        destDir = GetDirection();


        foreach(Transform child in transform)
        {
            if(child.gameObject.tag == "BoundingBox")
            {
                BoundingBox = child.gameObject.GetComponent<Collider>();
                if (!BoundingBox) Debug.Log("No bounding box found!");
            }
        }
    }

    private void Update()
    {
        if (txt)
        {
            if(Receiver)
            {
                txt.color = Color.red;
            }
            else
            {
                txt.color = Color.green;
            }
            txt.text = "Is receiving: " + Receiver + "\nCollider Enabled: " + coll.enabled;
        }
    }

    void Opened()
    {
        if (Receiver)
        {
            currentBlock.GetComponent<Pushables>().TrapdoorWarp(TrapDoorState.DestinationOpen);
            lastBlock = currentBlock;
        }
        else
        {
            currentBlock.GetComponent<Pushables>().TrapdoorWarp(this);
            currentBlock.GetComponent<Pushables>().TrapdoorWarp(this);
        }
    }

    void Closed()
    {
        if (!Receiver)
        {
            currentBlock.GetComponent<Pushables>().TrapdoorWarp(TrapDoorState.SourceClosed);
            PairedTrapdoor.GetComponent<Trapdoor>().Open();
        }
        currentBlock = null;
    }

    public void Send(GameObject block)
    {
        if (currentBlock == null)
        {
            if (block.tag == "Pushables") currentBlock = block;
        }
        else
        {
            if (currentBlock != block) Debug.LogError("Multiple Blocks received!");
        }
    }

    public void Receive(GameObject block)
    {
        if (block.tag == "Pushables")
        {
            currentBlock = block;
            Receiver = true;
            PairedTrapdoor.GetComponent<Trapdoor>().Close();
        }
    }

    public void Open()
    {
        anim.SetTrigger("Open Door");
    }

    public void Close()
    {
        anim.SetTrigger("Close Door");
    }

    public HitDirection GetDirection()
    {
        HitDirection dir = HitDirection.None;
        Vector3 direction = Destination - PairedTrapdoor.transform.position;
        if (direction == Vector3.up) dir = HitDirection.Top;
        if (direction == Vector3.down) dir = HitDirection.Bottom;
        if (direction == Vector3.forward) dir = HitDirection.Forward;
        if (direction == Vector3.back) dir = HitDirection.Back;
        if (direction == Vector3.left) dir = HitDirection.Left;
        if (direction == Vector3.right) dir = HitDirection.Right;
        if (dir == HitDirection.None) Debug.LogError("Unsupported rotation on targt trapdoor: " + direction);

        return dir;
    }

    private void OnDrawGizmos()
    {
        if (PairedTrapdoor) Destination = PairedTrapdoor.transform.position + PairedTrapdoor.transform.up;
        Gizmos.color = Color.green;
        if (PairedTrapdoor) Gizmos.DrawLine((transform.position + transform.up), (Destination));
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position + transform.up, 0.05f);
    }

    public Vector3 GetDestination
    {
        get { return Destination; }
    }

    public bool IsReceiver
    {
        get { if (lastBlock) return true; else return false; }
    }

    public void ClearBlock()
    {
        receivingBlock = null;
        lastBlock = null;
        currentBlock = null;
        Receiver = false;
        if (AnimationPlaying("TrapDoorOpened") || AnimationPlaying("TrapDoorOpen")) anim.SetTrigger("Close Door");
    }

    public bool IsNewBlock(GameObject obj)
    {
        if (obj == lastBlock) return false;
        if (obj == currentBlock) return false;
        return true;
    }

    private bool AnimationPlaying(string animName)
    {
        return (anim.GetCurrentAnimatorStateInfo(0).IsName(animName));
    }

    public bool BoundCheck()
    {
        if (!lastBlock)
        {
            Debug.Log("No block received!");
            return false;
        }
        bool hasFound = false;
        if (BoundingBox.bounds.Contains(lastBlock.transform.position))
        {
            if (lastBlock.GetComponent<Pushables>().Teleporting)
            {
                hasFound = false;
            }
            else
            {
                hasFound = true;
            }
        }
        return hasFound;
    }

    public void Reset()
    {
        ClearBlock();
        anim.Play("TrapDoorClosed");
    }

}
