using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBlock : MonoBehaviour {

    [SerializeField]
    protected int __weight;
    [SerializeField]
    protected bool __isVisible;

    protected float __blockScale = 1.0f;
    protected Vector3 __startPos;

	virtual protected void Start () {
		if (!GetComponent<Rigidbody>().useGravity)
        {
            __weight = 0;
        }
	}

    public float isVisible
    {
        get { return isVisible; }
    }

    public int Weight
    {
        get { return __weight; }
    }
}
