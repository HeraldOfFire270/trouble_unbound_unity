using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;

public class OnEventChangeText : MonoBehaviour {

    // No more than 8 items of text per level
    public string[] Text = new string[8];
    public bool StartAsBlank;
    [SerializeField]
    private LevelFlags[] TextFlags = new LevelFlags[8];
    [SerializeField]
    private int[] DisplayTime = new int[8];
    private bool[] hasDisplayed;

    private int currentText;
    private int DefaultDisplayTime;

    private TextMeshPro txt;
    private TextMeshProUGUI GUItxt;
    private FadeOut fadeBack;
    private bool isGUI;

    void Awake () {
        txt = GetComponent<TextMeshPro>();
        
        hasDisplayed = new bool[TextFlags.Length];
        if (!txt)
        {
            GUItxt = GetComponent<TextMeshProUGUI>();
            isGUI = true;
        }
        else
        {
            isGUI = false;
        }

        fadeBack = GetComponent<FadeOut>();
        if (fadeBack) DefaultDisplayTime = fadeBack.DisplayTime;

        // If there's no text attached, destroy the object.
        if (!txt && !GUItxt) Destroy(this);

        currentText = 0;
        if (StartAsBlank) currentText = -1;

        for(int i = 0; i < Text.Length; i++)
        { 
            Text[i] = Text[i].Replace("[KEYUP]", "<#ffff00>" + "Up" + "</color>");
            Text[i] = Text[i].Replace("[KEYLEFT]", "<#ffff00>" + "Left" + "</color>");
            Text[i] = Text[i].Replace("[KEYRIGHT]", "<#ffff00>" + "Right" + "</color>");
            Text[i] = Text[i].Replace("[KEYDOWN]", "<#ffff00>" + "Down" + "</color>");
            Text[i] = Text[i].Replace("[JUMP]", "<#ffff00>" + "Jump" + "</color>");
            Text[i] = Text[i].Replace("[KEYRESET]", "<#ffff00>" + "Escape" + "</color>");
        }

        for(int i = 0; i < Text.Length; i++)
        {
            hasDisplayed[i] = false;
        }
        Debug.Log("<color=red>LevelFlags.None = " + (int)LevelFlags.None + " </color>");
        Debug.Log("<color=red>LevelFlags.Flag1 = " + (int)LevelFlags.Flag1 + " </color>");
        Debug.Log("<color=red>LevelFlags.Flag2 = " + (int)LevelFlags.Flag2 + " </color>");
        Debug.Log("<color=red>LevelFlags.Flag3 = " + (int)LevelFlags.Flag3 + " </color>");
        Debug.Log("<color=red>LevelFlags.Flag4 = " + (int)LevelFlags.Flag4 + " </color>");
        Debug.Log("<color=red>LevelFlags.Flag5 = " + (int)LevelFlags.Flag5 + " </color>");
        Debug.Log("<color=red>LevelFlags.Flag6 = " + (int)LevelFlags.Flag6 + " </color>");

    }
	
	// Update is called once per frame
	void Update () {

        for (int i = 0; i < Text.Length; i++)
        {
            if(!hasDisplayed[i])
            {
                Debug.Log("Text has not been displayed: " + i);
                if (TextFlags[i] != LevelFlags.None)
                {
                    if (GameManager.CheckFlag(TextFlags[i]))
                    {
                        currentText = i;
                        Debug.Log("Text flag is set: " + TextFlags[i] + ", Current text is: " + currentText);
                        if (isGUI)
                        {
                            GUItxt.text = Text[i];
                        }
                        else
                        {
                            txt.text = Text[i];
                        }

                        hasDisplayed[i] = true;
                        if (fadeBack)
                        {
                            // If a specific time is set, use it, otherwise revert to the default
                            if (DisplayTime[i] <= 0)
                            {
                                fadeBack.DisplayTime = DefaultDisplayTime;
                            }
                            else
                            {
                                fadeBack.DisplayTime = DisplayTime[i];
                            }
                            fadeBack.Show();
                        }
                        break;
                    }
                    else
                    {
                        Debug.Log("Text condition not met: " + i + ", Condition is: " + TextFlags[i]);
                    }
                }
                else
                {
                    Debug.Log("Text has no flag set: " + i);
                }
            }
            else
            {
                Debug.Log("Text has been displayed: " + i);
            }
        }

	}

    public void ResetText()
    {
        for(int i = 0; i < Text.Length; i++)
        {
            hasDisplayed[i] = false;
        }

        currentText = 0;
        if (StartAsBlank) currentText = -1;
    }
}
