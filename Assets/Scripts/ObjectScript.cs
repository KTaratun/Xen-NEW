using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectScript : MonoBehaviour {

    public TileScript m_tile;
    public BoardScript m_boardScript;
    public int m_width;
    public TileScript.nbors m_facing;

    // Use this for initialization
    void Start ()
    {
        m_width = 1;
        m_facing = TileScript.nbors.bottom;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
