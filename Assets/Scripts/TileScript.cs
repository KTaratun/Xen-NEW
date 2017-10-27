using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileScript : MonoBehaviour {

    public enum nbors { left, right, top, bottom };
    public enum targetRestriction { NONE, HORVERT, DIAGONAL};

    static public Color c_attack = new Color(1, 0, 0, 0.5f);
    static public Color c_action = new Color(0, 1, 0, 0.5f);
    static public Color c_move = new Color(0, 0, 1, 0.5f);
    static public Color c_neutral = new Color(1, 1, 1, 0.5f);

    public GameObject m_holding;
    public TileScript[] m_neighbors;
    public List<TileScript> m_radius;
    public List<TileScript> m_targetRadius;
    public int m_x;
    public int m_z;
    public BoardScript m_boardScript;
    private Color m_oldColor;
    public GameObject m_traversed; // Used for path planning and to determine future occupancy of a tile
    public TileScript m_parent; // Used for determining path for AI
    //private LineRenderer m_line;

    // Use this for initialization
    void Start()
    {
        m_traversed = null;
        m_radius = new List<TileScript>();
        m_oldColor = Color.black;

        //m_line = gameObject.AddComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update() {

    }

    public void OnMouseDown()
    {
        if (PanelScript.CheckIfPanelOpen() || m_boardScript.m_camIsFrozen || m_boardScript.m_hoverButton)
            return;

        Renderer renderer = GetComponent<Renderer>();

        // Prevent players from being clicked on bringing up menus during move or attack
        if (m_holding && m_boardScript.m_currCharScript.m_tile.m_radius.Count > 0 && renderer.material.color == new Color(1, 1, 1, 0.5f))
            return;

        if (renderer.material.color == c_neutral && m_boardScript.m_selected != this && !PanelScript.GetPanel("Choose Panel").m_inView &&
            m_boardScript.m_currCharScript && m_holding != m_boardScript.m_currCharScript.gameObject)
        {
            if (m_boardScript.m_selected && m_boardScript.m_selected.m_holding && m_boardScript.m_selected.m_holding.tag == "Player")
            {
                Renderer oldRend = m_boardScript.m_selected.m_holding.transform.GetComponentInChildren<Renderer>();
                oldRend.materials[1].shader = oldRend.materials[0].shader;
                oldRend.materials[2].shader = oldRend.materials[0].shader;

                PanelScript.GetPanel("HUD Panel RIGHT").m_inView = false;
            }
            if (m_holding && m_holding.tag == "Player")
            {
                Renderer rend = m_holding.transform.GetComponentInChildren<Renderer>();
                rend.materials[2].shader = Resources.Load<Shader>("Outlined-Silhouette Only (NOT MINE)");
            }
            m_boardScript.m_selected = this;
        }
        else if (renderer.material.color == c_neutral && m_boardScript.m_selected == this && !PanelScript.GetPanel("Choose Panel").m_inView &&
            m_boardScript.m_currCharScript && m_holding != m_boardScript.m_currCharScript.gameObject)
        {
            if (m_boardScript.m_selected && m_boardScript.m_selected.m_holding && m_boardScript.m_selected.m_holding.tag == "Player")
            {
                Renderer oldRend = m_boardScript.m_selected.m_holding.transform.GetComponentInChildren<Renderer>();
                oldRend.materials[1].shader = oldRend.materials[0].shader;
                oldRend.materials[2].shader = oldRend.materials[0].shader;

                PanelScript.GetPanel("HUD Panel RIGHT").m_inView = false;
            }
            m_boardScript.m_selected = null;
        }
        else
            m_boardScript.m_selected = this;

        if (m_boardScript.m_selected == null)
            return;

        // If selecting a tile while moving
        if (renderer.material.color == Color.blue) // If tile is blue when clicked, perform movement code
        {
            Button[] buttons = PanelScript.m_confirmPanel.GetComponentsInChildren<Button>();
            buttons[1].GetComponent<ButtonScript>().m_object = m_boardScript.m_currCharScript.gameObject;
            buttons[1].GetComponent<ButtonScript>().ConfirmationButton("Move");
        }
        // If selecting a tile that is holding a character while using an action
        else if (renderer.material.color == Color.red || renderer.material.color == Color.green ||
            renderer.material.color == Color.yellow) // Otherwise if color is red, perform action code
        {
            Button[] buttons = PanelScript.m_confirmPanel.GetComponentsInChildren<Button>();
            buttons[1].GetComponent<ButtonScript>().m_object = m_boardScript.m_currCharScript.gameObject;
            buttons[1].GetComponent<ButtonScript>().ConfirmationButton("Action");
        }
        else if (m_holding && m_holding.tag == "Player" && !m_boardScript.m_isForcedMove)
        {
            if (m_boardScript.m_selected != this)
                return;

            m_boardScript.m_camera.GetComponent<CameraScript>().m_target = m_holding;

            if (m_holding == m_boardScript.m_currCharScript.gameObject)
            {
                PanelScript statusPanel = PanelScript.GetPanel("Status Panel");
                statusPanel.m_cScript = m_holding.GetComponent<CharacterScript>();
                statusPanel.PopulatePanel();
            }

            //if (m_holding == m_boardScript.m_currPlayer)
            //{
            //    // Assign character to panels
            //    PanelScript mainPanScript = PanelScript.GetPanel("Main Panel");
            //    mainPanScript.m_cScript = m_holding.GetComponent<CharacterScript>();
            //    mainPanScript.PopulatePanel();
            //}
            //else
            //{
            //    // Assign character to panels
            //    PanelScript charViewPanScript = PanelScript.GetPanel("CharacterViewer Panel");
            //    charViewPanScript.m_cScript = m_holding.GetComponent<CharacterScript>();
            //    charViewPanScript.PopulatePanel();
            //}
        }
    }

    public void HandleTile()
    {
        CharacterScript currChar = m_boardScript.m_currCharScript;
        TileScript oldTile = m_boardScript.m_oldTile;

        if (oldTile)
        {
            Renderer oTR = oldTile.GetComponent<Renderer>();
            if (oTR.material.color == Color.yellow)
                oldTile.ClearRadius();
            else if (oTR.material.color.a != 0)
                oTR.material.color = new Color(oTR.material.color.r, oTR.material.color.g, oTR.material.color.b, oTR.material.color.a - 0.5f);

            // Hacky fix for when you spawn colored m_tiles for range and your cursor starts on one of the m_tiles. If it has no alpha and isn't white
            if (oTR.material.color.a == 0f && oTR.material.color.r + oTR.material.color.g + oTR.material.color.b != 3)
                oTR.material.color = new Color(oTR.material.color.r, oTR.material.color.g, oTR.material.color.b, oTR.material.color.a + 0.5f);
        }

        Renderer tarRend = gameObject.GetComponent<Renderer>();

        // TARGET RED TILE

        // For special case, multi targeting attacks
        if (currChar.m_currAction.Length > 0 && tarRend.material.color == c_attack && CharacterScript.UniqueActionProperties(currChar.m_currAction, CharacterScript.uniAct.NON_RAD) >= 0 ||
            currChar.m_currAction.Length > 0 && tarRend.material.color == c_attack && int.Parse(DatabaseScript.GetActionData(currChar.m_currAction, DatabaseScript.actions.RAD)) > 0)
        {
            HandleRadius();
            return;
        }

        tarRend.material.color = new Color(tarRend.material.color.r, tarRend.material.color.g, tarRend.material.color.b, tarRend.material.color.a + 0.5f);
        m_boardScript.m_oldTile = this;
    }

    public void HandleRadius()
    {
        CharacterScript currChar = m_boardScript.m_currCharScript;
        string actRng = DatabaseScript.GetActionData(currChar.m_currAction, DatabaseScript.actions.RNG);

        int rad = currChar.m_currRadius + currChar.m_tempStats[(int)CharacterScript.sts.RAD];
        if (CharacterScript.UniqueActionProperties(currChar.m_currAction, CharacterScript.uniAct.NON_RAD) >= 0)
            rad = int.Parse(actRng) + currChar.m_tempStats[(int)CharacterScript.sts.RNG];

        bool targetSelf = true;
        if (CharacterScript.UniqueActionProperties(currChar.m_currAction, CharacterScript.uniAct.TAR_SELF) >= 0)
            targetSelf = false;

        targetRestriction tR = targetRestriction.NONE;
        if (CharacterScript.UniqueActionProperties(currChar.m_currAction, CharacterScript.uniAct.TAR_RES) >= 0)
            tR = (targetRestriction)CharacterScript.UniqueActionProperties(currChar.m_currAction, CharacterScript.uniAct.TAR_RES);

        bool isBlockable = true;
        if (CharacterScript.UniqueActionProperties(currChar.m_currAction, CharacterScript.uniAct.IS_NOT_BLOCK) >= 0)
            isBlockable = false;

        FetchTilesWithinRange(rad, Color.yellow, targetSelf, tR, isBlockable);
        m_boardScript.m_oldTile = this;
    }

    public void FetchTilesWithinRange(int _range, Color _color, bool _targetSelf, targetRestriction _targetingRestriction, bool _isBlockable)
    {
        // REFACTOR: Maybe less lists? Just a thought
        List<TileScript> workingList = new List<TileScript>();
        List<TileScript> storingList = new List<TileScript>();
        List<TileScript> oddGen = new List<TileScript>();
        List<TileScript> evenGen = new List<TileScript>();

        CharacterScript ownerCharScript = m_boardScript.m_currCharScript;

        // Handle attacks with radius
        TileScript originalTileScript = this;

        if (ownerCharScript.m_currAction.Length > 0 && _color != c_move)
        {
            string range = DatabaseScript.GetActionData(ownerCharScript.m_currAction, DatabaseScript.actions.RNG);
            string radius = DatabaseScript.GetActionData(ownerCharScript.m_currAction, DatabaseScript.actions.RAD);

            if (int.Parse(range) == 0 && int.Parse(radius) > 0)
            {
                _range = int.Parse(radius);
                _targetSelf = false;
                originalTileScript = ownerCharScript.m_tile;
            }

            if (int.Parse(radius) > 0)
                _isBlockable = false;
        }

        // Start with current tile in oddGen
        oddGen.Add(originalTileScript);

        if (_targetSelf || !_targetSelf && originalTileScript != ownerCharScript.m_tile.GetComponent<TileScript>())//|| !_targetSelf && this != currCharScript.m_tile.GetComponent<TileScript>())// || !_targetSelf && this != originalTileScript
        {
            Renderer myRend = originalTileScript.gameObject.GetComponent<Renderer>();
            m_oldColor = myRend.material.color;
            
            myRend.material.color = _color;
            
            if (_color == Color.yellow)
                m_targetRadius.Add(originalTileScript);
            else
                m_radius.Add(originalTileScript);
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
                    TileScript currNeighbor = workingList[0].m_neighbors[k];

                    if (!currNeighbor)
                        continue;

                    TileScript tScript = currNeighbor.GetComponent<TileScript>();

                    if (_targetingRestriction == targetRestriction.DIAGONAL)
                    {
                        currNeighbor = Diagonal(currNeighbor, originalTileScript, k);
                        if (!currNeighbor)
                            continue;

                        tScript = currNeighbor.GetComponent<TileScript>();
                    }

                    // if color is movement color and the current tile is holding someone
                    if (m_holding && m_holding.tag == "Player" && _color == c_move && tScript.m_holding && tScript.m_holding.tag != "PowerUp" ||
                        m_holding && m_holding.tag == "PowerUp" && _color == c_move && tScript.m_holding && tScript.m_holding.tag != "Player" ||
                        !_targetSelf && tScript == ownerCharScript.m_tile.GetComponent<TileScript>())
                        continue;

                    if (_targetingRestriction == targetRestriction.HORVERT && tScript.m_x != m_x && tScript.m_z != m_z)
                        continue;

                    if (_isBlockable && CheckIfBlocked(currNeighbor))
                        continue;

                    Renderer tR = currNeighbor.GetComponent<Renderer>();

                    // If the fetch was yellow but doesn't have radius, then we want to select all tiles in a specific direction ie. Piercing, thrust and diagnal
                    if (_color == Color.yellow && ownerCharScript.m_currAction.Length > 0 && tR.material.color == new Color(1, 1, 1, 0))
                    {
                        string[] actSeparated = ownerCharScript.m_currAction.Split('|');
                        string[] radius = actSeparated[(int)DatabaseScript.actions.RAD].Split(':');
                        if (int.Parse(radius[1]) == 0)
                            continue;
                    }

                    if (tR.material.color != _color)
                    {
                        if (_color == Color.yellow)
                            tScript.m_oldColor = tR.material.color;

                        tR.material.color = _color;
                        storingList.Add(tScript);

                        if (_color == Color.yellow)
                            m_targetRadius.Add(currNeighbor);
                        else
                            m_radius.Add(currNeighbor);
                    }
                }
                workingList.RemoveAt(0);
            }
        }
    }

    // Reset all tiles to their original color REFACTOR: maybe make this a static class or remove the argument?
    public void ClearRadius()
    {
        List<TileScript> radTiles = m_radius;

        if (m_targetRadius.Count > 0)
            radTiles = m_targetRadius;

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

    public List<TileScript> AITilePlanning(TileScript _targetTile)
    {
        TileScript currTileScript = this;

        List<TileScript> branches = new List<TileScript>();
        List<TileScript> visited = new List<TileScript>();
        List<TileScript> path = new List<TileScript>();
        TileScript adjacentTile = null;

        branches.Add(currTileScript);
        visited.Add(currTileScript);
        currTileScript.m_traversed = m_holding;

        while (branches.Count > 0)
        {
            currTileScript = branches[0];
            branches.RemoveAt(0);

            if (CaclulateDistance(currTileScript, _targetTile) < 2)
            {
                adjacentTile = currTileScript;
                break;
            }

            int[] order = { -1, -1, -1, -1 };
            for (int i = 0; i < order.Length; i++)
            {
                int rand = -1;
                while (rand == -1 || order[rand] != -1)
                    rand = Random.Range(0, 4);
                order[rand] = i;
            }

            int spacesAvailable = 0;
            for (int i = 0; i < 4; i++)
            {
                if (currTileScript.m_neighbors[order[i]] && !currTileScript.m_neighbors[order[i]].m_traversed &&
                    !currTileScript.m_neighbors[order[i]].m_holding)
                {
                    TileScript nei = currTileScript.m_neighbors[order[i]];
                    AddSORTED(branches, nei, _targetTile);
                    visited.Add(nei);
                    nei.m_traversed = m_holding;
                    nei.m_parent = currTileScript;
                    spacesAvailable++;
                }
            }
        }

        currTileScript = adjacentTile;
        while (currTileScript && currTileScript.m_parent)
        {
            path.Add(currTileScript);
            currTileScript = currTileScript.m_parent;
        }

        path.Reverse();

        for (int i = 0; i < visited.Count; i++)
        {
            visited[i].m_traversed = null;
            visited[i].m_parent = null;
        }

        return path;
    }

    private void AddSORTED(List<TileScript> _list, TileScript _tScript, TileScript _target)
    {
        if (_list.Count > 0)
            for (int i = 0; i < _list.Count; i++)
            {
                if (CaclulateDistance(_tScript, _target) < CaclulateDistance(_list[i], _target))
                {
                    _list.Insert(i, _tScript);
                    break;
                }
                else if (i == _list.Count - 1)
                {
                    _list.Add(_tScript);
                    break;
                }
            }
        else
            _list.Add(_tScript);
    }


    // Utilities
    static public int CaclulateDistance(TileScript _targetOne, TileScript _targetTwo)
    {
        return Mathf.Abs(_targetOne.m_x - _targetTwo.m_x) + Mathf.Abs(_targetOne.m_z - _targetTwo.m_z);
    }

    static public bool CheckForEmptyNeighbor(TileScript _tile)
    {
        for (int i = 0; i < _tile.m_neighbors.Length; i++)
        {
            if (!_tile.m_neighbors[i])
                continue;

            TileScript nei = _tile.m_neighbors[i].GetComponent<TileScript>();
            if (!nei.m_holding)
                return true;
        }
        return false;
    }

    private bool CheckIfBlocked(TileScript _target)
    {
        float laserMaxLength = Vector3.Distance(transform.position, _target.gameObject.transform.position);

        // REFACTOR: Don't instantiate during run time as much as possible
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        transform.LookAt(_target.gameObject.transform);

        Ray ray = new Ray(transform.position, transform.forward);

        transform.SetPositionAndRotation(pos, rot);

        RaycastHit raycastHit;
        Vector3 endPosition = _target.gameObject.transform.position;

        if (Physics.Raycast(ray, out raycastHit, laserMaxLength))
        {
            //endPosition = raycastHit.point;
            if (_target.m_holding && raycastHit.collider.gameObject == _target.m_holding)
                return false;
            else
            {
                if (raycastHit.collider && raycastHit.collider.tag == "Player" && !raycastHit.collider.GetComponent<CharacterScript>().m_isAlive ||
                    raycastHit.collider && raycastHit.collider.tag == "PowerUp")
                    return false;
                else
                    return true;
            }
        }

        return false;
    }

    private TileScript Diagonal(TileScript _neighbor, TileScript _originalTile, int _neiInd)
    {
        TileScript currNeighbor = _neighbor;
        // nbors { left, right, top, bottom };
        if (_neiInd == (int)nbors.left && currNeighbor.m_neighbors[(int)nbors.top])
        {
            TileScript newNeighbor = currNeighbor.m_neighbors[(int)nbors.top].GetComponent<TileScript>();
            int dif = Mathf.Abs(newNeighbor.m_x - _originalTile.m_x) - Mathf.Abs(newNeighbor.m_z - _originalTile.m_z);

            if  (dif == 0)
                return currNeighbor.m_neighbors[(int)nbors.top];
        }
        else if (_neiInd == (int)nbors.top && currNeighbor.m_neighbors[(int)nbors.right])
        {
            TileScript newNeighbor = currNeighbor.m_neighbors[(int)nbors.right].GetComponent<TileScript>();
            int dif = Mathf.Abs(newNeighbor.m_x - _originalTile.m_x) - Mathf.Abs(newNeighbor.m_z - _originalTile.m_z);

            if (dif == 0)
                return currNeighbor.m_neighbors[(int)nbors.right];
        }
        else if (_neiInd == (int)nbors.right && currNeighbor.m_neighbors[(int)nbors.bottom])
        {
            TileScript newNeighbor = currNeighbor.m_neighbors[(int)nbors.bottom].GetComponent<TileScript>();
            int dif = Mathf.Abs(newNeighbor.m_x - _originalTile.m_x) - Mathf.Abs(newNeighbor.m_z - _originalTile.m_z);

            if (dif == 0)
                return currNeighbor.m_neighbors[(int)nbors.bottom];
        }
        else if (_neiInd == (int)nbors.bottom && currNeighbor.m_neighbors[(int)nbors.left])
        {
            TileScript newNeighbor = currNeighbor.m_neighbors[(int)nbors.left].GetComponent<TileScript>();
            int dif = Mathf.Abs(newNeighbor.m_x - _originalTile.m_x) - Mathf.Abs(newNeighbor.m_z - _originalTile.m_z);

            if (dif == 0)
                return currNeighbor.m_neighbors[(int)nbors.left];
        }

        return null;
    }
}
