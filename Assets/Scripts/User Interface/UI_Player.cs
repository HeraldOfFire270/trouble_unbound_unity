using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_Player : MonoBehaviour {

    public GameObject player;
    public UIDisplay Info;

    private TextMeshPro UIText;

	// Use this for initialization
	void Start () {
        UIText = GetComponent<TextMeshPro>();
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 velocity = Vector3.zero;
        if (player) velocity = player.GetComponent<Rigidbody>().velocity;

        switch (Info)
        {
            case UIDisplay.Blank:
                UIText.text = "";
                break;
            case UIDisplay.DebugVelocity:
                UIText.text = "Player Velocity: " + velocity;
                break;
            case UIDisplay.DebugOnGround:
                if(player) UIText.text = "OnGround: " + player.GetComponent<Player>().DebugGrounded;
                break;
        }
	}
}
