using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FadeOut : MonoBehaviour {

    public int DisplayTime = 5;
    public float fadeOutSpeed = 0.1f;
    public float fadeInSpeed = 0.1f;
    public bool StartInvisible;
    public bool NoFadeOut;
    public LevelFlags TriggerEvent;
    public LevelFlags FadeInEvent;
    public LevelFlags FadeStartEvent;
    public LevelFlags FadeEndEvent;
    private TextMeshPro rendA;
    private TextMeshProUGUI rendB;
    private float opacity;
    private float lastOpacity;
    private bool isFading;
    private float timer;
    private bool startFade;

    private bool FadeIn;

	// Use this for initialization
	void Start () {
        rendA = GetComponent<TextMeshPro>();
        rendB = GetComponent<TextMeshProUGUI>();
        isFading = false;
        timer = 0.0f;

        if (StartInvisible)
        {
            opacity = 0.0f;
            lastOpacity = 0.0f;
            FadeIn = true;
        }
        else
        {
            opacity = 1.0f;
            lastOpacity = 1.0f;
            FadeIn = false;
        }

        if (TriggerEvent != LevelFlags.None)
        {
            startFade = false;
        }
        else
        {
            startFade = true;
        };

        if (rendA)
        {
            rendA.alpha = opacity;
        }
        else if (rendB)
        {
            rendB.alpha = opacity;
        }
    }
	
    public void ResetText()
    {
        isFading = false;
        timer = 0.0f;
        if (StartInvisible)
        {
            opacity = 0.0f;
            lastOpacity = 0.0f;
            FadeIn = true;
        }
        else
        {
            opacity = 1.0f;
            lastOpacity = 1.0f;
            FadeIn = false;
        }

        if (TriggerEvent != LevelFlags.None)
        {
            startFade = false;
        }
        else
        {
            startFade = true;
        };

        if (rendA)
        {
            rendA.alpha = opacity;
        }
        else if (rendB)
        {
            rendB.alpha = opacity;
        }
    }

    public void Show(bool startInvis = false)
    {
        StartInvisible = startInvis;
        isFading = false;
        timer = 0.0f;

        if (StartInvisible)
        {
            opacity = 0.0f;
            lastOpacity = 0.0f;
            FadeIn = true;
        }
        else
        {
            opacity = 1.0f;
            lastOpacity = 1.0f;
            FadeIn = false;
        }

        if (TriggerEvent != LevelFlags.None)
        {
            startFade = GameManager.CheckFlag(TriggerEvent);
        }
        else
        {
            startFade = true;
        };

        if (rendA)
        {
            rendA.alpha = opacity;
        }
        else if (rendB)
        {
            rendB.alpha = opacity;
        }
    }

	// Update is called once per frame
	void Update () {
        if (startFade)
        {
            if (StartInvisible && FadeIn)
            {
                opacity += fadeInSpeed;
                if (opacity > 1)
                {
                    lastOpacity = opacity;
                    opacity = 1.0f;
                    FadeIn = false;
                    if (FadeInEvent != LevelFlags.None)
                    {
                        GameManager.AddFlag(FadeInEvent);
                    }
                }
            }
            else
            {
                if (isFading)
                {
                    lastOpacity = opacity;
                    opacity -= fadeOutSpeed;
                    if (opacity < 0)
                    {
                        opacity = 0;
                        isFading = false;
                        if (FadeEndEvent != LevelFlags.None)
                        {
                            GameManager.AddFlag(FadeEndEvent);
                        }
                    }
                }
                else
                {
                    if (!NoFadeOut)
                    {
                        timer += Time.deltaTime;
                        if (Mathf.FloorToInt(timer) >= DisplayTime)
                        {
                            isFading = true;
                            if (FadeStartEvent != LevelFlags.None)
                            {
                                GameManager.AddFlag(FadeStartEvent);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if(GameManager.CheckFlag(TriggerEvent))
            {
                startFade = true;
            }
        }

        if (rendA)
        {
            rendA.alpha = opacity;
        }
        else if (rendB)
        {
            rendB.alpha = opacity;
        }

    }
}
