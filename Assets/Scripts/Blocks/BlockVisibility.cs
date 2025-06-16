using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockVisibility : MonoBehaviour {

    private bool isNew = true;
    private Color previousColor = Color.black;
    private float currentTransparency = 1.0f;
    private const float newTransparency = 0.3f;
    private const float fallOff = 0.1f;
    private Renderer rend;

    // Use this for initialization
    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (!rend)
        {
            rend = GetComponentInChildren<Renderer>();
        }
    }
    public void Translucency()
    {
        currentTransparency = newTransparency;

        if (isNew)
        {
            isNew = false;
            // Save the current shader
            previousColor = rend.material.GetColor("_Color");
        }

    }

    // Update is called once per frame
    void Update () {

        if (!isNew)
        {
            if (currentTransparency < 1.0f)
            {
                Color C = rend.material.GetColor("_Color");
                C.a = currentTransparency;
                rend.material.SetColor("_Color", C);
            }
            else
            {
                rend.material.SetColor("_Color", previousColor);
                Destroy(this);
            }
            currentTransparency += ((1.0f - newTransparency) * Time.deltaTime) / fallOff;
        }
    }
}
