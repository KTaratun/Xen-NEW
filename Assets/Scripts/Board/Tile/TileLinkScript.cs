using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileLinkScript : MonoBehaviour
{
    public enum targetRestriction { NONE, HORVERT, DIAGONAL };
    public enum nbors { BOTTOM, LEFT, TOP, RIGHT };

    static public Color c_attack = new Color(1, 0, 0, 0.5f);
    static public Color c_radius = Color.yellow;
    static public Color c_action = new Color(0, 1, 0, 0.5f);
    static public Color c_move = new Color(0, 0, 1, 0.5f);
    static public Color c_neutral = new Color(1, 1, 1, 0.5f);

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    static public void FetchTilesWithinRange(TileScript _origin, ObjectScript _owner, int _range, Color _color, bool _targetSelf, targetRestriction _targetingRestriction, bool _isBlockable)
    {
        // REFACTOR: Maybe less lists? Just a thought
        List<TileScript> workingList = new List<TileScript>();
        List<TileScript> storingList = new List<TileScript>();
        List<TileScript> oddGen = new List<TileScript>();
        List<TileScript> evenGen = new List<TileScript>();

        // Handle attacks with radius
        if (_owner.tag == "Player" && _owner.GetComponent<CharacterScript>().m_currAction && _color != c_move)
        {
            int range = _owner.GetComponent<CharacterScript>().m_currAction.m_range;
            int radius = _owner.GetComponent<CharacterScript>().m_currAction.m_radius;

            if (range == 0 && radius > 0)
            {
                _range = radius;
                _targetSelf = false;
                _origin = _owner.m_tile;
            }

            if (radius > 0)
                _isBlockable = false;
        }

        // Start with current tile in oddGen
        oddGen.Add(_origin);

        if (_targetSelf || !_targetSelf && _origin != _owner.m_tile.GetComponent<TileScript>())//|| !_targetSelf && this != currCharScript.m_tile.GetComponent<TileScript>())// || !_targetSelf && this != originalTileScript
        {
            Renderer myRend = _origin.gameObject.GetComponent<Renderer>();

            _origin.m_oldColor = myRend.material.color;

            myRend.material.color = _color;

            if (_color == c_radius)
                _origin.m_targetRadius.Add(_origin);
            else
                _origin.m_radius.Add(_origin);
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
                        currNeighbor = Diagonal(currNeighbor, _origin, k);
                        if (!currNeighbor)
                            continue;

                        tScript = currNeighbor.GetComponent<TileScript>();
                    }

                    // if color is movement color and the current tile is holding someone
                    if (_origin.m_holding && _origin.m_holding.tag == "Player" && _color == c_move && tScript.m_holding && tScript.m_holding.tag != "PowerUp" ||
                        !_targetSelf && tScript == _owner.m_tile.GetComponent<TileScript>())
                        continue;

                    if (_targetingRestriction == targetRestriction.HORVERT && tScript.m_x != _origin.m_x && tScript.m_z != _origin.m_z)
                        continue;

                    if (_isBlockable && CheckIfBlocked(_origin, currNeighbor))
                        continue;

                    Renderer tR = currNeighbor.GetComponent<Renderer>();

                    // If the fetch was yellow but doesn't have radius, then we want to select all tiles in a specific direction ie. Piercing, thrust and diagnal
                    if (_color == c_radius && _owner.tag == "Player" && _owner.GetComponent<CharacterScript>().m_currAction && tR.material.color == new Color(1, 1, 1, 0))
                    {
                        if (_owner.GetComponent<CharacterScript>().m_currAction.m_radius == 0 &&
                            _owner.GetComponent<CharacterScript>().m_tempStats[(int)CharacterScript.sts.RAD] == 0)
                            continue;
                    }

                    if (tR.material.color != _color)
                    {
                        if (_color == c_radius)
                            tScript.m_oldColor = tR.material.color;

                        tR.material.color = _color;
                        storingList.Add(tScript);

                        if (_color == c_radius)
                            _origin.m_targetRadius.Add(currNeighbor);
                        else
                            _origin.m_radius.Add(currNeighbor);
                    }
                }
                workingList.RemoveAt(0);
            }
        }
    }

    static public void FetchAllEmptyTiles()
    {
        BoardScript bS = GameObject.Find("Board").GetComponent<BoardScript>();
        GameManagerScript gS = GameObject.Find("Scene Manager").GetComponent<GameManagerScript>();

        for (int i = 0; i < bS.m_tiles.Length; i++)
        {
            if (!bS.m_tiles[i].m_holding)
            {
                Renderer tRend = bS.m_tiles[i].GetComponent<Renderer>();
                //bS.m_tiles[i].m_oldColor = tRend.material.color;
                gS.m_currCharScript.m_tile.m_radius.Add(bS.m_tiles[i]);
                tRend.material.color = c_move;
            }
        }
    }

    static public void HandleRadius(BoardScript _bScript, GameObject _tile)
    {
        GameManagerScript gS = GameObject.Find("Scene Manager").GetComponent<GameManagerScript>();

        CharacterScript currChar = gS.m_currCharScript;
        ActionScript act = currChar.m_currAction;

        int rad = currChar.m_tempStats[(int)CharacterScript.sts.RAD] + act.m_radius;
        if (act.UniqueActionProperties(ActionScript.uniAct.NON_RAD) >= 0)
            rad = act.m_range + currChar.m_tempStats[(int)CharacterScript.sts.RNG];

        bool targetSelf = false;
        Renderer tarRend = _tile.GetComponent<Renderer>();
        if (act.UniqueActionProperties(ActionScript.uniAct.TAR_SELF) >= 0 ||
            gS.m_currCharScript.m_tempStats[(int)CharacterScript.sts.RAD] > 0 && tarRend.material.color != c_attack)
            targetSelf = true;

        targetRestriction tR = targetRestriction.NONE;
        if (act.UniqueActionProperties(ActionScript.uniAct.TAR_RES) >= 0)
            tR = (targetRestriction)act.UniqueActionProperties(ActionScript.uniAct.TAR_RES);

        bool isBlockable = true;
        if (act.UniqueActionProperties(ActionScript.uniAct.IS_NOT_BLOCK) >= 0 ||
            gS.m_currCharScript.m_tempStats[(int)CharacterScript.sts.RAD] > 0)
            isBlockable = false;

        FetchTilesWithinRange(_tile.GetComponent<TileScript>(), currChar, rad, c_radius, targetSelf, tR, isBlockable);
        _bScript.m_oldTile = _tile.GetComponent<TileScript>();
    }

    static public void ClearRadius(TileScript _tileScript)
    {
        List<TileScript> radTiles = _tileScript.m_radius;

        if (_tileScript.m_targetRadius.Count > 0)
            radTiles = _tileScript.m_targetRadius;

        for (int i = 0; i < radTiles.Count; i++)
            radTiles[i].GetComponent<TileScript>().ClearTile();

        radTiles.Clear();
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
            if (!nei.m_holding || nei.m_holding.tag == "PowerUp")
                return true;
        }
        return false;
    }

    static public bool CheckIfBlocked(TileScript _origin, TileScript _target)
    {
        Transform originTransform = _origin.transform;

        float laserMaxLength = Vector3.Distance(originTransform.position, _target.gameObject.transform.position);

        // REFACTOR: Don't instantiate during run time as much as possible
        Vector3 pos = originTransform.position;
        Quaternion rot = originTransform.rotation;
        originTransform.LookAt(_target.gameObject.transform);

        Ray ray = new Ray(originTransform.position, originTransform.forward);

        originTransform.SetPositionAndRotation(pos, rot);

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

    static private TileScript Diagonal(TileScript _neighbor, TileScript _originalTile, int _neiInd)
    {
        TileScript currNeighbor = _neighbor;
        // nbors { left, right, top, bottom };
        if (_neiInd == (int)nbors.LEFT && currNeighbor.m_neighbors[(int)nbors.TOP])
        {
            TileScript newNeighbor = currNeighbor.m_neighbors[(int)nbors.TOP].GetComponent<TileScript>();
            int dif = Mathf.Abs(newNeighbor.m_x - _originalTile.m_x) - Mathf.Abs(newNeighbor.m_z - _originalTile.m_z);

            if (dif == 0)
                return currNeighbor.m_neighbors[(int)nbors.TOP];
        }
        else if (_neiInd == (int)nbors.TOP && currNeighbor.m_neighbors[(int)nbors.RIGHT])
        {
            TileScript newNeighbor = currNeighbor.m_neighbors[(int)nbors.RIGHT].GetComponent<TileScript>();
            int dif = Mathf.Abs(newNeighbor.m_x - _originalTile.m_x) - Mathf.Abs(newNeighbor.m_z - _originalTile.m_z);

            if (dif == 0)
                return currNeighbor.m_neighbors[(int)nbors.RIGHT];
        }
        else if (_neiInd == (int)nbors.RIGHT && currNeighbor.m_neighbors[(int)nbors.BOTTOM])
        {
            TileScript newNeighbor = currNeighbor.m_neighbors[(int)nbors.BOTTOM].GetComponent<TileScript>();
            int dif = Mathf.Abs(newNeighbor.m_x - _originalTile.m_x) - Mathf.Abs(newNeighbor.m_z - _originalTile.m_z);

            if (dif == 0)
                return currNeighbor.m_neighbors[(int)nbors.BOTTOM];
        }
        else if (_neiInd == (int)nbors.BOTTOM && currNeighbor.m_neighbors[(int)nbors.LEFT])
        {
            TileScript newNeighbor = currNeighbor.m_neighbors[(int)nbors.LEFT].GetComponent<TileScript>();
            int dif = Mathf.Abs(newNeighbor.m_x - _originalTile.m_x) - Mathf.Abs(newNeighbor.m_z - _originalTile.m_z);

            if (dif == 0)
                return currNeighbor.m_neighbors[(int)nbors.LEFT];
        }

        return null;
    }

    static public List<TileScript> AITilePlanning(TileScript _targetTile, TileScript _currTileScript)
    {
        List<TileScript> branches = new List<TileScript>();
        List<TileScript> visited = new List<TileScript>();
        List<TileScript> path = new List<TileScript>();
        TileScript adjacentTile = null;

        branches.Add(_currTileScript);
        visited.Add(_currTileScript);
        _currTileScript.m_traversed = _currTileScript.m_holding;

        while (branches.Count > 0)
        {
            _currTileScript = branches[0];
            branches.RemoveAt(0);

            if (CaclulateDistance(_currTileScript, _targetTile) < 2)
            {
                adjacentTile = _currTileScript;
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
                if (_currTileScript.m_neighbors[order[i]] && !_currTileScript.m_neighbors[order[i]].m_traversed &&
                    !_currTileScript.m_neighbors[order[i]].m_holding)
                {
                    TileScript nei = _currTileScript.m_neighbors[order[i]];
                    AddSORTED(branches, nei, _targetTile);
                    visited.Add(nei);
                    nei.m_traversed = _currTileScript.m_holding;
                    nei.m_parent = _currTileScript;
                    spacesAvailable++;
                }
            }
        }

        _currTileScript = adjacentTile;
        while (_currTileScript && _currTileScript.m_parent)
        {
            path.Add(_currTileScript);
            _currTileScript = _currTileScript.m_parent;
        }

        path.Reverse();

        for (int i = 0; i < visited.Count; i++)
        {
            visited[i].m_traversed = null;
            visited[i].m_parent = null;
        }

        return path;
    }

    static private void AddSORTED(List<TileScript> _list, TileScript _tScript, TileScript _target)
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
}
