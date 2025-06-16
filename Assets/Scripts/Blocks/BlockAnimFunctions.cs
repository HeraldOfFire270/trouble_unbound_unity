using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockAnimFunctions : MonoBehaviour {

    Animator anim;
    private Pushables parent;
    private bool isScenery = false;

    // Use this for initialization
    void Awake () {
        if (transform.root == this.transform) Destroy(this);
        parent = transform.root.GetComponent<Pushables>();
        if (parent == null) isScenery = true;
        anim = GetComponent<Animator>();
    }
	
    private void BlockDropped()
    {
        if (isScenery) return;
        anim.SetBool("Dropped", true);
        parent.TrapdoorWarp(TrapDoorState.BlockEntry);
    }

    private void BlockEmerged()
    {
        if (isScenery) return;
        anim.SetBool("Dropped", false);
        parent.TrapdoorWarp(TrapDoorState.BlockExit);
    }

    private void BlockMelted()
    {
        if (isScenery) return;
        anim.SetTrigger("Descaled");
        parent.Destroyed();
    }

    private void BlockSolidified()
    {
        // Do event if available
    }
}
