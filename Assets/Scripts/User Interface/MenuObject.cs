using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum MenuAction
{
    NoAction,
    StartNewGame,
    Resume,
    Restart,
    CloseGame,
    ReturnTitle,
    ReturnMenu,
    OpenLevelSelect,
    DisplayOptions,
    ToggleSound,
    ToggleMusic,
    Unlocks,
    Secrets,
}

public class MenuObject : MonoBehaviour,
    IPointerClickHandler,
    IDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler
    {

    [SerializeField]
    private bool SubMenuOption;
    [SerializeField]
    private MenuAction Action;
    [SerializeField]
    private Material Deselected;
    [SerializeField]
    private Material Selected;
    [SerializeField]
    private bool IsToggle = false;
    [SerializeField]
    private string ToggleTextOn;
    [SerializeField]
    private string ToggleTextOff;
    private CanvasRenderer rend;
    private LevelManager lm = null;
    private TMPro.TextMeshProUGUI txtMesh;

    // Use this for initialization
    void Awake  () {
        if (!Deselected) Deselected = gameObject.GetComponent<Material>();
        if (!Selected) Selected = gameObject.GetComponent<Material>();
        rend = gameObject.GetComponent<CanvasRenderer>();
        lm = GameObject.FindObjectOfType<LevelManager>();
        txtMesh = gameObject.GetComponent<TMPro.TextMeshProUGUI>();

        if (txtMesh != null) ToggleText();
    }
	
	// Update is called once per frame
	void Update () {
        
	}

    private void ToggleText()
    {
        if (!IsToggle) return;
        bool mBool = false;
        switch (Action)
        {
            case MenuAction.ToggleMusic:
                mBool = GameManager.MusicOn;
                break;
            case MenuAction.ToggleSound:
                mBool = GameManager.SoundOn;
                break;
        }

        if (mBool)
        {
            try { txtMesh.text = ToggleTextOn; } catch { Debug.LogError("No TextMesh object on toggle!"); }
        }
        else
        {
            try { txtMesh.text = ToggleTextOff; } catch { Debug.LogError("No TextMesh object on toggle!"); }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch(Action)
        {
            case MenuAction.StartNewGame:
                GameManager.LoadLevel("Tutorial");
                break;
            case MenuAction.DisplayOptions:
                if (lm)
                {
                    lm.Options();
                }
                break;
            case MenuAction.OpenLevelSelect:
                GameManager.LoadLevel("Level Select");
                break;
            case MenuAction.Restart:
                if (lm)
                {
                    lm.RestartLevel();
                }
                break;
            case MenuAction.Resume:
                if (lm)
                {
                    lm.Resume();
                }
                break;
            case MenuAction.CloseGame:
                Application.Quit();
                break;
            case MenuAction.ToggleMusic:
                GameManager.MusicOn = !GameManager.MusicOn;
                ToggleText();
                break;
            case MenuAction.ToggleSound:
                GameManager.SoundOn = !GameManager.SoundOn;
                ToggleText();
                break;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Action != MenuAction.NoAction)
        {
            IsSelected = true;
            rend.SetMaterial(Selected, 0);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Action != MenuAction.NoAction)
        {
            IsSelected = false;
            rend.SetMaterial(Deselected, 0);
        }
    }

    public bool IsSelected { get; private set; }

    public bool SubOption
    {
        get { return SubMenuOption; }
    }

}
