using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    [SerializeField]
    private bool IsMusic;
    private AudioSource sound;

    private LevelManager lm;
	// Use this for initialization
	void Awake () {
        lm = GameObject.FindObjectOfType<LevelManager>();
        sound = GetComponent<AudioSource>();
        if (IsMusic)
        {
            if (lm != null) sound.clip = lm.LevelMusic;
            sound.Play();
        }
	}
	
	void Update () {
        if (IsMusic)
        {
            if (GameManager.MusicOn)
            {
                sound.volume = GameManager.MusicVolume;
            }
            else
            {
                sound.volume = 0;
            }
        }
        else
        {
            if (GameManager.SoundOn)
            {
                sound.volume = GameManager.SoundVolume;
            }
            else
            {
                sound.volume = 0;
            }
        }
	}
}
