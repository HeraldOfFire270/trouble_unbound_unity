using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushBlock : BasicBlock {

    public float MoveSpeed = 0.1f;         // How fast does this block move when pushed?
    public BlockColor blockColor = BlockColor.Blue;
    public float PushAngle = 45.0f;
    public float ScaleSpeed = 0.1f;

    private bool isLocked;
    private Rigidbody rBody;
    private Vector3 moveTarget;
    private Transform TelePoint;
    private Vector3 warpTarget;
    private RigidbodyConstraints defaultConstraints;

    override protected void Start () {
        base.Start();
        rBody = GetComponent<Rigidbody>();

        __weight = 1;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private bool isGrounded()
    {
        bool raycastCheck;
        int LayerMask = (1 << 8 | (1 << 9));
        RaycastHit ray = new RaycastHit();
        raycastCheck = !Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out ray, 0.5f, LayerMask);
        return raycastCheck;
    }
}
