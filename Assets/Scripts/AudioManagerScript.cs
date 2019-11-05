using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerScript : MonoBehaviour {

    AudioSource m_audio;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlaySound(string _sound)
    {
        m_audio.PlayOneShot(Resources.Load<AudioClip>(_sound));
    }
}
