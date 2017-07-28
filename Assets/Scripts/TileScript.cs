using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour {

    public enum nbors {left, right, top, bottom};

    //public GameObject camera;
    public GameObject m_holding;
    public GameObject[] m_neighbors;
    public List<GameObject> m_radius;
    public int m_x;
    public int m_z;
    public BoardScript m_boardScript;

	// Use this for initialization
	void Start ()
    {
        m_radius = new List<GameObject>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnMouseDown()
    {
        // REFACTOR: Disallow the clicking of tiles if any menu is up
        PanelScript mainPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        PanelScript ActionPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.ACTION_PANEL].GetComponent<PanelScript>();
        PanelScript StsPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.STATUS_PANEL].GetComponent<PanelScript>();
        PanelScript AuxPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.AUXILIARY_PANEL].GetComponent<PanelScript>();
        if (mainPanScript.m_inView == true || ActionPanScript.m_inView == true || StsPanScript.m_inView == true || AuxPanScript.m_inView == true)
            return;

        m_boardScript.m_selected = gameObject;

        Renderer renderer = GetComponent<Renderer>();
        CharacterScript currPlayerScript = m_boardScript.m_currPlayer.GetComponent<CharacterScript>();
        TileScript currTileScript = m_boardScript.m_currTile.GetComponent<TileScript>();
        if (renderer.material.color == Color.blue) // If tile is blue when clicked, perform movement code
        {
            currPlayerScript.Movement(currTileScript, GetComponent<TileScript>());
            ClearRadius(currTileScript);

            return;
        }
        else if (renderer.material.color == new Color(1, 0, 0, 1) && m_holding) // Otherwise if color is red, perform action code
        {
            GameObject[] targets = new GameObject[1];
            targets[0] = m_holding;
            currPlayerScript.Action(targets);
            ClearRadius(currTileScript);

            return;
        }

        if (m_holding && m_holding.tag == "Player")
        {
            Renderer holdingR = m_holding.GetComponent<Renderer>();
            if (holdingR.material.color == Color.green)
            {
                mainPanScript.m_inView = true;

                // Assign character to panels
                mainPanScript.m_character = m_holding;
                CharacterScript cScript = m_holding.GetComponent<CharacterScript>();
                mainPanScript.m_cScript = cScript;
                mainPanScript.SetButtons();
            }
            else
            {
                AuxPanScript.m_inView = true;

                // Assign character to panels
                AuxPanScript.m_character = m_holding;
                CharacterScript cScript = m_holding.GetComponent<CharacterScript>();
                AuxPanScript.m_cScript = cScript;
                AuxPanScript.SetButtons();
            }
        }
    }

    // Reset all tiles to their original color
    private void ClearRadius(TileScript _tS)
    {
        if (_tS.m_radius.Count > 0)
        {
            for (int i = 0; i < _tS.m_radius.Count; i++)
            {
                Renderer sRend = _tS.m_radius[i].GetComponent<Renderer>();
                sRend.material.color = new Color(1, 1, 1, 0f);
            }
            _tS.m_radius.Clear();
        }
    }
}
