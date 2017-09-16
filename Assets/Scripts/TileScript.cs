using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileScript : MonoBehaviour {

    public enum nbors { left, right, top, bottom };
    public enum targetRestriction { NONE, HORVERT, DIAGONAL};

    public GameObject m_holding;
    public GameObject[] m_neighbors;
    public List<GameObject> m_radius;
    public List<GameObject> m_targetRadius;
    public int m_x;
    public int m_z;
    public BoardScript m_boardScript;
    private Color m_oldColor;
    //private LineRenderer m_line;

    // Use this for initialization
    void Start()
    {
        m_radius = new List<GameObject>();
        m_oldColor = Color.black;

        //m_line = gameObject.AddComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update() {

    }

    public void OnMouseDown()
    {
        if (PanelScript.CheckIfPanelOpen() || m_boardScript.m_camIsFrozen)
            return;

        TileScript currTileScript = m_boardScript.m_currTile.GetComponent<TileScript>();
        Renderer renderer = GetComponent<Renderer>();

        // Prevent players from being clicked on bringing up menus during move or attack
        if (m_holding && currTileScript.m_radius.Count > 0 && renderer.material.color == new Color(1, 1, 1, 0.5f))
            return;

        m_boardScript.m_selected = gameObject;

        // If selecting a tile while moving
        if (renderer.material.color == Color.blue) // If tile is blue when clicked, perform movement code
        {
            Button[] buttons = PanelScript.m_confirmPanel.GetComponentsInChildren<Button>();
            buttons[1].GetComponent<ButtonScript>().m_character = m_boardScript.m_currPlayer;
            buttons[1].GetComponent<ButtonScript>().ConfirmationButton("Move");
        }
        // If selecting a tile that is holding a character while using an action
        else if (renderer.material.color == Color.red && m_holding && m_holding.tag == "Player" || 
            renderer.material.color == Color.green && m_holding && m_holding.tag == "Player" ||
            renderer.material.color == Color.yellow) // Otherwise if color is red, perform action code
        {
            Button[] buttons = PanelScript.m_confirmPanel.GetComponentsInChildren<Button>();
            buttons[1].GetComponent<ButtonScript>().m_character = m_boardScript.m_currPlayer;
            buttons[1].GetComponent<ButtonScript>().ConfirmationButton("Action");
        }
        else if (m_holding && m_holding.tag == "Player" && !m_boardScript.m_isForcedMove)
        {
            if (m_holding == m_boardScript.m_currPlayer)
            {
                // Assign character to panels
                PanelScript mainPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.MAIN_PANEL].GetComponent<PanelScript>();
                mainPanScript.m_cScript = m_holding.GetComponent<CharacterScript>();
                mainPanScript.PopulatePanel();
            }
            else
            {
                // Assign character to panels
                PanelScript charViewPanScript = m_boardScript.m_panels[(int)BoardScript.pnls.CHARACTER_VIEWER_PANEL].GetComponent<PanelScript>();
                charViewPanScript.m_cScript = m_holding.GetComponent<CharacterScript>();
                charViewPanScript.PopulatePanel();
            }
        }
    }

    public void FetchTilesWithinRange(int _range, Color _color, bool _targetSelf, targetRestriction _targetingRestriction, bool _isBlockable)
    {
        // REFACTOR: Maybe less lists? Just a thought
        List<TileScript> workingList = new List<TileScript>();
        List<TileScript> storingList = new List<TileScript>();
        List<TileScript> oddGen = new List<TileScript>();
        List<TileScript> evenGen = new List<TileScript>();

        CharacterScript currCharScript = m_boardScript.m_currPlayer.GetComponent<CharacterScript>();

        // Handle attacks with radius
        GameObject originalTile = gameObject;
        TileScript originalTileScript = this;

        if (currCharScript.m_currAction.Length > 0 && _color != CharacterScript.c_move)
        {
            string[] actSeparated = currCharScript.m_currAction.Split('|');
            string[] range = actSeparated[(int)DatabaseScript.actions.RNG].Split(':');
            string[] radius = actSeparated[(int)DatabaseScript.actions.RAD].Split(':');

            if (int.Parse(range[1]) == 0)
            {
                _range = int.Parse(radius[1]);
                _targetSelf = false;
                originalTile = currCharScript.m_tile;
            }   originalTileScript = originalTile.GetComponent<TileScript>();
            
            if (int.Parse(radius[1]) > 0)
                _isBlockable = false;
        }

        // Start with current tile in oddGen
        oddGen.Add(originalTileScript);

        if (_targetSelf)//|| !_targetSelf && this != currCharScript.m_tile.GetComponent<TileScript>())// || !_targetSelf && this != originalTileScript
        {
            Renderer myRend = originalTile.GetComponent<Renderer>();
            m_oldColor = myRend.material.color;
            
            myRend.material.color = _color;
            
            if (_color == Color.yellow)
                m_targetRadius.Add(originalTile);
            else
                m_radius.Add(originalTile);
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
                    GameObject currNeighbor = workingList[0].m_neighbors[k];

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
                    if (_color == CharacterScript.c_move && tScript.m_holding || !_targetSelf && tScript == currCharScript.m_tile.GetComponent<TileScript>())
                        continue;

                    //if (tScript.m_holding && tScript.m_holding.tag == "Player" && tScript.m_holding.GetComponent<CharacterScript>().m_effects[(int)StatusScript.effects.PROTECT])
                    //    continue;

                    if (_targetingRestriction == targetRestriction.HORVERT && tScript.m_x != m_x && tScript.m_z != m_z)
                        continue;

                    if (_isBlockable && CheckIfBlocked(currNeighbor))
                        continue;

                    Renderer tR = currNeighbor.GetComponent<Renderer>();

                    // If the fetch was yellow but doesn't have radius, then we want to select all tiles in a specific direction ie. Piercing, thrust and diagnal
                    if (_color == Color.yellow && currCharScript.m_currAction.Length > 0 && tR.material.color == new Color(1, 1, 1, 0))
                    {
                        string[] actSeparated = currCharScript.m_currAction.Split('|');
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

    private bool CheckIfBlocked(GameObject _target)
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
            {
                if (raycastHit.collider && raycastHit.collider.tag == "Player" && !raycastHit.collider.GetComponent<CharacterScript>().m_isAlive)
                    return false;
                else
                    return true;
            }
        }

        return false;

        //m_line.SetPosition(0, transform.position);
        //m_line.SetPosition(1, endPosition);

    }

    private GameObject Diagonal(GameObject _neighbor, TileScript _originalTile, int _neiInd)
    {
        TileScript currNeighbor = _neighbor.GetComponent<TileScript>();
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

    // Reset all tiles to their original color REFACTOR: maybe make this a static class or remove the argument?
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
