﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMainCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameManager.MainCamera = GetComponent<Camera>();
	}
}
