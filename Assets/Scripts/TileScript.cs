using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour {

    public enum nbors {left, right, top, bottom};

    //public GameObject camera;
    public GameObject m_holding;
    public GameObject[] m_neighbors;
    public List<GameObject> m_radius;
    public List<GameObject> m_targetRadius;
    public int m_x;
    public int m_z;
    public BoardScript m_boardScript;
    private Color m_oldColor;

	// Use this for initialization
	void Start ()
    {
        m_radius = new List<GameObject>();
        m_oldColor = Color.black;
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
            List<GameObject> targets = new List<GameObject>();
            targets.Add(m_holding);
            currPlayerScript.Action(targets);
            TileScript tScript = currPlayerScript.m_tile.GetComponent<TileScript>();

            if (!currPlayerScript.m_isForcedMove)
                ClearRadius(currTileScript);

            return;
        }
        else if (renderer.material.color == Color.yellow)
        {
            List<GameObject> targets = new List<GameObject>();
            for (int i = 0; i < m_targetRadius.Count; i++)
            {
                TileScript tarTile = m_targetRadius[i].GetComponent<TileScript>();
                if (tarTile.m_holding)
                    targets.Add(tarTile.m_holding);
            }

            if (targets.Count > 0)
            {
                currPlayerScript.Action(targets);
                TileScript tScript = currPlayerScript.m_tile.GetComponent<TileScript>();

                ClearRadius(this);

                if (!currPlayerScript.m_isForcedMove)
                    ClearRadius(currTileScript);
            }

            return;
        }

        if (m_holding && m_holding.tag == "Player" && !currPlayerScript.m_isForcedMove)
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

    public void FetchTilesWithinRange(int _range, Color _color, bool _isOnlyHorVert)
    {
        // REFACTOR: Maybe less lists?
        List<TileScript> workingList = new List<TileScript>();
        List<TileScript> storingList = new List<TileScript>();
        List<TileScript> oddGen = new List<TileScript>();
        List<TileScript> evenGen = new List<TileScript>();

        // Start with current tile in oddGen
        oddGen.Add(this);

        if (_color == Color.yellow)
        {
            Renderer myRend = GetComponent<Renderer>();
            m_oldColor = myRend.material.color;
            myRend.material.color = _color;
            m_targetRadius.Add(gameObject);
        }

        for (int i = 0; i < _range; i++)
        {
            // Alternate between gens. Unload current gen and load up the gen and then swap next iteration
            if (oddGen.Count > 0)
            {
                workingList = oddGen;
                storingList = evenGen;
            }
            else if (evenGen.Count > 0)
            {
                workingList = evenGen;
                storingList = oddGen;
            }

            while (workingList.Count > 0)
            {
                for (int k = 0; k < 4; k++)
                {
                    if (!workingList[0].m_neighbors[k])
                        continue;

                    TileScript tScript = workingList[0].m_neighbors[k].GetComponent<TileScript>();

                    // if color is movement color
                    CharacterScript charScript = m_boardScript.m_currPlayer.GetComponent<CharacterScript>();
                    if (_color == new Color(0, 0, 1, 0.5f) && tScript.m_holding || _color == new Color(1, 0, 0, 0.5f) && workingList[0].m_neighbors[k] == m_boardScript.m_selected && charScript.m_currRadius == 0)
                        continue;

                    if (_isOnlyHorVert && tScript.m_x != m_x && tScript.m_z != m_z)
                        continue;

                    if (workingList[0].m_neighbors[k])
                    {
                        Renderer tR = workingList[0].m_neighbors[k].GetComponent<Renderer>();
                        if (tR.material.color != _color)
                        {
                            if (_color == Color.yellow)
                                tScript.m_oldColor = tR.material.color;

                            tR.material.color = _color;
                            storingList.Add(tScript);

                            if (_color == Color.yellow)
                                m_targetRadius.Add(workingList[0].m_neighbors[k]);
                            else
                                m_radius.Add(workingList[0].m_neighbors[k]);
                        }
                    }
                }
                workingList.RemoveAt(0);
            }
        }

        for (int i = 0; i < m_radius.Count; i++)
        {
            TileScript tScript = m_radius[i].GetComponent<TileScript>();
            int e = 3;
        }
    }

    // Reset all tiles to their original color
    public void ClearRadius(TileScript _tS)
    {
        List<GameObject> radTiles = _tS.m_radius;

        if (_tS.m_targetRadius.Count > 0)
            radTiles = _tS.m_targetRadius;

        for (int i = 0; i < radTiles.Count; i++)
        {
            TileScript radTileScript = radTiles[i].GetComponent<TileScript>();
            Renderer sRend = radTileScript.GetComponent<Renderer>();
            if (radTileScript.m_oldColor == Color.black)
                sRend.material.color = new Color(1, 1, 1, 0f);
            else
            {
                sRend.material.color = radTileScript.m_oldColor;
                radTileScript.m_oldColor = Color.black;
            }
        }
        radTiles.Clear();
    }
}
