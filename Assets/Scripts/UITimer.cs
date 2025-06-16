using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UITimer : MonoBehaviour {

    public bool isGameTimer = true;
    public bool CountUp;
    public float LowTimeInSeconds;
    public float MaxTimeInMinutes;
    public LevelFlags TimerStartEvent;
    public LevelFlags EventOnTimer;
    private TextMeshProUGUI txt;
    private float timer;
    private bool hasStarted;
    private bool Paused = false;

    private float seconds = 0;
    private float minutes = 0;
    private float hours = 0;

    private string newText;

	void Start () {
        LevelManager lm = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        txt = GetComponent<TextMeshProUGUI>();
        if (isGameTimer)
        {
            LowTimeInSeconds = lm.TimeWarning;
            MaxTimeInMinutes = lm.LevelTimer;
            CountUp = false;
            TimerStartEvent = LevelFlags.None;
        }

        if(!CountUp)
        {
            seconds = (MaxTimeInMinutes - Mathf.Floor(MaxTimeInMinutes)) * 60;
            minutes = Mathf.Floor(MaxTimeInMinutes) - (Mathf.Floor(MaxTimeInMinutes / 60.0f) * 60);
            hours = Mathf.Floor((MaxTimeInMinutes / 60));
        }

        if (TimerStartEvent == LevelFlags.None) hasStarted = true;

        newText = txt.text;
	}
	
	// Update is called once per frame
	void Update () {

        float totalTime = 0;
        string hrs;
        string mins;
        string secs;

        int h;
        int m;
        int s;

        if(hasStarted)
        {
            timer += Time.deltaTime;

            if (CountUp)
            {
                if(timer > 1)
                {
                    seconds += Mathf.FloorToInt(timer);
                    timer -= Mathf.FloorToInt(timer);
                }
                if (seconds == 60) { seconds = 0; minutes++; }
                if (minutes == 60) { minutes = 0; hours++; }

                s = Mathf.FloorToInt(seconds);
                m = Mathf.FloorToInt(minutes);
                h = Mathf.FloorToInt(hours);

                totalTime = (s * 60) + m + (s / 60);

                if(totalTime >= MaxTimeInMinutes)
                {
                    GameManager.AddFlag(EventOnTimer);
                }
                if(h > 0)
                {
                    hrs = hours.ToString() + ":";
                    if(m > 9)
                    {
                        mins = m.ToString() + ":";
                    }
                    else
                    {
                        mins = "0" + m.ToString() + ":";
                    }
                    if (s > 9)
                    {
                        secs = s.ToString();
                    }
                    else
                    {
                        secs = "0" + s.ToString();
                    }
                }
                else
                {
                    hrs = "";
                    mins = m.ToString() + ":";
                    if (s > 9)
                    {
                        secs = s.ToString();
                    }
                    else
                    {
                        secs = "0" + s.ToString();
                    }
                }

                newText = "Timer: " + hrs + mins + secs;
            }
            else
            {
                if (timer > 1)
                {
                    seconds -= Mathf.FloorToInt(timer);
                    timer -= Mathf.FloorToInt(timer);
                }
                if (seconds < 0) {
                    seconds = 59;
                    minutes--;
                    if (minutes < 0 && hours <= 0) { minutes = 0; seconds = 0; }
                }
                if (minutes < 0) { minutes = 59; hours--; }
                if (hours < 0) { hours = 0; }

                s = Mathf.FloorToInt(seconds);
                m = Mathf.FloorToInt(minutes);
                h = Mathf.FloorToInt(hours);

                totalTime = (h * 3600) + (m * 60) + s;

                if (h > 0)
                {
                    hrs = hours.ToString() + ":";
                    if (m > 9)
                    {
                        mins = m.ToString() + ":";
                    }
                    else
                    {
                        mins = "0" + m.ToString() + ":";
                    }
                    if (s > 9)
                    {
                        secs = s.ToString();
                    }
                    else
                    {
                        secs = "0" + s.ToString();
                    }
                }
                else
                {
                    hrs = "";
                    mins = m.ToString() + ":";
                    if (s > 9)
                    {
                        secs = s.ToString();
                    }
                    else
                    {
                        secs = "0" + s.ToString();
                    }
                }


                if (totalTime <= 0)
                {
                    GameManager.AddFlag(EventOnTimer);
                    newText = "<#ff0000>Timer: 0:00</color>";
                }
                else
                {
                    if(h > 0)
                {
                    hrs = hours.ToString() + ":";
                    if(m > 9)
                    {
                        mins = m.ToString() + ":";
                    }
                    else
                    {
                        mins = "0" + m.ToString() + ":";
                    }
                    if (s > 9)
                    {
                        secs = s.ToString();
                    }
                    else
                    {
                        secs = "0" + s.ToString();
                    }
                }
                else
                {
                    hrs = "";
                    mins = m.ToString() + ":";
                    if (s > 9)
                    {
                        secs = s.ToString();
                    }
                    else
                    {
                        secs = "0" + s.ToString();
                    }
                }

                    if(totalTime <= LowTimeInSeconds)
                    {
                        newText = "<#ff0000>Timer: " + mins + secs + "</color>";
                    }
                    else
                    {
                        if(hours > 0)
                        {
                            newText = "Timer: " + hrs + mins + secs;
                        }
                        else
                        {
                            newText = "Timer: " + mins + secs;
                        }
                    }
                }
            }
        }
        else
        {
            if (!Paused)
            {
                if (GameManager.CheckFlag(TimerStartEvent))
                {
                    hasStarted = true;
                }
            }
        }

		if(txt.text != newText)
        {
            txt.text = newText;
        }
	}

    public void ResetTimer()
    {
        if (!CountUp)
        {
            seconds = (MaxTimeInMinutes - Mathf.Floor(MaxTimeInMinutes)) * 60;
            minutes = Mathf.Floor(MaxTimeInMinutes) - (Mathf.Floor(MaxTimeInMinutes / 60.0f) * 60);
            hours = Mathf.Floor((MaxTimeInMinutes / 60));
        }

        if (TimerStartEvent == LevelFlags.None) hasStarted = true;
        timer = 0;
    }

    public void StopTimer()
    {
        Paused = true;
        hasStarted = false;
    }

    public void ResumeTimer()
    {
        Paused = false;
    }

    public int GetSeconds()
    {
        return Mathf.FloorToInt(seconds);
    }

    public int GetHours()
    {
        return Mathf.FloorToInt(hours);
    }

    public int GetMinutes()
    {
        return Mathf.FloorToInt(minutes);
    }
}
