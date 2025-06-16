using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetDetector : MonoBehaviour {

    public BlockColor targetColor = BlockColor.Blue;

    private void Awake()
    {
        if((int)targetColor < (int)BlockColor.Ice) GameManager.BlockCount++;
    }

}