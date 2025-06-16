using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

    public AudioClip LevelMusic;

    [Tooltip("Sets the event needed to win the level.")]
    public LevelFlags LevelWinEvent;
    [Tooltip("Flag to set upon meeting the level completion requirements.")]
    public LevelFlags EventOnLevelEnd;
    [Tooltip("Flag to set upon failing to meet the level completion requirements.")]
    public LevelFlags EventOnLevelFail;
    [Tooltip("Flag to set upon restarting a level.")]
    public LevelFlags EventOnRestart;
    [Tooltip("Sets the event needed to stop player movement temporarily.")]
    public LevelFlags FreezeEvent;          // Event freezes player movement
    [Tooltip("Length of time player should remain frozen, in seconds.")]
    public int FreezeTimer;                 // Freeze time in seconds
    [Tooltip("Flag to set when the freeze timer concludes.")]
    public LevelFlags EventOnFreezeTimer;
    [Tooltip("Sets the event needed to restore player movement.")]
    public LevelFlags UnfreezeEvent;        // Event releases all frozen objects
    [Tooltip("Time for level in minutes.")]
    public float LevelTimer; 
    [Tooltip("Time left, in seconds, to be displayed in as low time warning.")]
    public float TimeWarning;

    private float gameSpeed = 1.0f;
    private bool timerActive = false;
    private bool Ending;
    private float fTimer;

    private Player MainChar;
    private Animator ScreenFade;
    private GameObject PauseMenu;
    private MenuState menuState = MenuState.NoActiveMenu;
    private MenuState formerState = MenuState.PauseMenu;
    private int menuVisible = 0;        // 1 = 

    public bool gamePaused { get; private set; }

    private void Awake()
    {
        /// Just like in Highlander, there can be only one.
        LevelManager[] lm = FindObjectsOfType<LevelManager>();
        foreach(LevelManager l in lm)
        {
            if (l != this) Destroy(this.gameObject);
        }
    }

    void Start()
    {
        MainChar = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        if (!MainChar) Debug.LogWarning("No player in game!");

        ScreenFade = transform.GetChild(0).GetComponent<Animator>();
        if (!ScreenFade) Debug.LogWarning("No canvas attached to LevelManager!");

        PauseMenu = GameObject.Find("PauseMenu");
        gamePaused = false;
        HidePauseMenu();
    }


    // Update is called once per frame
    void Update() {

        CheckMenuState();

        if (gamePaused)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = gameSpeed;
        }

        if (timerActive)
        {
            fTimer += Time.deltaTime;
            if (Mathf.FloorToInt(fTimer) >= FreezeTimer)
            {
                if (EventOnFreezeTimer != LevelFlags.None)
                {
                    GameManager.AddFlag(EventOnFreezeTimer);
                }

                // Unfreeze player after time if there is no Unfreeze event
                // to prevent character from being frozen permanently
                if (UnfreezeEvent == LevelFlags.None)
                {
                    MainChar.Freeze(false);
                }
                timerActive = false;
                fTimer = 0;
            }
        }

        if (UnfreezeEvent != LevelFlags.None)
        {
            if (GameManager.CheckFlag(UnfreezeEvent))
            {
                MainChar.Freeze(false);
            }
        }

        if (GameManager.CheckWin())
        {
            if (EventOnLevelEnd != LevelFlags.None)
            {
                GameManager.AddFlag(EventOnLevelEnd);
            }
        }

        if (GameManager.Failed)
        {
            if (EventOnLevelFail != LevelFlags.None)
            {
                GameManager.AddFlag(EventOnLevelFail);
            }
        }

        if (GameManager.CheckFlag(LevelWinEvent))
        {
            // We've won, so move to the next level!
            GameManager.LoadNext();
        }

        if (!timerActive)
        {
            if (FreezeEvent != LevelFlags.None)
            {
                if (GameManager.CheckFlag(FreezeEvent))
                {
                    // Only freeze character if the unfreeze event hasn't yet triggered!
                    if (UnfreezeEvent != LevelFlags.None)
                    {
                        if (!GameManager.CheckFlag(UnfreezeEvent))
                        {
                            ScreenFade.SetTrigger("FadeOutNormal");
                            MainChar.Freeze(true);
                            timerActive = true;
                        }
                    }
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            switch(menuState)
            {
                case MenuState.NoActiveMenu:
                    gamePaused = true;
                    menuState = MenuState.PauseMenu;
                    break;
                case MenuState.PauseMenu:
                    gamePaused = false;
                    menuState = MenuState.NoActiveMenu;
                    break;
                case MenuState.OptionsMenu:
                    gamePaused = true;
                    menuState = MenuState.PauseMenu;
                    break;
            }
        }
    }

    public void Resume()
    {
        gamePaused = false;
        menuState = MenuState.NoActiveMenu;
    }

    public void Options()
    {
        gamePaused = true;
        menuState = MenuState.OptionsMenu;
    }

    public void SlowMo(bool enabled)
    {
        if (enabled)
        {
            gameSpeed = 0.5f;
        }
        else
        {
            gameSpeed = 1.0f;
        }
    }

    public void RestartLevel()
    {
        gamePaused = false;
        menuState = MenuState.NoActiveMenu;
        ScreenFade.SetTrigger("FadeOutQuick");

        GameObject[] block = GameObject.FindGameObjectsWithTag("Pushables");
        Pushables p;

        foreach (GameObject b in block)
        {
            p = b.GetComponent<Pushables>();
            if (p) p.ResetBlock();
        }

        block = GameObject.FindGameObjectsWithTag("Teleporter");
        Teleporter t;

        foreach (GameObject b in block)
        {
            t = b.GetComponent<Teleporter>();
            if (t) t.ResetTeleport();
        }

        block = GameObject.FindGameObjectsWithTag("UI");
        FadeOut f;
        OnEventChangeText e;

        foreach (GameObject b in block)
        {
            f = b.GetComponent<FadeOut>();
            e = b.GetComponent<OnEventChangeText>();
            if (f) f.ResetText();
            if (e) e.ResetText();
        }

        block = GameObject.FindGameObjectsWithTag("Lava");
        Magma m;

        foreach (GameObject b in block)
        {
            m = b.GetComponent<Magma>();
            if (m) m.Reset();
        }

        PanCamera Pcam = GameObject.FindObjectOfType<PanCamera>();

        if (Pcam != null) Pcam.Reset();

        GameManager.Failed = false;
        GameManager.LockedBlocks = 0;
        GameManager.ResetLevel();

        if (EventOnRestart != LevelFlags.None)
        {
            GameManager.AddFlag(EventOnRestart);
        }

        UITimer[] timers = GetComponentsInChildren<UITimer>();

        foreach (UITimer tm in timers)
        {
            tm.ResetTimer();
        }


        gamePaused = false;
        if (MainChar) MainChar.ResetPosition();
        timerActive = false;
        fTimer = 0.0f;
    }

    private void ShowPauseMenu()
    {
        HideOptionsMenu();
    }

    private void HidePauseMenu()
    {
        HideOptionsMenu();
        PauseMenu.SetActive(false);
    }

    private void ShowOptionsMenu()
    {
        PauseMenu.SetActive(true);
        MenuObject[] options = PauseMenu.GetComponentsInChildren<MenuObject>(true);
        foreach(MenuObject m in options)
        {
            if(m.SubOption)
            {
                m.gameObject.SetActive(true);
            }
            else
            {
                m.gameObject.SetActive(false);
            }
        }
    }

    private void HideOptionsMenu()
    {
        PauseMenu.SetActive(true);
        MenuObject[] options = PauseMenu.GetComponentsInChildren<MenuObject>(true);
        foreach (MenuObject m in options)
        {
            if (m.SubOption)
            {
                m.gameObject.SetActive(false);
            }
            else
            {
                m.gameObject.SetActive(true);
            }
        }
        GameManager.SaveConfig();
    }

    private void CheckMenuState()
    {
        switch(menuState)
        {
            case MenuState.PauseMenu:
                if(formerState != MenuState.PauseMenu)
                {
                    formerState = menuState;
                    ShowPauseMenu();
                }
                break;
            case MenuState.OptionsMenu:
                if (formerState != MenuState.OptionsMenu)
                {
                    formerState = menuState;
                    ShowOptionsMenu();
                }
                break;
            case MenuState.NoActiveMenu:
                if (formerState != MenuState.NoActiveMenu)
                {
                    formerState = menuState;
                    HidePauseMenu();
                }
            break;
        }
    }
}
