using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour {

    public enum nbors { left, right, top, bottom };

    //public GameObject camera;
    public GameObject m_holding;
    public GameObject[] m_neighbors;
    public List<GameObject> m_radius;
    public List<GameObject> m_targetRadius;
    public int m_x;
    public int m_z;
    public BoardScript m_boardScript;
    private Color m_oldColor;
    private LineRenderer m_line;

    // Use this for initialization
    void Start()
    {
        m_radius = new List<GameObject>();
        m_oldColor = Color.black;

        m_line = gameObject.AddComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update() {

    }

    public void OnMouseDown()
    {
        // REFACTOR: Disallow the clicking of tiles if any menu is up
        // If a menu is up, don't let the board be selected
        PanelScript mainPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
        PanelScript ActionPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.ACTION_PANEL].GetComponent<PanelScript>();
        PanelScript StsPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.STATUS_PANEL].GetComponent<PanelScript>();
        PanelScript AuxPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.AUXILIARY_PANEL].GetComponent<PanelScript>();
        if (mainPanScript.m_inView == true || ActionPanScript.m_inView == true || StsPanScript.m_inView == true || AuxPanScript.m_inView == true)
            return;

        CharacterScript currPlayerScript = m_boardScript.m_currPlayer.GetComponent<CharacterScript>();
        TileScript currTileScript = m_boardScript.m_currTile.GetComponent<TileScript>();
        Renderer renderer = GetComponent<Renderer>();

        // Prevent players from being clicked on bringing up menus during move or attack
        if (m_holding && currTileScript.m_radius.Count > 0 && renderer.material.color == new Color(1, 1, 1, 0.5f))
            return;

        m_boardScript.m_selected = gameObject;

        if (renderer.material.color == Color.blue) // If tile is blue when clicked, perform movement code
        {
            CharacterScript mover = currPlayerScript;
            TileScript moverTile = currTileScript;
            if (m_boardScript.m_isForcedMove)
            {
                mover = m_boardScript.m_isForcedMove.GetComponent<CharacterScript>();
                moverTile = mover.m_tile.GetComponent<TileScript>();
            }
            mover.Movement(moverTile, GetComponent<TileScript>(), false);

            return;
        }

        if (renderer.material.color == new Color(1, 0, 0, 1) && m_holding || renderer.material.color == new Color(0, 1, 0, 1) && m_holding) // Otherwise if color is red, perform action code
        {
            List<GameObject> targets = new List<GameObject>();
            targets.Add(m_holding);
            currPlayerScript.Action(targets);

            if (!m_boardScript.m_isForcedMove)
                ClearRadius(currTileScript);

            return;
        }
        if (renderer.material.color == Color.yellow)
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
                ClearRadius(this);

                if (!m_boardScript.m_isForcedMove)
                    ClearRadius(currTileScript);
            }
            return;
        }

        if (m_holding && m_holding.tag == "Player" && !m_boardScript.m_isForcedMove)
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

    public void FetchTilesWithinRange(int _range, Color _color, bool _targetSelf, bool _isOnlyHorVert)
    {
        // REFACTOR: Maybe less lists?
        List<TileScript> workingList = new List<TileScript>();
        List<TileScript> storingList = new List<TileScript>();
        List<TileScript> oddGen = new List<TileScript>();
        List<TileScript> evenGen = new List<TileScript>();

        // Start with current tile in oddGen
        oddGen.Add(this);

        if (_targetSelf)
        {
            Renderer myRend = GetComponent<Renderer>();
            m_oldColor = myRend.material.color;

            myRend.material.color = _color;

            if (_color == Color.yellow)
                m_targetRadius.Add(gameObject);
            else
                m_radius.Add(gameObject);
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

                    // if color is movement color and the current tile is holding someone
                    if (_color == new Color(0, 0, 1, 0.5f) && tScript.m_holding || !_targetSelf && workingList[0].m_neighbors[k] == gameObject)
                        continue;

                    if (_isOnlyHorVert && tScript.m_x != m_x && tScript.m_z != m_z)
                        continue;

                    if (_color == new Color(1, 0, 0, 0.5f) && CheckIfBlocked(workingList[0].m_neighbors[k]))
                        continue;

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
                workingList.RemoveAt(0);
            }
        }
    }

    public bool CheckIfBlocked(GameObject _target)
    {
        //float laserWidth = 0.1f;
        float laserMaxLength = Vector3.Distance(transform.position, _target.transform.position);
        //
        //Vector3[] initLaserPositions = new Vector3[2] { Vector3.zero, Vector3.zero };
        //m_line.SetPositions(initLaserPositions);
        //m_line.SetWidth(laserWidth, laserWidth);

        // REFACTOR: Don't instantiate during run time as much as possible
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        transform.LookAt(_target.transform);

        Ray ray = new Ray(transform.position, transform.forward);

        transform.SetPositionAndRotation(pos, rot);

        RaycastHit raycastHit;
        Vector3 endPosition = _target.transform.position;

        if (Physics.Raycast(ray, out raycastHit, laserMaxLength))
        {
            //endPosition = raycastHit.point;
            TileScript tarTileScript = _target.GetComponent<TileScript>();
            if (tarTileScript.m_holding && raycastHit.collider.gameObject == tarTileScript.m_holding)
                return false;
            else
                return true;
        }

        return false;

        //m_line.SetPosition(0, transform.position);
        //m_line.SetPosition(1, endPosition);

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
