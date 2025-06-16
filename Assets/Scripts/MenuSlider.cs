using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

enum ChangeValue{
    MusicVolume,
    SoundVolume,
}

public class MenuSlider : MonoBehaviour
{

    [SerializeField]
    private ChangeValue ValueToChange;
    public float SliderValue { get; private set; }

    private Slider slider;

    // Use this for initialization
    void Start() {
        slider = GetComponent<Slider>();
        if (!slider) Destroy(this);
        
        switch(ValueToChange)
        {
            case ChangeValue.MusicVolume:
                SliderValue = GameManager.MusicVolume;
                break;
            case ChangeValue.SoundVolume:
                SliderValue = GameManager.SoundVolume;
                break;
            default:
                SliderValue = 0.5f;
                break;
        }

        slider.value = SliderValue;
    }

    private void Update()
    {
        switch (ValueToChange)
        {
            case ChangeValue.MusicVolume:
                GameManager.MusicVolume = slider.value;
                break;
            case ChangeValue.SoundVolume:
                GameManager.SoundVolume = slider.value ;
                break;
            default:
                SliderValue = 0.5f;
                break;
        }
    }

}
