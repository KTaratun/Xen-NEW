using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardScript : MonoBehaviour {

    public enum pnls { MAIN_PANEL, ACTION_PANEL, STATUS_PANEL, HUD_LEFT_PANEL, HUD_RIGHT_PANEL, ROUND_PANEL,
        AUXILIARY_PANEL, STATUS_SELECTOR, ENERGY_SELECTOR, ACTION_PREVIEW, CHARACTER_VIEWER_PANEL, TOTAL_PANEL }

    public Camera m_camera; // Why it do that squiggle?
    public int m_roundCount;
    public int m_environmentalDensity; // This dictates how many environmental objs per tile
    public GameObject[] m_environmentOBJs;
    public List<GameObject> m_obstacles; // The actual array of objects on the board
    public GameObject[] m_panels; // All movable GUI on screen
    public GameObject[] m_tiles; // All m_tiles on the board
    public GameObject m_tile; // A reference to the m_tile prefab
    public GameObject m_character; // A reference to the character prefab
    public int m_width; // Width of the board
    public int m_height; // Height of the board
    public GameObject m_selected; // Last pressed tile
    private GameObject m_highlightedTile; // Currently hovered over tile
    public GameObject m_oldTile; // A pointer to the previously hovered over m_tile
    public GameObject m_currTile; // The m_tile of the player who's turn is currently up
    public GameObject m_currPlayer; // A pointer to the player that's turn is currently up
    public List<GameObject> m_characters; // A list of all characters within the game
    public GameObject[] m_players; 
    public List<GameObject> m_currRound; // A list of the players who have taken a turn in the current round
    public GameObject m_isForcedMove; // This is to keep track of whichever player is turning an action out of order
    public int m_livingPlayersInRound; // This is mainly for the turn panels. This tells them how far to move when I player finishes their turn.
    public bool m_camIsFrozen;

    // Use this for initialization
    void Start ()
    {
        PanelScript.MenuPanelInit("Canvas");
        m_roundCount = 0;
        m_isForcedMove = null;
        m_camIsFrozen = false;
        Renderer r = GetComponent<Renderer>();

        if (m_environmentalDensity <= 2)
            m_environmentalDensity = 10;
        // If the user didn't assign a m_tile size in the editor, auto adjust the m_tiles to fit the board
        if (m_height == 0 && m_width == 0)
        {
            m_width = (int) (r.bounds.size.x / m_tile.GetComponent<Renderer>().bounds.size.x);
            m_height = (int)(r.bounds.size.z / m_tile.GetComponent<Renderer>().bounds.size.z);
        }

        InitBoardTiles();
        AssignNeighbors();
        EnvironmentInit();
        CharacterInit();
        NewTurn();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetMouseButtonDown(1))
            OnRightClick();

        Hover();
    }

    private void EnvironmentInit()
    {
        int numOfObstacles = (m_height * m_width) / m_environmentalDensity;

        while (numOfObstacles > 0)
        {
            int randomOBJ = Random.Range(0, m_environmentOBJs.Length);

            GameObject newOBJ = Instantiate(m_environmentOBJs[randomOBJ]);
            RandomRotation(newOBJ);
            PlaceOnBoard(newOBJ);
            m_obstacles.Add(newOBJ);
            if (newOBJ.GetComponent<ObjectScript>().m_width == 0)
                numOfObstacles--;
            else
                numOfObstacles -= newOBJ.GetComponent<ObjectScript>().m_width;
        }
    }

    public void RandomRotation(GameObject _obj)
    {
        int randomRot = Random.Range(0, 5);
        _obj.transform.Rotate(0, randomRot * 90, 0);

        if (randomRot == 0)
            _obj.GetComponent<ObjectScript>().m_facing = TileScript.nbors.bottom;
        else if (randomRot == 1)
            _obj.GetComponent<ObjectScript>().m_facing = TileScript.nbors.left;
        else if (randomRot == 2)
            _obj.GetComponent<ObjectScript>().m_facing = TileScript.nbors.top;
        else if (randomRot == 3)
            _obj.GetComponent<ObjectScript>().m_facing = TileScript.nbors.right;
    }

    public void OnRightClick()
    {
        // If right click while the confirmation panel is up, do nothing but close that specific panel
        if (PanelScript.m_confirmPanel.m_inView)
        {
            PanelScript.RemoveFromHistory("");
            return;
        }

        CharacterScript charScript = m_currPlayer.GetComponent<CharacterScript>();
        TileScript currTScript = charScript.m_tile.GetComponent<TileScript>();

        if (currTScript.m_radius.Count == 0 || m_isForcedMove)
            return;

        PanelScript actPreviewScript = m_panels[(int)pnls.ACTION_PREVIEW].GetComponent<PanelScript>();
        actPreviewScript.m_inView = false;

        Renderer sRend = currTScript.m_radius[0].GetComponent<Renderer>();
        if (sRend.material.color == CharacterScript.c_attack || sRend.material.color == Color.yellow || sRend.material.color == CharacterScript.c_action)
        {
            PanelScript actPan = m_panels[(int)pnls.ACTION_PANEL].GetComponent<PanelScript>();
            actPan.m_inView = true;
        }
        //else if (sRend.material.color == CharacterScript.c_move)
        //{
        //    PanelScript panScript = m_panels[(int)pnls.MAIN_PANEL].GetComponent<PanelScript>();
        //    panScript.m_inView = true;
        //}

        if (m_highlightedTile)
        {
            TileScript highScript = m_highlightedTile.GetComponent<TileScript>();
            currTScript.ClearRadius(highScript);
        }

        currTScript.ClearRadius(currTScript);
    }

    public void InitBoardTiles()
    {
        Renderer r = GetComponent<Renderer>();
        m_tiles = new GameObject[m_width * m_height];

        for (int z = 0; z < m_height; z++)
        {
            for (int x = 0; x < m_width; x++)
            {
                GameObject newm_tile = Instantiate(m_tile);

                Renderer ntR = newm_tile.GetComponent<Renderer>();
                ntR.material.color = new Color(1, 1, 1, 0f);

                newm_tile.transform.SetPositionAndRotation(new Vector3(x * ntR.bounds.size.x + (r.bounds.min.x + ntR.bounds.size.x / 2), transform.position.y + 0.05f, z * ntR.bounds.size.z + (r.bounds.min.z + ntR.bounds.size.z / 2)), new Quaternion());

                TileScript tScript = newm_tile.GetComponent<TileScript>();
                tScript.m_x = x;
                tScript.m_z = z;
                tScript.m_boardScript = GetComponent<BoardScript>();

                m_tiles[x + z * m_width] = newm_tile;
            }
        }
    }

    public void AssignNeighbors()
    {
        for (int z = 0; z < m_height; z++)
        {
            for (int x = 0; x < m_width; x++)
            {
                GameObject tempm_tile = m_tiles[x + z * m_width];
                TileScript tScript = tempm_tile.GetComponent<TileScript>();

                tScript.m_neighbors = new GameObject[4];

                if (x != 0)
                    tScript.m_neighbors[(int)TileScript.nbors.left] = m_tiles[(x + z * m_width) - 1];
                if (x < m_width - 1)
                    tScript.m_neighbors[(int)TileScript.nbors.right] = m_tiles[(x + z * m_width) + 1];
                if (z != 0)
                    tScript.m_neighbors[(int)TileScript.nbors.bottom] = m_tiles[(x + z * m_width) - m_width];
                if (z < m_height - 1)
                    tScript.m_neighbors[(int)TileScript.nbors.top] = m_tiles[(x + z * m_width) + m_width];
            }
        }
    }

    public void CharacterInit()
    {
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 6; j++)
            {
                string key = i.ToString() + ',' + j.ToString();
                string name = PlayerPrefs.GetString(key + ",name");
                if (name.Length > 0)
                {
                    // Set up character
                    GameObject newChar = Instantiate(m_character);
                    CharacterScript cScript = newChar.GetComponent<CharacterScript>();

                    cScript = PlayerPrefScript.LoadChar(key, cScript);

                    if (i == 0)
                        cScript.m_teamColor = new Color(1, .5f, 0, 1);
                    else if (i == 1)
                        cScript.m_teamColor = new Color(.5f, 0, 1, 1);
                    else if (i == 2)
                        cScript.m_teamColor = new Color(.8f, 1, 0, 1);
                    else if (i == 3)
                        cScript.m_teamColor = Color.magenta;
                    
                    newChar.GetComponent<Renderer>().materials[0].color = cScript.m_teamColor;
                    newChar.GetComponent<Renderer>().materials[1].color = cScript.m_teamColor;

                    cScript.SetPopupSpheres("");
                    m_characters.Add(newChar);

                    // Link to player
                    cScript.m_player = m_players[i];
                    PlayerScript playScript = m_players[i].GetComponent<PlayerScript>();
                    playScript.m_characters.Add(newChar);

                    PlaceOnBoard(newChar);
                }
            }
    }

    private void PlaceOnBoard(GameObject _obj)
    {
        // Set up position
        TileScript script;
        ObjectScript objScript = _obj.GetComponent<ObjectScript>();
        bool isPlacable = false;
        int randX;
        int randZ;

        do
        {
            randX = Random.Range(0, m_width - 1);
            randZ = Random.Range(0, m_height - 1);

            script = m_tiles[randX + randZ * m_width].GetComponent<TileScript>();

            if (!script.m_holding)
            {
                if (objScript.m_width <= 1)
                    isPlacable = true;
                else if (objScript.m_width == 2 && script.m_neighbors[(int)objScript.m_facing] && !script.m_neighbors[(int)objScript.m_facing].GetComponent<TileScript>().m_holding)
                {
                    isPlacable = true;
                    script.m_neighbors[(int)objScript.m_facing].GetComponent<TileScript>().m_holding = _obj;
                }
            }

        } while (!isPlacable);

        script.m_holding = _obj;
        _obj.transform.SetPositionAndRotation(m_tiles[randX + randZ * m_width].transform.position, _obj.transform.rotation);
        objScript.m_tile = m_tiles[randX + randZ * m_width];
        objScript.m_boardScript = GetComponent<BoardScript>();
    }

    public void Hover()
    {
        if (PanelScript.m_confirmPanel.m_inView)
            return;
        else if (!m_currPlayer || PanelScript.CheckIfPanelOpen())
        {
            if (m_oldTile)
            {
                Renderer oTR = m_oldTile.GetComponent<Renderer>();
                if (oTR.material.color.a != 0)
                    oTR.material.color = new Color(oTR.material.color.r, oTR.material.color.g, oTR.material.color.b, oTR.material.color.a - 0.5f);
                m_oldTile = null;
            }
            return;
        }

        Ray ray;
        RaycastHit hit;

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out hit))
        {
            if (m_highlightedTile)
            {
                TileScript m_highlightedTileScript = m_highlightedTile.GetComponent<TileScript>();
                if (m_highlightedTileScript.m_holding)
                    DeselectHighlightedTile();
            }
            return;
        }

        GameObject tile = null;
        GameObject character = null;
        CharacterScript charScript = null;

        if (hit.collider.gameObject.tag == "Tile")
        {
            tile = hit.collider.gameObject;
            TileScript tScript = tile.GetComponent<TileScript>();
            if (tScript.m_holding && tScript.m_holding.tag == "Player")
            {
                character = tScript.m_holding;
                charScript = character.GetComponent<CharacterScript>();
            }
        }
        else if (hit.collider.GetComponent<ObjectScript>())
        {
            tile = hit.collider.GetComponent<ObjectScript>().m_tile;
            if (hit.collider.gameObject.tag == "Player")
            {
                character = hit.collider.gameObject;
                charScript = character.GetComponent<CharacterScript>();
            }
        }

        if (character)
            HighlightCharacter(character, hit.collider.gameObject);

        if (tile)
            HandleTile(tile);

        if (character && tile.GetComponent<Renderer>().material.color == new Color(1, 0, 0, 1) ||
            character && tile.GetComponent<Renderer>().material.color == Color.yellow)
        {
            PanelScript actPreviewScript = m_panels[(int)pnls.ACTION_PREVIEW].GetComponent<PanelScript>();
            actPreviewScript.m_cScript = charScript;
            actPreviewScript.PopulatePanel();
        }

        if (m_highlightedTile && hit.collider.gameObject != m_highlightedTile)
        {
            TileScript m_highlightedTileScript = m_highlightedTile.GetComponent<TileScript>();
            if (m_highlightedTileScript.m_holding && m_highlightedTileScript.m_holding.tag == "Player" && hit.collider.gameObject != m_highlightedTileScript.m_holding)
                DeselectHighlightedTile();
        }

        if (tile)
            m_highlightedTile = tile;
    }

    private void HandleTile(GameObject _target)
    {
        if (m_oldTile)
        {
            Renderer oTR = m_oldTile.GetComponent<Renderer>();
            if (oTR.material.color == Color.yellow)
            {
                TileScript oldTileScript = m_oldTile.GetComponent<TileScript>();
                oldTileScript.ClearRadius(oldTileScript);
            }
            else if (oTR.material.color.a != 0)
                oTR.material.color = new Color(oTR.material.color.r, oTR.material.color.g, oTR.material.color.b, oTR.material.color.a - 0.5f);

            // Hacky fix for when you spawn colored m_tiles for range and your cursor starts on one of the m_tiles
            if (oTR.material.color.r == 0 && oTR.material.color.b == 1 && oTR.material.color.a == 0f)
                oTR.material.color = new Color(oTR.material.color.r, oTR.material.color.g, oTR.material.color.b, oTR.material.color.a + 0.5f);
        }

        CharacterScript currCharScript = m_currPlayer.GetComponent<CharacterScript>();
        Renderer tarRend = _target.GetComponent<Renderer>();

        // TARGET RED TILE

        bool targetSelf = true;
        int rad = currCharScript.m_currRadius + currCharScript.m_tempStats[(int)CharacterScript.sts.RAD];

        TileScript selTileScript = _target.GetComponent<TileScript>();
        if (currCharScript.m_currAction.Length > 0 && tarRend.material.color == CharacterScript.c_attack) // REFACTOR: All this code just for piercing attack...
        {
            string actName = DatabaseScript.GetActionData(currCharScript.m_currAction, DatabaseScript.actions.NAME);
            string actRng = DatabaseScript.GetActionData(currCharScript.m_currAction, DatabaseScript.actions.RNG);

            if (actName == "Piercing ATK" || actName == "Thrust ATK")
            {
                selTileScript.FetchTilesWithinRange(int.Parse(actRng) + currCharScript.m_tempStats[(int)CharacterScript.sts.RNG], Color.yellow, true, TileScript.targetRestriction.HORVERT, false);
                m_oldTile = _target;
                return;
            }
            else if (actName == "Cross ATK" || actName == "Dual ATK")
            {
                selTileScript.FetchTilesWithinRange(int.Parse(actRng) + currCharScript.m_tempStats[(int)CharacterScript.sts.RNG], Color.yellow, false, TileScript.targetRestriction.DIAGONAL, false);
                m_oldTile = _target;
                return;
            }
            else if (actName == "Slash ATK")
            {
                selTileScript.FetchTilesWithinRange(int.Parse(actRng) + currCharScript.m_tempStats[(int)CharacterScript.sts.RNG], Color.yellow, false, TileScript.targetRestriction.HORVERT, true);
                m_oldTile = _target;
                return;
            }

            if (actName == "Fortifying ATK" || actName == "Blinding ATK")
                targetSelf = false;

            if (actName == "Magnet ATK")
                rad += currCharScript.m_tempStats[(int)CharacterScript.sts.TEC];
        }

        if (currCharScript.m_currRadius + currCharScript.m_tempStats[(int)CharacterScript.sts.RAD] > 0 && tarRend.material.color == CharacterScript.c_attack)
            selTileScript.FetchTilesWithinRange(rad, Color.yellow, targetSelf, TileScript.targetRestriction.NONE, true);
        else
            tarRend.material.color = new Color(tarRend.material.color.r, tarRend.material.color.g, tarRend.material.color.b, tarRend.material.color.a + 0.5f);

        m_oldTile = _target;
    }

    public void HighlightCharacter(GameObject _character, GameObject _hit)
    {
        CharacterScript charScript = _character.GetComponent<CharacterScript>();
        if (_character && _character != m_currPlayer)
        {
            // Change color of turn panel to indicate where the character is in the turn order
            if (charScript.m_turnPanel)
            {
                Image turnPanImage = charScript.m_turnPanel.GetComponent<Image>();
                turnPanImage.color = Color.cyan;
            }

            // Reveal right HUD with highlighted character's data
            PanelScript hudPanScript = m_panels[(int)pnls.HUD_RIGHT_PANEL].GetComponent<PanelScript>();
            hudPanScript.m_cScript = charScript;
            hudPanScript.PopulatePanel();

            Image[] hudEnergy = GameObject.Find("HUD RIGHT Energy").GetComponentsInChildren<Image>();
            int[] charEnergy = charScript.m_player.GetComponent<PlayerScript>().m_energy;
            for (int i = 0; i < charEnergy.Length; i++)
            {
                Text t = hudEnergy[i + 1].GetComponentInChildren<Text>();
                t.text = charEnergy[i].ToString();
            }
        }
    }

    private void DeselectHighlightedTile()
    {
        PanelScript hudPanScript = m_panels[(int)pnls.HUD_RIGHT_PANEL].GetComponent<PanelScript>();
        PanelScript actPreviewScript = m_panels[(int)pnls.ACTION_PREVIEW].GetComponent<PanelScript>();

        TileScript m_highlightedTileScript = m_highlightedTile.GetComponent<TileScript>();

        if (m_highlightedTileScript.m_holding.GetComponent<CharacterScript>())
        {
            CharacterScript colScript = m_highlightedTileScript.m_holding.GetComponent<CharacterScript>();
            if (colScript.m_turnPanel)
            {
                Image turnPanImage = colScript.m_turnPanel.GetComponent<Image>();
                if (colScript.m_effects[(int)StatusScript.effects.STUN] || !colScript.m_isAlive)
                    turnPanImage.color = new Color(1, .5f, .5f, 1);
                else
                    turnPanImage.color = new Color(colScript.m_teamColor.r + 0.3f, colScript.m_teamColor.g + 0.3f, colScript.m_teamColor.b + 0.3f, 1);
            }
        }
        hudPanScript.m_inView = false;
        m_highlightedTile = null;
        actPreviewScript.m_inView = false;
    }

    public void NewTurn()
    {
        if (m_currRound.Count == 0)
            NewRound();

        // Turn previous character back to original color
        if (m_currPlayer)
        {
            Renderer oldRend = m_currPlayer.transform.GetComponent<Renderer>();
            oldRend.materials[1].shader = oldRend.materials[0].shader;
            //Renderer oldRenderer = m_currPlayer.GetComponent<Renderer>();
            //oldRenderer.material.color = m_currPlayer.GetComponent<CharacterScript>().m_teamColor;
        }

        m_currPlayer = m_currRound[0];
        CharacterScript charScript = m_currPlayer.GetComponent<CharacterScript>();

        if (charScript.m_effects[(int)StatusScript.effects.STUN] || !charScript.m_isAlive)
        {
            charScript.m_turnPanel = null;
            StatusScript.UpdateStatus(m_currPlayer, StatusScript.mode.TURN_END);
            m_currRound.Remove(m_currPlayer);

            if (m_currRound.Count == 0)
                NewRound();

            m_currPlayer = m_currRound[0];
            charScript = m_currPlayer.GetComponent<CharacterScript>();
        }

        m_currTile = charScript.m_tile;

        charScript.m_turnPanel = null;

        PanelScript HUDLeftScript = m_panels[(int)pnls.HUD_LEFT_PANEL].GetComponent<PanelScript>();
        HUDLeftScript.m_cScript = m_currPlayer.GetComponent<CharacterScript>();
        HUDLeftScript.PopulatePanel();

        PlayerScript playScript = charScript.m_player.GetComponent<PlayerScript>();
        if (playScript)
            playScript.SetEnergyPanel();
 
        m_currRound.Remove(m_currPlayer);

        CameraScript camScript = m_camera.GetComponent<CameraScript>();
        camScript.m_target = m_currPlayer;

        Renderer rend = m_currPlayer.transform.GetComponent<Renderer>();
        rend.materials[1].shader = Resources.Load<Shader>("Outlined-Silhouette Only (NOT MINE)");
    }

    public void NewRound()
    {
        List<GameObject> tempChars = new List<GameObject>();

        m_livingPlayersInRound = 0;
        for (int i = 0; i < m_characters.Count; i++)
            if (m_characters[i].GetComponent<CharacterScript>().m_isAlive)
            {
                tempChars.Add(m_characters[i]);
                m_livingPlayersInRound++;
            }

        int numPool;
        do
        {
            numPool = 0;
            // Gather all characters speed to pull from
            for (int i = 0; i < tempChars.Count; i++)
            {
                CharacterScript charScript = tempChars[i].GetComponent<CharacterScript>();
                if (charScript.m_tempStats[(int)CharacterScript.sts.SPD] < 0 || charScript.m_effects[(int)StatusScript.effects.STUN] && i == 0)
                    continue;
                else
                    numPool += charScript.m_tempStats[(int)CharacterScript.sts.SPD];
            }

            int randNum = Random.Range(0, numPool);
            int currNum = 0;

            for (int i = 0; i < tempChars.Count; i++)
            {
                CharacterScript charScript = tempChars[i].GetComponent<CharacterScript>();
                if (charScript.m_tempStats[(int)CharacterScript.sts.SPD] <= 0 || charScript.m_effects[(int)StatusScript.effects.STUN] && i == 0)
                    continue;

                currNum += charScript.m_tempStats[(int)CharacterScript.sts.SPD];

                if (randNum < currNum)
                {
                    m_currRound.Add(tempChars[i]);

                    if (charScript.m_tempStats[(int)CharacterScript.sts.SPD] >= 10)
                        numPool -= 10;
                    else
                    {
                        numPool -= charScript.m_tempStats[(int)CharacterScript.sts.SPD];
                        tempChars.RemoveAt(i);
                    }

                    charScript.m_tempStats[(int)CharacterScript.sts.SPD] -= 10;
                    break;
                }
            }
        } while (m_currRound.Count < m_livingPlayersInRound && numPool > 0);

        for (int i = 0; i < m_characters.Count; i++)
        {
            CharacterScript charScript = m_characters[i].GetComponent<CharacterScript>();
            if (!charScript.m_effects[(int)StatusScript.effects.STUN] && charScript.m_isAlive)
                charScript.m_tempStats[(int)CharacterScript.sts.SPD] += 10;

            StatusScript.UpdateStatus(m_characters[i], StatusScript.mode.ROUND_END);
        }

        m_livingPlayersInRound = m_currRound.Count;
        m_roundCount++;
        PanelScript roundPanScript = m_panels[(int)pnls.ROUND_PANEL].GetComponent<PanelScript>();
        roundPanScript.PopulatePanel();
    }
}
