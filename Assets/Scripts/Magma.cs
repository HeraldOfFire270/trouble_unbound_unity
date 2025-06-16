using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Magma : MonoBehaviour {

    public Vector2 Animation_Speed;
    private Collider hitBox = null;
    private Renderer rend;
    private Animator blockAnims;
    private Vector2 texOffset = Vector2.zero;
    private float smallscale;

    public bool IsSolid { get; private set; }

	void Awake () {
        IsSolid = false;
        foreach (BoxCollider c in GetComponentsInChildren<BoxCollider>())
        {
            if (c.isTrigger)
            {
                hitBox = c;
            }
            else
            {
                c.enabled = false;
            }
        }

        if (hitBox == null) { hitBox = GetComponent<BoxCollider>(); hitBox.isTrigger = true; }

        foreach(Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (r.gameObject.tag == "Lava")
            {
                rend = r;
            }
        }

        blockAnims = GetComponentInChildren<Animator>();
        if (!blockAnims) Debug.Log("No animator attached to child block!");
	}
	
    private void LateUpdate()
    {
        texOffset += (Animation_Speed * Time.deltaTime);
        if(rend.enabled)
        {
            rend.materials[0].SetTextureOffset("_MainTex", texOffset);
        }
    }

    public void Solidify()
    {
        if (blockAnims)
        {
            blockAnims.SetTrigger("Solidify");
        }
        else
        {
            Debug.LogError("No animator found on magma!: " + blockAnims);
        }
        foreach (BoxCollider c in GetComponentsInChildren<BoxCollider>())
        {
            c.enabled = true;
        }

        hitBox.enabled = false;
        IsSolid = true;
    }

    public void Liquify()
    {
        if (blockAnims) blockAnims.Play("magmaDefault");
        foreach (BoxCollider c in GetComponentsInChildren<BoxCollider>())
        {
            c.enabled = false;
        }

        hitBox.enabled = true;
        IsSolid = false;
    }

    public void Reset()
    {
        Liquify();
    }
}
