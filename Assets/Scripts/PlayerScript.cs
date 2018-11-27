using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour {

    public enum eng { GRN, RED, WHT, BLU, TOT }

    public List<CharacterScript> m_characters;
    public int m_id;
    public int[] m_energy;
    public BoardScript m_bScript;

	// Use this for initialization
	void Start ()
    {
        //m_energy = new int[4];
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
