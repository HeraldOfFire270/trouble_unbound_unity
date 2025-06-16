using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateTexture : MonoBehaviour {

    [SerializeField]
    private Texture[] ImageSequence;
    [SerializeField]
    private float AnimationSpeed = 0.04f;
    private Material sprite;
    private int frame;

    private void Awake()
    {
        sprite = GetComponent<Renderer>().material;
        if (!sprite) { Debug.Log("No material to animate!"); Destroy(this); }
    }

	// Update is called once per frame
	void Update () {
        StartCoroutine("TextureLoop", AnimationSpeed);
        sprite.mainTexture = ImageSequence[frame];
	}

    IEnumerator TextureLoop(float delay)
    {
        yield return new WaitForSeconds(delay);
        frame = (++frame) % ImageSequence.Length;
        StopCoroutine("TextureLoop");
    }
}
